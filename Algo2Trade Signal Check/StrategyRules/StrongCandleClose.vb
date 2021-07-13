Imports Algo2TradeBLL
Imports System.Threading
Public Class StrongCandleClose
    Inherits Rule
    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal stockCategory As Integer, ByVal timeFrame As Integer, ByVal useHA As Boolean, ByVal stockName As String, ByVal fileName As String)
        MyBase.New(canceller, stockCategory, timeFrame, useHA, stockName, fileName)
    End Sub
    Public Overrides Async Function RunAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Trading Symbol")
        ret.Columns.Add("ATR %")
        ret.Columns.Add("Direction")
        ret.Columns.Add("Entry")
        ret.Columns.Add("X-Min Close")
        ret.Columns.Add("Avg Volume")

        RemoveHandler _cmn.Heartbeat, AddressOf OnHeartbeat

        Dim stockData As StockSelection = New StockSelection(_canceller, _category, _cmn, _fileName)
        AddHandler stockData.Heartbeat, AddressOf OnHeartbeat
        AddHandler stockData.WaitingFor, AddressOf OnWaitingFor
        AddHandler stockData.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
        AddHandler stockData.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete

        Dim stockList As List(Of String) = Nothing
        If _instrumentName Is Nothing OrElse _instrumentName = "" Then
            stockList = Await stockData.GetStockList(startDate).ConfigureAwait(False)
        Else
            stockList = New List(Of String)
            stockList.Add(_instrumentName)
        End If
        _canceller.Token.ThrowIfCancellationRequested()
        If stockList IsNot Nothing AndAlso stockList.Count > 0 Then
            Dim ctr As Integer = 0
            For Each runningStock In stockList
                _canceller.Token.ThrowIfCancellationRequested()
                ctr += 1
                OnHeartbeat(String.Format("Getting stock data for {0} #{1}/{2}", runningStock, ctr, stockList.Count))

                Dim eodPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.EOD_POSITIONAL, runningStock, startDate.AddDays(-180), endDate)
                _canceller.Token.ThrowIfCancellationRequested()
                Dim atrPayload As Dictionary(Of Date, Decimal) = Nothing
                Indicator.ATR.CalculateATR(14, eodPayload, atrPayload, True)

                Dim tradingDate As Date = startDate
                Dim previous100CandleAvailable As Boolean = False
                While tradingDate <= endDate
                    _canceller.Token.ThrowIfCancellationRequested()
                    OnHeartbeat(String.Format("Checking signal for {0} #{1}/{2} on {3}", runningStock, ctr, stockList.Count, tradingDate.ToString("dd-MMM-yyyy")))
                    If eodPayload IsNot Nothing AndAlso eodPayload.ContainsKey(tradingDate.Date) AndAlso eodPayload.Count > 100 Then
                        If Not previous100CandleAvailable Then
                            Dim previousNCandle = eodPayload.Where(Function(x)
                                                                       Return x.Key.Date <= tradingDate.Date
                                                                   End Function)
                            If previousNCandle IsNot Nothing AndAlso previousNCandle.Count >= 100 Then
                                previous100CandleAvailable = True
                            End If
                        End If
                        Dim currentDayCandle As Payload = eodPayload(tradingDate.Date)
                        If currentDayCandle IsNot Nothing AndAlso currentDayCandle.PreviousCandlePayload IsNot Nothing AndAlso
                            currentDayCandle.Close >= 100 AndAlso currentDayCandle.Close <= 6000 AndAlso previous100CandleAvailable Then
                            Dim atrPercentage As Decimal = (atrPayload(tradingDate.Date) / currentDayCandle.Close) * 100
                            If atrPercentage >= 3 Then
                                If currentDayCandle.CandleColor = Color.Green AndAlso currentDayCandle.PreviousCandlePayload.CandleColor = Color.Red Then
                                    If currentDayCandle.Close >= currentDayCandle.Low + currentDayCandle.CandleRange * 80 / 100 Then
                                        Dim row As DataRow = ret.NewRow
                                        row("Date") = tradingDate.ToString("dd-MMM-yyyy")
                                        row("Trading Symbol") = runningStock
                                        row("ATR %") = Math.Round(atrPercentage, 4)
                                        row("Direction") = "Buy"
                                        row("Entry") = currentDayCandle.Close
                                        row("X-Min Close") = GetXMinClose(tradingDate, runningStock)
                                        row("Avg Volume") = Math.Round(GetLast5DayAverageVolume(currentDayCandle), 0)

                                        ret.Rows.Add(row)
                                    End If
                                ElseIf currentDayCandle.CandleColor = Color.Red AndAlso currentDayCandle.PreviousCandlePayload.CandleColor = Color.Green Then
                                    If currentDayCandle.Close <= currentDayCandle.High - currentDayCandle.CandleRange * 80 / 100 Then
                                        Dim row As DataRow = ret.NewRow
                                        row("Date") = tradingDate.ToString("dd-MMM-yyyy")
                                        row("Trading Symbol") = runningStock
                                        row("ATR %") = Math.Round(atrPercentage, 4)
                                        row("Direction") = "Sell"
                                        row("Entry") = currentDayCandle.Close
                                        row("X-Min Close") = GetXMinClose(tradingDate, runningStock)
                                        row("Avg Volume") = Math.Round(GetLast5DayAverageVolume(currentDayCandle), 0)

                                        ret.Rows.Add(row)
                                    End If
                                End If
                            End If
                        End If
                    End If
                    tradingDate = tradingDate.AddDays(1)
                End While
            Next
        End If
        Return ret
    End Function

    Private Function GetLast5DayAverageVolume(currentDayCandle As Payload) As Double
        Dim ret As Double = Double.MinValue
        If currentDayCandle IsNot Nothing Then
            Dim sumVolume As Long = currentDayCandle.Volume
            Dim count As Integer = 0
            If currentDayCandle.PreviousCandlePayload IsNot Nothing Then
                sumVolume += currentDayCandle.PreviousCandlePayload.Volume
                count += 1
                If currentDayCandle.PreviousCandlePayload.PreviousCandlePayload IsNot Nothing Then
                    sumVolume += currentDayCandle.PreviousCandlePayload.PreviousCandlePayload.Volume
                    count += 1
                    If currentDayCandle.PreviousCandlePayload.PreviousCandlePayload.PreviousCandlePayload IsNot Nothing Then
                        sumVolume += currentDayCandle.PreviousCandlePayload.PreviousCandlePayload.PreviousCandlePayload.Volume
                        count += 1
                        If currentDayCandle.PreviousCandlePayload.PreviousCandlePayload.PreviousCandlePayload.PreviousCandlePayload IsNot Nothing Then
                            sumVolume += currentDayCandle.PreviousCandlePayload.PreviousCandlePayload.PreviousCandlePayload.PreviousCandlePayload.Volume
                            count += 1
                        End If
                    End If
                End If
            End If

            ret = sumVolume / count
        End If
        Return ret
    End Function

    Private Function GetXMinClose(tradingDate As Date, stockname As String) As Object
        Dim intradayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.Intraday_Cash, stockname, tradingDate.AddDays(1), tradingDate.AddDays(8))
        If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
            Dim exchangeStartTime As New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, 9, 15, 0)
            Dim XMinutePayload As Dictionary(Of Date, Payload)
            If _timeFrame > 1 Then
                XMinutePayload = Common.ConvertPayloadsToXMinutes(intradayPayload, _timeFrame, exchangeStartTime)
            Else
                XMinutePayload = intradayPayload
            End If

            Return XMinutePayload.FirstOrDefault.Value.Close
        Else
            Return "No Data Available"
        End If
    End Function
End Class