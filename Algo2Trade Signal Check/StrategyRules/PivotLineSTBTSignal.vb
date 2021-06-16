Imports Algo2TradeBLL
Imports System.Threading
Public Class PivotLineSTBTSignal
    Inherits Rule
    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal stockCategory As Integer, ByVal timeFrame As Integer, ByVal useHA As Boolean, ByVal stockName As String, ByVal fileName As String)
        MyBase.New(canceller, stockCategory, timeFrame, useHA, stockName, fileName)
    End Sub
    Public Overrides Async Function RunAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Trading Symbol")
        ret.Columns.Add("Open %")
        ret.Columns.Add("Low %")
        ret.Columns.Add("High %")
        ret.Columns.Add("Close %")

        Dim stockData As StockSelection = New StockSelection(_canceller, _category, _cmn, _fileName)
        AddHandler stockData.Heartbeat, AddressOf OnHeartbeat
        AddHandler stockData.WaitingFor, AddressOf OnWaitingFor
        AddHandler stockData.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
        AddHandler stockData.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
        Dim stockList As List(Of String) = Nothing
        If _instrumentName Is Nothing OrElse _instrumentName = "" Then
            stockList = Await stockData.GetStockList(endDate).ConfigureAwait(False)
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
                    Case Common.DataBaseTable.Intraday_Cash
                        stockPayload = _cmn.GetRawPayloadForSpecificTradingSymbol(_category, stock, startDate.AddDays(-8), endDate)
                    Case Else
                        Throw New NotImplementedException
                End Select
                _canceller.Token.ThrowIfCancellationRequested()
                If stockPayload IsNot Nothing AndAlso stockPayload.Count > 0 Then
                    OnHeartbeat(String.Format("Converting timeframe for {0}", stock))
                    Dim XMinutePayload As Dictionary(Of Date, Payload) = Nothing
                    Dim exchangeStartTime As Date = Date.MinValue
                    Select Case _category
                        Case Common.DataBaseTable.EOD_Cash, Common.DataBaseTable.EOD_Futures, Common.DataBaseTable.EOD_POSITIONAL, Common.DataBaseTable.Intraday_Cash, Common.DataBaseTable.Intraday_Futures
                            exchangeStartTime = New Date(Now.Year, Now.Month, Now.Day, 9, 15, 0)
                        Case Common.DataBaseTable.EOD_Commodity, Common.DataBaseTable.EOD_Currency, Common.DataBaseTable.Intraday_Commodity, Common.DataBaseTable.Intraday_Currency
                            exchangeStartTime = New Date(Now.Year, Now.Month, Now.Day, 9, 0, 0)
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
                    OnHeartbeat(String.Format("Calculating pivots for {0}", stock))
                    Dim pivotPayload As Dictionary(Of Date, PivotPoints) = Nothing
                    Indicator.Pivots.CalculatePivots(inputPayload, pivotPayload)
                    _canceller.Token.ThrowIfCancellationRequested()

                    Dim chkDate As Date = startDate
                    While chkDate <= endDate
                        _canceller.Token.ThrowIfCancellationRequested()

                        OnHeartbeat(String.Format("Checking signals for {0} on {1}", stock, chkDate.ToString("dd-MMM-yyyy")))
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
                            Dim currentDayFirstCandle As Payload = currentDayPayload.FirstOrDefault.Value
                            'Dim currentDayFirstCandle As Payload = stockPayload.Where(Function(x)
                            '                                                              Return x.Key = currentDayPayload.FirstOrDefault.Value.PayloadDate
                            '                                                          End Function).FirstOrDefault.Value

                            Dim previousDay As Date = currentDayFirstCandle.PreviousCandlePayload.PayloadDate

                            Dim previousDayPayload As Dictionary(Of Date, Payload) = Nothing
                            For Each runningPayload In inputPayload.Keys
                                _canceller.Token.ThrowIfCancellationRequested()
                                If runningPayload.Date = previousDay.Date Then
                                    If previousDayPayload Is Nothing Then previousDayPayload = New Dictionary(Of Date, Payload)
                                    previousDayPayload.Add(runningPayload, inputPayload(runningPayload))
                                End If
                            Next

                            Dim previousDayFirstCandle As Payload = previousDayPayload.FirstOrDefault.Value

                            Dim prePreviousPivot As PivotPoints = pivotPayload(previousDayFirstCandle.PreviousCandlePayload.PayloadDate)
                            Dim previousPivot As PivotPoints = pivotPayload(previousDayFirstCandle.PayloadDate)

                            If previousPivot.Pivot > prePreviousPivot.Pivot Then
                                If currentDayFirstCandle.PreviousCandlePayload.Close < previousPivot.Pivot Then
                                    Dim valid As Boolean = True
                                    For Each runningPayload In previousDayPayload
                                        _canceller.Token.ThrowIfCancellationRequested()
                                        If runningPayload.Value.Close > previousPivot.Resistance1 Then
                                            valid = False
                                            Exit For
                                        End If
                                    Next
                                    If valid Then
                                        Dim row As DataRow = ret.NewRow
                                        row("Date") = chkDate.ToString("dd-MMM-yyyy")
                                        row("Trading Symbol") = currentDayFirstCandle.TradingSymbol
                                        row("Open %") = Math.Round((1 - (currentDayFirstCandle.Open / currentDayFirstCandle.PreviousCandlePayload.Close)) * 100, 4)
                                        row("Low %") = Math.Round((1 - (currentDayFirstCandle.Low / currentDayFirstCandle.PreviousCandlePayload.Close)) * 100, 4)
                                        row("High %") = Math.Round((1 - (currentDayFirstCandle.High / currentDayFirstCandle.PreviousCandlePayload.Close)) * 100, 4)
                                        row("Close %") = Math.Round((1 - (currentDayFirstCandle.Close / currentDayFirstCandle.PreviousCandlePayload.Close)) * 100, 4)

                                        ret.Rows.Add(row)
                                    End If
                                End If
                            End If
                        End If
                        chkDate = chkDate.AddDays(1)
                    End While
                End If
            Next
        End If

        Return ret
    End Function
End Class