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

        cmbRule_SelectedIndexChanged(Nothing, Nothing)
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
                    rule = New NIFTYBANKOptions(_canceller, category, timeFrame, useHA, instrumentName, filePath)
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
                LoadSettings(Nothing)
                txtInstrumentName.Text = "NIFTY BANK"
                txtFilePath.Text = Nothing
                txtInstrumentName.Enabled = False
                txtFilePath.Enabled = False

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

        For Each runningPanel In panelList
            If panelName IsNot Nothing AndAlso runningPanel.Name = panelName.Name Then
                SetObjectVisible_ThreadSafe(runningPanel, True)
            Else
                SetObjectVisible_ThreadSafe(runningPanel, False)
            End If
        Next
    End Sub
End Class
