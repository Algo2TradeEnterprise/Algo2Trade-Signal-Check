Imports Algo2TradeBLL
Imports System.Threading
Public Class BTST_STBTSignal
    Inherits Rule

    Private _stockMinPayload As Dictionary(Of String, Dictionary(Of Date, Payload)) = Nothing
    Private _stockXMinPayload As Dictionary(Of String, Dictionary(Of Date, Payload)) = Nothing
    Private _stockEODPayload As Dictionary(Of String, Dictionary(Of Date, Payload)) = Nothing

    Private ReadOnly _ruleNumber As Integer
    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal stockCategory As Integer, ByVal timeFrame As Integer, ByVal useHA As Boolean, ByVal stockName As String, ByVal fileName As String, ByVal ruleNumber As Integer)
        MyBase.New(canceller, stockCategory, timeFrame, useHA, stockName, fileName)
        _ruleNumber = ruleNumber

        RemoveHandler _cmn.Heartbeat, AddressOf OnHeartbeat
    End Sub

    Public Overrides Async Function RunAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Trading Symbol")
        ret.Columns.Add("Signal")
        ret.Columns.Add("1 Min Open %")
        ret.Columns.Add("1 Min Low %")
        ret.Columns.Add("1 Min High %")
        ret.Columns.Add("1 Min Close %")
        ret.Columns.Add("X Min Open %")
        ret.Columns.Add("X Min Low %")
        ret.Columns.Add("X Min High %")
        ret.Columns.Add("X Min Close %")
        ret.Columns.Add("EOD Close %")

        Dim stockData As StockSelection = New StockSelection(_canceller, _category, _cmn, _fileName)
        AddHandler stockData.Heartbeat, AddressOf OnHeartbeat
        AddHandler stockData.WaitingFor, AddressOf OnWaitingFor
        AddHandler stockData.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
        AddHandler stockData.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
        Dim stockList As List(Of String) = Nothing
        If _instrumentName Is Nothing OrElse _instrumentName = "" Then
            stockList = Await stockData.GetStockList(endDate).ConfigureAwait(False)
        Else
            stockList = New List(Of String) From {_instrumentName}
        End If
        _canceller.Token.ThrowIfCancellationRequested()
        If stockList IsNot Nothing AndAlso stockList.Count > 0 Then
            Dim stkCtr As Integer = 0
            For Each runningStock In stockList
                stkCtr += 1
                OnHeartbeat(String.Format("Getting stock data for {0} #{1}/{2}", runningStock, stkCtr, stockList.Count))
                _canceller.Token.ThrowIfCancellationRequested()
                Dim intradayPayload As Dictionary(Of Date, Payload) = Nothing
                Dim eodPayload As Dictionary(Of Date, Payload) = Nothing
                Select Case _category
                    Case Common.DataBaseTable.Intraday_Cash, Common.DataBaseTable.EOD_Cash, Common.DataBaseTable.EOD_POSITIONAL
                        intradayPayload = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.Intraday_Cash, runningStock, startDate.AddDays(-8), endDate)
                        eodPayload = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.EOD_POSITIONAL, runningStock, startDate.AddDays(-8), endDate)
                    Case Else
                        Throw New NotImplementedException
                End Select
                If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 AndAlso
                    eodPayload IsNot Nothing AndAlso eodPayload.Count > 0 Then
                    Dim XMinutePayload As Dictionary(Of Date, Payload) = Nothing
                    Dim exchangeStartTime As Date = Date.MinValue
                    Select Case _category
                        Case Common.DataBaseTable.EOD_Cash, Common.DataBaseTable.EOD_Futures, Common.DataBaseTable.EOD_POSITIONAL, Common.DataBaseTable.Intraday_Cash, Common.DataBaseTable.Intraday_Futures
                            exchangeStartTime = New Date(Now.Year, Now.Month, Now.Day, 9, 15, 0)
                        Case Common.DataBaseTable.EOD_Commodity, Common.DataBaseTable.EOD_Currency, Common.DataBaseTable.Intraday_Commodity, Common.DataBaseTable.Intraday_Currency
                            exchangeStartTime = New Date(Now.Year, Now.Month, Now.Day, 9, 0, 0)
                    End Select
                    If _timeFrame > 1 Then
                        XMinutePayload = Common.ConvertPayloadsToXMinutes(intradayPayload, _timeFrame, exchangeStartTime)
                    Else
                        XMinutePayload = intradayPayload
                    End If
                    If _stockMinPayload Is Nothing Then _stockMinPayload = New Dictionary(Of String, Dictionary(Of Date, Payload))
                    _stockMinPayload.Add(runningStock, intradayPayload)
                    If _stockXMinPayload Is Nothing Then _stockXMinPayload = New Dictionary(Of String, Dictionary(Of Date, Payload))
                    _stockXMinPayload.Add(runningStock, XMinutePayload)
                    If _stockEODPayload Is Nothing Then _stockEODPayload = New Dictionary(Of String, Dictionary(Of Date, Payload))
                    _stockEODPayload.Add(runningStock, eodPayload)
                End If
            Next

            stkCtr = 0
            For Each runningStock In stockList
                stkCtr += 1
                OnHeartbeat(String.Format("Checking signal data for {0} #{1}/{2}", runningStock, stkCtr, stockList.Count))
                _canceller.Token.ThrowIfCancellationRequested()
                Dim minPayload As Dictionary(Of Date, Payload) = _stockMinPayload(runningStock)
                _canceller.Token.ThrowIfCancellationRequested()
                If minPayload IsNot Nothing AndAlso minPayload.Count > 0 Then
                    _canceller.Token.ThrowIfCancellationRequested()
                    Dim xMinPayload As Dictionary(Of Date, Payload) = _stockXMinPayload(runningStock)

                    Dim chkDate As Date = startDate
                    While chkDate <= endDate
                        _canceller.Token.ThrowIfCancellationRequested()
                        OnHeartbeat(String.Format("Checking signal data for {0} #{1}/{2} on {3}", runningStock, stkCtr, stockList.Count, chkDate.ToString("dd-MMM-yyyy")))

                        Dim currentDayXMinFirstCandle As Payload = xMinPayload.Where(Function(x)
                                                                                         Return x.Key.Date = chkDate.Date
                                                                                     End Function).FirstOrDefault.Value

                        Dim currentDayXMinLastCandle As Payload = xMinPayload.Where(Function(x)
                                                                                        Return x.Key.Date = chkDate.Date
                                                                                    End Function).LastOrDefault.Value

                        Dim currentDayMinFirstCandle As Payload = minPayload.Where(Function(x)
                                                                                       Return x.Key.Date = chkDate.Date
                                                                                   End Function).FirstOrDefault.Value

                        If currentDayMinFirstCandle IsNot Nothing AndAlso currentDayXMinFirstCandle IsNot Nothing AndAlso
                            currentDayXMinFirstCandle.PreviousCandlePayload IsNot Nothing AndAlso currentDayXMinLastCandle IsNot Nothing Then
                            Dim entryPrice As Decimal = currentDayXMinFirstCandle.PreviousCandlePayload.Close
                            Dim signal As Integer = GetSignal(runningStock, chkDate)
                            If signal = 1 Then
                                Dim row As DataRow = ret.NewRow
                                row("Date") = chkDate.ToString("dd-MMM-yyyy")
                                row("Trading Symbol") = runningStock
                                row("Signal") = "BTST"
                                row("1 Min Open %") = Math.Round(((currentDayMinFirstCandle.Open / entryPrice) - 1) * 100, 4)
                                row("1 Min Low %") = Math.Round(((currentDayMinFirstCandle.Low / entryPrice) - 1) * 100, 4)
                                row("1 Min High %") = Math.Round(((currentDayMinFirstCandle.High / entryPrice) - 1) * 100, 4)
                                row("1 Min Close %") = Math.Round(((currentDayMinFirstCandle.Close / entryPrice) - 1) * 100, 4)
                                row("X Min Open %") = Math.Round(((currentDayXMinFirstCandle.Open / entryPrice) - 1) * 100, 4)
                                row("X Min Low %") = Math.Round(((currentDayXMinFirstCandle.Low / entryPrice) - 1) * 100, 4)
                                row("X Min High %") = Math.Round(((currentDayXMinFirstCandle.High / entryPrice) - 1) * 100, 4)
                                row("X Min Close %") = Math.Round(((currentDayXMinFirstCandle.Close / entryPrice) - 1) * 100, 4)
                                row("EOD Close %") = Math.Round(((currentDayXMinLastCandle.Close / entryPrice) - 1) * 100, 4)

                                ret.Rows.Add(row)
                            ElseIf signal = -1 Then
                                Dim row As DataRow = ret.NewRow
                                row("Date") = chkDate.ToString("dd-MMM-yyyy")
                                row("Trading Symbol") = runningStock
                                row("Signal") = "STBT"
                                row("1 Min Open %") = Math.Round((1 - (currentDayMinFirstCandle.Open / entryPrice)) * 100, 4)
                                row("1 Min Low %") = Math.Round((1 - (currentDayMinFirstCandle.Low / entryPrice)) * 100, 4)
                                row("1 Min High %") = Math.Round((1 - (currentDayMinFirstCandle.High / entryPrice)) * 100, 4)
                                row("1 Min Close %") = Math.Round((1 - (currentDayMinFirstCandle.Close / entryPrice)) * 100, 4)
                                row("X Min Open %") = Math.Round((1 - (currentDayXMinFirstCandle.Open / entryPrice)) * 100, 4)
                                row("X Min Low %") = Math.Round((1 - (currentDayXMinFirstCandle.Low / entryPrice)) * 100, 4)
                                row("X Min High %") = Math.Round((1 - (currentDayXMinFirstCandle.High / entryPrice)) * 100, 4)
                                row("X Min Close %") = Math.Round((1 - (currentDayXMinFirstCandle.Close / entryPrice)) * 100, 4)
                                row("EOD Close %") = Math.Round((1 - (currentDayXMinLastCandle.Close / entryPrice)) * 100, 4)

                                ret.Rows.Add(row)
                            End If
                        End If
                        chkDate = chkDate.AddDays(1)
                    End While
                End If
            Next
        End If
        Return ret
    End Function

    Private Function GetSignal(stockName As String, checkDate As Date) As Integer
        Select Case _ruleNumber
            Case 0
                Return GetBidAskRatioSignal(stockName, checkDate)
            Case 1
                Return GetStrongCandleCloseSignal(stockName, checkDate)
            Case 2
                Return GetDeliveryPercentageSignal(stockName, checkDate)
            Case 3
                Return GetOpenHighLowSignal(stockName, checkDate)
            Case 4
                Return GetStrongHKSignal(stockName, checkDate)
            Case 5
                Return GetTopGainerLooserSignal(stockName, checkDate)
            Case 7
                Return GetLastBarClosesOutsideSupportResistanceSignal(stockName, checkDate)
            Case 8
                Return GetLastBarOutsideBarSignal(stockName, checkDate)
            Case 9
                Return GetLastBarClosesOutsideSwingHighLowSignal(stockName, checkDate)
            Case Else
                Throw New NotImplementedException
        End Select
    End Function

