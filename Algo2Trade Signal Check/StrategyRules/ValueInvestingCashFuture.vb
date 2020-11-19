Imports Algo2TradeBLL
Imports System.Threading
Public Class ValueInvestingCashFuture
    Inherits Rule
    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal stockCategory As Integer, ByVal timeFrame As Integer, ByVal useHA As Boolean, ByVal stockName As String, ByVal fileName As String)
        MyBase.New(canceller, stockCategory, timeFrame, useHA, stockName, fileName)
    End Sub
    Public Overrides Async Function RunAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Trading Symbol")
        ret.Columns.Add("Snapshot Date")
        ret.Columns.Add("Cash Close")
        ret.Columns.Add("Future Close")
        ret.Columns.Add("Cash High")
        ret.Columns.Add("Future High")

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
                    Dim cashStockPayload As Dictionary(Of Date, Payload) = Nothing
                    Dim futureStockPayload As Dictionary(Of Date, Payload) = Nothing
                    cashStockPayload = _cmn.GetRawPayload(Common.DataBaseTable.EOD_Cash, stock, chkDate, chkDate)
                    futureStockPayload = _cmn.GetRawPayload(Common.DataBaseTable.EOD_Futures, stock, chkDate, chkDate)
                    _canceller.Token.ThrowIfCancellationRequested()
                    If cashStockPayload IsNot Nothing AndAlso cashStockPayload.ContainsKey(chkDate.Date) AndAlso
                        futureStockPayload IsNot Nothing AndAlso futureStockPayload.ContainsKey(chkDate.Date) Then
                        Dim row As DataRow = ret.NewRow
                        row("Trading Symbol") = stock
                        row("Snapshot Date") = chkDate.Date.ToString("dd-MMM-yyyy")
                        row("Cash Close") = cashStockPayload.LastOrDefault.Value.Close
                        row("Future Close") = futureStockPayload.LastOrDefault.Value.Close
                        row("Cash High") = cashStockPayload.LastOrDefault.Value.High
                        row("Future High") = futureStockPayload.LastOrDefault.Value.High

                        ret.Rows.Add(row)
                    End If
                Next
            End If
            chkDate = chkDate.AddDays(1)
        End While
        Return ret
    End Function
End Class