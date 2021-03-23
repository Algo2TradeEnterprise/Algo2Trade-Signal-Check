Imports Algo2TradeBLL
Imports System.Threading
Public Class GetWeeklyCandle
    Inherits Rule
    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal stockCategory As Integer, ByVal timeFrame As Integer, ByVal useHA As Boolean, ByVal stockName As String, ByVal fileName As String)
        MyBase.New(canceller, stockCategory, timeFrame, useHA, stockName, fileName)
    End Sub
    Public Overrides Async Function RunAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Trading Symbol")
        ret.Columns.Add("Open")
        ret.Columns.Add("Low")
        ret.Columns.Add("High")
        ret.Columns.Add("Close")
        ret.Columns.Add("Volume")
        ret.Columns.Add("Color")

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
                Dim outputPayload As Dictionary(Of Date, Payload) = Nothing
                Dim chkDate As Date = startDate
                While chkDate <= endDate
                    _canceller.Token.ThrowIfCancellationRequested()
                    _canceller.Token.ThrowIfCancellationRequested()
                    Dim stockPayload As Dictionary(Of Date, Payload) = Nothing
                    Select Case _category
                        Case Common.DataBaseTable.Intraday_Cash, Common.DataBaseTable.EOD_Cash, Common.DataBaseTable.EOD_POSITIONAL
                            stockPayload = _cmn.GetRawPayload(Common.DataBaseTable.EOD_POSITIONAL, stock, startDate.AddDays(-15), chkDate)
                        Case Common.DataBaseTable.Intraday_Commodity, Common.DataBaseTable.EOD_Commodity
                            stockPayload = _cmn.GetRawPayload(Common.DataBaseTable.EOD_Commodity, stock, startDate.AddDays(-15), chkDate)
                        Case Common.DataBaseTable.Intraday_Currency, Common.DataBaseTable.EOD_Currency
                            stockPayload = _cmn.GetRawPayload(Common.DataBaseTable.EOD_Currency, stock, startDate.AddDays(-15), chkDate)
                        Case Common.DataBaseTable.Intraday_Futures, Common.DataBaseTable.EOD_Futures
                            stockPayload = _cmn.GetRawPayload(Common.DataBaseTable.EOD_Futures, stock, startDate.AddDays(-15), chkDate)
                        Case Common.DataBaseTable.Intraday_Futures_Options, Common.DataBaseTable.EOD_Futures_Options
                            stockPayload = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.EOD_Futures_Options, stock, startDate.AddDays(-15), chkDate)
                        Case Else
                            Throw New NotImplementedException
                    End Select
                    _canceller.Token.ThrowIfCancellationRequested()
                    If stockPayload IsNot Nothing AndAlso stockPayload.Count > 0 Then
                        Dim weeklyPayload As Dictionary(Of Date, Payload) = Common.ConvertDayPayloadsToWeek(stockPayload)

                        _canceller.Token.ThrowIfCancellationRequested()
                        Dim inputPayload As Dictionary(Of Date, Payload) = Nothing
                        If _useHA Then
                            Indicator.HeikenAshi.ConvertToHeikenAshi(weeklyPayload, inputPayload)
                        Else
                            inputPayload = weeklyPayload
                        End If

                        Dim lastPayload As Payload = inputPayload.LastOrDefault.Value

                        If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, Payload)
                        If outputPayload.ContainsKey(lastPayload.PayloadDate) Then
                            outputPayload(lastPayload.PayloadDate) = lastPayload
                        Else
                            outputPayload.Add(lastPayload.PayloadDate, lastPayload)
                        End If
                    End If
                    chkDate = chkDate.AddDays(1)
                End While

                If outputPayload IsNot Nothing AndAlso outputPayload.Count > 0 Then
                    For Each runningPayload In outputPayload.Keys
                        _canceller.Token.ThrowIfCancellationRequested()
                        Dim row As DataRow = ret.NewRow
                        row("Date") = outputPayload(runningPayload).PayloadDate
                        row("Trading Symbol") = outputPayload(runningPayload).TradingSymbol
                        row("Open") = outputPayload(runningPayload).Open
                        row("Low") = outputPayload(runningPayload).Low
                        row("High") = outputPayload(runningPayload).High
                        row("Close") = outputPayload(runningPayload).Close
                        row("Volume") = outputPayload(runningPayload).Volume
                        row("Color") = outputPayload(runningPayload).CandleColor.Name

                        ret.Rows.Add(row)
                    Next
                End If
            Next
        End If

        Return ret
    End Function
End Class