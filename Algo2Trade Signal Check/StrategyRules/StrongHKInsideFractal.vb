Imports Algo2TradeBLL
Imports System.Threading
Imports Utilities.Numbers

Public Class StrongHKInsideFractal
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
        ret.Columns.Add("Direction")

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
                        'If _useHA Then
                        Indicator.HeikenAshi.ConvertToHeikenAshi(XMinutePayload, inputPayload)
                        'Else
                        '    inputPayload = XMinutePayload
                        'End If
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
                            Dim fractalHighPayload As Dictionary(Of Date, Decimal) = Nothing
                            Dim fractalLowPayload As Dictionary(Of Date, Decimal) = Nothing
                            Indicator.FractalBands.CalculateFractal(inputPayload, fractalHighPayload, fractalLowPayload)

                            For Each runningPayload In currentDayPayload
                                _canceller.Token.ThrowIfCancellationRequested()
                                If runningPayload.Value.PreviousCandlePayload IsNot Nothing AndAlso
                                    runningPayload.Value.PreviousCandlePayload.PayloadDate.Date = chkDate.Date Then
                                    If runningPayload.Value.PreviousCandlePayload.CandleStrengthHeikenAshi = Payload.StrongCandle.Bullish Then
                                        If runningPayload.Value.PreviousCandlePayload.Open > fractalLowPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate) AndAlso
                                            runningPayload.Value.PreviousCandlePayload.Close < fractalHighPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate) AndAlso
                                            runningPayload.Value.PreviousCandlePayload.High > fractalHighPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate) Then
                                            Dim entryPrice As Decimal = ConvertFloorCeling(runningPayload.Value.PreviousCandlePayload.Low, 0.05, RoundOfType.Floor)
                                            If entryPrice = runningPayload.Value.PreviousCandlePayload.Low Then
                                                Dim buffer As Decimal = CalculateBuffer(entryPrice, RoundOfType.Floor)
                                                entryPrice = entryPrice - buffer
                                            End If
                                            If runningPayload.Value.Low <= entryPrice Then
                                                Dim row As DataRow = ret.NewRow
                                                row("Date") = runningPayload.Value.PayloadDate
                                                row("Trading Symbol") = runningPayload.Value.TradingSymbol
                                                row("Open") = Math.Round(runningPayload.Value.Open, 2)
                                                row("Low") = Math.Round(runningPayload.Value.Low, 2)
                                                row("High") = Math.Round(runningPayload.Value.High, 2)
                                                row("Close") = Math.Round(runningPayload.Value.Close, 2)
                                                row("Direction") = "SELL"

                                                ret.Rows.Add(row)
                                            End If
                                        End If
                                    ElseIf runningPayload.Value.PreviousCandlePayload.CandleStrengthHeikenAshi = Payload.StrongCandle.Bearish Then
                                        If runningPayload.Value.PreviousCandlePayload.Open < fractalHighPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate) AndAlso
                                            runningPayload.Value.PreviousCandlePayload.Close > fractalLowPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate) AndAlso
                                            runningPayload.Value.PreviousCandlePayload.Low < fractalLowPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate) Then
                                            Dim entryPrice As Decimal = ConvertFloorCeling(runningPayload.Value.PreviousCandlePayload.High, 0.05, RoundOfType.Celing)
                                            If entryPrice = runningPayload.Value.PreviousCandlePayload.High Then
                                                Dim buffer As Decimal = CalculateBuffer(entryPrice, RoundOfType.Floor)
                                                entryPrice = entryPrice + buffer
                                            End If
                                            If runningPayload.Value.High >= entryPrice Then
                                                Dim row As DataRow = ret.NewRow
                                                row("Date") = runningPayload.Value.PayloadDate
                                                row("Trading Symbol") = runningPayload.Value.TradingSymbol
                                                row("Open") = Math.Round(runningPayload.Value.Open, 2)
                                                row("Low") = Math.Round(runningPayload.Value.Low, 2)
                                                row("High") = Math.Round(runningPayload.Value.High, 2)
                                                row("Close") = Math.Round(runningPayload.Value.Close, 2)
                                                row("Direction") = "BUY"

                                                ret.Rows.Add(row)
                                            End If
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