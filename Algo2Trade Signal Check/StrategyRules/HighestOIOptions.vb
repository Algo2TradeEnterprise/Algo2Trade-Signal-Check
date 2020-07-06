Imports Algo2TradeBLL
Imports System.Threading
Public Class HighestOIOptions
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
        ret.Columns.Add("Previous Day OI")

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
                    Dim stockPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayload(_category, stock, chkDate.AddDays(-200), chkDate)
                    _canceller.Token.ThrowIfCancellationRequested()
                    If stockPayload IsNot Nothing AndAlso stockPayload.Count > 0 Then
                        Dim XMinutePayload As Dictionary(Of Date, Payload) = stockPayload
                        _canceller.Token.ThrowIfCancellationRequested()
                        Dim inputPayload As Dictionary(Of Date, Payload) = stockPayload
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
                            Dim optionContracts As List(Of String) = Await GetCurrentOptionContractsAsync(stock, chkDate).ConfigureAwait(False)
                            If optionContracts IsNot Nothing AndAlso optionContracts.Count > 0 Then
                                Dim optionStockPayloads As Dictionary(Of String, Payload) = Nothing
                                For Each runningOption In optionContracts
                                    Dim eodOptPayload As Dictionary(Of Date, Payload) = Await GetRawPayloadForOptionsAsync(runningOption, chkDate.AddDays(-200), chkDate).ConfigureAwait(False)
                                    If eodOptPayload IsNot Nothing AndAlso eodOptPayload.Count > 0 Then
                                        Dim lastDayOptPayload As Payload = eodOptPayload.LastOrDefault.Value
                                        If lastDayOptPayload IsNot Nothing AndAlso lastDayOptPayload.PreviousCandlePayload IsNot Nothing AndAlso
                                            lastDayOptPayload.PayloadDate.Date = chkDate.Date Then
                                            lastDayOptPayload.OI = lastDayOptPayload.PreviousCandlePayload.OI
                                            If optionStockPayloads Is Nothing Then optionStockPayloads = New Dictionary(Of String, Payload)
                                            optionStockPayloads.Add(runningOption, lastDayOptPayload)
                                        End If
                                    End If
                                Next
                                If optionStockPayloads IsNot Nothing AndAlso optionStockPayloads.Count > 0 Then
                                    Dim counter As Integer = 0
                                    For Each runningPayload In optionStockPayloads.Where(Function(x)
                                                                                             Return x.Value.TradingSymbol.EndsWith("CE")
                                                                                         End Function).OrderByDescending(Function(y)
                                                                                                                             Return y.Value.OI
                                                                                                                         End Function)
                                        _canceller.Token.ThrowIfCancellationRequested()
                                        Dim row As DataRow = ret.NewRow
                                        row("Date") = runningPayload.Value.PayloadDate.ToString("dd-MMM-yyyy")
                                        row("Trading Symbol") = runningPayload.Value.TradingSymbol
                                        row("Open") = runningPayload.Value.Open
                                        row("Low") = runningPayload.Value.Low
                                        row("High") = runningPayload.Value.High
                                        row("Close") = runningPayload.Value.Close
                                        row("Previous Day OI") = runningPayload.Value.OI

                                        ret.Rows.Add(row)

                                        counter += 1
                                        If counter >= 5 Then Exit For
                                    Next

                                    counter = 0
                                    For Each runningPayload In optionStockPayloads.Where(Function(x)
                                                                                             Return x.Value.TradingSymbol.EndsWith("PE")
                                                                                         End Function).OrderByDescending(Function(y)
                                                                                                                             Return y.Value.OI
                                                                                                                         End Function)
                                        _canceller.Token.ThrowIfCancellationRequested()
                                        Dim row As DataRow = ret.NewRow
                                        row("Date") = runningPayload.Value.PayloadDate.ToString("dd-MMM-yyyy")
                                        row("Trading Symbol") = runningPayload.Value.TradingSymbol
                                        row("Open") = runningPayload.Value.Open
                                        row("Low") = runningPayload.Value.Low
                                        row("High") = runningPayload.Value.High
                                        row("Close") = runningPayload.Value.Close
                                        row("Previous Day OI") = runningPayload.Value.OI

                                        ret.Rows.Add(row)

                                        counter += 1
                                        If counter >= 5 Then Exit For
                                    Next
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

    Private Async Function GetCurrentOptionContractsAsync(ByVal rawInstrumentName As String, ByVal tradingDate As Date) As Task(Of List(Of String))
        Dim ret As List(Of String) = Nothing
        Dim tableName As String = "active_instruments_futures"
        Select Case _category
            Case Common.DataBaseTable.EOD_Commodity
                tableName = "active_instruments_commodity"
            Case Common.DataBaseTable.EOD_Currency
                tableName = "active_instruments_currency"
            Case Common.DataBaseTable.EOD_Futures
                tableName = "active_instruments_futures"
            Case Else
                Throw New NotImplementedException
        End Select
        Dim queryString As String = String.Format("SELECT `INSTRUMENT_TOKEN`,`TRADING_SYMBOL`,`EXPIRY` FROM `{0}` WHERE `AS_ON_DATE`='{1}' AND `TRADING_SYMBOL` REGEXP '^{2}[0-9][0-9]*' AND `SEGMENT`='NFO-OPT'",
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
                         .Expiry = If(IsDBNull(dt.Rows(i).Item(2)), Date.MaxValue, dt.Rows(i).Item(2))}
                    If activeInstruments Is Nothing Then activeInstruments = New List(Of ActiveInstrumentData)
                    activeInstruments.Add(instrumentData)
                Next
                If activeInstruments IsNot Nothing AndAlso activeInstruments.Count > 0 Then
                    _canceller.Token.ThrowIfCancellationRequested()
                    Dim minExpiry As Date = activeInstruments.Min(Function(x)
                                                                      If x.Expiry.Date >= tradingDate.Date Then
                                                                          Return x.Expiry
                                                                      Else
                                                                          Return Date.MaxValue
                                                                      End If
                                                                  End Function)
                    If minExpiry <> Date.MinValue Then
                        For Each runningOptStock In activeInstruments
                            _canceller.Token.ThrowIfCancellationRequested()
                            If runningOptStock.Expiry.Date = minExpiry.Date Then
                                If ret Is Nothing Then ret = New List(Of String)
                                ret.Add(runningOptStock.TradingSymbol)
                            End If
                        Next
                    End If
                End If
            End If
        End If
        Return ret
    End Function

    Private Class ActiveInstrumentData
        Public Token As String
        Public TradingSymbol As String
        Public Expiry As Date
    End Class

    Private Async Function GetRawPayloadForOptionsAsync(ByVal tradingSymbol As String, ByVal startDate As Date, ByVal endDate As Date) As Task(Of Dictionary(Of Date, Payload))
        Dim ret As Dictionary(Of Date, Payload) = Nothing
        _canceller.Token.ThrowIfCancellationRequested()
        Dim queryString As String = Nothing
        Select Case _category
            Case Common.DataBaseTable.EOD_Currency
                queryString = String.Format("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDate`,`TradingSymbol`,`OI` FROM `eod_prices_opt_currency` WHERE `TradingSymbol`='{0}' AND `SnapshotDate`>={1} AND `SnapshotDate`<='{2}'", tradingSymbol, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"))
            Case Common.DataBaseTable.EOD_Commodity
                queryString = String.Format("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDate`,`TradingSymbol`,`OI` FROM `eod_prices_opt_commodity` WHERE `TradingSymbol`='{0}' AND `SnapshotDate`>={1} AND `SnapshotDate`<='{2}'", tradingSymbol, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"))
            Case Common.DataBaseTable.EOD_Futures
                queryString = String.Format("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDate`,`TradingSymbol`,`OI` FROM `eod_prices_opt_futures` WHERE `TradingSymbol`='{0}' AND `SnapshotDate`>={1} AND `SnapshotDate`<='{2}'", tradingSymbol, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"))
            Case Else
                Throw New NotImplementedException
        End Select

        _canceller.Token.ThrowIfCancellationRequested()
        If tradingSymbol IsNot Nothing Then
            OnHeartbeat(String.Format("Fetching raw candle data from DataBase for {0} on {1}", tradingSymbol, endDate.ToShortDateString))
            Dim dt As DataTable = Await _cmn.RunSelectAsync(queryString).ConfigureAwait(False)
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                _canceller.Token.ThrowIfCancellationRequested()
                ret = Common.ConvertDataTableToPayload(dt, 0, 1, 2, 3, 4, 5, 6, 7)
            End If
        End If
        Return ret
    End Function
End Class