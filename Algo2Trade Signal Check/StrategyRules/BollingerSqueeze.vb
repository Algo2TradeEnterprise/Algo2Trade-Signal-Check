Imports Algo2TradeBLL
Imports System.Threading
Public Class BollingerSqueeze
    Inherits Rule
    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal stockCategory As Integer, ByVal timeFrame As Integer, ByVal useHA As Boolean, ByVal stockName As String, ByVal fileName As String)
        MyBase.New(canceller, stockCategory, timeFrame, useHA, stockName, fileName)
    End Sub
    Public Overrides Async Function RunAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Trading Symbol")
        ret.Columns.Add("SMA")
        ret.Columns.Add("Keltner High")
        ret.Columns.Add("Keltner Low")
        ret.Columns.Add("Bollinger High")
        ret.Columns.Add("Bollinger Low")
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
                        If _timeFrame > 1 Then
                            Dim exchangeStartTime As Date = New Date(chkDate.Year, chkDate.Month, chkDate.Day, 9, 15, 0)
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
                            Dim keltnerSMAPayload As Dictionary(Of Date, Decimal) = Nothing
                            Dim keltnerHighPayload As Dictionary(Of Date, Decimal) = Nothing
                            Dim keltnerLowPayload As Dictionary(Of Date, Decimal) = Nothing
                            Indicator.KeltnerChannel.CalculateSMAKeltnerChannel(20, 1.5, inputPayload, keltnerHighPayload, keltnerLowPayload, keltnerSMAPayload)

                            Dim bollingerSMAPayload As Dictionary(Of Date, Decimal) = Nothing
                            Dim bollingerHighPayload As Dictionary(Of Date, Decimal) = Nothing
                            Dim bollingerLowPayload As Dictionary(Of Date, Decimal) = Nothing
                            Indicator.BollingerBands.CalculateBollingerBands(20, Payload.PayloadFields.Close, 2, inputPayload, bollingerHighPayload, bollingerLowPayload, bollingerSMAPayload)

                            For Each runningPayload In currentDayPayload.Keys
                                _canceller.Token.ThrowIfCancellationRequested()

                                Dim row As DataRow = ret.NewRow
                                row("Date") = inputPayload(runningPayload).PayloadDate
                                row("Trading Symbol") = inputPayload(runningPayload).TradingSymbol
                                row("SMA") = keltnerSMAPayload(runningPayload)
                                row("Keltner High") = keltnerHighPayload(runningPayload)
                                row("Keltner Low") = keltnerLowPayload(runningPayload)
                                row("Bollinger High") = bollingerHighPayload(runningPayload)
                                row("Bollinger Low") = bollingerLowPayload(runningPayload)
                                row("Signal") = bollingerHighPayload(runningPayload) < keltnerHighPayload(runningPayload) AndAlso bollingerLowPayload(runningPayload) > keltnerLowPayload(runningPayload)

                                ret.Rows.Add(row)
                            Next
                        End If
                    End If
                Next
            End If
            chkDate = chkDate.AddDays(1)
        End While
        Return ret
    End Function
End Class
