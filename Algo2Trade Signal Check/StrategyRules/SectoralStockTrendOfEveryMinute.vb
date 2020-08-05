Imports Algo2TradeBLL
Imports System.IO
Imports System.Threading
Imports Utilities.DAL
Imports Utilities.Network

Public Class SectoralStockTrendOfEveryMinute
    Inherits Rule
    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal stockCategory As Integer, ByVal timeFrame As Integer, ByVal useHA As Boolean, ByVal stockName As String, ByVal fileName As String)
        MyBase.New(canceller, stockCategory, timeFrame, useHA, stockName, fileName)
    End Sub
    Public Overrides Async Function RunAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Up Trend %")
        ret.Columns.Add("Down Trend %")

        Dim stockData As StockSelection = New StockSelection(_canceller, _category, _cmn, _fileName)
        AddHandler stockData.Heartbeat, AddressOf OnHeartbeat
        AddHandler stockData.WaitingFor, AddressOf OnWaitingFor
        AddHandler stockData.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
        AddHandler stockData.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
        Dim chkDate As Date = startDate
        While chkDate <= endDate
            _canceller.Token.ThrowIfCancellationRequested()
            Dim sectorName As String = _instrumentName

            Dim stockList As List(Of String) = Await GetSectoralStocklist(sectorName).ConfigureAwait(False)
            _canceller.Token.ThrowIfCancellationRequested()
            If stockList IsNot Nothing AndAlso stockList.Count > 0 Then
                Dim totalData As Dictionary(Of Date, Dictionary(Of String, Tuple(Of Payload, Decimal))) = Nothing
                For Each stock In stockList
                    _canceller.Token.ThrowIfCancellationRequested()

                    ret.Columns.Add(String.Format("{0} Close", stock.ToUpper))
                    ret.Columns.Add(String.Format("{0} VWAP", stock.ToUpper))

                    Dim stockPayload As Dictionary(Of Date, Payload) = Nothing
                    Select Case _category
                        Case Common.DataBaseTable.Intraday_Cash, Common.DataBaseTable.Intraday_Commodity, Common.DataBaseTable.Intraday_Currency, Common.DataBaseTable.Intraday_Futures
                            stockPayload = _cmn.GetRawPayload(_category, stock, chkDate.AddDays(-8), chkDate)
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
                            Dim vwapPayload As Dictionary(Of Date, Decimal) = Nothing
                            Indicator.VWAP.CalculateVWAP(inputPayload, vwapPayload)

                            For Each runningPayload In currentDayPayload
                                _canceller.Token.ThrowIfCancellationRequested()
                                If totalData Is Nothing Then totalData = New Dictionary(Of Date, Dictionary(Of String, Tuple(Of Payload, Decimal)))
                                If Not totalData.ContainsKey(runningPayload.Key) Then
                                    totalData.Add(runningPayload.Key, New Dictionary(Of String, Tuple(Of Payload, Decimal)))
                                End If
                                If Not totalData(runningPayload.Key).ContainsKey(stock) Then
                                    totalData(runningPayload.Key).Add(stock, New Tuple(Of Payload, Decimal)(runningPayload.Value, vwapPayload(runningPayload.Key)))
                                End If
                            Next
                        End If
                    End If
                Next
                If totalData IsNot Nothing AndAlso totalData.Count > 0 Then
                    For Each runningData In totalData
                        Dim row As DataRow = ret.NewRow
                        row("Date") = runningData.Key.ToString("dd-MMM-yyyy HH:mm:ss")

                        If runningData.Value IsNot Nothing AndAlso runningData.Value.Count > 0 Then
                            Dim upTrend As Integer = 0
                            Dim downTrend As Integer = 0
                            For Each runningStock In runningData.Value
                                If runningStock.Value.Item1.Close > runningStock.Value.Item2 Then
                                    upTrend += 1
                                ElseIf runningStock.Value.Item1.Close < runningStock.Value.Item2 Then
                                    downTrend += 1
                                End If
                                row(String.Format("{0} Close", runningStock.Key.ToUpper)) = runningStock.Value.Item1.Close
                                row(String.Format("{0} VWAP", runningStock.Key.ToUpper)) = runningStock.Value.Item2
                            Next
                            row("Up Trend %") = Math.Round((upTrend / stockList.Count) * 100, 2)
                            row("Down Trend %") = Math.Round((downTrend / stockList.Count) * 100, 2)
                        End If
                        ret.Rows.Add(row)
                    Next
                End If
            End If
            chkDate = chkDate.AddDays(1)
        End While
        Return ret
    End Function

    Private Function GetSectorURL(ByVal sectorName As String) As String
        Dim ret As String = Nothing
        Select Case sectorName.Trim.ToUpper
            Case "NIFTY50"
                ret = "https://www1.nseindia.com/content/indices/ind_nifty50list.csv"
            Case "NIFTYNEXT50"
                ret = "https://www1.nseindia.com/content/indices/ind_niftynext50list.csv"
            Case "NIFTY100"
                ret = "https://www1.nseindia.com/content/indices/ind_nifty100list.csv"
            Case "NIFTY200"
                ret = "https://www1.nseindia.com/content/indices/ind_nifty200list.csv"
            Case "NIFTYAUTO"
                ret = "https://www1.nseindia.com/content/indices/ind_niftyautolist.csv"
            Case "NIFTYBANK"
                ret = "https://www1.nseindia.com/content/indices/ind_niftybanklist.csv"
            Case "NIFTYENERGY"
                ret = "https://www1.nseindia.com/content/indices/ind_niftyenergylist.csv"
            Case "NIFTYFINSERV"
                ret = "https://www1.nseindia.com/content/indices/ind_niftyfinancelist.csv"
            Case "NIFTYFMCG"
                ret = "https://www1.nseindia.com/content/indices/ind_niftyfmcglist.csv"
            Case "NIFTYIT"
                ret = "https://www1.nseindia.com/content/indices/ind_niftyitlist.csv"
            Case "NIFTYMEDIA"
                ret = "https://www1.nseindia.com/content/indices/ind_niftymedialist.csv"
            Case "NIFTYMETAL"
                ret = "https://www1.nseindia.com/content/indices/ind_niftymetallist.csv"
            Case "NIFTYMNC"
                ret = "https://www1.nseindia.com/content/indices/ind_niftymnclist.csv"
            Case "NIFTYPHARMA"
                ret = "https://www1.nseindia.com/content/indices/ind_niftypharmalist.csv"
            Case "NIFTYPSUBANK"
                ret = "https://www1.nseindia.com/content/indices/ind_niftypsubanklist.csv"
            Case "NIFTYREALTY"
                ret = "https://www1.nseindia.com/content/indices/ind_niftyrealtylist.csv"
            Case "NIFTYCOMMODITIES"
                ret = "https://www1.nseindia.com/content/indices/ind_niftycommoditieslist.csv"
            Case "NIFTYCONSUMPTION"
                ret = "https://www1.nseindia.com/content/indices/ind_niftyconsumptionlist.csv"
            Case "NIFTYINFRA"
                ret = "https://www1.nseindia.com/content/indices/ind_niftyinfralist.csv"
            Case "NIFTYPSE"
                ret = "https://www1.nseindia.com/content/indices/ind_niftypselist.csv"
            Case "NIFTYSERVSECTOR"
                ret = "https://www1.nseindia.com/content/indices/ind_niftyservicelist.csv"
            Case "NIFTYPVTBANK"
                ret = "https://www1.nseindia.com/content/indices/ind_nifty_privatebanklist.csv"
            Case Else
                Throw New NotImplementedException
        End Select
        Return ret
    End Function

    Private Async Function GetSectorStockFileAsync(ByVal sectorName As String, ByVal filename As String) As Task(Of Boolean)
        Dim ret As Boolean = False
        Using browser As New HttpBrowser(Nothing, Net.DecompressionMethods.GZip, TimeSpan.FromSeconds(30), _canceller)
            AddHandler browser.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
            AddHandler browser.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
            AddHandler browser.WaitingFor, AddressOf OnWaitingFor
            AddHandler browser.Heartbeat, AddressOf OnHeartbeat

            browser.KeepAlive = True
            Dim headersToBeSent As New Dictionary(Of String, String)
            headersToBeSent.Add("Host", "www1.nseindia.com")
            headersToBeSent.Add("Upgrade-Insecure-Requests", "1")
            headersToBeSent.Add("Sec-Fetch-Mode", "navigate")
            headersToBeSent.Add("Sec-Fetch-Site", "none")

            Dim targetURL As String = GetSectorURL(sectorName)
            If targetURL IsNot Nothing Then
                ret = Await browser.GetFileAsync(targetURL, filename, False, headersToBeSent).ConfigureAwait(False)
            End If
        End Using
        Return ret
    End Function

    Private Async Function GetSectoralStocklist(ByVal sectorName As String) As Task(Of List(Of String))
        Dim ret As List(Of String) = Nothing
        Dim filename As String = Path.Combine(My.Application.Info.DirectoryPath, String.Format("Sectoral Index {0}.csv", sectorName))
        Dim fileAvailable As Boolean = Await GetSectorStockFileAsync(sectorName, filename).ConfigureAwait(False)
        If fileAvailable AndAlso File.Exists(filename) Then
            OnHeartbeat("Reading stock file")
            Dim dt As DataTable = Nothing
            Using csv As New CSVHelper(filename, ",", _canceller)
                'AddHandler csv.Heartbeat, AddressOf OnHeartbeat
                dt = csv.GetDataTableFromCSV(0)
            End Using
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                For i = 0 To dt.Rows.Count - 1
                    If ret Is Nothing Then ret = New List(Of String)
                    ret.Add(dt.Rows(i).Item("Symbol").ToString.ToUpper)
                Next
            End If
        End If
        If File.Exists(filename) Then File.Delete(filename)
        Return ret
    End Function
End Class
