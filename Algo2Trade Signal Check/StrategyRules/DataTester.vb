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
        ret.Columns.Add("ATR High Band")
        ret.Columns.Add("ATR Low Band")
        ret.Columns.Add("V Value")
        ret.Columns.Add("Breakout")
        ret.Columns.Add("Breakout Gap")

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
        Dim inputPayload As Dictionary(Of Date, Payload) = Common.ConvertPayloadsToXMinutes(stockPayload, _timeFrame, exchangeStartTime)

        Dim atrHighBand As Dictionary(Of Date, Decimal) = Nothing
        Dim atrLowBand As Dictionary(Of Date, Decimal) = Nothing
        Indicator.ATRBands.CalculateATRBands(2, 5, Payload.PayloadFields.Close, inputPayload, atrHighBand, atrLowBand)

        Dim lastV As Decimal = Decimal.MaxValue
        Dim lastBreakout As Date = startDate.Date
        For Each runningPayload In inputPayload
            _canceller.Token.ThrowIfCancellationRequested()
            If runningPayload.Value.PreviousCandlePayload IsNot Nothing AndAlso
                runningPayload.Value.PreviousCandlePayload.PreviousCandlePayload IsNot Nothing AndAlso
                runningPayload.Value.PreviousCandlePayload.PreviousCandlePayload.PreviousCandlePayload IsNot Nothing Then
                If atrHighBand(runningPayload.Value.PreviousCandlePayload.PreviousCandlePayload.PayloadDate) < atrHighBand(runningPayload.Value.PreviousCandlePayload.PayloadDate) AndAlso
                    atrHighBand(runningPayload.Value.PreviousCandlePayload.PreviousCandlePayload.PayloadDate) < atrHighBand(runningPayload.Value.PreviousCandlePayload.PreviousCandlePayload.PreviousCandlePayload.PayloadDate) Then
                    If atrHighBand(runningPayload.Value.PreviousCandlePayload.PreviousCandlePayload.PayloadDate) <= lastV Then
                        lastV = Math.Round(atrHighBand(runningPayload.Value.PreviousCandlePayload.PreviousCandlePayload.PayloadDate), 2)
                    End If
                End If
            End If
            If runningPayload.Key >= startDate.Date Then
                Dim breakoutDone As Boolean = False
                Dim breakoutGap As Integer = Integer.MinValue
                If lastV <> Decimal.MaxValue AndAlso runningPayload.Value.High > lastV Then
                    breakoutDone = True
                    breakoutGap = DateDiff(DateInterval.Day, lastBreakout.Date, runningPayload.Key.Date)
                    lastBreakout = runningPayload.Key.Date
                End If

                Dim row As DataRow = ret.NewRow
                row("Date") = runningPayload.Key.ToString("dd-MMM-yyyy HH:mm:ss")
                row("Trading Symbol") = runningPayload.Value.TradingSymbol
                row("Open") = runningPayload.Value.Open
                row("Low") = runningPayload.Value.Low
                row("High") = runningPayload.Value.High
                row("Close") = runningPayload.Value.Close
                row("ATR High Band") = Math.Round(atrHighBand(runningPayload.Key), 2)
                row("ATR Low Band") = Math.Round(atrLowBand(runningPayload.Key), 2)
                row("V Value") = If(lastV <> Decimal.MaxValue, lastV, "")
                row("Breakout") = breakoutDone
                row("Breakout Gap") = If(breakoutGap <> Integer.MinValue, breakoutGap, "")

                ret.Rows.Add(row)

                If breakoutDone Then lastV = Decimal.MaxValue
            End If
        Next
        Return ret
    End Function
End Class
