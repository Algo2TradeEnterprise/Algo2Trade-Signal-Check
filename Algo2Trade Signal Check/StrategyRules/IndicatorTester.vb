Imports Algo2TradeBLL
Imports System.Threading
Public Class IndicatorTester
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
        ret.Columns.Add("Volume")
        ret.Columns.Add("Trend")

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
                        Case Common.DataBaseTable.Intraday_Cash, Common.DataBaseTable.Intraday_Commodity, Common.DataBaseTable.Intraday_Currency, Common.DataBaseTable.Intraday_Futures, Common.DataBaseTable.Intraday_Futures_Options
                            stockPayload = _cmn.GetRawPayloadForSpecificTradingSymbol(_category, stock, chkDate.AddDays(-300), chkDate)
                        Case Common.DataBaseTable.EOD_Cash, Common.DataBaseTable.EOD_Commodity, Common.DataBaseTable.EOD_Currency, Common.DataBaseTable.EOD_Futures, Common.DataBaseTable.EOD_POSITIONAL, Common.DataBaseTable.EOD_Futures_Options
                            stockPayload = _cmn.GetRawPayloadForSpecificTradingSymbol(_category, stock, chkDate.AddDays(-300), chkDate)
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
                            Dim trendPayload As Dictionary(Of Date, Color) = Nothing
                            CalculateHKKeltnerChannelTrend(inputPayload, trendPayload)

                            For Each runningPayload In currentDayPayload.Keys
                                _canceller.Token.ThrowIfCancellationRequested()
                                Dim row As DataRow = ret.NewRow
                                row("Date") = inputPayload(runningPayload).PayloadDate.ToString("dd-MMM-yyyy")
                                row("Time") = inputPayload(runningPayload).PayloadDate.ToString("HH:mm:ss")
                                row("Trading Symbol") = inputPayload(runningPayload).TradingSymbol
                                row("Open") = inputPayload(runningPayload).Open
                                row("Low") = inputPayload(runningPayload).Low
                                row("High") = inputPayload(runningPayload).High
                                row("Close") = inputPayload(runningPayload).Close
                                row("Volume") = inputPayload(runningPayload).Volume
                                row("Trend") = trendPayload(runningPayload).Name

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

#Region "HK Trend Calculation"
    Private Sub CalculateHKKeltnerChannelTrend(ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputPayload As Dictionary(Of Date, Color))
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            Dim hkPayload As Dictionary(Of Date, Payload) = Nothing
            Indicator.HeikenAshi.ConvertToHeikenAshi(inputPayload, hkPayload)
            Dim emaPayload As Dictionary(Of Date, Decimal) = Nothing
            Dim highKeltnerPayload As Dictionary(Of Date, Decimal) = Nothing
            Dim lowKeltnerPayload As Dictionary(Of Date, Decimal) = Nothing
            Indicator.KeltnerChannel.CalculateEMAKeltnerChannel(50, 0.5, hkPayload, highKeltnerPayload, lowKeltnerPayload, emaPayload)

            Dim trend As Color = Color.White
            For Each runningPayload In hkPayload
                If runningPayload.Value.CandleStrengthHeikenAshi = Payload.StrongCandle.Bullish Then
                    If runningPayload.Value.Close > highKeltnerPayload(runningPayload.Key) Then
                        trend = Color.Green
                    End If
                ElseIf runningPayload.Value.CandleStrengthHeikenAshi = Payload.StrongCandle.Bearish Then
                    If runningPayload.Value.Close < lowKeltnerPayload(runningPayload.Key) Then
                        trend = Color.Red
                    End If
                End If

                If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, Color)
                outputPayload.Add(runningPayload.Key, trend)
            Next
        End If
    End Sub
#End Region
End Class
