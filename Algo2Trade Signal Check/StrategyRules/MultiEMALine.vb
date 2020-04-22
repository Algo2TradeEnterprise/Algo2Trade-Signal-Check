Imports Algo2TradeBLL
Imports System.Threading
Public Class MultiEMALine
    Inherits Rule
    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal stockCategory As Integer, ByVal timeFrame As Integer, ByVal useHA As Boolean, ByVal stockName As String, ByVal fileName As String)
        MyBase.New(canceller, stockCategory, timeFrame, useHA, stockName, fileName)
    End Sub
    Public Overrides Async Function RunAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Trading Symbol")
        ret.Columns.Add("Signal")

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
                        Dim XMinutePayload As Dictionary(Of Date, Payload) = Nothing
                        Dim exchangeStartTime As Date = Date.MinValue
                        Select Case _category
                            Case Common.DataBaseTable.EOD_Cash, Common.DataBaseTable.EOD_Futures, Common.DataBaseTable.EOD_POSITIONAL, Common.DataBaseTable.Intraday_Cash, Common.DataBaseTable.Intraday_Futures
                                exchangeStartTime = New Date(chkDate.Year, chkDate.Month, chkDate.Day, 9, 15, 0)
                            Case Common.DataBaseTable.EOD_Commodity, Common.DataBaseTable.EOD_Currency, Common.DataBaseTable.Intraday_Commodity, Common.DataBaseTable.Intraday_Currency
                                exchangeStartTime = New Date(chkDate.Year, chkDate.Month, chkDate.Day, 9, 0, 0)
                        End Select
                        If _timeFrame > 1 Then
                            XMinutePayload = Common.ConvertPayloadsToXMinutes(stockPayload, _timeFrame, exchangeStartTime)
                        Else
                            XMinutePayload = stockPayload
                        End If
                        _canceller.Token.ThrowIfCancellationRequested()
                        Dim inputPayload As Dictionary(Of Date, Payload) = Nothing
                        If _useHA Then
                            Indicator.HeikenAshi.ConvertToHeikenAshi(XMinutePayload, inputPayload)
                        Else
                            inputPayload = XMinutePayload
                        End If
                        _canceller.Token.ThrowIfCancellationRequested()
                        Dim currentDayPayload As Dictionary(Of Date, Payload) = Nothing
                        For Each runningPayload In inputPayload.Keys
                            _canceller.Token.ThrowIfCancellationRequested()
                            If runningPayload.Date = chkDate.Date Then
                                If currentDayPayload Is Nothing Then currentDayPayload = New Dictionary(Of Date, Payload)
                                currentDayPayload.Add(runningPayload, inputPayload(runningPayload))
                            End If
                        Next

                        'Main Logic
                        If currentDayPayload IsNot Nothing AndAlso currentDayPayload.Count > 0 Then
                            Dim ema50Payload As Dictionary(Of Date, Decimal) = Nothing
                            Dim ema100Payload As Dictionary(Of Date, Decimal) = Nothing
                            Dim ema150Payload As Dictionary(Of Date, Decimal) = Nothing
                            Indicator.EMA.CalculateEMA(50, Payload.PayloadFields.Close, inputPayload, ema50Payload)
                            Indicator.EMA.CalculateEMA(100, Payload.PayloadFields.Close, inputPayload, ema100Payload)
                            Indicator.EMA.CalculateEMA(150, Payload.PayloadFields.Close, inputPayload, ema150Payload)

                            Dim readyToBuy As Integer = 0
                            Dim readyToSell As Integer = 0
                            For Each runningPayload In currentDayPayload
                                _canceller.Token.ThrowIfCancellationRequested()
                                If runningPayload.Value.PreviousCandlePayload.PayloadDate.Date = runningPayload.Key.Date Then
                                    If ema50Payload(runningPayload.Value.PreviousCandlePayload.PayloadDate) > ema100Payload(runningPayload.Value.PreviousCandlePayload.PayloadDate) AndAlso
                                        ema100Payload(runningPayload.Value.PreviousCandlePayload.PayloadDate) > ema150Payload(runningPayload.Value.PreviousCandlePayload.PayloadDate) Then
                                        readyToSell = 0
                                        If runningPayload.Value.PreviousCandlePayload.Close > ema50Payload(runningPayload.Value.PreviousCandlePayload.PayloadDate) Then
                                            If readyToBuy = 0 Then
                                                readyToBuy = 1
                                            ElseIf readyToBuy = 2 Then
                                                readyToBuy = 3
                                            End If
                                        ElseIf runningPayload.Value.PreviousCandlePayload.Close < ema50Payload(runningPayload.Value.PreviousCandlePayload.PayloadDate) Then
                                            If runningPayload.Value.PreviousCandlePayload.Close > ema150Payload(runningPayload.Value.PreviousCandlePayload.PayloadDate) Then
                                                If readyToBuy = 1 Then
                                                    readyToBuy = 2
                                                End If
                                            Else
                                                readyToBuy = 0
                                            End If
                                        End If
                                        If readyToBuy = 3 Then
                                            Dim row As DataRow = ret.NewRow
                                            row("Date") = runningPayload.Value.PayloadDate
                                            row("Trading Symbol") = runningPayload.Value.TradingSymbol
                                            row("Signal") = "Buy"
                                            ret.Rows.Add(row)
                                            readyToBuy = 4
                                        End If
                                    End If
                                    If ema50Payload(runningPayload.Value.PreviousCandlePayload.PayloadDate) < ema100Payload(runningPayload.Value.PreviousCandlePayload.PayloadDate) AndAlso
                                        ema100Payload(runningPayload.Value.PreviousCandlePayload.PayloadDate) < ema150Payload(runningPayload.Value.PreviousCandlePayload.PayloadDate) Then
                                        readyToBuy = 0
                                        If runningPayload.Value.PreviousCandlePayload.Close < ema50Payload(runningPayload.Value.PreviousCandlePayload.PayloadDate) Then
                                            If readyToSell = 0 Then
                                                readyToSell = 1
                                            ElseIf readyToSell = 2 Then
                                                readyToSell = 3
                                            End If
                                        ElseIf runningPayload.Value.PreviousCandlePayload.Close > ema50Payload(runningPayload.Value.PreviousCandlePayload.PayloadDate) Then
                                            If runningPayload.Value.PreviousCandlePayload.Close < ema150Payload(runningPayload.Value.PreviousCandlePayload.PayloadDate) Then
                                                If readyToSell = 1 Then
                                                    readyToSell = 2
                                                End If
                                            Else
                                                readyToSell = 0
                                            End If
                                        End If
                                        If readyToSell = 3 Then
                                            Dim row As DataRow = ret.NewRow
                                            row("Date") = runningPayload.Value.PayloadDate
                                            row("Trading Symbol") = runningPayload.Value.TradingSymbol
                                            row("Signal") = "Sell"
                                            ret.Rows.Add(row)
                                            readyToSell = 4
                                        End If
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