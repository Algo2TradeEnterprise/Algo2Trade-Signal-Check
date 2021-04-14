Imports Algo2TradeBLL
Imports System.Threading
Public Class FractalHighBreakoutBelowSupport
    Inherits Rule
    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal stockCategory As Integer, ByVal timeFrame As Integer, ByVal useHA As Boolean, ByVal stockName As String, ByVal fileName As String)
        MyBase.New(canceller, stockCategory, timeFrame, useHA, stockName, fileName)
    End Sub
    Public Overrides Async Function RunAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Trading Symbol")

        Dim stockData As StockSelection = New StockSelection(_canceller, _category, _cmn, _fileName)
        AddHandler stockData.Heartbeat, AddressOf OnHeartbeat
        AddHandler stockData.WaitingFor, AddressOf OnWaitingFor
        AddHandler stockData.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
        AddHandler stockData.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete

        Dim stockList As List(Of String) = Nothing
        If _instrumentName Is Nothing OrElse _instrumentName = "" Then
            stockList = New List(Of String) From
            {"ADANIPORTS", "ASIANPAINT", "AXISBANK", "BAJAJ-AUTO", "BAJFINANCE", "BAJAJFINSV", "BPCL", "BHARTIARTL", "BRITANNIA", "CIPLA", "COALINDIA", "DIVISLAB", "DRREDDY", "EICHERMOT", "GRASIM", "HCLTECH", "HDFCBANK", "HEROMOTOCO", "HINDALCO", "HINDUNILVR", "HDFC", "ICICIBANK", "ITC", "IOC", "INDUSINDBK", "INFY", "JSWSTEEL", "KOTAKBANK", "LT", "M&M", "MARUTI", "NTPC", "NESTLEIND", "ONGC", "POWERGRID", "RELIANCE", "SHREECEM", "SBIN", "SUNPHARMA", "TCS", "TATAMOTORS", "TATASTEEL", "TECHM", "TITAN", "UPL", "ULTRACEMCO", "WIPRO"}
        Else
            stockList = New List(Of String)
            stockList.Add(_instrumentName)
        End If

        Dim allStockPayload As Dictionary(Of String, Dictionary(Of Date, Payload)) = Nothing
        Dim allStockFractalHighPayload As Dictionary(Of String, Dictionary(Of Date, Decimal)) = Nothing
        Dim allStockFractalLowPayload As Dictionary(Of String, Dictionary(Of Date, Decimal)) = Nothing
        Dim allStockPivotPayload As Dictionary(Of String, Dictionary(Of Date, PivotPoints)) = Nothing

        Dim ctr0 As Integer = 0
        For Each stock In stockList
            ctr0 += 1
            OnHeartbeat(String.Format("Fetching Data #{0}/{1}", ctr0, stockList.Count))
            Dim stockPayload As Dictionary(Of Date, Payload) = Nothing
            Select Case _category
                Case Common.DataBaseTable.Intraday_Cash, Common.DataBaseTable.Intraday_Commodity, Common.DataBaseTable.Intraday_Currency, Common.DataBaseTable.Intraday_Futures
                    stockPayload = _cmn.GetRawPayloadForSpecificTradingSymbol(_category, stock, startDate.AddDays(-8), endDate)
                Case Else
                    Throw New NotImplementedException
            End Select
            If stockPayload IsNot Nothing AndAlso stockPayload.Count > 0 Then
                OnHeartbeat(String.Format("Calculating Indicators #{0}/{1}", ctr0, stockList.Count))
                Dim XMinutePayload As Dictionary(Of Date, Payload) = Nothing
                Dim exchangeStartTime As Date = Date.MinValue
                Select Case _category
                    Case Common.DataBaseTable.EOD_Cash, Common.DataBaseTable.EOD_Futures, Common.DataBaseTable.EOD_POSITIONAL, Common.DataBaseTable.Intraday_Cash, Common.DataBaseTable.Intraday_Futures
                        exchangeStartTime = New Date(endDate.Year, endDate.Month, endDate.Day, 9, 15, 0)
                    Case Common.DataBaseTable.EOD_Commodity, Common.DataBaseTable.EOD_Currency, Common.DataBaseTable.Intraday_Commodity, Common.DataBaseTable.Intraday_Currency
                        exchangeStartTime = New Date(endDate.Year, endDate.Month, endDate.Day, 9, 0, 0)
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

                Dim fractalHighPayload As Dictionary(Of Date, Decimal) = Nothing
                Dim fractalLowPayload As Dictionary(Of Date, Decimal) = Nothing
                Dim pivotPayload As Dictionary(Of Date, PivotPoints) = Nothing
                Indicator.FractalBands.CalculateFractal(inputPayload, fractalHighPayload, fractalLowPayload)
                Indicator.Pivots.CalculatePivots(inputPayload, pivotPayload)

                If allStockPayload Is Nothing Then allStockPayload = New Dictionary(Of String, Dictionary(Of Date, Payload))
                allStockPayload.Add(stock, inputPayload)
                If allStockFractalHighPayload Is Nothing Then allStockFractalHighPayload = New Dictionary(Of String, Dictionary(Of Date, Decimal))
                allStockFractalHighPayload.Add(stock, fractalHighPayload)
                If allStockFractalLowPayload Is Nothing Then allStockFractalLowPayload = New Dictionary(Of String, Dictionary(Of Date, Decimal))
                allStockFractalLowPayload.Add(stock, fractalLowPayload)
                If allStockPivotPayload Is Nothing Then allStockPivotPayload = New Dictionary(Of String, Dictionary(Of Date, PivotPoints))
                allStockPivotPayload.Add(stock, pivotPayload)
            End If
        Next

        Dim chkDate As Date = startDate
        While chkDate <= endDate
            _canceller.Token.ThrowIfCancellationRequested()
            _canceller.Token.ThrowIfCancellationRequested()
            If stockList IsNot Nothing AndAlso stockList.Count > 0 Then
                Dim ctr As Integer = 0
                For Each stock In stockList
                    _canceller.Token.ThrowIfCancellationRequested()
                    ctr += 1
                    OnHeartbeat(String.Format("Running #{0}/{1} on {2}", ctr, stockList.Count, chkDate.ToString("dd-MMM-yyyy")))
                    If allStockPayload.ContainsKey(stock) Then
                        _canceller.Token.ThrowIfCancellationRequested()
                        Dim inputPayload As Dictionary(Of Date, Payload) = allStockPayload(stock)
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
                            Dim fractalHighPayload As Dictionary(Of Date, Decimal) = allStockFractalHighPayload(stock)
                            Dim fractalLowPayload As Dictionary(Of Date, Decimal) = allStockFractalLowPayload(stock)
                            Dim pivotPayload As Dictionary(Of Date, PivotPoints) = allStockPivotPayload(stock)
                            For Each runningPayload In currentDayPayload.Keys
                                _canceller.Token.ThrowIfCancellationRequested()
                                Dim currentCandle As Payload = currentDayPayload(runningPayload)
                                If currentCandle.PreviousCandlePayload IsNot Nothing Then
                                    If fractalHighPayload(currentCandle.PreviousCandlePayload.PayloadDate) < pivotPayload(currentCandle.PreviousCandlePayload.PayloadDate).Support3 AndAlso
                                        fractalLowPayload(currentCandle.PreviousCandlePayload.PayloadDate) < pivotPayload(currentCandle.PreviousCandlePayload.PayloadDate).Support3 AndAlso
                                        fractalLowPayload(currentCandle.PreviousCandlePayload.PayloadDate) < fractalHighPayload(currentCandle.PreviousCandlePayload.PayloadDate) Then
                                        Dim row As DataRow = ret.NewRow
                                        row("Date") = chkDate.ToString("dd-MMM-yyyy")
                                        row("Trading Symbol") = currentCandle.TradingSymbol

                                        ret.Rows.Add(row)
                                        Exit For
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