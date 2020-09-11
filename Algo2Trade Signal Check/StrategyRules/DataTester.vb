Imports Algo2TradeBLL
Imports System.Threading
Public Class DataTester
    Inherits Rule
    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal stockCategory As Integer, ByVal timeFrame As Integer, ByVal useHA As Boolean, ByVal stockName As String, ByVal fileName As String)
        MyBase.New(canceller, stockCategory, timeFrame, useHA, stockName, fileName)
    End Sub
    Public Overrides Async Function RunAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Trading Symbol 1")
        ret.Columns.Add("Open 1")
        ret.Columns.Add("Low 1")
        ret.Columns.Add("High 1")
        ret.Columns.Add("Close 1")
        ret.Columns.Add("Volume 1")
        ret.Columns.Add("Trading Symbol 2")
        ret.Columns.Add("Open 2")
        ret.Columns.Add("Low 2")
        ret.Columns.Add("High 2")
        ret.Columns.Add("Close 2")
        ret.Columns.Add("Volume 2")
        ret.Columns.Add("Trading Symbol 3")
        ret.Columns.Add("Open 3")
        ret.Columns.Add("Low 3")
        ret.Columns.Add("High 3")
        ret.Columns.Add("Close 3")
        ret.Columns.Add("Volume 3")
        ret.Columns.Add("Trading Symbol 4")
        ret.Columns.Add("Open 4")
        ret.Columns.Add("Low 4")
        ret.Columns.Add("High 4")
        ret.Columns.Add("Close 4")
        ret.Columns.Add("Volume 4")

        Dim stockData As StockSelection = New StockSelection(_canceller, _category, _cmn, _fileName)
        AddHandler stockData.Heartbeat, AddressOf OnHeartbeat
        AddHandler stockData.WaitingFor, AddressOf OnWaitingFor
        AddHandler stockData.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
        AddHandler stockData.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
        _canceller.Token.ThrowIfCancellationRequested()
        Dim hdfcbankTradingSymbol As String = _cmn.GetCurrentTradingSymbol(Common.DataBaseTable.EOD_Futures, startDate, "HDFCBANK")
        Dim hdfcbankPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.EOD_Futures, hdfcbankTradingSymbol, startDate, endDate)
        Dim icicibankTradingSymbol As String = _cmn.GetCurrentTradingSymbol(Common.DataBaseTable.EOD_Futures, startDate, "ICICIBANK")
        Dim icicibankPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.EOD_Futures, icicibankTradingSymbol, startDate, endDate)
        Dim hdfcbankOptions As Dictionary(Of Decimal, String) = Await GetOptionTradingSymbols(hdfcbankTradingSymbol, "CE", startDate)
        Dim icicibankOptions As Dictionary(Of Decimal, String) = Await GetOptionTradingSymbols(icicibankTradingSymbol, "PE", startDate)
        Dim hdfcbankPrice As Decimal = hdfcbankPayload.FirstOrDefault.Value.Open
        Dim hdfcbankOptionTradingSymbol As String = hdfcbankOptions.Where(Function(x)
                                                                              Return x.Key >= hdfcbankPrice + (hdfcbankPrice * 5 / 100)
                                                                          End Function).OrderBy(Function(y)
                                                                                                    Return y.Key
                                                                                                End Function).FirstOrDefault.Value
        Dim icicibankPrice As Decimal = icicibankPayload.FirstOrDefault.Value.Open
        Dim icicibankOptionTradingSymbol As String = icicibankOptions.Where(Function(x)
                                                                                Return x.Key <= icicibankPrice - (icicibankPrice * 5 / 100)
                                                                            End Function).OrderBy(Function(y)
                                                                                                      Return y.Key
                                                                                                  End Function).LastOrDefault.Value

        Dim hdfcbankOptionPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.EOD_Futures_Options, hdfcbankOptionTradingSymbol, startDate, endDate)
        Dim icicibankOptionPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.EOD_Futures_Options, icicibankOptionTradingSymbol, startDate, endDate)

        For Each runningPayload In hdfcbankPayload.Keys
            _canceller.Token.ThrowIfCancellationRequested()
            Dim row As DataRow = ret.NewRow
            row("Date") = runningPayload.ToString("dd-MMM-yyyy")
            row("Trading Symbol 1") = hdfcbankPayload(runningPayload).TradingSymbol
            row("Open 1") = hdfcbankPayload(runningPayload).Open
            row("Low 1") = hdfcbankPayload(runningPayload).Low
            row("High 1") = hdfcbankPayload(runningPayload).High
            row("Close 1") = hdfcbankPayload(runningPayload).Close
            row("Volume 1") = hdfcbankPayload(runningPayload).Volume
            row("Trading Symbol 2") = If(icicibankPayload.ContainsKey(runningPayload), icicibankPayload(runningPayload).TradingSymbol, "")
            row("Open 2") = If(icicibankPayload.ContainsKey(runningPayload), icicibankPayload(runningPayload).Open, "")
            row("Low 2") = If(icicibankPayload.ContainsKey(runningPayload), icicibankPayload(runningPayload).Low, "")
            row("High 2") = If(icicibankPayload.ContainsKey(runningPayload), icicibankPayload(runningPayload).High, "")
            row("Close 2") = If(icicibankPayload.ContainsKey(runningPayload), icicibankPayload(runningPayload).Close, "")
            row("Volume 2") = If(icicibankPayload.ContainsKey(runningPayload), icicibankPayload(runningPayload).Volume, "")
            row("Trading Symbol 3") = If(hdfcbankOptionPayload.ContainsKey(runningPayload), hdfcbankOptionPayload(runningPayload).TradingSymbol, "")
            row("Open 3") = If(hdfcbankOptionPayload.ContainsKey(runningPayload), hdfcbankOptionPayload(runningPayload).Open, "")
            row("Low 3") = If(hdfcbankOptionPayload.ContainsKey(runningPayload), hdfcbankOptionPayload(runningPayload).Low, "")
            row("High 3") = If(hdfcbankOptionPayload.ContainsKey(runningPayload), hdfcbankOptionPayload(runningPayload).High, "")
            row("Close 3") = If(hdfcbankOptionPayload.ContainsKey(runningPayload), hdfcbankOptionPayload(runningPayload).Close, "")
            row("Volume 3") = If(hdfcbankOptionPayload.ContainsKey(runningPayload), hdfcbankOptionPayload(runningPayload).Volume, "")
            row("Trading Symbol 4") = If(icicibankOptionPayload.ContainsKey(runningPayload), icicibankOptionPayload(runningPayload).TradingSymbol, "")
            row("Open 4") = If(icicibankOptionPayload.ContainsKey(runningPayload), icicibankOptionPayload(runningPayload).Open, "")
            row("Low 4") = If(icicibankOptionPayload.ContainsKey(runningPayload), icicibankOptionPayload(runningPayload).Low, "")
            row("High 4") = If(icicibankOptionPayload.ContainsKey(runningPayload), icicibankOptionPayload(runningPayload).High, "")
            row("Close 4") = If(icicibankOptionPayload.ContainsKey(runningPayload), icicibankOptionPayload(runningPayload).Close, "")
            row("Volume 4") = If(icicibankOptionPayload.ContainsKey(runningPayload), icicibankOptionPayload(runningPayload).Volume, "")

            ret.Rows.Add(row)
        Next
        Return ret
    End Function

    Private Async Function GetOptionTradingSymbols(ByVal futureTradingSymbol As String, ByVal instrumentType As String, ByVal tradingDate As Date) As Task(Of Dictionary(Of Decimal, String))
        Dim ret As Dictionary(Of Decimal, String) = Nothing
        Dim optionTradingSymbol As String = String.Format("{0}%{1}", futureTradingSymbol.Substring(0, futureTradingSymbol.Count - 3), instrumentType)
        Dim query As String = "SELECT DISTINCT(`TradingSymbol`) FROM `eod_prices_opt_futures` WHERE `TradingSymbol` LIKE '{0}' AND `SnapshotDate`='{1}'"
        query = String.Format(query, optionTradingSymbol, tradingDate.ToString("yyyy-MM-dd"))
        Dim dt As DataTable = Await _cmn.RunSelectAsync(query).ConfigureAwait(False)
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim i As Integer = 0
            While Not i = dt.Rows.Count()
                If Not IsDBNull(dt.Rows(i).Item("TradingSymbol")) Then
                    Dim tradingSymbol As String = dt.Rows(i).Item("TradingSymbol")
                    Dim strikePrice As String = Utilities.Strings.GetTextBetween(futureTradingSymbol.Substring(0, futureTradingSymbol.Count - 3), instrumentType, tradingSymbol)
                    If strikePrice IsNot Nothing AndAlso IsNumeric(strikePrice) Then
                        If ret Is Nothing Then ret = New Dictionary(Of Decimal, String)
                        ret.Add(Val(strikePrice), tradingSymbol)
                    End If
                End If
                i += 1
            End While
        End If
        Return ret
    End Function
End Class
