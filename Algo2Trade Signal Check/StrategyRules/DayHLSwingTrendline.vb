Imports Algo2TradeBLL
Imports System.Threading
Public Class DayHLSwingTrendline
    Inherits Rule

    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal stockCategory As Integer, ByVal timeFrame As Integer, ByVal useHA As Boolean, ByVal stockName As String, ByVal fileName As String)
        MyBase.New(canceller, stockCategory, timeFrame, useHA, stockName, fileName)
    End Sub

    Public Overrides Async Function RunAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Trading Symbol")
        ret.Columns.Add("Highest Candle")
        ret.Columns.Add("Swing")
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
                        OnHeartbeat("Processing data")
                        If currentDayPayload IsNot Nothing AndAlso currentDayPayload.Count > 0 Then
                            Dim swingPayload As Dictionary(Of Date, Indicator.Swing) = Nothing
                            Indicator.SwingHighLow.CalculateSwingHighLow(inputPayload, False, swingPayload)

                            Dim emaPayload As Dictionary(Of Date, Decimal) = Nothing
                            Indicator.EMA.CalculateEMA(13, Payload.PayloadFields.Close, inputPayload, emaPayload)

                            Dim lastSignal As Integer = 0
                            Dim lastHighTrendLine As TrendLineVeriables = Nothing
                            Dim lastLowTrendLine As TrendLineVeriables = Nothing
                            For Each runningCandle In currentDayPayload
                                _canceller.Token.ThrowIfCancellationRequested()
                                If runningCandle.Value.PreviousCandlePayload.PreviousCandlePayload.PayloadDate.Date = chkDate.Date Then
                                    Dim swing As Indicator.Swing = swingPayload(runningCandle.Value.PreviousCandlePayload.PreviousCandlePayload.PayloadDate)
                                    If swing.SwingHighTime.Date = chkDate.Date Then
                                        Dim highestCandle As Payload = GetHighestHigh(currentDayPayload, runningCandle.Value.PreviousCandlePayload)
                                        If swing.SwingHighTime > highestCandle.PayloadDate AndAlso swing.SwingHigh < highestCandle.High Then
                                            Dim swingTime As Date = swing.SwingHighTime
                                            If currentDayPayload(swingTime.AddMinutes(_timeFrame)).High = swing.SwingHigh Then
                                                swingTime = swingTime.AddMinutes(_timeFrame)
                                            End If
                                            Dim x1 As Decimal = 0
                                            Dim y1 As Decimal = highestCandle.High
                                            Dim x2 As Decimal = currentDayPayload.Where(Function(x)
                                                                                            Return x.Key > highestCandle.PayloadDate AndAlso x.Key <= swingTime
                                                                                        End Function).Count
                                            Dim y2 As Decimal = swing.SwingHigh
                                            Dim highTrendLine As TrendLineVeriables = Common.GetEquationOfTrendLine(x1, y1, x2, y2)
                                            highTrendLine.Point1 = highestCandle.PayloadDate
                                            highTrendLine.Point2 = swingTime
                                            If swing.SwingHigh < emaPayload(swing.SwingHighTime) Then
                                                highTrendLine = lastHighTrendLine
                                            End If
                                            If highTrendLine IsNot Nothing Then
                                                Dim firstCandle As Payload = currentDayPayload(highTrendLine.Point2.AddMinutes(_timeFrame * -1))
                                                Dim middleCandle As Payload = currentDayPayload(highTrendLine.Point2)
                                                Dim lastCandle As Payload = currentDayPayload(highTrendLine.Point2.AddMinutes(_timeFrame))
                                                If firstCandle.Close > emaPayload(firstCandle.PayloadDate) AndAlso
                                                    middleCandle.Close > emaPayload(middleCandle.PayloadDate) AndAlso
                                                    lastCandle.Close > emaPayload(lastCandle.PayloadDate) Then
                                                    highTrendLine = lastHighTrendLine
                                                End If
                                            End If
                                            If highTrendLine IsNot Nothing AndAlso Not IsValidTrendLine(highTrendLine, currentDayPayload) Then
                                                If lastHighTrendLine IsNot Nothing AndAlso lastHighTrendLine.Point1 <> highTrendLine.Point1 Then
                                                    lastHighTrendLine = Nothing
                                                End If
                                                highTrendLine = lastHighTrendLine
                                            End If
                                            If highTrendLine IsNot Nothing Then
                                                lastHighTrendLine = highTrendLine
                                                Dim counter As Integer = currentDayPayload.Where(Function(x)
                                                                                                     Return x.Key > highTrendLine.Point1 AndAlso x.Key <= runningCandle.Key
                                                                                                 End Function).Count
                                                Dim highPoint As Decimal = Math.Round(highTrendLine.M * counter + highTrendLine.C, 2)
                                                If lastSignal <> 1 AndAlso runningCandle.Value.Low > highPoint AndAlso runningCandle.Value.Close > emaPayload(runningCandle.Key) Then
                                                    Dim row As DataRow = ret.NewRow
                                                    row("Date") = runningCandle.Value.PayloadDate.ToString("dd-MMM-yyyy HH:mm:ss")
                                                    row("Trading Symbol") = runningCandle.Value.TradingSymbol
                                                    row("Highest Candle") = highTrendLine.Point1.ToString("HH:mm:ss")
                                                    row("Swing") = highTrendLine.Point2.ToString("HH:mm:ss")
                                                    row("Signal") = "Buy"

                                                    ret.Rows.Add(row)
                                                    lastSignal = 1
                                                End If
                                            End If
                                        End If
                                    End If
                                    If swing.SwingLowTime.Date = chkDate.Date Then
                                        Dim lowestCandle As Payload = GetLowestLow(currentDayPayload, runningCandle.Value.PreviousCandlePayload)
                                        If swing.SwingLowTime > lowestCandle.PayloadDate AndAlso swing.SwingLow > lowestCandle.Low Then
                                            Dim swingTime As Date = swing.SwingLowTime
                                            If currentDayPayload(swingTime.AddMinutes(_timeFrame)).Low = swing.SwingLow Then
                                                swingTime = swingTime.AddMinutes(_timeFrame)
                                            End If
                                            Dim x1 As Decimal = 0
                                            Dim y1 As Decimal = lowestCandle.Low
                                            Dim x2 As Decimal = currentDayPayload.Where(Function(x)
                                                                                            Return x.Key > lowestCandle.PayloadDate AndAlso x.Key <= swingTime
                                                                                        End Function).Count
                                            Dim y2 As Decimal = swing.SwingLow
                                            Dim lowTrendLine As TrendLineVeriables = Common.GetEquationOfTrendLine(x1, y1, x2, y2)
                                            lowTrendLine.Point1 = lowestCandle.PayloadDate
                                            lowTrendLine.Point2 = swingTime
                                            If swing.SwingLow > emaPayload(swing.SwingLowTime) Then
                                                lowTrendLine = lastLowTrendLine
                                            End If
                                            If lowTrendLine IsNot Nothing Then
                                                Dim firstCandle As Payload = currentDayPayload(lowTrendLine.Point2.AddMinutes(_timeFrame * -1))
                                                Dim middleCandle As Payload = currentDayPayload(lowTrendLine.Point2)
                                                Dim lastCandle As Payload = currentDayPayload(lowTrendLine.Point2.AddMinutes(_timeFrame))
                                                If firstCandle.Close < emaPayload(firstCandle.PayloadDate) AndAlso
                                                    middleCandle.Close < emaPayload(middleCandle.PayloadDate) AndAlso
                                                    lastCandle.Close < emaPayload(lastCandle.PayloadDate) Then
                                                    lowTrendLine = lastLowTrendLine
                                                End If
                                            End If
                                            If lowTrendLine IsNot Nothing AndAlso Not IsValidTrendLine(lowTrendLine, currentDayPayload) Then
                                                If lastLowTrendLine IsNot Nothing AndAlso lastLowTrendLine.Point1 <> lowTrendLine.Point1 Then
                                                    lastLowTrendLine = Nothing
                                                End If
                                                lowTrendLine = lastLowTrendLine
                                            End If
                                            If lowTrendLine IsNot Nothing Then
                                                lastLowTrendLine = lowTrendLine
                                                Dim counter As Integer = currentDayPayload.Where(Function(x)
                                                                                                     Return x.Key > lowTrendLine.Point1 AndAlso x.Key <= runningCandle.Key
                                                                                                 End Function).Count
                                                Dim lowPoint As Decimal = Math.Round(lowTrendLine.M * counter + lowTrendLine.C, 2)
                                                If lastSignal <> -1 AndAlso runningCandle.Value.High < lowPoint AndAlso runningCandle.Value.Close < emaPayload(runningCandle.Key) Then
                                                    Dim row As DataRow = ret.NewRow
                                                    row("Date") = runningCandle.Value.PayloadDate.ToString("dd-MMM-yyyy HH:mm:ss")
                                                    row("Trading Symbol") = runningCandle.Value.TradingSymbol
                                                    row("Highest Candle") = lowTrendLine.Point1.ToString("HH:mm:ss")
                                                    row("Swing") = lowTrendLine.Point2.ToString("HH:mm:ss")
                                                    row("Signal") = "Sell"

                                                    ret.Rows.Add(row)
                                                    lastSignal = -1
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

    Private Function IsValidTrendLine(ByVal trendline As TrendLineVeriables, ByVal inputPayload As Dictionary(Of Date, Payload)) As Boolean
        Dim ret As Boolean = True
        For Each runningPayload In inputPayload
            If runningPayload.Key > trendline.Point1 AndAlso runningPayload.Key < trendline.Point2 Then
                Dim counter As Integer = inputPayload.Where(Function(x)
                                                                Return x.Key > trendline.Point1 AndAlso x.Key <= runningPayload.Key
                                                            End Function).Count
                Dim point As Decimal = Math.Round(trendline.M * counter + trendline.C, 2)
                If runningPayload.Value.High > point AndAlso runningPayload.Value.Low < point Then
                    ret = False
                    Exit For
                End If
            End If
        Next
        Return ret
    End Function

    Private Function GetHighestHigh(ByVal inputPayload As Dictionary(Of Date, Payload), ByVal signalCandle As Payload) As Payload
        Dim ret As Payload = Nothing
        For Each runningPayload In inputPayload
            If runningPayload.Key.Date = signalCandle.PayloadDate.Date AndAlso runningPayload.Key <= signalCandle.PayloadDate Then
                If ret Is Nothing Then ret = runningPayload.Value
                If runningPayload.Value.High >= ret.High Then
                    ret = runningPayload.Value
                End If
            End If
        Next
        Return ret
    End Function

    Private Function GetLowestLow(ByVal inputPayload As Dictionary(Of Date, Payload), ByVal signalCandle As Payload) As Payload
        Dim ret As Payload = Nothing
        For Each runningPayload In inputPayload
            If runningPayload.Key.Date = signalCandle.PayloadDate.Date AndAlso runningPayload.Key <= signalCandle.PayloadDate Then
                If ret Is Nothing Then ret = runningPayload.Value
                If runningPayload.Value.Low <= ret.Low Then
                    ret = runningPayload.Value
                End If
            End If
        Next
        Return ret
    End Function
End Class