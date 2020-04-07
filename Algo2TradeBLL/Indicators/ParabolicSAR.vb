Imports System.Drawing

Namespace Indicator
    Public Module ParabolicSAR
        Public Sub CalculatePSAR(ByVal minimumAF As Decimal, ByVal maximumAF As Decimal, ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputPSARPayload As Dictionary(Of Date, Decimal), ByRef outputTrendPayload As Dictionary(Of Date, Color))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                Dim previousTrend As Decimal = 0
                Dim previousPSAR As Decimal = 0
                Dim previousEP As Decimal = 0
                Dim previousEPSAR As Decimal = 0
                Dim previousAF As Decimal = 0
                Dim previousAFDiff As Decimal = 0
                For Each runningPayload In inputPayload.Keys
                    Dim trend As Decimal = 0
                    Dim PSAR As Decimal = 0
                    Dim EP As Decimal = 0
                    Dim EPSAR As Decimal = 0
                    Dim AF As Decimal = 0
                    Dim AFDiff As Decimal = 0
                    If inputPayload(runningPayload).PreviousCandlePayload IsNot Nothing AndAlso
                        inputPayload(runningPayload).PreviousCandlePayload.PreviousCandlePayload IsNot Nothing AndAlso
                        inputPayload(runningPayload).PreviousCandlePayload.PreviousCandlePayload.PreviousCandlePayload IsNot Nothing Then
                        If previousTrend > 0 AndAlso previousPSAR + previousAFDiff > inputPayload(runningPayload).Low Then
                            PSAR = previousEP
                        ElseIf previousTrend < 0 AndAlso previousPSAR + previousAFDiff < inputPayload(runningPayload).High Then
                            PSAR = previousEP
                        Else
                            PSAR = previousPSAR + previousAFDiff
                        End If
                        If PSAR < inputPayload(runningPayload).High Then
                            trend = 1
                        ElseIf PSAR > inputPayload(runningPayload).Low Then
                            trend = -1
                        End If
                        If trend > 0 AndAlso inputPayload(runningPayload).High > previousEP Then
                            EP = inputPayload(runningPayload).High
                        ElseIf trend > 0 AndAlso inputPayload(runningPayload).High <= previousEP Then
                            EP = previousEP
                        ElseIf trend < 0 AndAlso inputPayload(runningPayload).Low < previousEP Then
                            EP = inputPayload(runningPayload).Low
                        Else
                            EP = previousEP
                        End If
                        EPSAR = EP - PSAR

                        Dim tentativeAF As Decimal = minimumAF
                        If trend = previousTrend Then
                            If previousAF = maximumAF Then
                                tentativeAF = maximumAF
                            ElseIf trend > 0 AndAlso EP > previousEP Then
                                tentativeAF = previousAF + minimumAF
                            ElseIf trend > 0 AndAlso EP <= previousEP Then
                                tentativeAF = previousAF
                            ElseIf trend < 0 AndAlso EP < previousEP Then
                                tentativeAF = previousAF + minimumAF
                            ElseIf trend < 0 AndAlso EP >= previousEP Then
                                tentativeAF = previousAF
                            End If
                        Else
                            tentativeAF = minimumAF
                        End If
                        If tentativeAF > maximumAF Then
                            AF = maximumAF
                        Else
                            AF = tentativeAF
                        End If

                        AFDiff = AF * EPSAR
                    ElseIf inputPayload(runningPayload).PreviousCandlePayload IsNot Nothing AndAlso
                        inputPayload(runningPayload).PreviousCandlePayload.PreviousCandlePayload IsNot Nothing Then
                        PSAR = Math.Min(inputPayload(runningPayload).Low, Math.Min(inputPayload(runningPayload).PreviousCandlePayload.Low, inputPayload(runningPayload).PreviousCandlePayload.PreviousCandlePayload.Low))
                        trend = 1
                        EP = inputPayload(runningPayload).High
                        EPSAR = EP - PSAR
                        AF = minimumAF
                        AFDiff = AF * EPSAR
                    End If

                    previousTrend = trend
                    previousPSAR = PSAR
                    previousEP = EP
                    previousEPSAR = EPSAR
                    previousAF = AF
                    previousAFDiff = AFDiff

                    If outputPSARPayload Is Nothing Then outputPSARPayload = New Dictionary(Of Date, Decimal)
                    outputPSARPayload.Add(runningPayload, PSAR)
                    If outputTrendPayload Is Nothing Then outputTrendPayload = New Dictionary(Of Date, Color)
                    outputTrendPayload.Add(runningPayload, If(trend < 0, Color.Red, If(trend > 0, Color.Green, Color.White)))
                Next
            End If
        End Sub
    End Module
End Namespace
