Imports Algo2TradeBLL
Imports System.Threading
Public Class DataTester
    Inherits Rule
    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal stockCategory As Integer, ByVal timeFrame As Integer, ByVal useHA As Boolean, ByVal stockName As String, ByVal fileName As String)
        MyBase.New(canceller, stockCategory, timeFrame, useHA, stockName, fileName)
    End Sub
    Public Overrides Async Function RunAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Trading Symbol")
        ret.Columns.Add("Date")
        ret.Columns.Add("Close")

        Dim stockData As StockSelection = New StockSelection(_canceller, _category, _cmn, _fileName)
        AddHandler stockData.Heartbeat, AddressOf OnHeartbeat
        AddHandler stockData.WaitingFor, AddressOf OnWaitingFor
        AddHandler stockData.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
        AddHandler stockData.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
        _canceller.Token.ThrowIfCancellationRequested()


        Dim eodPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(_category, _instrumentName, startDate, endDate)
        If eodPayload IsNot Nothing AndAlso eodPayload.Count > 0 Then
            Dim weeklyPayload As Dictionary(Of Date, Payload) = Common.ConvertDayPayloadsToWeek(eodPayload)
            If weeklyPayload IsNot Nothing AndAlso weeklyPayload.Count > 0 Then
                For Each runningPayload In weeklyPayload.Values
                    Dim row As DataRow = ret.NewRow
                    row("Trading Symbol") = runningPayload.TradingSymbol
                    row("Date") = runningPayload.PayloadDate.ToString("dd-MMM-yyyy")
                    row("Close") = runningPayload.Close

                    ret.Rows.Add(row)
                Next
            End If
        End If
        Return ret
    End Function

End Class