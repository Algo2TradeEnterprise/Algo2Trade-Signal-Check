Imports Algo2TradeBLL
Imports System.Threading
Public Class GetStockTrendDirection
    Inherits Rule
    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal stockCategory As Integer, ByVal timeFrame As Integer, ByVal useHA As Boolean, ByVal stockName As String, ByVal fileName As String)
        MyBase.New(canceller, stockCategory, timeFrame, useHA, stockName, fileName)
    End Sub
    Public Overrides Async Function RunAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Trading Symbol")
        ret.Columns.Add("Trend")
        ret.Columns.Add("Time Range")
        ret.Columns.Add("Single Direction")

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
                            Dim changePerPayload As Dictionary(Of Date, Decimal) = Nothing
                            Dim currentDayOpen As Decimal = currentDayPayload.FirstOrDefault.Value.Open
                            For Each runningPayload In currentDayPayload.Keys
                                _canceller.Token.ThrowIfCancellationRequested()
                                Dim changePer As Decimal = Math.Round(((inputPayload(runningPayload).Close / currentDayOpen) - 1) * 100, 3)

                                If changePerPayload Is Nothing Then changePerPayload = New Dictionary(Of Date, Decimal)
                                changePerPayload.Add(runningPayload, changePer)
                            Next

                            If changePerPayload IsNot Nothing AndAlso changePerPayload.Count > 0 Then
                                Dim trend As Integer = If(changePerPayload.FirstOrDefault.Value > 0, 1, -1)
                                Dim startTime As Date = currentDayPayload.FirstOrDefault.Key
                                Dim noEntry As Boolean = True
                                For Each runningPayload In changePerPayload
                                    If runningPayload.Value > 0 AndAlso trend < 0 Then
                                        Dim row As DataRow = ret.NewRow
                                        row("Date") = chkDate.ToString("dd-MMM-yyyy")
                                        row("Trading Symbol") = currentDayPayload(runningPayload.Key).TradingSymbol
                                        row("Trend") = "SELL"
                                        row("Time Range") = String.Format("{0} - {1}", startTime.ToString("HH:mm"), runningPayload.Key.AddMinutes(-1).ToString("HH:mm"))
                                        row("Single Direction") = False

                                        ret.Rows.Add(row)

                                        trend = 1
                                        startTime = runningPayload.Key
                                        noEntry = False
                                    ElseIf runningPayload.Value < 0 AndAlso trend > 0 Then
                                        Dim row As DataRow = ret.NewRow
                                        row("Date") = chkDate.ToString("dd-MMM-yyyy")
                                        row("Trading Symbol") = currentDayPayload(runningPayload.Key).TradingSymbol
                                        row("Trend") = "BUY"
                                        row("Time Range") = String.Format("{0} - {1}", startTime.ToString("HH:mm"), runningPayload.Key.AddMinutes(-1).ToString("HH:mm"))
                                        row("Single Direction") = False

                                        ret.Rows.Add(row)

                                        trend = -1
                                        startTime = runningPayload.Key
                                        noEntry = False
                                    End If
                                Next
                                If noEntry Then
                                    Dim lastTime As Date = currentDayPayload.LastOrDefault.Key

                                    Dim row As DataRow = ret.NewRow
                                    row("Date") = chkDate.ToString("dd-MMM-yyyy")
                                    row("Trading Symbol") = currentDayPayload(lastTime).TradingSymbol
                                    row("Trend") = If(trend > 0, "BUY", "SELL")
                                    row("Time Range") = String.Format("{0} - {1}", startTime.ToString("HH:mm"), lastTime.ToString("HH:mm"))
                                    row("Single Direction") = True

                                    ret.Rows.Add(row)
                                End If
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