Imports System.Drawing

Namespace Indicator
    Public Module ParabolicSAR
        'Public Sub CalculatePSAR(ByVal minimumAF As Decimal, ByVal maximumAF As Decimal, ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputPSARPayload As Dictionary(Of Date, Decimal), ByRef outputTrendPayload As Dictionary(Of Date, Color))
        '    If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
        '        Dim previousTrend As Decimal = 0
        '        Dim previousPSAR As Decimal = 0
        '        Dim previousEP As Decimal = 0
        '        Dim previousEPSAR As Decimal = 0
        '        Dim previousAF As Decimal = 0
        '        Dim previousAFDiff As Decimal = 0
        '        For Each runningPayload In inputPayload.Keys
        '            Dim trend As Decimal = 0
        '            Dim PSAR As Decimal = 0
        '            Dim EP As Decimal = 0
        '            Dim EPSAR As Decimal = 0
        '            Dim AF As Decimal = 0
        '            Dim AFDiff As Decimal = 0
        '            If inputPayload(runningPayload).PreviousCandlePayload IsNot Nothing AndAlso
        '                inputPayload(runningPayload).PreviousCandlePayload.PreviousCandlePayload IsNot Nothing AndAlso
        '                inputPayload(runningPayload).PreviousCandlePayload.PreviousCandlePayload.PreviousCandlePayload IsNot Nothing Then
        '                If previousTrend > 0 AndAlso previousPSAR + previousAFDiff > inputPayload(runningPayload).Low Then
        '                    PSAR = previousEP
        '                ElseIf previousTrend < 0 AndAlso previousPSAR + previousAFDiff < inputPayload(runningPayload).High Then
        '                    PSAR = previousEP
        '                Else
        '                    PSAR = previousPSAR + previousAFDiff
        '                End If
        '                If PSAR < inputPayload(runningPayload).High Then
        '                    trend = 1
        '                ElseIf PSAR > inputPayload(runningPayload).Low Then
        '                    trend = -1
        '                End If
        '                If trend > 0 AndAlso inputPayload(runningPayload).High > previousEP Then
        '                    EP = inputPayload(runningPayload).High
        '                ElseIf trend > 0 AndAlso inputPayload(runningPayload).High <= previousEP Then
        '                    EP = previousEP
        '                ElseIf trend < 0 AndAlso inputPayload(runningPayload).Low < previousEP Then
        '                    EP = inputPayload(runningPayload).Low
        '                Else
        '                    EP = previousEP
        '                End If
        '                EPSAR = EP - PSAR

        '                Dim tentativeAF As Decimal = minimumAF
        '                If trend = previousTrend Then
        '                    If previousAF = maximumAF Then
        '                        tentativeAF = maximumAF
        '                    ElseIf trend > 0 AndAlso EP > previousEP Then
        '                        tentativeAF = previousAF + minimumAF
        '                    ElseIf trend > 0 AndAlso EP <= previousEP Then
        '                        tentativeAF = previousAF
        '                    ElseIf trend < 0 AndAlso EP < previousEP Then
        '                        tentativeAF = previousAF + minimumAF
        '                    ElseIf trend < 0 AndAlso EP >= previousEP Then
        '                        tentativeAF = previousAF
        '                    End If
        '                Else
        '                    tentativeAF = minimumAF
        '                End If
        '                If tentativeAF > maximumAF Then
        '                    AF = maximumAF
        '                Else
        '                    AF = tentativeAF
        '                End If

        '                AFDiff = AF * EPSAR
        '            ElseIf inputPayload(runningPayload).PreviousCandlePayload IsNot Nothing AndAlso
        '                inputPayload(runningPayload).PreviousCandlePayload.PreviousCandlePayload IsNot Nothing Then
        '                PSAR = Math.Min(inputPayload(runningPayload).Low, Math.Min(inputPayload(runningPayload).PreviousCandlePayload.Low, inputPayload(runningPayload).PreviousCandlePayload.PreviousCandlePayload.Low))
        '                trend = 1
        '                EP = inputPayload(runningPayload).High
        '                EPSAR = EP - PSAR
        '                AF = minimumAF
        '                AFDiff = AF * EPSAR
        '            End If

        '            previousTrend = trend
        '            previousPSAR = PSAR
        '            previousEP = EP
        '            previousEPSAR = EPSAR
        '            previousAF = AF
        '            previousAFDiff = AFDiff

        '            If outputPSARPayload Is Nothing Then outputPSARPayload = New Dictionary(Of Date, Decimal)
        '            outputPSARPayload.Add(runningPayload, Math.Round(PSAR, 4))
        '            If outputTrendPayload Is Nothing Then outputTrendPayload = New Dictionary(Of Date, Color)
        '            outputTrendPayload.Add(runningPayload, If(trend < 0, Color.Red, If(trend > 0, Color.Green, Color.White)))
        '        Next
        '    End If
        'End Sub
        Public Sub CalculatePSAR(ByVal minimumAF As Decimal, ByVal maximumAF As Decimal, ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputPSARPayload As Dictionary(Of Date, Decimal), ByRef outputTrendPayload As Dictionary(Of Date, Color))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                Dim uptrend As Boolean = False
                Dim EP As Decimal = 0
                Dim SAR As Decimal = 0
                Dim AF As Decimal = minimumAF
                Dim nextBarSAR As Decimal = 0
                Dim bar_index As Integer = 0
                For Each runningPayload In inputPayload
                    If bar_index > 0 Then
                        Dim firstTrendBar As Boolean = False
                        SAR = nextBarSAR
                        If bar_index = 1 Then
                            Dim prevSAR As Decimal = 0
                            Dim prevEP As Decimal = 0
                            Dim lowPrev As Decimal = runningPayload.Value.PreviousCandlePayload.Low
                            Dim highPrev As Decimal = runningPayload.Value.PreviousCandlePayload.High
                            Dim closeCur As Decimal = runningPayload.Value.Close
                            Dim closePrev As Decimal = runningPayload.Value.PreviousCandlePayload.Close
                            If closeCur > closePrev Then
                                uptrend = True
                                EP = runningPayload.Value.High
                                prevSAR = lowPrev
                                prevEP = runningPayload.Value.High
                            Else
                                uptrend = False
                                EP = runningPayload.Value.Low
                                prevSAR = highPrev
                                prevEP = runningPayload.Value.Low
                            End If
                            firstTrendBar = True
                            SAR = prevSAR + minimumAF * (prevEP - prevSAR)
                        End If

                        If uptrend Then
                            If SAR > runningPayload.Value.Low Then
                                firstTrendBar = True
                                uptrend = False
                                SAR = Math.Max(EP, runningPayload.Value.High)
                                EP = runningPayload.Value.Low
                                AF = minimumAF
                            End If
                        Else
                            If SAR < runningPayload.Value.High Then
                                firstTrendBar = True
                                uptrend = True
                                SAR = Math.Min(EP, runningPayload.Value.Low)
                                EP = runningPayload.Value.High
                                AF = minimumAF
                            End If
                        End If

                        If Not firstTrendBar Then
                            If uptrend Then
                                If runningPayload.Value.High > EP Then
                                    EP = runningPayload.Value.High
                                    AF = Math.Min(AF + minimumAF, maximumAF)
                                End If
                            Else
                                If runningPayload.Value.Low < EP Then
                                    EP = runningPayload.Value.Low
                                    AF = Math.Min(AF + minimumAF, maximumAF)
                                End If
                            End If
                        End If

                        If uptrend Then
                            SAR = Math.Min(SAR, runningPayload.Value.PreviousCandlePayload.Low)
                            If bar_index > 1 Then
                                SAR = Math.Min(SAR, runningPayload.Value.PreviousCandlePayload.PreviousCandlePayload.Low)
                            End If
                        Else
                            SAR = Math.Max(SAR, runningPayload.Value.PreviousCandlePayload.High)
                            If bar_index > 1 Then
                                SAR = Math.Max(SAR, runningPayload.Value.PreviousCandlePayload.PreviousCandlePayload.High)
                            End If
                        End If

                        nextBarSAR = SAR + AF * (EP - SAR)
                    End If

                    If outputPSARPayload Is Nothing Then outputPSARPayload = New Dictionary(Of Date, Decimal)
                    outputPSARPayload.Add(runningPayload.Key, SAR)
                    If outputTrendPayload Is Nothing Then outputTrendPayload = New Dictionary(Of Date, Color)
                    outputTrendPayload.Add(runningPayload.Key, If(uptrend, Color.Green, Color.Red))

                    bar_index += 1
                Next
            End If
        End Sub
    End Module
End Namespace
