﻿Imports Algo2TradeBLL
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
        ret.Columns.Add("15 Min Open %")
        ret.Columns.Add("15 Min Low %")
        ret.Columns.Add("15 Min High %")
        ret.Columns.Add("15 Min Close %")

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

                        Dim currentDayMinFirstCandle As Payload = minPayload.Where(Function(x)
                                                                                       Return x.Key.Date = chkDate.Date
                                                                                   End Function).FirstOrDefault.Value

                        If currentDayMinFirstCandle IsNot Nothing AndAlso currentDayXMinFirstCandle IsNot Nothing Then
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
                                row("15 Min Open %") = Math.Round(((currentDayXMinFirstCandle.Open / entryPrice) - 1) * 100, 4)
                                row("15 Min Low %") = Math.Round(((currentDayXMinFirstCandle.Low / entryPrice) - 1) * 100, 4)
                                row("15 Min High %") = Math.Round(((currentDayXMinFirstCandle.High / entryPrice) - 1) * 100, 4)
                                row("15 Min Close %") = Math.Round(((currentDayXMinFirstCandle.Close / entryPrice) - 1) * 100, 4)

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
                                row("15 Min Open %") = Math.Round((1 - (currentDayXMinFirstCandle.Open / entryPrice)) * 100, 4)
                                row("15 Min Low %") = Math.Round((1 - (currentDayXMinFirstCandle.Low / entryPrice)) * 100, 4)
                                row("15 Min High %") = Math.Round((1 - (currentDayXMinFirstCandle.High / entryPrice)) * 100, 4)
                                row("15 Min Close %") = Math.Round((1 - (currentDayXMinFirstCandle.Close / entryPrice)) * 100, 4)

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
#End Region
End Class