#Region "Rules"
    Private Function GetBidAskRatioSignal(stockName As String, checkDate As Date) As Integer
        Throw New NotImplementedException
    End Function

    Private Function GetStrongCandleCloseSignal(stockName As String, checkDate As Date) As Integer
        Dim ret As Integer = 0
        If _stockEODPayload IsNot Nothing AndAlso _stockEODPayload.ContainsKey(stockName) Then
            If _stockEODPayload(stockName) IsNot Nothing AndAlso _stockEODPayload(stockName).ContainsKey(checkDate.Date) Then
                Dim currentDayCandle As Payload = _stockEODPayload(stockName)(checkDate.Date)
                If currentDayCandle IsNot Nothing AndAlso currentDayCandle.PreviousCandlePayload IsNot Nothing AndAlso
                    currentDayCandle.PreviousCandlePayload.PreviousCandlePayload IsNot Nothing Then
                    If currentDayCandle.PreviousCandlePayload.CandleColor = Color.Green AndAlso
                        currentDayCandle.PreviousCandlePayload.PreviousCandlePayload.CandleColor = Color.Red Then
                        If currentDayCandle.PreviousCandlePayload.Close >= currentDayCandle.PreviousCandlePayload.Low + currentDayCandle.PreviousCandlePayload.CandleRange * 80 / 100 Then
                            ret = 1
                        End If
                    ElseIf currentDayCandle.PreviousCandlePayload.CandleColor = Color.Red AndAlso
                        currentDayCandle.PreviousCandlePayload.PreviousCandlePayload.CandleColor = Color.Green Then
                        If currentDayCandle.PreviousCandlePayload.Close <= currentDayCandle.PreviousCandlePayload.High - currentDayCandle.PreviousCandlePayload.CandleRange * 80 / 100 Then
                            ret = -1
                        End If
                    End If
                End If
            End If
        End If
        Return ret
    End Function

    Private Function GetDeliveryPercentageSignal(stockName As String, checkDate As Date) As Integer
        Dim ret As Integer = 0
        If _stockEODPayload IsNot Nothing AndAlso _stockEODPayload.ContainsKey(stockName) Then
            If _stockEODPayload(stockName) IsNot Nothing AndAlso _stockEODPayload(stockName).ContainsKey(checkDate.Date) Then
                Dim currentDayCandle As Payload = _stockEODPayload(stockName)(checkDate.Date)
                If currentDayCandle IsNot Nothing AndAlso currentDayCandle.PreviousCandlePayload IsNot Nothing Then
                    Dim query As String = String.Format("SELECT `SnapshotDate`,`DeliveryPercentage` FROM `eod_positional_data` WHERE `TradingSymbol`='{0}' AND `SnapshotDate`<='{1}' ORDER BY `SnapshotDate` DESC LIMIT 5", stockName, currentDayCandle.PreviousCandlePayload.PayloadDate.ToString("yyyy-MM-dd"))
                    Dim dt As DataTable = _cmn.RunSelect(query)
                    If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                        Dim deliveryPercentage As Dictionary(Of Date, Decimal) = Nothing
                        For Each runningRow As DataRow In dt.Rows
                            If deliveryPercentage Is Nothing Then deliveryPercentage = New Dictionary(Of Date, Decimal)
                            deliveryPercentage.Add(runningRow("SnapshotDate"), runningRow("DeliveryPercentage"))
                        Next
                        If deliveryPercentage IsNot Nothing AndAlso deliveryPercentage.Count = 5 AndAlso
                            deliveryPercentage.ContainsKey(currentDayCandle.PreviousCandlePayload.PayloadDate) Then
                            Dim avg As Decimal = deliveryPercentage.Values.Average()
                            If deliveryPercentage(currentDayCandle.PreviousCandlePayload.PayloadDate) >= 50 AndAlso
                                deliveryPercentage(currentDayCandle.PreviousCandlePayload.PayloadDate) >= avg Then
                                ret = 1
                            End If
                        End If
                    End If
                End If
            End If
        End If
        Return ret
    End Function

    Private Function GetOpenHighLowSignal(stockName As String, checkDate As Date) As Integer
        Dim ret As Integer = 0
        If _stockEODPayload IsNot Nothing AndAlso _stockEODPayload.ContainsKey(stockName) Then
            If _stockEODPayload(stockName) IsNot Nothing AndAlso _stockEODPayload(stockName).ContainsKey(checkDate.Date) Then
                Dim currentDayCandle As Payload = _stockEODPayload(stockName)(checkDate.Date)
                If currentDayCandle IsNot Nothing AndAlso currentDayCandle.PreviousCandlePayload IsNot Nothing Then
                    If currentDayCandle.PreviousCandlePayload.Open = currentDayCandle.PreviousCandlePayload.High Then
                        ret = -1
                    ElseIf currentDayCandle.PreviousCandlePayload.Open = currentDayCandle.PreviousCandlePayload.Low Then
                        ret = 1
                    End If
                End If
            End If
        End If
        Return ret
    End Function

    Private Function GetStrongHKSignal(stockName As String, checkDate As Date) As Integer
        Dim ret As Integer = 0
        If _stockXMinPayload IsNot Nothing AndAlso _stockXMinPayload.ContainsKey(stockName) Then
            Dim hkPayload As Dictionary(Of Date, Payload) = Nothing
            Indicator.HeikenAshi.ConvertToHeikenAshi(_stockXMinPayload(stockName), hkPayload)
            If hkPayload IsNot Nothing AndAlso hkPayload.Count > 0 Then
                Dim currentDayXMinFirstCandle As Payload = hkPayload.Where(Function(x)
                                                                               Return x.Key.Date = checkDate.Date
                                                                           End Function).FirstOrDefault.Value
                If currentDayXMinFirstCandle IsNot Nothing AndAlso currentDayXMinFirstCandle.PreviousCandlePayload IsNot Nothing Then
                    If currentDayXMinFirstCandle.PreviousCandlePayload.CandleStrengthHeikenAshi = Payload.StrongCandle.Bullish Then
                        ret = 1
                    ElseIf currentDayXMinFirstCandle.PreviousCandlePayload.CandleStrengthHeikenAshi = Payload.StrongCandle.Bearish Then
                        ret = -1
                    End If
                End If
            End If
        End If
        Return ret
    End Function

    Private Function GetTopGainerLooserSignal(stockName As String, checkDate As Date) As Integer
        Dim ret As Integer = 0
        If _stockEODPayload IsNot Nothing AndAlso _stockEODPayload.ContainsKey(stockName) AndAlso
            _stockEODPayload(stockName).ContainsKey(checkDate.Date) Then
            Dim currentDayCandle As Payload = _stockEODPayload(stockName)(checkDate.Date)
            If currentDayCandle IsNot Nothing AndAlso currentDayCandle.PreviousCandlePayload IsNot Nothing Then
                Dim changePer As Dictionary(Of String, Decimal) = Nothing
                For Each runningStock In _stockEODPayload.Keys
                    If _stockEODPayload(runningStock).ContainsKey(currentDayCandle.PreviousCandlePayload.PayloadDate) Then
                        If _stockEODPayload(runningStock)(currentDayCandle.PreviousCandlePayload.PayloadDate) IsNot Nothing AndAlso
                            _stockEODPayload(runningStock)(currentDayCandle.PreviousCandlePayload.PayloadDate).PreviousCandlePayload IsNot Nothing Then
                            If changePer Is Nothing Then changePer = New Dictionary(Of String, Decimal)
                            changePer.Add(runningStock, ((_stockEODPayload(runningStock)(currentDayCandle.PreviousCandlePayload.PayloadDate).Close / _stockEODPayload(runningStock)(currentDayCandle.PreviousCandlePayload.PayloadDate).PreviousCandlePayload.Close) - 1) * 100)
                        End If
                    End If
                Next
                If changePer IsNot Nothing AndAlso changePer.Count > 0 Then
                    Dim ctr As Integer = 0
                    For Each runningStock In changePer.OrderByDescending(Function(x)
                                                                             Return x.Value
                                                                         End Function)
                        ctr += 1
                        If runningStock.Key.ToUpper = stockName.ToUpper Then
                            ret = 1
                            Exit For
                        End If
                        If ctr >= 5 Then Exit For
                    Next
                    ctr = 0
                    For Each runningStock In changePer.OrderBy(Function(x)
                                                                   Return x.Value
                                                               End Function)
                        ctr += 1
                        If runningStock.Key.ToUpper = stockName.ToUpper Then
                            ret = -1
                            Exit For
                        End If
                        If ctr >= 5 Then Exit For
                    Next
                End If
            End If
        End If
        Return ret
    End Function

    Private Function GetLastBarClosesOutsideSupportResistanceSignal(stockName As String, checkDate As Date) As Integer
        Dim ret As Integer = 0
        If _stockXMinPayload IsNot Nothing AndAlso _stockXMinPayload.ContainsKey(stockName) Then
            Dim pivotPayload As Dictionary(Of Date, PivotPoints) = Nothing
            Indicator.Pivots.CalculatePivots(_stockXMinPayload(stockName), pivotPayload)

            Dim currentDayXMinFirstCandle As Payload = _stockXMinPayload(stockName).Where(Function(x)
                                                                                              Return x.Key.Date = checkDate.Date
                                                                                          End Function).FirstOrDefault.Value
            If currentDayXMinFirstCandle IsNot Nothing AndAlso currentDayXMinFirstCandle.PreviousCandlePayload IsNot Nothing AndAlso
                currentDayXMinFirstCandle.PreviousCandlePayload.PreviousCandlePayload IsNot Nothing AndAlso
                pivotPayload.ContainsKey(currentDayXMinFirstCandle.PreviousCandlePayload.PreviousCandlePayload.PayloadDate) Then
                Dim pivot As PivotPoints = pivotPayload(currentDayXMinFirstCandle.PreviousCandlePayload.PreviousCandlePayload.PayloadDate)
                If currentDayXMinFirstCandle.PreviousCandlePayload.Close >= pivot.Resistance1 Then
                    ret = -1
                ElseIf currentDayXMinFirstCandle.PreviousCandlePayload.Close <= pivot.Support1 Then
                    ret = 1
                End If
            End If
        End If
        Return ret
    End Function

    Private Function GetLastBarOutsideBarSignal(stockName As String, checkDate As Date) As Integer
        Dim ret As Integer = 0
        If _stockXMinPayload IsNot Nothing AndAlso _stockXMinPayload.ContainsKey(stockName) Then
            Dim currentDayXMinFirstCandle As Payload = _stockXMinPayload(stockName).Where(Function(x)
                                                                                              Return x.Key.Date = checkDate.Date
                                                                                          End Function).FirstOrDefault.Value
            If currentDayXMinFirstCandle IsNot Nothing AndAlso currentDayXMinFirstCandle.PreviousCandlePayload IsNot Nothing AndAlso
                currentDayXMinFirstCandle.PreviousCandlePayload.PreviousCandlePayload IsNot Nothing Then
                If currentDayXMinFirstCandle.PreviousCandlePayload.High >= currentDayXMinFirstCandle.PreviousCandlePayload.PreviousCandlePayload.High AndAlso
                    currentDayXMinFirstCandle.PreviousCandlePayload.Low <= currentDayXMinFirstCandle.PreviousCandlePayload.PreviousCandlePayload.Low Then
                    If currentDayXMinFirstCandle.PreviousCandlePayload.CandleColor = Color.Green Then
                        ret = 1
                    ElseIf currentDayXMinFirstCandle.PreviousCandlePayload.CandleColor = Color.Red Then
                        ret = -1
                    End If
                End If
            End If
        End If
        Return ret
    End Function

    Private Function GetLastBarClosesOutsideSwingHighLowSignal(stockName As String, checkDate As Date) As Integer
        Dim ret As Integer = 0
        If _stockXMinPayload IsNot Nothing AndAlso _stockXMinPayload.ContainsKey(stockName) Then
            Dim swingPayload As Dictionary(Of Date, Indicator.Swing) = Nothing
            Indicator.SwingHighLow.CalculateSwingHighLow(_stockXMinPayload(stockName), True, swingPayload)

            Dim currentDayXMinFirstCandle As Payload = _stockXMinPayload(stockName).Where(Function(x)
                                                                                              Return x.Key.Date = checkDate.Date
                                                                                          End Function).FirstOrDefault.Value
            If currentDayXMinFirstCandle IsNot Nothing AndAlso currentDayXMinFirstCandle.PreviousCandlePayload IsNot Nothing AndAlso
                currentDayXMinFirstCandle.PreviousCandlePayload.PreviousCandlePayload IsNot Nothing AndAlso
                swingPayload.ContainsKey(currentDayXMinFirstCandle.PreviousCandlePayload.PreviousCandlePayload.PayloadDate) Then
                Dim swing As Indicator.Swing = swingPayload(currentDayXMinFirstCandle.PreviousCandlePayload.PreviousCandlePayload.PayloadDate)
                If currentDayXMinFirstCandle.PreviousCandlePayload.Close >= swing.SwingHigh Then
                    ret = 1
                ElseIf currentDayXMinFirstCandle.PreviousCandlePayload.Close <= swing.SwingLow Then
                    ret = -1
                End If
            End If
        End If
        Return ret
    End Function
#End Region
End Class