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
        ret.Columns.Add("Date")
        ret.Columns.Add("Trading Symbol")
        ret.Columns.Add("Open")
        ret.Columns.Add("Low")
        ret.Columns.Add("High")
        ret.Columns.Add("Close")

        Dim stockData As StockSelection = New StockSelection(_canceller, _category, _cmn, _fileName)
        AddHandler stockData.Heartbeat, AddressOf OnHeartbeat
        AddHandler stockData.WaitingFor, AddressOf OnWaitingFor
        AddHandler stockData.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
        AddHandler stockData.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
        _canceller.Token.ThrowIfCancellationRequested()

        Dim stockPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(_category, _instrumentName, startDate.AddMonths(-1), endDate)
        Dim exchangeStartTime As Date = Date.MinValue
        Select Case _category
            Case Common.DataBaseTable.EOD_Cash, Common.DataBaseTable.EOD_Futures, Common.DataBaseTable.EOD_POSITIONAL, Common.DataBaseTable.Intraday_Cash, Common.DataBaseTable.Intraday_Futures
                exchangeStartTime = New Date(Now.Year, Now.Month, Now.Day, 9, 15, 0)
            Case Common.DataBaseTable.EOD_Commodity, Common.DataBaseTable.EOD_Currency, Common.DataBaseTable.Intraday_Commodity, Common.DataBaseTable.Intraday_Currency
                exchangeStartTime = New Date(Now.Year, Now.Month, Now.Day, 9, 0, 0)
        End Select
        'Dim inputPayload As Dictionary(Of Date, Payload) = Common.ConvertPayloadsToXMinutes(stockPayload, _timeFrame, exchangeStartTime)
        Dim inputPayload As Dictionary(Of Date, Payload) = Common.ConvertDayPayloadsToWeek(stockPayload)

        Dim lastV As Decimal = Decimal.MaxValue
        Dim lastBreakout As Date = startDate.Date
        For Each runningPayload In inputPayload
            _canceller.Token.ThrowIfCancellationRequested()
            If runningPayload.Key >= startDate.Date Then
                Dim row As DataRow = ret.NewRow
                row("Date") = runningPayload.Key.ToString("dd-MMM-yyyy")
                row("Trading Symbol") = runningPayload.Value.TradingSymbol
                row("Open") = runningPayload.Value.Open
                row("Low") = runningPayload.Value.Low
                row("High") = runningPayload.Value.High
                row("Close") = runningPayload.Value.Close
                ret.Rows.Add(row)
            End If
        Next
        Return ret
    End Function
End Class
