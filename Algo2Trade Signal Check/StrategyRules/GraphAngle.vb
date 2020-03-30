Imports Algo2TradeBLL
Imports System.Threading
Public Class GraphAngle
    Inherits Rule

    Private _endTime As Date
    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal stockCategory As Integer, ByVal timeFrame As Integer, ByVal useHA As Boolean, ByVal stockName As String, ByVal fileName As String, ByVal endTime As Date)
        MyBase.New(canceller, stockCategory, timeFrame, useHA, stockName, fileName)
        _endTime = endTime
    End Sub
    Public Overrides Async Function RunAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Trading Symbol")
        ret.Columns.Add("Total Candles")
        ret.Columns.Add("Total Candles On The Line")
        ret.Columns.Add("Percentage")

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
                        OnHeartbeat("Processing data")
                        If currentDayPayload IsNot Nothing AndAlso currentDayPayload.Count > 0 Then
                            Dim endTime As Date = New Date(chkDate.Year, chkDate.Month, chkDate.Day, _endTime.Hour, _endTime.Minute, _endTime.Second)
                            Dim candleToCheck As IEnumerable(Of KeyValuePair(Of Date, Payload)) = currentDayPayload.Where(Function(x)
                                                                                                                              Return x.Key >= exchangeStartTime AndAlso
                                                                                                                              x.Key <= endTime
                                                                                                                          End Function)
                            If candleToCheck IsNot Nothing AndAlso candleToCheck.Count > 0 Then
                                Dim firstCandleTime As Date = candleToCheck.Min(Function(x)
                                                                                    Return x.Key
                                                                                End Function)
                                Dim firstCandle As Payload = currentDayPayload(firstCandleTime)

                                Dim highestHigh As Decimal = candleToCheck.Max(Function(x)
                                                                                   Return x.Value.High
                                                                               End Function)
                                Dim lowestlow As Decimal = candleToCheck.Min(Function(x)
                                                                                 Return x.Value.Low
                                                                             End Function)
                                Dim diff As Decimal = highestHigh - lowestlow
                                Dim totalCandles As Integer = candleToCheck.Count
                                Dim eachPointValue As Decimal = diff / totalCandles

                                Dim totalCandlesOnTheLine As Integer = 0
                                Dim counter As Integer = 0
                                For Each runningCandle In candleToCheck.OrderBy(Function(x)
                                                                                    Return x.Key
                                                                                End Function)
                                    Dim y1 As Decimal = 1 * counter + 0
                                    Dim y2 As Decimal = -1 * counter + 0

                                    Dim y1Price As Decimal = lowestlow + y1 * eachPointValue
                                    Dim y2Price As Decimal = highestHigh + y2 * eachPointValue

                                    If (runningCandle.Value.High >= y1Price AndAlso runningCandle.Value.Low <= y1Price) OrElse
                                        (runningCandle.Value.High >= y2Price AndAlso runningCandle.Value.Low <= y2Price) Then
                                        totalCandlesOnTheLine += 1
                                    End If
                                Next

                                Dim row As DataRow = ret.NewRow
                                row("Date") = currentDayPayload.FirstOrDefault.Value.PayloadDate.ToString("dd-MM-yyyy")
                                row("Trading Symbol") = currentDayPayload.FirstOrDefault.Value.TradingSymbol
                                row("Total Candles") = totalCandles
                                row("Total Candles On The Line") = totalCandlesOnTheLine
                                row("Percentage") = Math.Round((totalCandlesOnTheLine / totalCandles) * 100, 2)
                                ret.Rows.Add(row)
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
