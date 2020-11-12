Imports Algo2TradeBLL
Imports System.Threading
Public Class FirstStrongHKAfterOppositeStrongHK
    Inherits Rule
    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal stockCategory As Integer, ByVal timeFrame As Integer, ByVal useHA As Boolean, ByVal stockName As String, ByVal fileName As String)
        MyBase.New(canceller, stockCategory, timeFrame, useHA, stockName, fileName)
    End Sub
    Public Overrides Async Function RunAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Instrument")
        ret.Columns.Add("Time")
        ret.Columns.Add("Signal")
        ret.Columns.Add("Signal Type")
        ret.Columns.Add("Nifty %")
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
                    Dim stockIntradayPayload As Dictionary(Of Date, Payload) = Nothing
                    Dim stockEODPayload As Dictionary(Of Date, Payload) = Nothing

                    Dim niftyIntradayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.Intraday_Cash, "NIFTY 50", chkDate.AddDays(-50), chkDate)
                    Dim niftyEODPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.EOD_Cash, "NIFTY 50", chkDate.AddDays(-50), chkDate)

                    Dim eodTable As Common.DataBaseTable = Common.DataBaseTable.EOD_Cash
                    Select Case _category
                        Case Common.DataBaseTable.Intraday_Cash
                            eodTable = Common.DataBaseTable.EOD_Cash
                        Case Common.DataBaseTable.Intraday_Commodity
                            eodTable = Common.DataBaseTable.EOD_Commodity
                        Case Common.DataBaseTable.Intraday_Currency
                            eodTable = Common.DataBaseTable.EOD_Currency
                        Case Common.DataBaseTable.Intraday_Futures
                            eodTable = Common.DataBaseTable.EOD_Futures
                    End Select
                    Select Case _category
                        Case Common.DataBaseTable.Intraday_Cash, Common.DataBaseTable.Intraday_Commodity, Common.DataBaseTable.Intraday_Currency, Common.DataBaseTable.Intraday_Futures
                            stockIntradayPayload = _cmn.GetRawPayload(_category, stock, chkDate.AddDays(-50), chkDate)
                            stockEODPayload = _cmn.GetRawPayload(eodTable, stock, chkDate.AddDays(-200), chkDate)
                        Case Common.DataBaseTable.EOD_Cash, Common.DataBaseTable.EOD_Commodity, Common.DataBaseTable.EOD_Currency, Common.DataBaseTable.EOD_Futures, Common.DataBaseTable.EOD_POSITIONAL
                            'stockIntradayPayload = _cmn.GetRawPayload(_category, stock, chkDate.AddDays(-200), chkDate)
                            Throw New ApplicationException("Choose intraday table type")
                    End Select
                    _canceller.Token.ThrowIfCancellationRequested()
                    If stockIntradayPayload IsNot Nothing AndAlso stockIntradayPayload.Count > 0 AndAlso
                        stockEODPayload IsNot Nothing AndAlso stockEODPayload.Count > 0 Then
                        Dim XMinutePayload As Dictionary(Of Date, Payload) = Nothing
                        Dim exchangeStartTime As Date = Date.MinValue
                        Select Case _category
                            Case Common.DataBaseTable.EOD_Cash, Common.DataBaseTable.EOD_Futures, Common.DataBaseTable.EOD_POSITIONAL, Common.DataBaseTable.Intraday_Cash, Common.DataBaseTable.Intraday_Futures
                                exchangeStartTime = New Date(chkDate.Year, chkDate.Month, chkDate.Day, 9, 15, 0)
                            Case Common.DataBaseTable.EOD_Commodity, Common.DataBaseTable.EOD_Currency, Common.DataBaseTable.Intraday_Commodity, Common.DataBaseTable.Intraday_Currency
                                exchangeStartTime = New Date(chkDate.Year, chkDate.Month, chkDate.Day, 9, 0, 0)
                        End Select
                        If _timeFrame > 1 Then
                            XMinutePayload = Common.ConvertPayloadsToXMinutes(stockIntradayPayload, _timeFrame, exchangeStartTime)
                        Else
                            XMinutePayload = stockIntradayPayload
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
                        If currentDayPayload IsNot Nothing AndAlso currentDayPayload.Count > 0 AndAlso
                            niftyIntradayPayload IsNot Nothing AndAlso niftyIntradayPayload.Count > 0 AndAlso
                            niftyEODPayload IsNot Nothing AndAlso niftyEODPayload.ContainsKey(chkDate.Date) Then
                            'Dim hkIntradayPayload As Dictionary(Of Date, Payload) = Nothing
                            'Indicator.HeikenAshi.ConvertToHeikenAshi(inputPayload, hkIntradayPayload)
                            Dim hkEODPayload As Dictionary(Of Date, Payload) = Nothing
                            Indicator.HeikenAshi.ConvertToHeikenAshi(stockEODPayload, hkEODPayload)

                            Dim niftyClose As Decimal = niftyEODPayload(chkDate.Date).PreviousCandlePayload.Close

                            If hkEODPayload IsNot Nothing AndAlso hkEODPayload.ContainsKey(chkDate.Date) Then
                                Dim niftyChngPer As Decimal = ((niftyEODPayload(chkDate.Date).Open - niftyClose) / niftyClose) * 100
                                If Math.Round(hkEODPayload(chkDate.Date).PreviousCandlePayload.Open, 2) = Math.Round(hkEODPayload(chkDate.Date).PreviousCandlePayload.Low, 2) Then
                                    For Each runningPayload In hkEODPayload.OrderByDescending(Function(x)
                                                                                                  Return x.Key
                                                                                              End Function)
                                        _canceller.Token.ThrowIfCancellationRequested()
                                        If runningPayload.Key < hkEODPayload(chkDate.Date).PreviousCandlePayload.PayloadDate Then
                                            If Math.Round(runningPayload.Value.Open, 2) = Math.Round(runningPayload.Value.Low, 2) Then
                                                Exit For
                                            ElseIf Math.Round(runningPayload.Value.Open, 2) = Math.Round(runningPayload.Value.High, 2) Then
                                                Dim row As DataRow = ret.NewRow
                                                row("Date") = chkDate.Date.ToString("dd-MMM-yyyy")
                                                row("Instrument") = runningPayload.Value.TradingSymbol
                                                row("Time") = chkDate.Date.ToString("HH:mm:ss")
                                                row("Signal") = "Buy"
                                                row("Signal Type") = "EOD"
                                                row("Nifty %") = niftyChngPer
                                                ret.Rows.Add(row)
                                                Exit For
                                            End If
                                        End If
                                    Next
                                ElseIf Math.Round(hkEODPayload(chkDate.Date).PreviousCandlePayload.Open, 2) = Math.Round(hkEODPayload(chkDate.Date).PreviousCandlePayload.High, 2) Then
                                    For Each runningPayload In hkEODPayload.OrderByDescending(Function(x)
                                                                                                  Return x.Key
                                                                                              End Function)
                                        _canceller.Token.ThrowIfCancellationRequested()
                                        If runningPayload.Key < hkEODPayload(chkDate.Date).PreviousCandlePayload.PayloadDate Then
                                            If Math.Round(runningPayload.Value.Open, 2) = Math.Round(runningPayload.Value.High, 2) Then
                                                Exit For
                                            ElseIf Math.Round(runningPayload.Value.Open, 2) = Math.Round(runningPayload.Value.Low, 2) Then
                                                Dim row As DataRow = ret.NewRow
                                                row("Date") = chkDate.Date.ToString("dd-MMM-yyyy")
                                                row("Instrument") = runningPayload.Value.TradingSymbol
                                                row("Time") = chkDate.Date.ToString("HH:mm:ss")
                                                row("Signal") = "Sell"
                                                row("Signal Type") = "EOD"
                                                row("Nifty %") = niftyChngPer
                                                ret.Rows.Add(row)
                                                Exit For
                                            End If
                                        End If
                                    Next
                                End If
                            End If

                            For Each runningCurrentPayload In currentDayPayload
                                _canceller.Token.ThrowIfCancellationRequested()
                                Dim niftyChngPer As Decimal = ((niftyIntradayPayload(runningCurrentPayload.Value.PreviousCandlePayload.PayloadDate).Close - niftyClose) / niftyClose) * 100

                                If Math.Round(runningCurrentPayload.Value.PreviousCandlePayload.Open, 2) = Math.Round(runningCurrentPayload.Value.PreviousCandlePayload.Low, 2) Then
                                    For Each runningPayload In inputPayload.OrderByDescending(Function(x)
                                                                                                  Return x.Key
                                                                                              End Function)
                                        _canceller.Token.ThrowIfCancellationRequested()
                                        If runningPayload.Key < runningCurrentPayload.Value.PreviousCandlePayload.PayloadDate Then
                                            If Math.Round(runningPayload.Value.Open, 2) = Math.Round(runningPayload.Value.Low, 2) Then
                                                Exit For
                                            ElseIf Math.Round(runningPayload.Value.Open, 2) = Math.Round(runningPayload.Value.High, 2) Then
                                                Dim row As DataRow = ret.NewRow
                                                row("Date") = runningCurrentPayload.Key.ToString("dd-MMM-yyyy")
                                                row("Instrument") = runningPayload.Value.TradingSymbol
                                                row("Time") = runningCurrentPayload.Key.ToString("HH:mm:ss")
                                                row("Signal") = "Buy"
                                                row("Signal Type") = "Intraday"
                                                row("Nifty %") = niftyChngPer
                                                ret.Rows.Add(row)
                                                Exit For
                                            End If
                                        End If
                                    Next
                                ElseIf Math.Round(runningCurrentPayload.Value.PreviousCandlePayload.Open, 2) = Math.Round(runningCurrentPayload.Value.PreviousCandlePayload.High, 2) Then
                                    For Each runningPayload In inputPayload.OrderByDescending(Function(x)
                                                                                                  Return x.Key
                                                                                              End Function)
                                        _canceller.Token.ThrowIfCancellationRequested()
                                        If runningPayload.Key < runningCurrentPayload.Value.PreviousCandlePayload.PayloadDate Then
                                            If Math.Round(runningPayload.Value.Open, 2) = Math.Round(runningPayload.Value.High, 2) Then
                                                Exit For
                                            ElseIf Math.Round(runningPayload.Value.Open, 2) = Math.Round(runningPayload.Value.Low, 2) Then
                                                Dim row As DataRow = ret.NewRow
                                                row("Date") = runningCurrentPayload.Key.ToString("dd-MMM-yyyy")
                                                row("Instrument") = runningPayload.Value.TradingSymbol
                                                row("Time") = runningCurrentPayload.Key.ToString("HH:mm:ss")
                                                row("Signal") = "Sell"
                                                row("Signal Type") = "Intraday"
                                                row("Nifty %") = niftyChngPer
                                                ret.Rows.Add(row)
                                                Exit For
                                            End If
                                        End If
                                    Next
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