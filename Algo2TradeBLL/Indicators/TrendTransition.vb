Namespace Indicator
    Public Module TrendTransition
        ''' <summary>
        ''' Transition of trend when candle closes above the lowest high for an existing downtrend getting converted to uptrend and vice versa.
        ''' Output denotes '0':No Trend, '1':Up Trend, '-1':Down Trend
        ''' </summary>
        ''' <param name="inputPayload"></param>
        ''' <param name="outputPayload"></param>
        Public Sub CalculateTrendTransition(ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputPayload As Dictionary(Of Date, Integer))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                Dim runningTrend As Integer = 0
                Dim highestLow As Decimal = Decimal.MinValue
                Dim lowestHigh As Decimal = Decimal.MaxValue
                For Each runningPayload In inputPayload
                    If runningTrend = 0 Then
                        If runningPayload.Value.Close <= highestLow Then
                            runningTrend = -1
                            highestLow = Decimal.MinValue
                            lowestHigh = runningPayload.Value.High
                        ElseIf runningPayload.Value.Close >= lowestHigh Then
                            runningTrend = 1
                            lowestHigh = Decimal.MaxValue
                            highestLow = runningPayload.Value.Low
                        Else
                            highestLow = Math.Max(highestLow, runningPayload.Value.Low)
                            lowestHigh = Math.Min(lowestHigh, runningPayload.Value.High)
                        End If
                    ElseIf runningTrend = 1 Then
                        If runningPayload.Value.Close <= highestLow Then
                            runningTrend = -1
                            highestLow = Decimal.MinValue
                            lowestHigh = runningPayload.Value.High
                        Else
                            highestLow = Math.Max(highestLow, runningPayload.Value.Low)
                        End If
                    ElseIf runningTrend = -1 Then
                        If runningPayload.Value.Close >= lowestHigh Then
                            runningTrend = 1
                            lowestHigh = Decimal.MaxValue
                            highestLow = runningPayload.Value.Low
                        Else
                            lowestHigh = Math.Min(lowestHigh, runningPayload.Value.High)
                        End If
                    End If

                    If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, Integer)
                    outputPayload.Add(runningPayload.Key, runningTrend)
                Next
            End If
        End Sub
    End Module
End Namespace