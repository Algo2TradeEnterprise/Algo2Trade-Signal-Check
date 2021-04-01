Imports Algo2TradeBLL
Imports System.Threading
Public Class FractalConfirmationOnHTHK
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
        ret.Columns.Add("Signal Candle")
        ret.Columns.Add("Stoploss")

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
                        Case Common.DataBaseTable.Intraday_Futures_Options
                            stockPayload = _cmn.GetRawPayloadForSpecificTradingSymbol(_category, stock, chkDate.AddDays(-8), chkDate)
                        Case Common.DataBaseTable.EOD_Futures_Options
                            stockPayload = _cmn.GetRawPayloadForSpecificTradingSymbol(_category, stock, chkDate.AddDays(-200), chkDate)
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
                            Dim xMinPayload As Dictionary(Of Date, Payload) = Common.ConvertPayloadsToXMinutes(inputPayload, 15, New Date(Now.Year, Now.Month, Now.Day, 9, 15, 0))
                            Dim htHKPayload As Dictionary(Of Date, Payload) = Nothing
                            Indicator.HeikenAshi.ConvertToHeikenAshi(xMinPayload, htHKPayload)
                            Dim fractalHighPayload As Dictionary(Of Date, Decimal) = Nothing
                            Dim fractalLowPayload As Dictionary(Of Date, Decimal) = Nothing
                            Indicator.FractalBands.CalculateFractal(inputPayload, fractalHighPayload, fractalLowPayload)

                            Dim tradingSymbol As String = _cmn.GetCurrentTradingSymbol(Common.DataBaseTable.EOD_Futures, chkDate, stock)
                            Dim lotSize As Integer = _cmn.GetLotSize(Common.DataBaseTable.EOD_Futures, tradingSymbol, chkDate)

                            Dim activeDirection As Integer = 0
                            Dim sl As Decimal = Decimal.MinValue
                            Dim lastSignl As Integer = 0
                            Dim lastSignalCandle As Payload = Nothing
                            Dim lastSupportCandle As Payload = Nothing
                            For Each runningPayload In currentDayPayload.Keys
                                _canceller.Token.ThrowIfCancellationRequested()
                                Dim currentXMinute As Date = Common.GetCurrentXMinuteCandleTime(runningPayload, New Date(Now.Year, Now.Month, Now.Day, 9, 15, 0), 15)
                                If htHKPayload.ContainsKey(currentXMinute) Then
                                    Dim previousHTHKCandle As Payload = htHKPayload(currentXMinute).PreviousCandlePayload
                                    If previousHTHKCandle IsNot Nothing Then
                                        Dim currentCandle As Payload = currentDayPayload(runningPayload)
                                        If activeDirection <> 0 Then
                                            If activeDirection = 1 AndAlso currentCandle.Low <= sl Then
                                                activeDirection = 0
                                                sl = Decimal.MinValue
                                            ElseIf activeDirection = -1 AndAlso currentCandle.High >= sl Then
                                                activeDirection = 0
                                                sl = Decimal.MinValue
                                            Else
                                                'If activeDirection = -1 AndAlso previousHTHKCandle.CandleColor = Color.Green Then
                                                '    activeDirection = 0
                                                '    sl = Decimal.MinValue
                                                'ElseIf activeDirection = 1 AndAlso previousHTHKCandle.CandleColor = Color.Red Then
                                                '    activeDirection = 0
                                                '    sl = Decimal.MinValue
                                                'End If
                                            End If
                                        End If
                                        If activeDirection = 0 Then
                                            If lastSignalCandle IsNot Nothing Then
                                                If lastSignl = 1 Then
                                                    If currentCandle.High > lastSignalCandle.High Then
                                                        sl = GetStoploss(If(lastSupportCandle, lastSignalCandle), lastSignalCandle, 1, currentDayPayload, fractalLowPayload)

                                                        Dim row As DataRow = ret.NewRow
                                                        row("Date") = inputPayload(runningPayload).PayloadDate
                                                        row("Trading Symbol") = inputPayload(runningPayload).TradingSymbol
                                                        row("Signal") = "BUY"
                                                        row("Signal Candle") = lastSignalCandle.PayloadDate.ToString("HH:mm:ss")
                                                        row("Stoploss") = (lastSignalCandle.High - sl) * lotSize
                                                        ret.Rows.Add(row)

                                                        lastSignl = 0
                                                        lastSignalCandle = Nothing
                                                        lastSupportCandle = Nothing
                                                        activeDirection = 1
                                                    End If
                                                ElseIf lastSignl = -1 Then
                                                    If currentCandle.Low < lastSignalCandle.Low Then
                                                        sl = GetStoploss(If(lastSupportCandle, lastSignalCandle), lastSignalCandle, -1, currentDayPayload, fractalHighPayload)

                                                        Dim row As DataRow = ret.NewRow
                                                        row("Date") = inputPayload(runningPayload).PayloadDate
                                                        row("Trading Symbol") = inputPayload(runningPayload).TradingSymbol
                                                        row("Signal") = "SELL"
                                                        row("Signal Candle") = lastSignalCandle.PayloadDate.ToString("HH:mm:ss")
                                                        row("Stoploss") = (sl - lastSignalCandle.Low) * lotSize
                                                        ret.Rows.Add(row)

                                                        lastSignl = 0
                                                        lastSignalCandle = Nothing
                                                        lastSupportCandle = Nothing
                                                        activeDirection = -1
                                                    End If
                                                End If
                                            End If

                                            If previousHTHKCandle.CandleColor = Color.Green Then
                                                If lastSignl <> 1 Then
                                                    lastSignl = 0
                                                    lastSignalCandle = Nothing
                                                    lastSupportCandle = Nothing
                                                End If
                                                If lastSupportCandle IsNot Nothing Then
                                                    If currentCandle.CandleColor = Color.Green Then
                                                        lastSignalCandle = currentCandle
                                                    End If
                                                Else
                                                    If currentCandle.CandleColor = Color.Red AndAlso
                                                    currentCandle.Close <= fractalLowPayload(currentCandle.PayloadDate) Then
                                                        lastSupportCandle = currentCandle
                                                        lastSignl = 1
                                                    End If
                                                End If
                                                If currentCandle.Low <= fractalLowPayload(currentCandle.PayloadDate) AndAlso
                                                    currentCandle.Close >= fractalLowPayload(currentCandle.PayloadDate) Then
                                                    If lastSignalCandle Is Nothing Then
                                                        lastSignalCandle = currentCandle
                                                        lastSignl = 1
                                                    Else
                                                        If lastSupportCandle IsNot Nothing Then
                                                            If fractalLowPayload(lastSupportCandle.PayloadDate) <> fractalLowPayload(currentCandle.PayloadDate) Then
                                                                lastSignalCandle = currentCandle
                                                                lastSignl = 1
                                                            End If
                                                        Else
                                                            lastSignalCandle = currentCandle
                                                            lastSignl = 1
                                                        End If
                                                    End If
                                                End If
                                            ElseIf previousHTHKCandle.CandleColor = Color.Red Then
                                                If lastSignl <> -1 Then
                                                    lastSignl = 0
                                                    lastSignalCandle = Nothing
                                                    lastSupportCandle = Nothing
                                                End If
                                                If lastSupportCandle IsNot Nothing Then
                                                    If currentCandle.CandleColor = Color.Red Then
                                                        lastSignalCandle = currentCandle
                                                    End If
                                                Else
                                                    If currentCandle.CandleColor = Color.Green AndAlso
                                                    currentCandle.Close >= fractalHighPayload(currentCandle.PayloadDate) Then
                                                        lastSupportCandle = currentCandle
                                                        lastSignl = -1
                                                    End If
                                                End If
                                                If currentCandle.High >= fractalHighPayload(currentCandle.PayloadDate) AndAlso
                                                    currentCandle.Close <= fractalHighPayload(currentCandle.PayloadDate) Then
                                                    If lastSignalCandle Is Nothing Then
                                                        lastSignalCandle = currentCandle
                                                        lastSignl = -1
                                                    Else
                                                        If lastSupportCandle IsNot Nothing Then
                                                            If fractalHighPayload(lastSupportCandle.PayloadDate) <> fractalHighPayload(currentCandle.PayloadDate) Then
                                                                lastSignalCandle = currentCandle
                                                                lastSignl = -1
                                                            End If
                                                        Else
                                                            lastSignalCandle = currentCandle
                                                            lastSignl = -1
                                                        End If
                                                    End If
                                                End If
                                            End If
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

    Private Function GetStoploss(ByVal supportCandle As Payload, ByVal signalCandle As Payload, ByVal signalDirection As Integer, ByVal currentDayPayload As Dictionary(Of Date, Payload), ByVal fractalPayload As Dictionary(Of Date, Decimal)) As Decimal
        Dim ret As Decimal = Decimal.MinValue
        If signalCandle.PayloadDate < supportCandle.PayloadDate Then supportCandle = signalCandle
        If signalDirection = 1 Then
            Dim lowestLow As Decimal = currentDayPayload.Min(Function(x)
                                                                 If x.Key >= supportCandle.PayloadDate AndAlso x.Key <= signalCandle.PayloadDate Then
                                                                     Return Math.Min(x.Value.Low, fractalPayload(x.Key))
                                                                 Else
                                                                     Return Decimal.MaxValue
                                                                 End If
                                                             End Function)
            ret = lowestLow
        ElseIf signalDirection = -1 Then
            Dim highestHigh As Decimal = currentDayPayload.Max(Function(x)
                                                                   If x.Key >= supportCandle.PayloadDate AndAlso x.Key <= signalCandle.PayloadDate Then
                                                                       Return Math.Max(x.Value.High, fractalPayload(x.Key))
                                                                   Else
                                                                       Return Decimal.MinValue
                                                                   End If
                                                               End Function)
            ret = highestHigh
        End If
        Return ret
    End Function
End Class