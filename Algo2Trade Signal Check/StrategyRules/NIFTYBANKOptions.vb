Imports Algo2TradeBLL
Imports System.Threading
Public Class NIFTYBANKOptions
    Inherits Rule
    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal stockCategory As Integer, ByVal timeFrame As Integer, ByVal useHA As Boolean, ByVal stockName As String, ByVal fileName As String)
        MyBase.New(canceller, stockCategory, timeFrame, useHA, stockName, fileName)
    End Sub
    Public Overrides Async Function RunAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Trading Symbol")
        ret.Columns.Add("Lot Size")
        ret.Columns.Add("Signal Candle Time")

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
                        If _useHA Then
                            Indicator.HeikenAshi.ConvertToHeikenAshi(XMinutePayload, inputPayload)
                        Else
                            inputPayload = XMinutePayload
                        End If
                        _canceller.Token.ThrowIfCancellationRequested()
                        Dim currentDayPayload As Dictionary(Of Date, Payload) = Nothing
                        Dim signalPayload As Dictionary(Of Date, Payload) = Nothing

                        Dim ctr As Integer = 0
                        For Each runningPayload In inputPayload.Keys
                            _canceller.Token.ThrowIfCancellationRequested()
                            If runningPayload.Date = chkDate.Date Then
                                If currentDayPayload Is Nothing Then currentDayPayload = New Dictionary(Of Date, Payload)
                                currentDayPayload.Add(runningPayload, inputPayload(runningPayload))

                                If ctr < 7 Then
                                    If signalPayload Is Nothing Then signalPayload = New Dictionary(Of Date, Payload)
                                    signalPayload.Add(runningPayload, inputPayload(runningPayload))
                                End If
                                ctr += 1
                            End If
                        Next

                        'Main Logic
                        If currentDayPayload IsNot Nothing AndAlso currentDayPayload.Count > 0 AndAlso
                            signalPayload IsNot Nothing AndAlso signalPayload.Count = 7 Then
                            Dim highestHigh As Decimal = signalPayload.Max(Function(x)
                                                                               Return x.Value.High
                                                                           End Function)
                            Dim lowestLow As Decimal = signalPayload.Min(Function(x)
                                                                             Return x.Value.Low
                                                                         End Function)

                            Dim signalCandle As Payload = Nothing
                            Dim optionType As String = Nothing
                            For Each runningPayload In currentDayPayload.Keys
                                _canceller.Token.ThrowIfCancellationRequested()
                                If runningPayload > signalPayload.LastOrDefault.Key Then
                                    If currentDayPayload(runningPayload).Close > highestHigh Then
                                        signalCandle = currentDayPayload(runningPayload)
                                        optionType = "CE"
                                        Exit For
                                    ElseIf currentDayPayload(runningPayload).Close < lowestLow Then
                                        signalCandle = currentDayPayload(runningPayload)
                                        optionType = "PE"
                                        Exit For
                                    End If
                                End If
                            Next
                            If signalCandle IsNot Nothing Then
                                Dim optionContracts As List(Of ActiveInstrumentData) = Await GetCurrentOptionContractsAsync("BANKNIFTY", chkDate).ConfigureAwait(False)
                                If optionContracts IsNot Nothing AndAlso optionContracts.Count > 0 Then
                                    Dim optionStrike As Dictionary(Of Decimal, ActiveInstrumentData) = Nothing
                                    For Each runningOption In optionContracts
                                        Dim tradingSymbol As String = runningOption.TradingSymbol
                                        If tradingSymbol.EndsWith(optionType) Then
                                            Dim strikeData As String = Utilities.Strings.GetTextBetween("BANKNIFTY", optionType, tradingSymbol)
                                            Dim strike As String = strikeData.Substring(5)
                                            If strike IsNot Nothing AndAlso strike.Trim <> "" AndAlso IsNumeric(strike) Then
                                                If optionStrike Is Nothing Then optionStrike = New Dictionary(Of Decimal, ActiveInstrumentData)
                                                optionStrike.Add(Val(strike), runningOption)
                                            End If
                                        End If
                                    Next
                                    If optionStrike IsNot Nothing AndAlso optionStrike.Count > 0 Then
                                        Dim selectedStrike As Decimal = 0
                                        If optionType = "CE" Then
                                            selectedStrike = optionStrike.Where(Function(x)
                                                                                    Return x.Key <= signalCandle.Close
                                                                                End Function).OrderByDescending(Function(y)
                                                                                                                    Return y.Key
                                                                                                                End Function).FirstOrDefault.Key
                                        ElseIf optionType = "PE" Then
                                            selectedStrike = optionStrike.Where(Function(x)
                                                                                    Return x.Key >= signalCandle.Close
                                                                                End Function).OrderByDescending(Function(y)
                                                                                                                    Return y.Key
                                                                                                                End Function).LastOrDefault.Key
                                        End If

                                        If selectedStrike <> 0 Then
                                            Dim row As DataRow = ret.NewRow
                                            row("Date") = chkDate.ToString("dd-MMM-yyyy")
                                            row("Trading Symbol") = optionStrike(selectedStrike).TradingSymbol
                                            row("Lot Size") = optionStrike(selectedStrike).LotSize
                                            row("Signal Candle Time") = signalCandle.PayloadDate.ToString("dd-MMM-yyyy HH:mm:ss")

                                            ret.Rows.Add(row)
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                Next
            End If
            chkDate = chkDate.AddDays(1)
        End While
        Return ret
    End Function

    Private Class ActiveInstrumentData
        Public Token As String
        Public TradingSymbol As String
        Public Expiry As Date
        Public LotSize As Integer
    End Class

    Private Async Function GetCurrentOptionContractsAsync(ByVal rawInstrumentName As String, ByVal tradingDate As Date) As Task(Of List(Of ActiveInstrumentData))
        Dim ret As List(Of ActiveInstrumentData) = Nothing
        Dim tableName As String = "active_instruments_futures"
        Dim queryString As String = String.Format("SELECT `INSTRUMENT_TOKEN`,`TRADING_SYMBOL`,`EXPIRY`,`LOT_SIZE` FROM `{0}` WHERE `AS_ON_DATE`='{1}' AND `TRADING_SYMBOL` REGEXP '^{2}[0-9][0-9]*' AND `SEGMENT`='NFO-OPT'",
                                                   tableName, tradingDate.ToString("yyyy-MM-dd"), rawInstrumentName)
        _canceller.Token.ThrowIfCancellationRequested()
        Dim dt As DataTable = Await _cmn.RunSelectAsync(queryString).ConfigureAwait(False)
        _canceller.Token.ThrowIfCancellationRequested()
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                Dim activeInstruments As List(Of ActiveInstrumentData) = Nothing
                For i = 0 To dt.Rows.Count - 1
                    _canceller.Token.ThrowIfCancellationRequested()
                    Dim instrumentData As New ActiveInstrumentData With
                        {.Token = dt.Rows(i).Item(0),
                         .TradingSymbol = dt.Rows(i).Item(1).ToString.ToUpper,
                         .Expiry = If(IsDBNull(dt.Rows(i).Item(2)), Date.MaxValue, dt.Rows(i).Item(2)),
                         .LotSize = dt.Rows(i).Item(3)}
                    If activeInstruments Is Nothing Then activeInstruments = New List(Of ActiveInstrumentData)
                    activeInstruments.Add(instrumentData)
                Next
                If activeInstruments IsNot Nothing AndAlso activeInstruments.Count > 0 Then
                    _canceller.Token.ThrowIfCancellationRequested()
                    Dim minExpiry As Date = activeInstruments.Min(Function(x)
                                                                      If x.Expiry.Date > tradingDate.Date Then
                                                                          Return x.Expiry
                                                                      Else
                                                                          Return Date.MaxValue
                                                                      End If
                                                                  End Function)
                    If minExpiry <> Date.MinValue Then
                        For Each runningOptStock In activeInstruments
                            _canceller.Token.ThrowIfCancellationRequested()
                            If runningOptStock.Expiry.Date = minExpiry.Date Then
                                If ret Is Nothing Then ret = New List(Of ActiveInstrumentData)
                                ret.Add(runningOptStock)
                            End If
                        Next
                    End If
                End If
            End If
        End If
        Return ret
    End Function
End Class
