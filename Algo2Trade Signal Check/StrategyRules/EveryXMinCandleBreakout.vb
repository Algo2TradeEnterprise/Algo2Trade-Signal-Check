Imports Algo2TradeBLL
Imports System.Threading
Public Class EveryXMinCandleBreakout
    Inherits Rule
    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal stockCategory As Integer, ByVal timeFrame As Integer, ByVal useHA As Boolean, ByVal stockName As String, ByVal fileName As String)
        MyBase.New(canceller, stockCategory, timeFrame, useHA, stockName, fileName)
    End Sub
    Public Overrides Async Function RunAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Time")
        ret.Columns.Add("Trading Symbol")
        ret.Columns.Add("Open")
        ret.Columns.Add("Low")
        ret.Columns.Add("High")
        ret.Columns.Add("Close")
        ret.Columns.Add("Direction")
        ret.Columns.Add("Max Dreawup")
        ret.Columns.Add("Max Dreawdown")

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
                Dim ctr As Integer = 0
                For Each stock In stockList
                    _canceller.Token.ThrowIfCancellationRequested()
                    ctr += 1
                    OnHeartbeat(String.Format("Running #{0}/{1} on {2}", ctr, stockList.Count, chkDate.ToString("dd-MMM-yyyy")))
                    Dim stockPayload As Dictionary(Of Date, Payload) = Nothing
                    Select Case _category
                        Case Common.DataBaseTable.Intraday_Cash, Common.DataBaseTable.Intraday_Commodity, Common.DataBaseTable.Intraday_Currency, Common.DataBaseTable.Intraday_Futures
                            stockPayload = _cmn.GetRawPayloadForSpecificTradingSymbol(_category, stock, chkDate.AddDays(-5), chkDate)
                        Case Common.DataBaseTable.EOD_Cash, Common.DataBaseTable.EOD_Commodity, Common.DataBaseTable.EOD_Currency, Common.DataBaseTable.EOD_Futures, Common.DataBaseTable.EOD_POSITIONAL
                            stockPayload = _cmn.GetRawPayload(_category, stock, chkDate.AddDays(-200), chkDate)
                        Case Common.DataBaseTable.Intraday_Futures_Options
                            stockPayload = _cmn.GetRawPayloadForSpecificTradingSymbol(_category, stock, chkDate.AddDays(-8), chkDate)
                        Case Common.DataBaseTable.EOD_Futures_Options
                            stockPayload = _cmn.GetRawPayloadForSpecificTradingSymbol(_category, stock, chkDate.AddDays(-200), chkDate)
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
                            Dim lastSignalTime As Date = New Date(chkDate.Year, chkDate.Month, chkDate.Day, 11, 0, 0)
                            For Each runningPayload In currentDayPayload.Keys
                                _canceller.Token.ThrowIfCancellationRequested()
                                Dim runningCandle As Payload = currentDayPayload(runningPayload)
                                If runningCandle.PreviousCandlePayload IsNot Nothing AndAlso
                                    runningCandle.PreviousCandlePayload.PayloadDate.Date = chkDate.Date AndAlso
                                    runningCandle.PreviousCandlePayload.PayloadDate <= lastSignalTime Then
                                    Dim signalCandle As Payload = runningCandle.PreviousCandlePayload
                                    Dim buffer As Decimal = CalculateBuffer(signalCandle.Close, Utilities.Numbers.NumberManipulation.RoundOfType.Celing)

                                    Dim buyPrice As Decimal = signalCandle.High + buffer
                                    Dim sellPrice As Decimal = signalCandle.Low - buffer
                                    Dim entryDirection As Integer = 0
                                    If runningCandle.High >= buyPrice AndAlso runningCandle.Low <= sellPrice Then
                                        If runningCandle.CandleColor = Color.Green Then
                                            entryDirection = -1
                                        Else
                                            entryDirection = 1
                                        End If
                                    ElseIf runningCandle.High >= buyPrice Then
                                        entryDirection = 1
                                    ElseIf runningCandle.Low <= sellPrice Then
                                        entryDirection = -1
                                    End If
                                    If entryDirection <> 0 Then
                                        Dim highestPoint As Decimal = currentDayPayload.Max(Function(x)
                                                                                                If x.Key >= runningCandle.PayloadDate Then
                                                                                                    Return x.Value.High
                                                                                                Else
                                                                                                    Return Decimal.MinValue
                                                                                                End If
                                                                                            End Function)
                                        Dim lowestPoint As Decimal = currentDayPayload.Min(Function(x)
                                                                                               If x.Key >= runningCandle.PayloadDate Then
                                                                                                   Return x.Value.Low
                                                                                               Else
                                                                                                   Return Decimal.MaxValue
                                                                                               End If
                                                                                           End Function)

                                        If entryDirection = 1 Then
                                            Dim row As DataRow = ret.NewRow
                                            row("Date") = signalCandle.PayloadDate.ToString("dd-MMM-yyyy")
                                            row("Time") = signalCandle.PayloadDate.ToString("HH:mm:ss")
                                            row("Trading Symbol") = signalCandle.TradingSymbol
                                            row("Open") = signalCandle.Open
                                            row("Low") = signalCandle.Low
                                            row("High") = signalCandle.High
                                            row("Close") = signalCandle.Close
                                            row("Direction") = "Buy"
                                            row("Max Dreawup") = highestPoint - buyPrice
                                            row("Max Dreawdown") = buyPrice - lowestPoint

                                            ret.Rows.Add(row)
                                        ElseIf entryDirection = -1 Then
                                            Dim row As DataRow = ret.NewRow
                                            row("Date") = signalCandle.PayloadDate.ToString("dd-MMM-yyyy")
                                            row("Time") = signalCandle.PayloadDate.ToString("HH:mm:ss")
                                            row("Trading Symbol") = signalCandle.TradingSymbol
                                            row("Open") = signalCandle.Open
                                            row("Low") = signalCandle.Low
                                            row("High") = signalCandle.High
                                            row("Close") = signalCandle.Close
                                            row("Direction") = "Sell"
                                            row("Max Dreawup") = sellPrice - lowestPoint
                                            row("Max Dreawdown") = highestPoint - sellPrice

                                            ret.Rows.Add(row)
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
End Class