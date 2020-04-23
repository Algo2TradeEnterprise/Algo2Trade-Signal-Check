Imports Algo2TradeBLL
Imports System.Threading
Public Class MultiTimeframeMultiMA
    Inherits Rule

    Private ReadOnly _higherTimeframe As Integer
    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal stockCategory As Integer, ByVal timeFrame As Integer, ByVal useHA As Boolean, ByVal stockName As String, ByVal fileName As String, ByVal higherTF As Integer)
        MyBase.New(canceller, stockCategory, timeFrame, useHA, stockName, fileName)
        _higherTimeframe = higherTF
    End Sub
    Public Overrides Async Function RunAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)

        If _higherTimeframe <= _timeFrame Then Throw New ApplicationException("Higher timeframe can not be less than or equal to main timeframe")

        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Trading Symbol")
        ret.Columns.Add("Signal Candle")
        ret.Columns.Add("Signal")
        ret.Columns.Add("Price")

        Dim stockData As StockSelection = New StockSelection(_canceller, _category, _cmn, _fileName)
        AddHandler stockData.Heartbeat, AddressOf OnHeartbeat
        AddHandler stockData.WaitingFor, AddressOf OnWaitingFor
        AddHandler stockData.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
        AddHandler stockData.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
        Dim chkDate As Date = startDate
        While chkDate <= endDate
            _canceller.Token.ThrowIfCancellationRequested()
            Dim stockList As List(Of String) = Nothing
            If _instrumentName Is Nothing OrElse _instrumentName = "" Then
                stockList = Await stockData.GetStockList(chkDate).ConfigureAwait(False)
            Else
                stockList = New List(Of String)
                stockList.Add(_instrumentName)
            End If
            _canceller.Token.ThrowIfCancellationRequested()
            If stockList IsNot Nothing AndAlso stockList.Count > 0 Then
                For Each stock In stockList
                    _canceller.Token.ThrowIfCancellationRequested()
                    Dim stockPayload As Dictionary(Of Date, Payload) = Nothing
                    Select Case _category
                        Case Common.DataBaseTable.Intraday_Cash, Common.DataBaseTable.Intraday_Commodity, Common.DataBaseTable.Intraday_Currency, Common.DataBaseTable.Intraday_Futures
                            stockPayload = _cmn.GetRawPayload(_category, stock, chkDate.AddDays(-8), chkDate)
                        Case Common.DataBaseTable.EOD_Cash, Common.DataBaseTable.EOD_Commodity, Common.DataBaseTable.EOD_Currency, Common.DataBaseTable.EOD_Futures, Common.DataBaseTable.EOD_POSITIONAL
                            stockPayload = _cmn.GetRawPayload(_category, stock, chkDate.AddDays(-200), chkDate)
                    End Select
                    _canceller.Token.ThrowIfCancellationRequested()
                    If stockPayload IsNot Nothing AndAlso stockPayload.Count > 0 Then
                        Dim XMinuteLFPayload As Dictionary(Of Date, Payload) = Nothing
                        Dim XMinuteHFPayload As Dictionary(Of Date, Payload) = Nothing
                        Dim exchangeStartTime As Date = Date.MinValue
                        Select Case _category
                            Case Common.DataBaseTable.EOD_Cash, Common.DataBaseTable.EOD_Futures, Common.DataBaseTable.EOD_POSITIONAL, Common.DataBaseTable.Intraday_Cash, Common.DataBaseTable.Intraday_Futures
                                exchangeStartTime = New Date(chkDate.Year, chkDate.Month, chkDate.Day, 9, 15, 0)
                            Case Common.DataBaseTable.EOD_Commodity, Common.DataBaseTable.EOD_Currency, Common.DataBaseTable.Intraday_Commodity, Common.DataBaseTable.Intraday_Currency
                                exchangeStartTime = New Date(chkDate.Year, chkDate.Month, chkDate.Day, 9, 0, 0)
                        End Select
                        If _timeFrame > 1 Then
                            XMinuteLFPayload = Common.ConvertPayloadsToXMinutes(stockPayload, _timeFrame, exchangeStartTime)
                        Else
                            XMinuteLFPayload = stockPayload
                        End If
                        _canceller.Token.ThrowIfCancellationRequested()
                        XMinuteHFPayload = Common.ConvertPayloadsToXMinutes(stockPayload, _higherTimeframe, exchangeStartTime)
                        _canceller.Token.ThrowIfCancellationRequested()
                        Dim inputLFPayload As Dictionary(Of Date, Payload) = Nothing
                        Dim inputHFPayload As Dictionary(Of Date, Payload) = Nothing
                        If _useHA Then
                            Indicator.HeikenAshi.ConvertToHeikenAshi(XMinuteLFPayload, inputLFPayload)
                            Indicator.HeikenAshi.ConvertToHeikenAshi(XMinuteHFPayload, inputHFPayload)
                        Else
                            inputLFPayload = XMinuteLFPayload
                            inputHFPayload = XMinuteHFPayload
                        End If
                        _canceller.Token.ThrowIfCancellationRequested()
                        Dim currentDayLFPayload As Dictionary(Of Date, Payload) = Nothing
                        For Each runningPayload In inputLFPayload.Keys
                            _canceller.Token.ThrowIfCancellationRequested()
                            If runningPayload.Date = chkDate.Date Then
                                If currentDayLFPayload Is Nothing Then currentDayLFPayload = New Dictionary(Of Date, Payload)
                                currentDayLFPayload.Add(runningPayload, inputLFPayload(runningPayload))
                            End If
                        Next

                        'Main Logic
                        If currentDayLFPayload IsNot Nothing AndAlso currentDayLFPayload.Count > 0 Then
                            Dim sma21LFPayload As Dictionary(Of Date, Decimal) = Nothing
                            Dim sma13LFPayload As Dictionary(Of Date, Decimal) = Nothing
                            Dim sma8LFPayload As Dictionary(Of Date, Decimal) = Nothing
                            Indicator.SMA.CalculateSMA(21, Payload.PayloadFields.Close, inputLFPayload, sma21LFPayload)
                            Indicator.SMA.CalculateSMA(13, Payload.PayloadFields.Close, inputLFPayload, sma13LFPayload)
                            Indicator.SMA.CalculateSMA(8, Payload.PayloadFields.Close, inputLFPayload, sma8LFPayload)

                            Dim sma21HFPayload As Dictionary(Of Date, Decimal) = Nothing
                            Dim sma8HFPayload As Dictionary(Of Date, Decimal) = Nothing
                            Indicator.SMA.CalculateSMA(21, Payload.PayloadFields.Close, inputHFPayload, sma21HFPayload)
                            Indicator.SMA.CalculateSMA(8, Payload.PayloadFields.Close, inputHFPayload, sma8HFPayload)

                            Dim readyToBuy As Integer = 0
                            Dim buySignalCandle As Payload = Nothing
                            Dim readyToSell As Integer = 0
                            Dim sellSignalCandle As Payload = Nothing
                            For Each runningPayload In currentDayLFPayload
                                _canceller.Token.ThrowIfCancellationRequested()
                                Dim higherTFTime As Date = Common.GetCurrentXMinuteCandleTime(runningPayload.Key, exchangeStartTime, _higherTimeframe)
                                If higherTFTime <> Date.MinValue AndAlso inputHFPayload.ContainsKey(higherTFTime) AndAlso
                                    inputHFPayload(higherTFTime).PreviousCandlePayload IsNot Nothing AndAlso
                                    inputHFPayload(higherTFTime).PreviousCandlePayload.PayloadDate.Date = runningPayload.Key.Date Then
                                    Dim hfCandle As Payload = inputHFPayload(higherTFTime)
                                    If sma8HFPayload(hfCandle.PreviousCandlePayload.PayloadDate) > sma21HFPayload(hfCandle.PreviousCandlePayload.PayloadDate) Then
                                        If hfCandle.PreviousCandlePayload.Low < sma8HFPayload(hfCandle.PreviousCandlePayload.PayloadDate) Then
                                            If readyToBuy = 0 Then
                                                readyToBuy = 1
                                            End If
                                        End If
                                        If readyToBuy = 1 Then
                                            If hfCandle.PreviousCandlePayload.Close > sma8HFPayload(hfCandle.PreviousCandlePayload.PayloadDate) Then
                                                readyToBuy = 2
                                            End If
                                        ElseIf readyToBuy = 2 Then
                                            If sma8LFPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate) > sma13LFPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate) AndAlso
                                                sma13LFPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate) > sma21LFPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate) Then
                                                If runningPayload.Value.PreviousCandlePayload.Low < sma8LFPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate) Then
                                                    buySignalCandle = runningPayload.Value.PreviousCandlePayload
                                                End If
                                                If runningPayload.Value.PreviousCandlePayload.Close < sma21LFPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate) Then
                                                    buySignalCandle = Nothing
                                                End If
                                                If buySignalCandle IsNot Nothing Then
                                                    Dim subPayload As List(Of KeyValuePair(Of Date, Payload)) = Common.GetSubPayload(currentDayLFPayload, buySignalCandle.PayloadDate, 5, True)
                                                    If subPayload IsNot Nothing AndAlso subPayload.Count > 0 Then
                                                        Dim highestHigh As Decimal = subPayload.Max(Function(x)
                                                                                                        Return x.Value.High
                                                                                                    End Function)
                                                        If runningPayload.Value.High >= highestHigh Then
                                                            Dim row As DataRow = ret.NewRow
                                                            row("Date") = runningPayload.Value.PayloadDate
                                                            row("Trading Symbol") = runningPayload.Value.TradingSymbol
                                                            row("Signal Candle") = buySignalCandle.PayloadDate.ToString("HH:mm:ss")
                                                            row("Signal") = "Buy"
                                                            row("Price") = highestHigh
                                                            ret.Rows.Add(row)
                                                            readyToBuy = 0
                                                            buySignalCandle = Nothing
                                                        End If
                                                    End If
                                                End If
                                            Else
                                                readyToBuy = 1
                                                buySignalCandle = Nothing
                                            End If
                                        End If
                                    Else
                                        readyToBuy = 1
                                        buySignalCandle = Nothing
                                    End If
                                    If sma8HFPayload(hfCandle.PreviousCandlePayload.PayloadDate) < sma21HFPayload(hfCandle.PreviousCandlePayload.PayloadDate) Then
                                        If hfCandle.PreviousCandlePayload.High > sma8HFPayload(hfCandle.PreviousCandlePayload.PayloadDate) Then
                                            If readyToSell = 0 Then
                                                readyToSell = 1
                                            End If
                                        End If
                                        If readyToSell = 1 Then
                                            If hfCandle.PreviousCandlePayload.Close < sma8HFPayload(hfCandle.PreviousCandlePayload.PayloadDate) Then
                                                readyToSell = 2
                                            End If
                                        ElseIf readyToSell = 2 Then
                                            If sma8LFPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate) < sma13LFPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate) AndAlso
                                                sma13LFPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate) < sma21LFPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate) Then
                                                If runningPayload.Value.PreviousCandlePayload.High > sma8LFPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate) Then
                                                    sellSignalCandle = runningPayload.Value.PreviousCandlePayload
                                                End If
                                                If runningPayload.Value.PreviousCandlePayload.Close > sma21LFPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate) Then
                                                    sellSignalCandle = Nothing
                                                End If
                                                If sellSignalCandle IsNot Nothing Then
                                                    Dim subPayload As List(Of KeyValuePair(Of Date, Payload)) = Common.GetSubPayload(currentDayLFPayload, sellSignalCandle.PayloadDate, 5, True)
                                                    If subPayload IsNot Nothing AndAlso subPayload.Count > 0 Then
                                                        Dim lowestLow As Decimal = subPayload.Min(Function(x)
                                                                                                      Return x.Value.Low
                                                                                                  End Function)
                                                        If runningPayload.Value.Low <= lowestLow Then
                                                            Dim row As DataRow = ret.NewRow
                                                            row("Date") = runningPayload.Value.PayloadDate
                                                            row("Trading Symbol") = runningPayload.Value.TradingSymbol
                                                            row("Signal Candle") = sellSignalCandle.PayloadDate.ToString("HH:mm:ss")
                                                            row("Signal") = "Sell"
                                                            row("Price") = lowestLow
                                                            ret.Rows.Add(row)
                                                            readyToSell = 0
                                                            sellSignalCandle = Nothing
                                                        End If
                                                    End If
                                                End If
                                            Else
                                                readyToSell = 1
                                                sellSignalCandle = Nothing
                                            End If
                                        End If
                                    Else
                                        readyToSell = 1
                                        sellSignalCandle = Nothing
                                    End If
                                End If
                            Next
                        End If
                    End If
                Next
            End If
            chkDate = chkDate.AddDays(1)
        End While
        Return ret
    End Function
End Class