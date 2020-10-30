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
        ret.Columns.Add("Close 1")
        ret.Columns.Add("Trading Symbol 2")
        ret.Columns.Add("Close 2")
        ret.Columns.Add("Trading Symbol 3")
        ret.Columns.Add("Close 3")
        ret.Columns.Add("Trading Symbol 4")
        ret.Columns.Add("Close 4")

        Dim stockData As StockSelection = New StockSelection(_canceller, _category, _cmn, _fileName)
        AddHandler stockData.Heartbeat, AddressOf OnHeartbeat
        AddHandler stockData.WaitingFor, AddressOf OnWaitingFor
        AddHandler stockData.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
        AddHandler stockData.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
        _canceller.Token.ThrowIfCancellationRequested()


        Dim spotPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.EOD_Cash, "NIFTY 50", startDate, startDate)
        If spotPayload IsNot Nothing AndAlso spotPayload.Count > 0 Then
            Dim closePrice As Decimal = spotPayload.LastOrDefault.Value.Close
            Dim lowerStrikePrice As Decimal = Math.Floor(closePrice / 50) * 50
            Dim upperStrikePrice As Decimal = Math.Ceiling(closePrice / 50) * 50
            Dim strikePrice As Decimal = Decimal.MinValue
            If upperStrikePrice Mod 100 = 0 Then
                strikePrice = upperStrikePrice
            Else
                strikePrice = lowerStrikePrice
            End If

            Dim spotIntradayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.Intraday_Cash, "NIFTY 50", startDate, endDate)
            Dim strikeCEPayload As Dictionary(Of Date, Payload) = Await GetOptionStockData(endDate.Date, "NIFTY", strikePrice, "CE", startDate, endDate).ConfigureAwait(False)
            Dim strikePEPayload As Dictionary(Of Date, Payload) = Await GetOptionStockData(endDate.Date, "NIFTY", strikePrice, "PE", startDate, endDate).ConfigureAwait(False)
            Dim aboveStrikeCEPayload As Dictionary(Of Date, Payload) = Await GetOptionStockData(endDate.Date, "NIFTY", strikePrice + 500, "CE", startDate, endDate).ConfigureAwait(False)
            Dim belowStrikePEPayload As Dictionary(Of Date, Payload) = Await GetOptionStockData(endDate.Date, "NIFTY", strikePrice - 500, "PE", startDate, endDate).ConfigureAwait(False)
            If spotIntradayPayload IsNot Nothing AndAlso spotIntradayPayload.Count > 0 AndAlso
                strikeCEPayload IsNot Nothing AndAlso strikeCEPayload.Count > 0 AndAlso
                strikePEPayload IsNot Nothing AndAlso strikePEPayload.Count > 0 AndAlso
                aboveStrikeCEPayload IsNot Nothing AndAlso aboveStrikeCEPayload.Count > 0 AndAlso
                belowStrikePEPayload IsNot Nothing AndAlso belowStrikePEPayload.Count > 0 Then
                Dim dateList As List(Of Date) = spotIntradayPayload.Keys.ToList
                Dim preDate As Date = Date.MinValue
                For Each runningDate In dateList
                    _canceller.Token.ThrowIfCancellationRequested()
                    If Not strikeCEPayload.ContainsKey(runningDate) Then
                        If preDate <> Date.MinValue AndAlso strikeCEPayload.ContainsKey(preDate) Then
                            strikeCEPayload.Add(runningDate, strikeCEPayload(preDate))
                        End If
                    End If
                    If Not strikePEPayload.ContainsKey(runningDate) Then
                        If preDate <> Date.MinValue AndAlso strikePEPayload.ContainsKey(preDate) Then
                            strikePEPayload.Add(runningDate, strikePEPayload(preDate))
                        End If
                    End If
                    If Not aboveStrikeCEPayload.ContainsKey(runningDate) Then
                        If preDate <> Date.MinValue AndAlso aboveStrikeCEPayload.ContainsKey(preDate) Then
                            aboveStrikeCEPayload.Add(runningDate, aboveStrikeCEPayload(preDate))
                        End If
                    End If
                    If Not belowStrikePEPayload.ContainsKey(runningDate) Then
                        If preDate <> Date.MinValue AndAlso belowStrikePEPayload.ContainsKey(preDate) Then
                            belowStrikePEPayload.Add(runningDate, belowStrikePEPayload(preDate))
                        End If
                    End If

                    preDate = runningDate
                Next


                For Each runningDate In dateList
                    _canceller.Token.ThrowIfCancellationRequested()
                    Dim row As DataRow = ret.NewRow
                    row("Date") = runningDate.ToString("dd-MMM-yyyy HH:mm:ss")
                    If strikeCEPayload.ContainsKey(runningDate) Then
                        row("Trading Symbol 1") = strikeCEPayload(runningDate).TradingSymbol
                        row("Close 1") = strikeCEPayload(runningDate).Close
                    Else
                        row("Trading Symbol 1") = ""
                        row("Close 1") = ""
                    End If
                    If strikePEPayload.ContainsKey(runningDate) Then
                        row("Trading Symbol 2") = strikePEPayload(runningDate).TradingSymbol
                        row("Close 2") = strikePEPayload(runningDate).Close
                    Else
                        row("Trading Symbol 2") = ""
                        row("Close 2") = ""
                    End If
                    If aboveStrikeCEPayload.ContainsKey(runningDate) Then
                        row("Trading Symbol 3") = aboveStrikeCEPayload(runningDate).TradingSymbol
                        row("Close 3") = aboveStrikeCEPayload(runningDate).Close
                    Else
                        row("Trading Symbol 3") = ""
                        row("Close 3") = ""
                    End If
                    If belowStrikePEPayload.ContainsKey(runningDate) Then
                        row("Trading Symbol 4") = belowStrikePEPayload(runningDate).TradingSymbol
                        row("Close 4") = belowStrikePEPayload(runningDate).Close
                    Else
                        row("Trading Symbol 4") = ""
                        row("Close 4") = ""
                    End If
                    ret.Rows.Add(row)
                Next
            End If
        End If
        Return ret
    End Function

    Private Async Function GetOptionStockData(ByVal expiry As Date, ByVal rawInstrumentName As String, ByVal strikePrice As Decimal, ByVal optionType As String, ByVal startDate As Date, ByVal endDate As Date) As Task(Of Dictionary(Of Date, Payload))
        Dim ret As Dictionary(Of Date, Payload) = Nothing
        Dim tradingSymbol As String = ""
        Dim lastDayOfTheMonth As Date = New Date(expiry.Year, expiry.Month, Date.DaysInMonth(expiry.Year, expiry.Month))
        Dim lastThursDayOfTheMonth As Date = lastDayOfTheMonth
        While lastThursDayOfTheMonth.DayOfWeek <> DayOfWeek.Thursday
            lastThursDayOfTheMonth = lastThursDayOfTheMonth.AddDays(-1)
        End While
        If expiry = lastThursDayOfTheMonth Then
            tradingSymbol = String.Format("{0}{1}{2}{3}", rawInstrumentName.ToUpper, expiry.ToString("yyMMM"), strikePrice, optionType.ToUpper)
        Else
            Dim dateString As String = ""
            If expiry.Month > 9 Then
                dateString = String.Format("{0}{1}{2}", expiry.ToString("yy"), Microsoft.VisualBasic.Left(expiry.ToString("MMM"), 1), expiry.ToString("dd"))
            Else
                dateString = expiry.ToString("yyMdd")
            End If
            tradingSymbol = String.Format("{0}{1}{2}{3}", rawInstrumentName.ToUpper, dateString, strikePrice, optionType.ToUpper)
        End If

        Dim queryString As String = String.Format("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDateTime`,`TradingSymbol`,`OI`
                                                   FROM `intraday_prices_opt_futures`
                                                   WHERE `TradingSymbol`='{0}'
                                                   AND `SnapshotDate`>='{1}' 
                                                   AND `SnapshotDate`<='{2}'",
                                                   tradingSymbol, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"))

        Dim dt As DataTable = Await _cmn.RunSelectAsync(queryString).ConfigureAwait(False)
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim i As Integer = 0
            ret = New Dictionary(Of Date, Payload)
            While Not i = dt.Rows.Count()
                Dim tempPayload As Payload = New Payload(Payload.CandleDataSource.Chart)
                tempPayload.Open = dt.Rows(i).Item(0)
                tempPayload.Low = dt.Rows(i).Item(1)
                tempPayload.High = dt.Rows(i).Item(2)
                tempPayload.Close = dt.Rows(i).Item(3)
                tempPayload.Volume = dt.Rows(i).Item(4)
                tempPayload.PayloadDate = dt.Rows(i).Item(5)
                tempPayload.TradingSymbol = dt.Rows(i).Item(6)

                ret.Add(tempPayload.PayloadDate, tempPayload)
                i += 1
            End While
        End If
        Return ret
    End Function
End Class
