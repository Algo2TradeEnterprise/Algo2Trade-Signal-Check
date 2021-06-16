Imports Utilities.DAL
Imports System.Threading
Imports System.IO

Public Class frmMain

#Region "Common Delegates"
    Delegate Sub SetObjectEnableDisable_Delegate(ByVal [obj] As Object, ByVal [value] As Boolean)
    Public Sub SetObjectEnableDisable_ThreadSafe(ByVal [obj] As Object, ByVal [value] As Boolean)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [obj].InvokeRequired Then
            Dim MyDelegate As New SetObjectEnableDisable_Delegate(AddressOf SetObjectEnableDisable_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[obj], [value]})
        Else
            [obj].Enabled = [value]
        End If
    End Sub

    Delegate Sub SetObjectVisible_Delegate(ByVal [obj] As Object, ByVal [value] As Boolean)
    Public Sub SetObjectVisible_ThreadSafe(ByVal [obj] As Object, ByVal [value] As Boolean)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [obj].InvokeRequired Then
            Dim MyDelegate As New SetObjectVisible_Delegate(AddressOf SetObjectVisible_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[obj], [value]})
        Else
            [obj].Visible = [value]
        End If
    End Sub

    Delegate Sub SetLabelText_Delegate(ByVal [label] As Label, ByVal [text] As String)
    Public Sub SetLabelText_ThreadSafe(ByVal [label] As Label, ByVal [text] As String)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [label].InvokeRequired Then
            Dim MyDelegate As New SetLabelText_Delegate(AddressOf SetLabelText_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[label], [text]})
        Else
            [label].Text = [text]
        End If
    End Sub

    Delegate Function GetLabelText_Delegate(ByVal [label] As Label) As String
    Public Function GetLabelText_ThreadSafe(ByVal [label] As Label) As String
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [label].InvokeRequired Then
            Dim MyDelegate As New GetLabelText_Delegate(AddressOf GetLabelText_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[label]})
        Else
            Return [label].Text
        End If
    End Function

    Delegate Sub SetLabelTag_Delegate(ByVal [label] As Label, ByVal [tag] As String)
    Public Sub SetLabelTag_ThreadSafe(ByVal [label] As Label, ByVal [tag] As String)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [label].InvokeRequired Then
            Dim MyDelegate As New SetLabelTag_Delegate(AddressOf SetLabelTag_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[label], [tag]})
        Else
            [label].Tag = [tag]
        End If
    End Sub

    Delegate Function GetLabelTag_Delegate(ByVal [label] As Label) As String
    Public Function GetLabelTag_ThreadSafe(ByVal [label] As Label) As String
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [label].InvokeRequired Then
            Dim MyDelegate As New GetLabelTag_Delegate(AddressOf GetLabelTag_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[label]})
        Else
            Return [label].Tag
        End If
    End Function
    Delegate Sub SetToolStripLabel_Delegate(ByVal [toolStrip] As StatusStrip, ByVal [label] As ToolStripStatusLabel, ByVal [text] As String)
    Public Sub SetToolStripLabel_ThreadSafe(ByVal [toolStrip] As StatusStrip, ByVal [label] As ToolStripStatusLabel, ByVal [text] As String)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [toolStrip].InvokeRequired Then
            Dim MyDelegate As New SetToolStripLabel_Delegate(AddressOf SetToolStripLabel_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[toolStrip], [label], [text]})
        Else
            [label].Text = [text]
        End If
    End Sub

    Delegate Function GetToolStripLabel_Delegate(ByVal [toolStrip] As StatusStrip, ByVal [label] As ToolStripLabel) As String
    Public Function GetToolStripLabel_ThreadSafe(ByVal [toolStrip] As StatusStrip, ByVal [label] As ToolStripLabel) As String
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [toolStrip].InvokeRequired Then
            Dim MyDelegate As New GetToolStripLabel_Delegate(AddressOf GetToolStripLabel_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[toolStrip], [label]})
        Else
            Return [label].Text
        End If
    End Function

    Delegate Function GetDateTimePickerValue_Delegate(ByVal [dateTimePicker] As DateTimePicker) As Date
    Public Function GetDateTimePickerValue_ThreadSafe(ByVal [dateTimePicker] As DateTimePicker) As Date
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [dateTimePicker].InvokeRequired Then
            Dim MyDelegate As New GetDateTimePickerValue_Delegate(AddressOf GetDateTimePickerValue_ThreadSafe)
            Return Me.Invoke(MyDelegate, New DateTimePicker() {[dateTimePicker]})
        Else
            Return [dateTimePicker].Value
        End If
    End Function

    Delegate Function GetNumericUpDownValue_Delegate(ByVal [numericUpDown] As NumericUpDown) As Integer
    Public Function GetNumericUpDownValue_ThreadSafe(ByVal [numericUpDown] As NumericUpDown) As Integer
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [numericUpDown].InvokeRequired Then
            Dim MyDelegate As New GetNumericUpDownValue_Delegate(AddressOf GetNumericUpDownValue_ThreadSafe)
            Return Me.Invoke(MyDelegate, New NumericUpDown() {[numericUpDown]})
        Else
            Return [numericUpDown].Value
        End If
    End Function

    Delegate Function GetComboBoxIndex_Delegate(ByVal [combobox] As ComboBox) As Integer
    Public Function GetComboBoxIndex_ThreadSafe(ByVal [combobox] As ComboBox) As Integer
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [combobox].InvokeRequired Then
            Dim MyDelegate As New GetComboBoxIndex_Delegate(AddressOf GetComboBoxIndex_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[combobox]})
        Else
            Return [combobox].SelectedIndex
        End If
    End Function

    Delegate Function GetComboBoxItem_Delegate(ByVal [ComboBox] As ComboBox) As String
    Public Function GetComboBoxItem_ThreadSafe(ByVal [ComboBox] As ComboBox) As String
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [ComboBox].InvokeRequired Then
            Dim MyDelegate As New GetComboBoxItem_Delegate(AddressOf GetComboBoxItem_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[ComboBox]})
        Else
            Return [ComboBox].SelectedItem.ToString
        End If
    End Function

    Delegate Function GetTextBoxText_Delegate(ByVal [textBox] As TextBox) As String
    Public Function GetTextBoxText_ThreadSafe(ByVal [textBox] As TextBox) As String
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [textBox].InvokeRequired Then
            Dim MyDelegate As New GetTextBoxText_Delegate(AddressOf GetTextBoxText_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[textBox]})
        Else
            Return [textBox].Text
        End If
    End Function

    Delegate Function GetCheckBoxChecked_Delegate(ByVal [checkBox] As CheckBox) As Boolean
    Public Function GetCheckBoxChecked_ThreadSafe(ByVal [checkBox] As CheckBox) As Boolean
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [checkBox].InvokeRequired Then
            Dim MyDelegate As New GetCheckBoxChecked_Delegate(AddressOf GetCheckBoxChecked_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[checkBox]})
        Else
            Return [checkBox].Checked
        End If
    End Function

    Delegate Function GetRadioButtonChecked_Delegate(ByVal [radioButton] As RadioButton) As Boolean
    Public Function GetRadioButtonChecked_ThreadSafe(ByVal [radioButton] As RadioButton) As Boolean
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [radioButton].InvokeRequired Then
            Dim MyDelegate As New GetRadioButtonChecked_Delegate(AddressOf GetRadioButtonChecked_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[radioButton]})
        Else
            Return [radioButton].Checked
        End If
    End Function

    Delegate Sub SetDatagridBindDatatable_Delegate(ByVal [datagrid] As DataGridView, ByVal [table] As DataTable)
    Public Sub SetDatagridBindDatatable_ThreadSafe(ByVal [datagrid] As DataGridView, ByVal [table] As DataTable)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [datagrid].InvokeRequired Then
            Dim MyDelegate As New SetDatagridBindDatatable_Delegate(AddressOf SetDatagridBindDatatable_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[datagrid], [table]})
        Else
            [datagrid].DataSource = [table]
        End If
    End Sub
