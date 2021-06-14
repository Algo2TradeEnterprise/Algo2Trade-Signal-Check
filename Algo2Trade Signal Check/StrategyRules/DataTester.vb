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
        ret.Columns.Add("Instrument 1")
        ret.Columns.Add("Open 1")
        ret.Columns.Add("Low 1")
        ret.Columns.Add("High 1")
        ret.Columns.Add("Close 1")
        ret.Columns.Add("Instrument 2")
        ret.Columns.Add("Open 2")
        ret.Columns.Add("Low 2")
        ret.Columns.Add("High 2")
        ret.Columns.Add("Close 2")
        ret.Columns.Add("Ratio Open")
        ret.Columns.Add("Ratio Low")
        ret.Columns.Add("Ratio High")
        ret.Columns.Add("Ratio Close")
        'ret.Columns.Add("Fractal High")
        'ret.Columns.Add("Fractal Low")
        ret.Columns.Add("Bollinger High")
        ret.Columns.Add("Bollinger Low")
        ret.Columns.Add("SMA")

        Dim payload1 As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(_category, "AXISBANK", startDate.AddDays(-20), endDate)
        Dim payload2 As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(_category, "SBIN", startDate.AddDays(-20), endDate)

        If payload1 IsNot Nothing AndAlso payload1.Count > 0 AndAlso
            payload2 IsNot Nothing AndAlso payload2.Count > 0 Then
            Dim xMinPayload1 As Dictionary(Of Date, Payload) = Nothing
            Dim xMinPayload2 As Dictionary(Of Date, Payload) = Nothing
            If _timeFrame > 1 Then
                Dim exchangeStartTime As Date = New Date(Now.Year, Now.Month, Now.Day, 9, 15, 0)
                xMinPayload1 = Common.ConvertPayloadsToXMinutes(payload1, _timeFrame, exchangeStartTime)
                xMinPayload2 = Common.ConvertPayloadsToXMinutes(payload2, _timeFrame, exchangeStartTime)
            Else
                xMinPayload1 = payload1
                xMinPayload2 = payload2
            End If

            Dim ratioPayload As Dictionary(Of Date, Payload) = Nothing
            Dim previousPayload As Payload = Nothing
            For Each runningPayload In xMinPayload1.Keys
                _canceller.Token.ThrowIfCancellationRequested()
                If xMinPayload2.ContainsKey(runningPayload) Then
                    Dim candle1 As Payload = xMinPayload1(runningPayload)
                    Dim candle2 As Payload = xMinPayload2(runningPayload)

                    Dim candle As Payload = New Payload(Payload.CandleDataSource.Calculated) With {
                        .PayloadDate = runningPayload,
                        .Open = candle1.Open / candle2.Open,
                        .Low = candle1.Low / candle2.Low,
                        .High = candle1.High / candle2.High,
                        .Close = candle1.Close / candle2.Close,
                        .Volume = Math.Round(candle1.Volume / candle2.Volume, 0),
                        .PreviousCandlePayload = previousPayload
                    }

                    If ratioPayload Is Nothing Then ratioPayload = New Dictionary(Of Date, Payload)
                    ratioPayload.Add(candle.PayloadDate, candle)

                    previousPayload = candle
                End If
            Next

            If ratioPayload IsNot Nothing AndAlso ratioPayload.Count > 0 Then
                OnHeartbeat("Calculating Indicators")
                'Dim fractalHighPayload As Dictionary(Of Date, Decimal) = Nothing
                'Dim fractalLowPayload As Dictionary(Of Date, Decimal) = Nothing
                'Indicator.FractalBands.CalculateFractal(ratioPayload, fractalHighPayload, fractalLowPayload)

                Dim bollingerHighPayload As Dictionary(Of Date, Decimal) = Nothing
                Dim bollingerLowPayload As Dictionary(Of Date, Decimal) = Nothing
                Dim smaPayload As Dictionary(Of Date, Decimal) = Nothing
                Indicator.BollingerBands.CalculateBollingerBands(200, Payload.PayloadFields.Close, 2, ratioPayload, bollingerHighPayload, bollingerLowPayload, smaPayload)

                Dim lastSignal As Integer = 0
                Dim lastSignalCandle As Payload = Nothing
                Dim lastFavourableFractalCandle As Payload = Nothing
                For Each runningPayload In ratioPayload
                    _canceller.Token.ThrowIfCancellationRequested()
                    'If runningPayload.Key.Date >= startDate.Date Then
                    Dim row As DataRow = ret.NewRow
                    row("Date") = runningPayload.Key.ToString("dd-MMM-yyyy HH:mm:ss")
                    row("Instrument 1") = xMinPayload1(runningPayload.Key).TradingSymbol
                    row("Open 1") = xMinPayload1(runningPayload.Key).Open
                    row("Low 1") = xMinPayload1(runningPayload.Key).Low
                    row("High 1") = xMinPayload1(runningPayload.Key).High
                    row("Close 1") = xMinPayload1(runningPayload.Key).Close
                    row("Instrument 2") = xMinPayload2(runningPayload.Key).TradingSymbol
                    row("Open 2") = xMinPayload2(runningPayload.Key).Open
                    row("Low 2") = xMinPayload2(runningPayload.Key).Low
                    row("High 2") = xMinPayload2(runningPayload.Key).High
                    row("Close 2") = xMinPayload2(runningPayload.Key).Close
                    row("Ratio Open") = Math.Round(ratioPayload(runningPayload.Key).Open, 4)
                    row("Ratio Low") = Math.Round(ratioPayload(runningPayload.Key).Low, 4)
                    row("Ratio High") = Math.Round(ratioPayload(runningPayload.Key).High, 4)
                    row("Ratio Close") = Math.Round(ratioPayload(runningPayload.Key).Close, 4)
                    'row("Fractal High") = Math.Round(fractalHighPayload(runningPayload.Key), 4)
                    'row("Fractal Low") = Math.Round(fractalLowPayload(runningPayload.Key), 4)
                    row("Bollinger High") = Math.Round(bollingerHighPayload(runningPayload.Key), 4)
                    row("Bollinger Low") = Math.Round(bollingerLowPayload(runningPayload.Key), 4)
                    row("SMA") = Math.Round(smaPayload(runningPayload.Key), 4)
                    ret.Rows.Add(row)




                    'If lastSignalCandle IsNot Nothing Then
                    '    If lastSignal = 1 Then
                    '        If lastFavourableFractalCandle IsNot Nothing Then
                    '            If runningPayload.Value.High > fractalHighPayload(lastFavourableFractalCandle.PayloadDate) Then
                    '                Dim leftTgt As Decimal = ((smaPayload(runningPayload.Key) - fractalHighPayload(lastFavourableFractalCandle.PayloadDate)) / fractalHighPayload(lastFavourableFractalCandle.PayloadDate)) * 100
                    '                If leftTgt >= 2 Then
                    '                    Dim row As DataRow = ret.NewRow
                    '                    row("Date") = runningPayload.Key.ToString("dd-MMM-yyyy HH:mm:ss")
                    '                    row("Signal") = "BUY"
                    '                    row("Left %") = Math.Round(leftTgt, 2)
                    '                    ret.Rows.Add(row)
                    '                End If

                    '                lastSignal = 0
                    '                lastSignalCandle = Nothing
                    '                lastFavourableFractalCandle = Nothing
                    '            End If
                    '        End If
                    '        If fractalHighPayload(runningPayload.Key) < fractalHighPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate) Then
                    '            lastFavourableFractalCandle = runningPayload.Value
                    '        End If
                    '    ElseIf lastSignal = -1 Then
                    '        If lastFavourableFractalCandle IsNot Nothing Then
                    '            If runningPayload.Value.Low < fractalLowPayload(lastFavourableFractalCandle.PayloadDate) Then
                    '                Dim leftTgt As Decimal = ((fractalLowPayload(lastFavourableFractalCandle.PayloadDate) - smaPayload(runningPayload.Key)) / smaPayload(runningPayload.Key)) * 100
                    '                If leftTgt >= 2 Then
                    '                    Dim row As DataRow = ret.NewRow
                    '                    row("Date") = runningPayload.Key.ToString("dd-MMM-yyyy HH:mm:ss")
                    '                    row("Signal") = "SELL"
                    '                    row("Left %") = Math.Round(leftTgt, 2)
                    '                    ret.Rows.Add(row)
                    '                End If

                    '                lastSignal = 0
                    '                lastSignalCandle = Nothing
                    '                lastFavourableFractalCandle = Nothing
                    '            End If
                    '        End If
                    '        If fractalLowPayload(runningPayload.Key) > fractalLowPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate) Then
                    '            lastFavourableFractalCandle = runningPayload.Value
                    '        End If
                    '    End If
                    'Else
                    '    If runningPayload.Value.Close > smaPayload(runningPayload.Key) Then
                    '        If runningPayload.Value.High >= bollingerHighPayload(runningPayload.Key) OrElse
                    '            fractalHighPayload(runningPayload.Key) >= bollingerHighPayload(runningPayload.Key) Then
                    '            lastSignal = -1
                    '            lastSignalCandle = runningPayload.Value
                    '        End If
                    '    ElseIf runningPayload.Value.Close < smaPayload(runningPayload.Key) Then
                    '        If runningPayload.Value.Low <= bollingerLowPayload(runningPayload.Key) OrElse
                    '            fractalLowPayload(runningPayload.Key) <= bollingerLowPayload(runningPayload.Key) Then
                    '            lastSignal = 1
                    '            lastSignalCandle = runningPayload.Value
                    '        End If
                    '    End If
                    'End If
                    'End If
                Next
            End If
        End If
        Return ret
    End Function

End Class