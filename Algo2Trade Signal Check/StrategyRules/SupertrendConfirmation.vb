Imports Algo2TradeBLL
Imports System.Threading
Public Class SupertrendConfirmation
    Inherits Rule

    Private ReadOnly _maximumRangePer As Decimal = 0
    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal stockCategory As Integer, ByVal timeFrame As Integer, ByVal useHA As Boolean, ByVal stockName As String, ByVal fileName As String, ByVal maxRangePer As Decimal)
        MyBase.New(canceller, stockCategory, timeFrame, useHA, stockName, fileName)
        _maximumRangePer = maxRangePer
    End Sub
    Public Overrides Async Function RunAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Trading Symbol")
        ret.Columns.Add("Range")
        ret.Columns.Add("Range %")
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
                            stockPayload = _cmn.GetRawPayload(_category, stock, chkDate.AddDays(-20), chkDate)
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
                        If currentDayPayload IsNot Nothing AndAlso currentDayPayload.Count > 0 Then
                            Dim supertrendPayload As Dictionary(Of Date, Decimal) = Nothing
                            Dim supertrendColorPayload As Dictionary(Of Date, Color) = Nothing
                            Indicator.Supertrend.CalculateSupertrend(7, 3, inputPayload, supertrendPayload, supertrendColorPayload)

                            For Each runningPayload In currentDayPayload
                                _canceller.Token.ThrowIfCancellationRequested()
                                If runningPayload.Value.PreviousCandlePayload IsNot Nothing AndAlso
                                    runningPayload.Value.PreviousCandlePayload.PayloadDate.Date = chkDate.Date Then
                                    If supertrendColorPayload(runningPayload.Key) = Color.Green Then
                                        If runningPayload.Value.CandleColor = Color.Green AndAlso
                                            runningPayload.Value.PreviousCandlePayload.CandleColor = Color.Red Then
                                            Dim low As Decimal = Math.Min(runningPayload.Value.Low, runningPayload.Value.PreviousCandlePayload.Low)
                                            Dim high As Decimal = Math.Max(runningPayload.Value.High, runningPayload.Value.PreviousCandlePayload.High)
                                            Dim range As Decimal = high - low
                                            Dim rangePer As Decimal = Math.Round((range / low) * 100, 4)
                                            If rangePer <= _maximumRangePer Then
                                                Dim row As DataRow = ret.NewRow
                                                row("Date") = runningPayload.Value.PayloadDate.ToString("dd-MMM-yyyy HH:mm:ss")
                                                row("Trading Symbol") = runningPayload.Value.TradingSymbol
                                                row("Range") = range
                                                row("Range %") = rangePer
                                                row("Direction") = "Buy"

                                                ret.Rows.Add(row)
                                            End If
                                        End If
                                    ElseIf supertrendColorPayload(runningPayload.Key) = Color.Red Then
                                        If runningPayload.Value.CandleColor = Color.Red AndAlso
                                            runningPayload.Value.PreviousCandlePayload.CandleColor = Color.Green Then
                                            Dim low As Decimal = Math.Min(runningPayload.Value.Low, runningPayload.Value.PreviousCandlePayload.Low)
                                            Dim high As Decimal = Math.Max(runningPayload.Value.High, runningPayload.Value.PreviousCandlePayload.High)
                                            Dim range As Decimal = high - low
                                            Dim rangePer As Decimal = Math.Round((range / low) * 100, 4)
                                            If rangePer <= _maximumRangePer Then
                                                Dim row As DataRow = ret.NewRow
                                                row("Date") = runningPayload.Value.PayloadDate.ToString("dd-MMM-yyyy HH:mm:ss")
                                                row("Trading Symbol") = runningPayload.Value.TradingSymbol
                                                row("Range") = range
                                                row("Range %") = rangePer
                                                row("Direction") = "Sell"

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

    Private Function GetHighestATR(ByVal atrPayload As Dictionary(Of Date, Decimal), ByVal signalTime As Date) As Decimal
        Return atrPayload.Max(Function(x)
                                  If x.Key.Date = signalTime.Date AndAlso x.Key <= signalTime Then
                                      Return x.Value
                                  Else
                                      Return Decimal.MinValue
                                  End If
                              End Function)
    End Function

    Private Function CalculateSlab(ByVal price As Decimal, ByVal atr As Decimal) As Decimal
        Dim ret As Decimal = 0.5
        Dim slabList As List(Of Decimal) = New List(Of Decimal) From {0.5, 1, 2.5, 5, 10, 15}
        Dim supportedSlabList As List(Of Decimal) = slabList.FindAll(Function(x)
                                                                         Return x <= atr / 8
                                                                     End Function)
        If supportedSlabList IsNot Nothing AndAlso supportedSlabList.Count > 0 Then
            ret = supportedSlabList.Max
            If price * 1 / 100 < ret Then
                Dim newSupportedSlabList As List(Of Decimal) = supportedSlabList.FindAll(Function(x)
                                                                                             Return x <= price * 1 / 100
                                                                                         End Function)
                If newSupportedSlabList IsNot Nothing AndAlso newSupportedSlabList.Count > 0 Then
                    ret = newSupportedSlabList.Max
                End If
            End If
        End If
        Return ret
    End Function
End Class