#End Region

#Region "Event Handlers"
    Private Sub OnHeartbeat(message As String)
        SetLabelText_ThreadSafe(lblProgress, message)
    End Sub
    Private Sub OnDocumentDownloadComplete()
        'OnHeartbeat("Document download compelete")
    End Sub
    Private Sub OnDocumentRetryStatus(currentTry As Integer, totalTries As Integer)
        OnHeartbeat(String.Format("Try #{0}/{1}: Connecting...", currentTry, totalTries))
    End Sub
    Public Sub OnWaitingFor(ByVal elapsedSecs As Integer, ByVal totalSecs As Integer, ByVal msg As String)
        OnHeartbeat(String.Format("{0}, waiting {1}/{2} secs", msg, elapsedSecs, totalSecs))
    End Sub
#End Region

    Private _canceller As CancellationTokenSource

    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SetObjectEnableDisable_ThreadSafe(btnView, True)
        SetObjectEnableDisable_ThreadSafe(btnCancel, False)
        SetObjectEnableDisable_ThreadSafe(btnExport, False)

        cmbCategory.SelectedIndex = My.Settings.Category
        cmbRule.SelectedIndex = My.Settings.Rule
        nmrcTimeFrame.Value = My.Settings.TimeFrame
        chkbHA.Checked = My.Settings.UseHA
        txtInstrumentName.Text = My.Settings.Intrument
        txtFilePath.Text = My.Settings.File
        If My.Settings.FromDate <> Date.MinValue Then dtpckrFromDate.Value = My.Settings.FromDate
        If My.Settings.ToDate <> Date.MinValue Then dtpckrToDate.Value = My.Settings.ToDate
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        _canceller.Cancel()
    End Sub

    Private Sub btnExport_Click(sender As Object, e As EventArgs) Handles btnExport.Click
        If dgvSignal IsNot Nothing AndAlso dgvSignal.Rows.Count > 0 Then
            saveFile.AddExtension = True
            saveFile.FileName = String.Format("{0}.csv", GetComboBoxItem_ThreadSafe(cmbRule))
            saveFile.Filter = "CSV (*.csv)|*.csv"
            saveFile.ShowDialog()
        Else
            MessageBox.Show("Empty DataGrid. Nothing to export.", "Signal Check CSV File", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    Private Sub saveFile_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles saveFile.FileOk
        Using export As New CSVHelper(saveFile.FileName, ",", _canceller)
            export.GetCSVFromDataGrid(dgvSignal)
        End Using
        If MessageBox.Show("Do you want to open file?", "Signal Check CSV File", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            Process.Start(saveFile.FileName)
        End If
    End Sub

    Private Sub btnBrowse_Click(sender As Object, e As EventArgs) Handles btnBrowse.Click
        opnFile.Filter = "|*.csv"
        opnFile.ShowDialog()
    End Sub

    Private Sub opnFile_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles opnFile.FileOk
        Dim extension As String = Path.GetExtension(opnFile.FileName)
        If extension = ".csv" Then
            txtFilePath.Text = opnFile.FileName
        Else
            MsgBox("File Type not supported. Please Try again.", MsgBoxStyle.Critical)
        End If
    End Sub

    Private Async Sub btnView_Click(sender As Object, e As EventArgs) Handles btnView.Click
        SetDatagridBindDatatable_ThreadSafe(dgvSignal, Nothing)
        SetObjectEnableDisable_ThreadSafe(btnView, False)
        SetObjectEnableDisable_ThreadSafe(btnCancel, True)
        SetObjectEnableDisable_ThreadSafe(btnExport, False)

        My.Settings.Category = cmbCategory.SelectedIndex
        My.Settings.Rule = cmbRule.SelectedIndex
        My.Settings.FromDate = dtpckrFromDate.Value
        My.Settings.ToDate = dtpckrToDate.Value
        My.Settings.TimeFrame = nmrcTimeFrame.Value
        My.Settings.UseHA = chkbHA.Checked
        My.Settings.Intrument = txtInstrumentName.Text
        My.Settings.File = txtFilePath.Text
        My.Settings.Save()
        Await Task.Run(AddressOf ViewDataAsync).ConfigureAwait(False)
    End Sub

    Private Async Function ViewDataAsync() As Task
        _canceller = New CancellationTokenSource
        Dim startDate As Date = GetDateTimePickerValue_ThreadSafe(dtpckrFromDate)
        Dim endDate As Date = GetDateTimePickerValue_ThreadSafe(dtpckrToDate)
        Dim selectedRule As Integer = GetComboBoxIndex_ThreadSafe(cmbRule)
        Dim category As Integer = GetComboBoxIndex_ThreadSafe(cmbCategory) + 2
        Dim timeFrame As Integer = GetNumericUpDownValue_ThreadSafe(nmrcTimeFrame)
        Dim useHA As Boolean = GetCheckBoxChecked_ThreadSafe(chkbHA)
        Dim instrumentName As String = GetTextBoxText_ThreadSafe(txtInstrumentName)
        Dim filePath As String = GetTextBoxText_ThreadSafe(txtFilePath)

        Try
            Dim dt As DataTable = Nothing
            Dim rule As Rule = Nothing
            Select Case selectedRule
                Case 0
                    rule = New StallPattern(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 1
                    rule = New PiercingAndDarkCloud(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 2
                    rule = New OneSidedVolume(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 3
                    rule = New ConstrictionAtBreakout(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 4
                    rule = New HKTrendOpposingByVolume(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 5
                    rule = New HKTemporaryPause(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 6
                    rule = New HKReversal(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 7
                    rule = New GetRawCandle(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 8
                    rule = New DailyStrongHKOppositeVolume(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 9
                    rule = New FractalCut2MA(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 10
                    rule = New VolumeIndex(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 11
                    rule = New EODSignal(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 12
                    rule = New PinBarFormation(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 13
                    rule = New BollingerWithATRBands(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 14
                    rule = New LowLossHighGainVWAP(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 15
                    rule = New DoubleVolumeEOD(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 16
                    rule = New FractalBreakoutShortTrend(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 17
                    rule = New DonchianBreakoutShortTrend(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 18
                    rule = New PinocchioBarFormation(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 19
                    rule = New MarketOpenHABreakoutScreener(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 20
                    rule = New VolumeWithCandleRange(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 21
                    rule = New DayHighLow(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 22
                    rule = New LowSLCandle(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 23
                    rule = New InsideBarHighLow(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 24
                    rule = New ReversaHHLLBreakout(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 25
                    rule = New DoubleInsideBar(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 26
                    rule = New HighLowSupportResistance(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 27
                    rule = New OHL(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 28
                    rule = New SpotFutureArbritrage(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 29
                    rule = New SwingCandle(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 30
                    rule = New SupertrendSMAOpenHighLow(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 31
                    rule = New DoubleTopDoubleBottom(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 32
                    rule = New WickBeyondSlabLevel(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 33
                    rule = New CandleRangeWithATR(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 34
                    rule = New FractalDip(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 35
                    rule = New RangeIdentifier(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 36
                    rule = New IndicatorTester(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 37
                    rule = New BollingerSqueeze(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 38
                    rule = New InsideBarBreakout(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 39
                    rule = New LowSLFractal(_canceller, category, timeFrame, useHA, instrumentName, filePath, GetTextBoxText_ThreadSafe(txtLowSLFractalATRMultiplier))
                Case 40
                    rule = New GraphAngle(_canceller, category, timeFrame, useHA, instrumentName, filePath, GetDateTimePickerValue_ThreadSafe(dtPckrGraphAngleEndTime), GetTextBoxText_ThreadSafe(txtGraphAngleSDMul), GetTextBoxText_ThreadSafe(txtGraphAngleCandlePer))
                Case 41
                    rule = New MultiEMALine(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 42
                    rule = New MultiTimeframeMultiMA(_canceller, category, timeFrame, useHA, instrumentName, filePath, GetNumericUpDownValue_ThreadSafe(nmrcMultiTFMultiMAHigherTF))
                Case 43
                    rule = New XMinVWAP(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 44
                    rule = New PriceVolumeImbalance(_canceller, category, timeFrame, useHA, instrumentName, filePath, GetNumericUpDownValue_ThreadSafe(nmrcPriceVolumeImbalancePeriod), GetTextBoxText_ThreadSafe(txtPriceVolumeImbalanceSDMul))
                Case 45
                    rule = New FirstCandleDifference(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 46
                    rule = New SmallBodyCandles(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 47
                    rule = New ReverseCandles(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 48
                    rule = New InsideWickCandles(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 49
                    rule = New HighestOIOptions(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 50
                    rule = New SqueezeZone(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 51
                    rule = New FibonacciTrendline(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 52
                    rule = New SupertrendConfirmation(_canceller, category, timeFrame, useHA, instrumentName, filePath, GetTextBoxText_ThreadSafe(txtSupertrendConfirmationMaxRangePer))
                Case 53
                    rule = New DayHLSwingTrendline(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 54
                    rule = New SectoralStockTrendOfEveryMinute(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 55
                    rule = New PreviousDayHKTrendVWAPSignals(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 56
                    rule = New GetStockTrend(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 57
                    rule = New GetStockTrendDirection(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 58
                    rule = New DataTester(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 59
                    rule = New IchimokuSignal(_canceller, category, timeFrame, useHA, instrumentName, filePath, GetRadioButtonChecked_ThreadSafe(rdbIchimokuSignalLaggingSpanConversionBaseLine))
                Case 60
                    rule = New MACDCrossoverSwing(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 61
                    rule = New PreviousDayHighLowBreak(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 62
                    rule = New HighVolumeOppositeColor(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 63
                    rule = New HammerCandleStickPattern(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 64
                    rule = New FirstStrongHKAfterOppositeStrongHK(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 65
                    rule = New ValueInvestingCashFuture(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 66
                    rule = New BTST_XMin(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 67
                    rule = New GetWeeklyCandle(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 68
                    rule = New FractalBreakoutTowardsMA(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 69
                    rule = New FractalConfirmationOnHTHK(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 70
                    rule = New EveryXMinCandleBreakout(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 71
                    rule = New FractalHighBreakoutBelowSupport(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 72
                    rule = New OutsideFractalTowardsSupertrend(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 73
                    rule = New StrongHKInsideFractal(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 74
                    rule = New PairHighLowBreak(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case 75
                    rule = New PivotLineBTSTSignal(_canceller, category, timeFrame, useHA, instrumentName, filePath)
                Case Else
                    Throw New NotImplementedException
            End Select
            AddHandler rule.Heartbeat, AddressOf OnHeartbeat
            AddHandler rule.WaitingFor, AddressOf OnWaitingFor
            AddHandler rule.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
            AddHandler rule.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
            dt = Await rule.RunAsync(startDate, endDate).ConfigureAwait(False)
            SetDatagridBindDatatable_ThreadSafe(dgvSignal, dt)
        Catch cx As OperationCanceledException
            MsgBox(String.Format("Error: {0}", cx.Message), MsgBoxStyle.Critical)
        Catch ex As Exception
            MsgBox(String.Format("Error: {0}", ex.ToString), MsgBoxStyle.Critical)
        Finally
            SetLabelText_ThreadSafe(lblProgress, String.Format("Process Complete. Number of records: {0}", dgvSignal.Rows.Count))
            SetObjectEnableDisable_ThreadSafe(btnView, True)
            SetObjectEnableDisable_ThreadSafe(btnCancel, False)
            SetObjectEnableDisable_ThreadSafe(btnExport, True)
        End Try
    End Function

    Private Sub cmbRule_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbRule.SelectedIndexChanged
        Dim index As Integer = GetComboBoxIndex_ThreadSafe(cmbRule)
        Select Case index
            Case 0
            Case 1
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 2
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 3
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Full candle should be outside fractal and candle range less than half atr")
            Case 4
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 5
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 6
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 7
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 8
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 9
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 10
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 11
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 12
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Previous candle bottom wick is 60% of candle range and Previous 3 candle range is 1/2 of ATR and previous 3 candle forms lower high and lower low and current candle breaks previous candle high and vice versa")
            Case 13
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 14
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Previous candle touches the vwap and current candle breaks high/low of previou candle. Entry at previous candle high/low. Stoploss at vwap. Loss is less than 5% of required capital(using 1 future lot and 13x margin). If loss stoploss is less than 1/3 of candle range the 'Tag'=Candle Range or 'Tag'=Capital")
            Case 15
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 16
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 17
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 18
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 19
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 20
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 21
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 22
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Current candle volume is greater than 90% of previous candle volume. Current candle range is greater than 1/3 ATR of the candle. And current candle range with buffer stoploss amount is greater than ₹1000 for respective quantity(calculated for ₹15000 capital)")
            Case 23
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("In a collection of current candle and previous two candle, any one of them is inside bar and difference between highest high and lowest low is less than current candle ATR")
            Case 24
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Previous three candles form HH-HL and current candle breaks previous candle low and vice versa")
            Case 25
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 26
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Top High/Low matching candles")
            Case 27
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 28
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 29
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Candle which creates both swing high and low")
            Case 30
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Daily candle should be above supertrend and 200 MA and for first X minute Open=High and vice versa")
            Case 31
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Doule fractal top with max distance of 1 ATR and vice versa")
            Case 32
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Candle wick is outside the open or close slab level")
            Case 33
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Candle Range % according to ATR and Slab")
            Case 34
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Fractal high dips lower fractal or vice versa")
            Case 35
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Identify a range of candles")
            Case 36
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Indicator Testing purpose")
            Case 37
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Bollinger Band (20,2) squeeze into Keltner Channel(20,1.5)")
            Case 38
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Second candle of the day is an inside bar and third candle breaks first candle high/low")
            Case 39
                txtLowSLFractalATRMultiplier.Text = 1
                LoadSettings(pnlLowSLFractal)
                lblDescription.Text = String.Format("Fractal high, low difference less than equal to X ATR")
            Case 40
                dtPckrGraphAngleEndTime.Value = New Date(Now.Year, Now.Month, Now.Day, 9, 30, 0)
                txtGraphAngleSDMul.Text = 1
                txtGraphAngleCandlePer.Text = 90
                LoadSettings(pnlGraphAngle)
                lblDescription.Text = String.Format("X% candle(Average of OHLC) of total candles passes through N-SD(SDPA of points of 45° line) of 45° line(diagonal line of a square which x-axis is defined by total number of candles and y-axis is defined by difference between highest high and lowest low[coverted to total number of candles]) with in specified time (plus 15 minutes buffer).")
            Case 41
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("For buy candle close>50 EMA>100 EMA>150 EMA, then close<50 EMA or close<100 EMA and again candle close>50 EMA>100 EMA>150 EMA")
            Case 42
                nmrcMultiTFMultiMAHigherTF.Value = 15
                LoadSettings(pnlMultiTFMultiMA)
                lblDescription.Text = String.Format("Description...")
            Case 43
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("X-Min Close<1% of X-Min Open and Volume>2*SMA(Volume,10) and Close crossed below vwap and vice versa")
            Case 44
                nmrcPriceVolumeImbalancePeriod.Value = 20
                txtPriceVolumeImbalanceSDMul.Text = 3
                LoadSettings(pnlPriceVolumeImbalance)
                lblDescription.Text = "X-period moving average of (High-Low)/Volume is outside +/- xSD"
            Case 45
                LoadSettings(Nothing)
                lblDescription.Text = "Difference of first candle of the day close with previous day first candle of the day"
            Case 46
                LoadSettings(Nothing)
                lblDescription.Text = "Current candle body<=1/4 of previous candle body and current candle should be formed at the edge of the previous candle, ie. it should not cross the midpoint of the previous candle. Previous candle should have body more than 50% of its range."
            Case 47
                LoadSettings(Nothing)
                lblDescription.Text = "Current candle color Red but creats as Higher high and vice versa"
            Case 48
                LoadSettings(Nothing)
                lblDescription.Text = "Current candle body less than previous candle wick"
            Case 49
                LoadSettings(Nothing)
                lblDescription.Text = "Top 5 OI Option Stocks"
            Case 50
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Bollinger Band (20,2) squeeze into ATR Band(50,1)")
            Case 51
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 52
                LoadSettings(pnlSupertrendConfirmation)
                txtSupertrendConfirmationMaxRangePer.Text = 0.25
                lblDescription.Text = String.Format("When supertrend is green and a red candle followed by green candle and two candle range % <= x %")
            Case 53
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 54
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Give sector name in `Instrument Name`")
            Case 55
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 56
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Get stock trend of every minute with respect to previous day close(previous day last minute candle)")
            Case 57
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Get stock trend direction by time range with respect to current day open")
            Case 58
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Do not use it without checking code. It is only for testing purpose")
            Case 59
                LoadSettings(pnlIchimokuSignal)
                rdbIchimokuSignalLaggingSpanConversionBaseLine.Checked = True
                lblDescription.Text = String.Format("")
            Case 60
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 61
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Time when candle breaks previous day high/low")
            Case 62
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Candle Volume Greater than Previous Candle Volume ")
            Case 63
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Candle which looks like hammers")
            Case 64
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 65
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return Cash and Future EOD Data for the given stock")
            Case 66
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 67
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 68
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 69
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 70
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 71
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 72
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 73
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case 74
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("When simultaneously Pair 1 first x-Min candle high breaks and Pair 2 first x-Min candle low breaks. And vice versa. Pair stockname seperated by '_' e.g. ABC_XYZ")
            Case 75
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Description ...")
            Case Else
                Throw New NotImplementedException
        End Select
    End Sub

    Private Sub cmbCategory_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbCategory.SelectedIndexChanged
        Dim index As Integer = GetComboBoxIndex_ThreadSafe(cmbCategory)
        Select Case index
            Case 0, 1, 2, 3, 9
                nmrcTimeFrame.Value = My.Settings.TimeFrame
                nmrcTimeFrame.Enabled = True
            Case 4, 5, 6, 7, 8, 10
                nmrcTimeFrame.Value = 1
                nmrcTimeFrame.Enabled = False
            Case Else
                Throw New NotImplementedException
        End Select
    End Sub

    Private Sub LoadSettings(ByVal panelName As Panel)
        Dim panelList As List(Of Panel) = New List(Of Panel)
        panelList.Add(pnlLowSLFractal)
        panelList.Add(pnlGraphAngle)
        panelList.Add(pnlMultiTFMultiMA)
        panelList.Add(pnlPriceVolumeImbalance)
        panelList.Add(pnlSupertrendConfirmation)
        panelList.Add(pnlIchimokuSignal)

        For Each runningPanel In panelList
            If panelName IsNot Nothing AndAlso runningPanel.Name = panelName.Name Then
                SetObjectVisible_ThreadSafe(runningPanel, True)
            Else
                SetObjectVisible_ThreadSafe(runningPanel, False)
            End If
        Next
    End Sub
End Class
