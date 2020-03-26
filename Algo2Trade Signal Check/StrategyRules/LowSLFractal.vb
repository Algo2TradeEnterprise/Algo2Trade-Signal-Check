Imports Algo2TradeBLL
Imports System.Threading
Public Class LowSLFractal
    Inherits Rule

    Private ReadOnly _atrMultiplier As Decimal

    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal stockCategory As Integer, ByVal timeFrame As Integer, ByVal useHA As Boolean, ByVal stockName As String, ByVal fileName As String, ByVal atrMultiplier As Decimal)
        MyBase.New(canceller, stockCategory, timeFrame, useHA, stockName, fileName)
        _atrMultiplier = atrMultiplier
    End Sub
    Public Overrides Async Function RunAsync(startDate As Date, endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Instrument")
        ret.Columns.Add("Quantity")
        ret.Columns.Add("Stoploss")
        ret.Columns.Add("Fractal Range")
        ret.Columns.Add("ATR")
        ret.Columns.Add("Multiplier")

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
                    End Select
                    _canceller.Token.ThrowIfCancellationRequested()
                    If stockPayload IsNot Nothing AndAlso stockPayload.Count > 0 Then
                        OnHeartbeat("Processing Data")
                        Dim XMinutePayload As Dictionary(Of Date, Payload) = Nothing
                        If _timeFrame > 1 Then
                            Dim exchangeStartTime As Date = New Date(chkDate.Year, chkDate.Month, chkDate.Day, 9, 15, 0)
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
                        Dim fractalHighPayload As Dictionary(Of Date, Decimal) = Nothing
                        Dim fractalLowPayload As Dictionary(Of Date, Decimal) = Nothing
                        Indicator.FractalBands.CalculateFractal(inputPayload, fractalHighPayload, fractalLowPayload)
                        Dim atrPayload As Dictionary(Of Date, Decimal) = Nothing
                        Indicator.ATR.CalculateATR(14, inputPayload, atrPayload)
                        If currentDayPayload IsNot Nothing AndAlso currentDayPayload.Count > 0 Then
                            Dim lotSize As Integer = _cmn.GetLotSize(_category, currentDayPayload.FirstOrDefault.Value.TradingSymbol, currentDayPayload.FirstOrDefault.Key.Date)

                            Dim preFractalHigh As Decimal = 0
                            Dim preFractalLow As Decimal = 0
                            For Each runningPayload In currentDayPayload.Keys
                                _canceller.Token.ThrowIfCancellationRequested()
                                Dim fractalHigh As Decimal = fractalHighPayload(runningPayload)
                                Dim fractalLow As Decimal = fractalLowPayload(runningPayload)
                                If preFractalHigh <> fractalHigh OrElse preFractalLow <> fractalLow Then
                                    If currentDayPayload(runningPayload).Close > fractalLow AndAlso currentDayPayload(runningPayload).Close < fractalHigh Then
                                        Dim slPoint As Decimal = Math.Abs(fractalHigh - fractalLow)
                                        Dim pl As Decimal = CalculatePL(currentDayPayload(runningPayload).TradingSymbol, currentDayPayload(runningPayload).High, currentDayPayload(runningPayload).High - slPoint, lotSize, lotSize)
                                        'If Math.Abs(pl) >= Math.Abs(_MinSLAmount) AndAlso Math.Abs(pl) <= Math.Abs(_MaxSLAmount) Then
                                        Dim multiplier As Decimal = slPoint / atrPayload(runningPayload)
                                        If multiplier <= _atrMultiplier Then
                                            Dim row As DataRow = ret.NewRow
                                            row("Date") = runningPayload
                                            row("Instrument") = currentDayPayload(runningPayload).TradingSymbol
                                            row("Quantity") = lotSize
                                            row("Stoploss") = pl
                                            row("Fractal Range") = slPoint
                                            row("ATR") = Math.Round(atrPayload(runningPayload), 4)
                                            row("Multiplier") = Math.Round(multiplier, 4)
                                            ret.Rows.Add(row)

                                            preFractalHigh = fractalHigh
                                            preFractalLow = fractalLow
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

    Private Function CalculatePL(ByVal stockName As String, ByVal buyPrice As Decimal, ByVal sellPrice As Decimal, ByVal quantity As Integer, ByVal lotSize As Integer) As Decimal
        Dim potentialBrokerage As New Calculator.BrokerageAttributes
        Dim calculator As New Calculator.BrokerageCalculator(_canceller)

        Select Case _category
            Case Common.DataBaseTable.EOD_Cash, Common.DataBaseTable.Intraday_Cash
                calculator.Intraday_Equity(buyPrice, sellPrice, quantity, potentialBrokerage)
            Case Common.DataBaseTable.EOD_Commodity, Common.DataBaseTable.Intraday_Commodity
                calculator.Commodity_MCX(stockName, buyPrice, sellPrice, quantity / lotSize, potentialBrokerage)
            Case Common.DataBaseTable.EOD_Currency, Common.DataBaseTable.Intraday_Currency
                calculator.Currency_Futures(buyPrice, sellPrice, quantity / lotSize, potentialBrokerage)
            Case Common.DataBaseTable.EOD_Futures, Common.DataBaseTable.Intraday_Futures
                calculator.FO_Futures(buyPrice, sellPrice, quantity, potentialBrokerage)
            Case Common.DataBaseTable.EOD_POSITIONAL
                calculator.Delivery_Equity(buyPrice, sellPrice, quantity, potentialBrokerage)
        End Select

        Return potentialBrokerage.NetProfitLoss
    End Function
End Class
