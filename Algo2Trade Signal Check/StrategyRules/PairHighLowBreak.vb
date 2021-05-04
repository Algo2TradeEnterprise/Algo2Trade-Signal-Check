Imports Algo2TradeBLL
Imports System.Threading
Public Class PairHighLowBreak
    Inherits Rule
    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal stockCategory As Integer, ByVal timeFrame As Integer, ByVal useHA As Boolean, ByVal stockName As String, ByVal fileName As String)
        MyBase.New(canceller, stockCategory, timeFrame, useHA, stockName, fileName)
    End Sub
    Public Overrides Async Function RunAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Pair 1")
        ret.Columns.Add("Pair 2")

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
                For Each runningStock In stockList
                    _canceller.Token.ThrowIfCancellationRequested()
                    If runningStock.Contains("_") Then
                        Dim stock1 As String = runningStock.Split("_")(0)
                        Dim stock2 As String = runningStock.Split("_")(1)
                        Dim stockPayload1 As Dictionary(Of Date, Payload) = Nothing
                        Dim stockPayload2 As Dictionary(Of Date, Payload) = Nothing
                        Select Case _category
                            Case Common.DataBaseTable.Intraday_Cash, Common.DataBaseTable.Intraday_Commodity, Common.DataBaseTable.Intraday_Currency, Common.DataBaseTable.Intraday_Futures
                                stockPayload1 = _cmn.GetRawPayload(_category, stock1, chkDate.AddDays(-8), chkDate)
                                stockPayload2 = _cmn.GetRawPayload(_category, stock2, chkDate.AddDays(-8), chkDate)
                            Case Common.DataBaseTable.EOD_Cash, Common.DataBaseTable.EOD_Commodity, Common.DataBaseTable.EOD_Currency, Common.DataBaseTable.EOD_Futures, Common.DataBaseTable.EOD_POSITIONAL
                                stockPayload1 = _cmn.GetRawPayload(_category, stock1, chkDate.AddDays(-200), chkDate)
                                stockPayload2 = _cmn.GetRawPayload(_category, stock2, chkDate.AddDays(-200), chkDate)
                            Case Common.DataBaseTable.Intraday_Futures_Options
                                stockPayload1 = _cmn.GetRawPayloadForSpecificTradingSymbol(_category, stock1, chkDate.AddDays(-8), chkDate)
                                stockPayload2 = _cmn.GetRawPayloadForSpecificTradingSymbol(_category, stock2, chkDate.AddDays(-8), chkDate)
                            Case Common.DataBaseTable.EOD_Futures_Options
                                stockPayload1 = _cmn.GetRawPayloadForSpecificTradingSymbol(_category, stock1, chkDate.AddDays(-200), chkDate)
                                stockPayload2 = _cmn.GetRawPayloadForSpecificTradingSymbol(_category, stock2, chkDate.AddDays(-200), chkDate)
                        End Select
                        _canceller.Token.ThrowIfCancellationRequested()
                        If stockPayload1 IsNot Nothing AndAlso stockPayload1.Count > 0 AndAlso
                            stockPayload2 IsNot Nothing AndAlso stockPayload2.Count > 0 Then
                            Dim XMinutePayload1 As Dictionary(Of Date, Payload) = Nothing
                            Dim XMinutePayload2 As Dictionary(Of Date, Payload) = Nothing
                            Dim exchangeStartTime As Date = Date.MinValue
                            Select Case _category
                                Case Common.DataBaseTable.EOD_Cash, Common.DataBaseTable.EOD_Futures, Common.DataBaseTable.EOD_POSITIONAL, Common.DataBaseTable.Intraday_Cash, Common.DataBaseTable.Intraday_Futures
                                    exchangeStartTime = New Date(chkDate.Year, chkDate.Month, chkDate.Day, 9, 15, 0)
                                Case Common.DataBaseTable.EOD_Commodity, Common.DataBaseTable.EOD_Currency, Common.DataBaseTable.Intraday_Commodity, Common.DataBaseTable.Intraday_Currency
                                    exchangeStartTime = New Date(chkDate.Year, chkDate.Month, chkDate.Day, 9, 0, 0)
                            End Select
                            If _timeFrame > 1 Then
                                XMinutePayload1 = Common.ConvertPayloadsToXMinutes(stockPayload1, _timeFrame, exchangeStartTime)
                                XMinutePayload2 = Common.ConvertPayloadsToXMinutes(stockPayload2, _timeFrame, exchangeStartTime)
                            Else
                                XMinutePayload1 = stockPayload1
                                XMinutePayload2 = stockPayload2
                            End If
                            _canceller.Token.ThrowIfCancellationRequested()
                            Dim inputPayload1 As Dictionary(Of Date, Payload) = Nothing
                            Dim inputPayload2 As Dictionary(Of Date, Payload) = Nothing
                            If _useHA Then
                                Indicator.HeikenAshi.ConvertToHeikenAshi(XMinutePayload1, inputPayload1)
                                Indicator.HeikenAshi.ConvertToHeikenAshi(XMinutePayload2, inputPayload2)
                            Else
                                inputPayload1 = XMinutePayload1
                                inputPayload2 = XMinutePayload2
                            End If
                            _canceller.Token.ThrowIfCancellationRequested()
                            Dim currentDayPayload1 As Dictionary(Of Date, Payload) = Nothing
                            Dim currentDayPayload2 As Dictionary(Of Date, Payload) = Nothing
                            Dim currentDayTimes As List(Of Date) = Nothing
                            For Each runningPayload In inputPayload1.Keys
                                _canceller.Token.ThrowIfCancellationRequested()
                                If runningPayload.Date = chkDate.Date Then
                                    If currentDayPayload1 Is Nothing Then currentDayPayload1 = New Dictionary(Of Date, Payload)
                                    currentDayPayload1.Add(runningPayload, inputPayload1(runningPayload))
                                    If currentDayTimes Is Nothing Then currentDayTimes = New List(Of Date)
                                    If Not currentDayTimes.Contains(runningPayload) Then currentDayTimes.Add(runningPayload)
                                End If
                            Next
                            For Each runningPayload In inputPayload2.Keys
                                _canceller.Token.ThrowIfCancellationRequested()
                                If runningPayload.Date = chkDate.Date Then
                                    If currentDayPayload2 Is Nothing Then currentDayPayload2 = New Dictionary(Of Date, Payload)
                                    currentDayPayload2.Add(runningPayload, inputPayload2(runningPayload))
                                    If currentDayTimes Is Nothing Then currentDayTimes = New List(Of Date)
                                    If Not currentDayTimes.Contains(runningPayload) Then currentDayTimes.Add(runningPayload)
                                End If
                            Next

                            'Main Logic
                            If currentDayPayload1 IsNot Nothing AndAlso currentDayPayload1.Count > 0 AndAlso
                                currentDayPayload2 IsNot Nothing AndAlso currentDayPayload2.Count > 0 AndAlso
                                currentDayTimes IsNot Nothing AndAlso currentDayTimes.Count > 0 Then
                                Dim signalCandle1 As Payload = currentDayPayload1.FirstOrDefault.Value
                                Dim signalCandle2 As Payload = currentDayPayload2.FirstOrDefault.Value

                                For Each runningTime In currentDayTimes.OrderBy(Function(x)
                                                                                    Return x
                                                                                End Function)
                                    If currentDayPayload1.ContainsKey(runningTime) AndAlso
                                        currentDayPayload2.ContainsKey(runningTime) Then
                                        If runningTime > signalCandle1.PayloadDate AndAlso
                                            runningTime > signalCandle2.PayloadDate Then
                                            If currentDayPayload1(runningTime).Low <= signalCandle1.Low AndAlso
                                                currentDayPayload2(runningTime).High >= signalCandle2.High Then
                                                Dim row As DataRow = ret.NewRow
                                                row("Date") = runningTime.ToString("dd-MMM-yyyy HH:mm:ss")
                                                row("Pair 1") = signalCandle1.TradingSymbol
                                                row("Pair 2") = signalCandle2.TradingSymbol

                                                ret.Rows.Add(row)
                                            ElseIf currentDayPayload1(runningTime).High >= signalCandle1.High AndAlso
                                                currentDayPayload2(runningTime).Low <= signalCandle2.Low Then
                                                Dim row As DataRow = ret.NewRow
                                                row("Date") = runningTime.ToString("dd-MMM-yyyy HH:mm:ss")
                                                row("Pair 1") = signalCandle1.TradingSymbol
                                                row("Pair 2") = signalCandle2.TradingSymbol

                                                ret.Rows.Add(row)
                                            End If
                                        End If
                                    End If
                                Next
                            End If
                        End If
                    End If
                Next
            End If
            chkDate = chkDate.AddDays(1)
        End While
        Return ret
    End Function
End Class