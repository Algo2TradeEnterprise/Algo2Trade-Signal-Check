Imports Algo2TradeBLL
Imports System.Threading
Public Class HighVolumeOppositeColor
    Inherits Rule
    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal stockCategory As Integer, ByVal timeFrame As Integer, ByVal useHA As Boolean, ByVal stockName As String, ByVal fileName As String)
        MyBase.New(canceller, stockCategory, timeFrame, useHA, stockName, fileName)
    End Sub
    Public Overrides Async Function RunAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Trading Symbol")
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
                            Dim signalCandle As Payload = Nothing
                            For Each runningPayload In currentDayPayload
                                _canceller.Token.ThrowIfCancellationRequested()
                                If signalCandle IsNot Nothing Then
                                    If signalCandle.CandleColor = Color.Green Then
                                        Dim entry As Decimal = signalCandle.Low - CalculateBuffer(signalCandle.Low, Utilities.Numbers.NumberManipulation.RoundOfType.Floor)
                                        If runningPayload.Value.Low <= entry Then
                                            Dim row As DataRow = ret.NewRow
                                            row("Date") = signalCandle.PayloadDate.ToString("dd-MMM-yyyy HH:mm:ss")
                                            row("Trading Symbol") = signalCandle.TradingSymbol
                                            row("Direction") = "SELL"

                                            ret.Rows.Add(row)
                                            signalCandle = Nothing
                                        End If
                                    ElseIf signalCandle.CandleColor = Color.Red Then
                                        Dim entry As Decimal = signalCandle.High + CalculateBuffer(signalCandle.High, Utilities.Numbers.NumberManipulation.RoundOfType.Floor)
                                        If runningPayload.Value.High >= entry Then
                                            Dim row As DataRow = ret.NewRow
                                            row("Date") = signalCandle.PayloadDate.ToString("dd-MMM-yyyy HH:mm:ss")
                                            row("Trading Symbol") = signalCandle.TradingSymbol
                                            row("Direction") = "BUY"

                                            ret.Rows.Add(row)
                                            signalCandle = Nothing
                                        End If
                                    End If
                                End If
                                _canceller.Token.ThrowIfCancellationRequested()
                                If runningPayload.Value.PreviousCandlePayload.PayloadDate.Date = chkDate.Date Then
                                    If runningPayload.Value.Volume > runningPayload.Value.PreviousCandlePayload.Volume Then
                                        signalCandle = runningPayload.Value
                                    End If
                                End If
                                _canceller.Token.ThrowIfCancellationRequested()
                            Next
                        End If
                    End If
                Next
            End If
            chkDate = chkDate.AddDays(1)
        End While
        Return ret
    End Function

    Private Function GetLowestSwingHigh(ByVal swingPayload As Dictionary(Of Date, Indicator.Swing), ByVal startTime As Date, ByVal endTime As Date) As Indicator.Swing
        Dim ret As Indicator.Swing = Nothing
        For Each runningPayload In swingPayload
            If runningPayload.Key >= startTime AndAlso runningPayload.Key <= endTime Then
                If ret Is Nothing Then
                    ret = runningPayload.Value
                Else
                    If runningPayload.Value.SwingHigh <= ret.SwingHigh Then
                        ret = runningPayload.Value
                    End If
                End If
            End If
        Next
        Return ret
    End Function

    Private Function GetHighestSwingLow(ByVal swingPayload As Dictionary(Of Date, Indicator.Swing), ByVal startTime As Date, ByVal endTime As Date) As Indicator.Swing
        Dim ret As Indicator.Swing = Nothing
        For Each runningPayload In swingPayload
            If runningPayload.Key >= startTime AndAlso runningPayload.Key <= endTime Then
                If ret Is Nothing Then
                    ret = runningPayload.Value
                Else
                    If runningPayload.Value.SwingLow >= ret.SwingLow Then
                        ret = runningPayload.Value
                    End If
                End If
            End If
        Next
        Return ret
    End Function
End Class