﻿Imports Algo2TradeBLL
Imports System.Threading
Public Class PreviousDayHighLowBreak
    Inherits Rule
    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal stockCategory As Integer, ByVal timeFrame As Integer, ByVal useHA As Boolean, ByVal stockName As String, ByVal fileName As String)
        MyBase.New(canceller, stockCategory, timeFrame, useHA, stockName, fileName)
    End Sub
    Public Overrides Async Function RunAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Trading Symbol")
        ret.Columns.Add("Previous Day High")
        ret.Columns.Add("Previous Day Low")
        ret.Columns.Add("High Breakout")
        ret.Columns.Add("Low Breakout")

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
                            stockPayload = _cmn.GetRawPayload(_category, stock, chkDate.AddDays(-8), chkDate)
                        Case Common.DataBaseTable.EOD_Cash, Common.DataBaseTable.EOD_Commodity, Common.DataBaseTable.EOD_Currency, Common.DataBaseTable.EOD_Futures, Common.DataBaseTable.EOD_POSITIONAL, Common.DataBaseTable.EOD_Futures_Options
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
                        If currentDayPayload IsNot Nothing AndAlso currentDayPayload.Count > 0 Then
                            Dim previousTradingDay As Date = currentDayPayload.FirstOrDefault.Value.PreviousCandlePayload.PayloadDate.Date
                            Dim previousDayHigh As Decimal = inputPayload.Max(Function(x)
                                                                                  If x.Key.Date = previousTradingDay.Date Then
                                                                                      Return x.Value.High
                                                                                  Else
                                                                                      Return Decimal.MinValue
                                                                                  End If
                                                                              End Function)
                            Dim previousDayLow As Decimal = inputPayload.Min(Function(x)
                                                                                 If x.Key.Date = previousTradingDay.Date Then
                                                                                     Return x.Value.Low
                                                                                 Else
                                                                                     Return Decimal.MaxValue
                                                                                 End If
                                                                             End Function)

                            Dim row As DataRow = ret.NewRow
                            row("Date") = chkDate.ToString("dd-MMM-yyyy")
                            row("Trading Symbol") = currentDayPayload.FirstOrDefault.Value.TradingSymbol
                            row("Previous Day High") = previousDayHigh
                            row("Previous Day Low") = previousDayLow

                            Dim highBreak As Boolean = False
                            Dim lowBreak As Boolean = False
                            For Each runningPayload In currentDayPayload
                                _canceller.Token.ThrowIfCancellationRequested()
                                If Not highBreak AndAlso runningPayload.Value.High >= previousDayHigh Then
                                    highBreak = True
                                    row("High Breakout") = runningPayload.Key.ToString("HH:mm:ss")
                                End If
                                If Not lowBreak AndAlso runningPayload.Value.Low <= previousDayLow Then
                                    lowBreak = True
                                    row("Low Breakout") = runningPayload.Key.ToString("HH:mm:ss")
                                End If

                                If highBreak AndAlso lowBreak Then Exit For
                            Next
                            If Not highBreak Then row("High Breakout") = "#N/A"
                            If Not lowBreak Then row("Low Breakout") = "#N/A"
                            ret.Rows.Add(row)
                        End If
                    End If
                Next
            End If
            chkDate = chkDate.AddDays(1)
        End While
        Return ret
    End Function
End Class