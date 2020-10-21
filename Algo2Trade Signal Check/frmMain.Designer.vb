<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMain))
        Me.saveFile = New System.Windows.Forms.SaveFileDialog()
        Me.btnExport = New System.Windows.Forms.Button()
        Me.chkbHA = New System.Windows.Forms.CheckBox()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.nmrcTimeFrame = New System.Windows.Forms.NumericUpDown()
        Me.lblTimeFrame = New System.Windows.Forms.Label()
        Me.dtpckrToDate = New System.Windows.Forms.DateTimePicker()
        Me.dtpckrFromDate = New System.Windows.Forms.DateTimePicker()
        Me.lblToDate = New System.Windows.Forms.Label()
        Me.lblFromDate = New System.Windows.Forms.Label()
        Me.btnView = New System.Windows.Forms.Button()
        Me.txtInstrumentName = New System.Windows.Forms.TextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.cmbCategory = New System.Windows.Forms.ComboBox()
        Me.lblCategory = New System.Windows.Forms.Label()
        Me.cmbRule = New System.Windows.Forms.ComboBox()
        Me.lblRule = New System.Windows.Forms.Label()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.btnBrowse = New System.Windows.Forms.Button()
        Me.txtFilePath = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.dgvSignal = New System.Windows.Forms.DataGridView()
        Me.opnFile = New System.Windows.Forms.OpenFileDialog()
        Me.lblDescription = New System.Windows.Forms.Label()
        Me.pnlLowSLFractal = New System.Windows.Forms.Panel()
        Me.txtLowSLFractalATRMultiplier = New System.Windows.Forms.TextBox()
        Me.lblLowSLFractalATRMultiplier = New System.Windows.Forms.Label()
        Me.pnlGraphAngle = New System.Windows.Forms.Panel()
        Me.txtGraphAngleCandlePer = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.txtGraphAngleSDMul = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.dtPckrGraphAngleEndTime = New System.Windows.Forms.DateTimePicker()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.pnlMultiTFMultiMA = New System.Windows.Forms.Panel()
        Me.nmrcMultiTFMultiMAHigherTF = New System.Windows.Forms.NumericUpDown()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.pnlPriceVolumeImbalance = New System.Windows.Forms.Panel()
        Me.txtPriceVolumeImbalanceSDMul = New System.Windows.Forms.TextBox()
        Me.nmrcPriceVolumeImbalancePeriod = New System.Windows.Forms.NumericUpDown()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.pnlSupertrendConfirmation = New System.Windows.Forms.Panel()
        Me.txtSupertrendConfirmationMaxRangePer = New System.Windows.Forms.TextBox()
        Me.lblSupertrendConfirmationMaxRangePer = New System.Windows.Forms.Label()
        Me.pnlIchimokuSignal = New System.Windows.Forms.Panel()
        Me.grpbxIchimokuSignalType = New System.Windows.Forms.GroupBox()
        Me.rdbIchimokuSignalLaggingSpanConversionBaseLine = New System.Windows.Forms.RadioButton()
        Me.rdbIchimokuSignalLaggingSpan = New System.Windows.Forms.RadioButton()
        CType(Me.nmrcTimeFrame, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel1.SuspendLayout()
        CType(Me.dgvSignal, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlLowSLFractal.SuspendLayout()
        Me.pnlGraphAngle.SuspendLayout()
        Me.pnlMultiTFMultiMA.SuspendLayout()
        CType(Me.nmrcMultiTFMultiMAHigherTF, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlPriceVolumeImbalance.SuspendLayout()
        CType(Me.nmrcPriceVolumeImbalancePeriod, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlSupertrendConfirmation.SuspendLayout()
        Me.pnlIchimokuSignal.SuspendLayout()
        Me.grpbxIchimokuSignalType.SuspendLayout()
        Me.SuspendLayout()
        '
        'saveFile
        '
        '
        'btnExport
        '
        Me.btnExport.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnExport.Location = New System.Drawing.Point(1140, 658)
        Me.btnExport.Margin = New System.Windows.Forms.Padding(4)
        Me.btnExport.Name = "btnExport"
        Me.btnExport.Size = New System.Drawing.Size(100, 28)
        Me.btnExport.TabIndex = 24
        Me.btnExport.Text = "Export"
        Me.btnExport.UseVisualStyleBackColor = True
        '
        'chkbHA
        '
        Me.chkbHA.AutoSize = True
        Me.chkbHA.Location = New System.Drawing.Point(962, 15)
        Me.chkbHA.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.chkbHA.Name = "chkbHA"
        Me.chkbHA.Size = New System.Drawing.Size(149, 21)
        Me.chkbHA.TabIndex = 30
        Me.chkbHA.Text = "HeikenAshi Candle"
        Me.chkbHA.UseVisualStyleBackColor = True
        '
        'btnCancel
        '
        Me.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.btnCancel.Location = New System.Drawing.Point(1125, 44)
        Me.btnCancel.Margin = New System.Windows.Forms.Padding(4)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(100, 28)
        Me.btnCancel.TabIndex = 29
        Me.btnCancel.Text = "Stop"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'nmrcTimeFrame
        '
        Me.nmrcTimeFrame.Location = New System.Drawing.Point(883, 12)
        Me.nmrcTimeFrame.Margin = New System.Windows.Forms.Padding(4)
        Me.nmrcTimeFrame.Name = "nmrcTimeFrame"
        Me.nmrcTimeFrame.Size = New System.Drawing.Size(58, 22)
        Me.nmrcTimeFrame.TabIndex = 28
        '
        'lblTimeFrame
        '
        Me.lblTimeFrame.AutoSize = True
        Me.lblTimeFrame.Location = New System.Drawing.Point(760, 13)
        Me.lblTimeFrame.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblTimeFrame.Name = "lblTimeFrame"
        Me.lblTimeFrame.Size = New System.Drawing.Size(126, 17)
        Me.lblTimeFrame.TabIndex = 27
        Me.lblTimeFrame.Text = "Signal TimeFrame:"
        '
        'dtpckrToDate
        '
        Me.dtpckrToDate.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.dtpckrToDate.Location = New System.Drawing.Point(277, 50)
        Me.dtpckrToDate.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.dtpckrToDate.Name = "dtpckrToDate"
        Me.dtpckrToDate.Size = New System.Drawing.Size(108, 22)
        Me.dtpckrToDate.TabIndex = 26
        '
        'dtpckrFromDate
        '
        Me.dtpckrFromDate.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.dtpckrFromDate.Location = New System.Drawing.Point(93, 48)
        Me.dtpckrFromDate.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.dtpckrFromDate.Name = "dtpckrFromDate"
        Me.dtpckrFromDate.Size = New System.Drawing.Size(108, 22)
        Me.dtpckrFromDate.TabIndex = 25
        '
        'lblToDate
        '
        Me.lblToDate.AutoSize = True
        Me.lblToDate.Location = New System.Drawing.Point(211, 51)
        Me.lblToDate.Name = "lblToDate"
        Me.lblToDate.Size = New System.Drawing.Size(63, 17)
        Me.lblToDate.TabIndex = 24
        Me.lblToDate.Text = "To Date:"
        '
        'lblFromDate
        '
        Me.lblFromDate.AutoSize = True
        Me.lblFromDate.Location = New System.Drawing.Point(11, 50)
        Me.lblFromDate.Name = "lblFromDate"
        Me.lblFromDate.Size = New System.Drawing.Size(78, 17)
        Me.lblFromDate.TabIndex = 23
        Me.lblFromDate.Text = "From Date:"
        '
        'btnView
        '
        Me.btnView.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.btnView.Location = New System.Drawing.Point(1125, 7)
        Me.btnView.Margin = New System.Windows.Forms.Padding(4)
        Me.btnView.Name = "btnView"
        Me.btnView.Size = New System.Drawing.Size(100, 28)
        Me.btnView.TabIndex = 22
        Me.btnView.Text = "View"
        Me.btnView.UseVisualStyleBackColor = True
        '
        'txtInstrumentName
        '
        Me.txtInstrumentName.Location = New System.Drawing.Point(518, 52)
        Me.txtInstrumentName.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.txtInstrumentName.Name = "txtInstrumentName"
        Me.txtInstrumentName.Size = New System.Drawing.Size(205, 22)
        Me.txtInstrumentName.TabIndex = 21
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(398, 53)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(119, 17)
        Me.Label4.TabIndex = 20
        Me.Label4.Text = "Instrument Name:"
        '
        'cmbCategory
        '
        Me.cmbCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbCategory.FormattingEnabled = True
        Me.cmbCategory.Items.AddRange(New Object() {"Intraday Cash", "Intraday Commodity", "Intraday Currency", "Intraday Futures", "EOD Cash", "EOD Commodity", "EOD Currency", "EOD Futures", "EOD Postional", "Intraday Future Options", "EOD Future Options"})
        Me.cmbCategory.Location = New System.Drawing.Point(607, 9)
        Me.cmbCategory.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.cmbCategory.Name = "cmbCategory"
        Me.cmbCategory.Size = New System.Drawing.Size(146, 24)
        Me.cmbCategory.TabIndex = 19
        '
        'lblCategory
        '
        Me.lblCategory.AutoSize = True
        Me.lblCategory.Location = New System.Drawing.Point(466, 11)
        Me.lblCategory.Name = "lblCategory"
        Me.lblCategory.Size = New System.Drawing.Size(139, 17)
        Me.lblCategory.TabIndex = 18
        Me.lblCategory.Text = "Instrument Category:"
        '
        'cmbRule
        '
        Me.cmbRule.FormattingEnabled = True
        Me.cmbRule.Items.AddRange(New Object() {"Stall Pattern", "Piercing And Dark Cloud", "One Sided Volume", "Constriction At Breakout", "HK Trend Opposing By Volume", "HK Temporary Pause", "HK Reversal", "Get Raw Candle", "Daily Strong HK Opposite Color Volume", "Fractal Cut 2 MA", "Volume Index", "EOD Signal", "Pin Bar Formation", "Bollinger With ATR Bands", "Low Loss High Gain VWAP", "Double Volume EOD", "Fractal Breakout Short Trend", "Donchian Breakout Short Trend", "Pinocchio Bar Formation", "Market Open HA Breakout Screener", "Volume With Candle Range", "DayHighLow", "Low SL Candle", "Inside Bar High Low", "Reversal HHLL Breakout", "Double Inside Bar", "High Low Support Resistance", "Open=High/Open=Low", "Spot Future Arbritrage", "Swing Candle", "Supertrend SMA Open High/Low", "Double Top Double Bottom", "Wick Beyond Slab Level", "Candle Range With ATR", "Fractal Dip", "Range Identifier", "Indicator Tester", "Bollinger Squeeze", "Inside Bar Breakout", "Low SL Fractal", "Graph Angle", "Multi EMA Line", "Multi Timeframe Multi MA", "X-Min VWAP", "Price Volume Imbalance", "First Candle Difference", "Small Body Candles", "Reverse Candles", "Inside Wick Candles", "Highest OI Options", "Squeeze Zone", "Fibonacci Trendline", "Supertrend Confirmation", "Day High Low Swing Trendline", "Sectoral Trend Of Every Minute", "Previous Day HK Trend VWAP Signals", "Get Stock Trend", "Get Stock Trend Direction", "Data Tester", "Ichimoku Signal", "MACD Crossover Swing", "Previous Day High Low Break", "High Volume Opposite Color", "Hammer Candle Stick Pattern"})
        Me.cmbRule.Location = New System.Drawing.Point(108, 7)
        Me.cmbRule.Margin = New System.Windows.Forms.Padding(4)
        Me.cmbRule.Name = "cmbRule"
        Me.cmbRule.Size = New System.Drawing.Size(354, 24)
        Me.cmbRule.TabIndex = 17
        '
        'lblRule
        '
        Me.lblRule.AutoSize = True
        Me.lblRule.Location = New System.Drawing.Point(11, 12)
        Me.lblRule.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblRule.Name = "lblRule"
        Me.lblRule.Size = New System.Drawing.Size(93, 17)
        Me.lblRule.TabIndex = 16
        Me.lblRule.Text = "Choose Rule:"
        '
        'lblProgress
        '
        Me.lblProgress.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblProgress.Location = New System.Drawing.Point(4, 657)
        Me.lblProgress.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(1128, 29)
        Me.lblProgress.TabIndex = 23
        Me.lblProgress.Text = "Progess Status ....."
        '
        'Panel1
        '
        Me.Panel1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Panel1.Controls.Add(Me.btnBrowse)
        Me.Panel1.Controls.Add(Me.txtFilePath)
        Me.Panel1.Controls.Add(Me.Label1)
        Me.Panel1.Controls.Add(Me.chkbHA)
        Me.Panel1.Controls.Add(Me.btnCancel)
        Me.Panel1.Controls.Add(Me.nmrcTimeFrame)
        Me.Panel1.Controls.Add(Me.lblTimeFrame)
        Me.Panel1.Controls.Add(Me.dtpckrToDate)
        Me.Panel1.Controls.Add(Me.dtpckrFromDate)
        Me.Panel1.Controls.Add(Me.lblToDate)
        Me.Panel1.Controls.Add(Me.lblFromDate)
        Me.Panel1.Controls.Add(Me.btnView)
        Me.Panel1.Controls.Add(Me.txtInstrumentName)
        Me.Panel1.Controls.Add(Me.Label4)
        Me.Panel1.Controls.Add(Me.cmbCategory)
        Me.Panel1.Controls.Add(Me.lblCategory)
        Me.Panel1.Controls.Add(Me.cmbRule)
        Me.Panel1.Controls.Add(Me.lblRule)
        Me.Panel1.Location = New System.Drawing.Point(4, 4)
        Me.Panel1.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(1237, 84)
        Me.Panel1.TabIndex = 22
        '
        'btnBrowse
        '
        Me.btnBrowse.Location = New System.Drawing.Point(1055, 52)
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.Size = New System.Drawing.Size(33, 23)
        Me.btnBrowse.TabIndex = 33
        Me.btnBrowse.Text = "..."
        Me.btnBrowse.UseVisualStyleBackColor = True
        '
        'txtFilePath
        '
        Me.txtFilePath.Location = New System.Drawing.Point(799, 52)
        Me.txtFilePath.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.txtFilePath.Name = "txtFilePath"
        Me.txtFilePath.Size = New System.Drawing.Size(252, 22)
        Me.txtFilePath.TabIndex = 32
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(730, 53)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(67, 17)
        Me.Label1.TabIndex = 31
        Me.Label1.Text = "File Path:"
        '
        'dgvSignal
        '
        Me.dgvSignal.AllowUserToAddRows = False
        Me.dgvSignal.AllowUserToDeleteRows = False
        Me.dgvSignal.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgvSignal.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvSignal.Location = New System.Drawing.Point(4, 161)
        Me.dgvSignal.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.dgvSignal.Name = "dgvSignal"
        Me.dgvSignal.ReadOnly = True
        Me.dgvSignal.RowTemplate.Height = 24
        Me.dgvSignal.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect
        Me.dgvSignal.Size = New System.Drawing.Size(1237, 492)
        Me.dgvSignal.TabIndex = 21
        '
        'opnFile
        '
        '
        'lblDescription
        '
        Me.lblDescription.Location = New System.Drawing.Point(5, 90)
        Me.lblDescription.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblDescription.Name = "lblDescription"
        Me.lblDescription.Size = New System.Drawing.Size(706, 69)
        Me.lblDescription.TabIndex = 25
        Me.lblDescription.Text = "Description ....."
        '
        'pnlLowSLFractal
        '
        Me.pnlLowSLFractal.Controls.Add(Me.txtLowSLFractalATRMultiplier)
        Me.pnlLowSLFractal.Controls.Add(Me.lblLowSLFractalATRMultiplier)
        Me.pnlLowSLFractal.Location = New System.Drawing.Point(748, 90)
        Me.pnlLowSLFractal.Name = "pnlLowSLFractal"
        Me.pnlLowSLFractal.Size = New System.Drawing.Size(200, 50)
        Me.pnlLowSLFractal.TabIndex = 26
        '
        'txtLowSLFractalATRMultiplier
        '
        Me.txtLowSLFractalATRMultiplier.Location = New System.Drawing.Point(115, 13)
        Me.txtLowSLFractalATRMultiplier.Name = "txtLowSLFractalATRMultiplier"
        Me.txtLowSLFractalATRMultiplier.Size = New System.Drawing.Size(70, 22)
        Me.txtLowSLFractalATRMultiplier.TabIndex = 1
        '
        'lblLowSLFractalATRMultiplier
        '
        Me.lblLowSLFractalATRMultiplier.AutoSize = True
        Me.lblLowSLFractalATRMultiplier.Location = New System.Drawing.Point(12, 16)
        Me.lblLowSLFractalATRMultiplier.Name = "lblLowSLFractalATRMultiplier"
        Me.lblLowSLFractalATRMultiplier.Size = New System.Drawing.Size(96, 17)
        Me.lblLowSLFractalATRMultiplier.TabIndex = 0
        Me.lblLowSLFractalATRMultiplier.Text = "ATR Multiplier"
        '
        'pnlGraphAngle
        '
        Me.pnlGraphAngle.Controls.Add(Me.txtGraphAngleCandlePer)
        Me.pnlGraphAngle.Controls.Add(Me.Label5)
        Me.pnlGraphAngle.Controls.Add(Me.txtGraphAngleSDMul)
        Me.pnlGraphAngle.Controls.Add(Me.Label3)
        Me.pnlGraphAngle.Controls.Add(Me.dtPckrGraphAngleEndTime)
        Me.pnlGraphAngle.Controls.Add(Me.Label2)
        Me.pnlGraphAngle.Location = New System.Drawing.Point(748, 90)
        Me.pnlGraphAngle.Name = "pnlGraphAngle"
        Me.pnlGraphAngle.Size = New System.Drawing.Size(461, 50)
        Me.pnlGraphAngle.TabIndex = 28
        '
        'txtGraphAngleCandlePer
        '
        Me.txtGraphAngleCandlePer.Location = New System.Drawing.Point(370, 12)
        Me.txtGraphAngleCandlePer.Name = "txtGraphAngleCandlePer"
        Me.txtGraphAngleCandlePer.Size = New System.Drawing.Size(46, 22)
        Me.txtGraphAngleCandlePer.TabIndex = 5
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(283, 16)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(85, 17)
        Me.Label5.TabIndex = 4
        Me.Label5.Text = "Percentage:"
        '
        'txtGraphAngleSDMul
        '
        Me.txtGraphAngleSDMul.Location = New System.Drawing.Point(228, 12)
        Me.txtGraphAngleSDMul.Name = "txtGraphAngleSDMul"
        Me.txtGraphAngleSDMul.Size = New System.Drawing.Size(46, 22)
        Me.txtGraphAngleSDMul.TabIndex = 3
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(196, 16)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(31, 17)
        Me.Label3.TabIndex = 2
        Me.Label3.Text = "SD:"
        '
        'dtPckrGraphAngleEndTime
        '
        Me.dtPckrGraphAngleEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtPckrGraphAngleEndTime.Location = New System.Drawing.Point(86, 13)
        Me.dtPckrGraphAngleEndTime.Name = "dtPckrGraphAngleEndTime"
        Me.dtPckrGraphAngleEndTime.ShowUpDown = True
        Me.dtPckrGraphAngleEndTime.Size = New System.Drawing.Size(103, 22)
        Me.dtPckrGraphAngleEndTime.TabIndex = 1
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(12, 16)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(72, 17)
        Me.Label2.TabIndex = 0
        Me.Label2.Text = "End Time:"
        '
        'pnlMultiTFMultiMA
        '
        Me.pnlMultiTFMultiMA.Controls.Add(Me.nmrcMultiTFMultiMAHigherTF)
        Me.pnlMultiTFMultiMA.Controls.Add(Me.Label7)
        Me.pnlMultiTFMultiMA.Location = New System.Drawing.Point(748, 90)
        Me.pnlMultiTFMultiMA.Name = "pnlMultiTFMultiMA"
        Me.pnlMultiTFMultiMA.Size = New System.Drawing.Size(461, 50)
        Me.pnlMultiTFMultiMA.TabIndex = 29
        '
        'nmrcMultiTFMultiMAHigherTF
        '
        Me.nmrcMultiTFMultiMAHigherTF.Location = New System.Drawing.Point(147, 15)
        Me.nmrcMultiTFMultiMAHigherTF.Name = "nmrcMultiTFMultiMAHigherTF"
        Me.nmrcMultiTFMultiMAHigherTF.Size = New System.Drawing.Size(79, 22)
        Me.nmrcMultiTFMultiMAHigherTF.TabIndex = 3
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(15, 16)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(125, 17)
        Me.Label7.TabIndex = 2
        Me.Label7.Text = "Higher Timeframe:"
        '
        'pnlPriceVolumeImbalance
        '
        Me.pnlPriceVolumeImbalance.Controls.Add(Me.txtPriceVolumeImbalanceSDMul)
        Me.pnlPriceVolumeImbalance.Controls.Add(Me.nmrcPriceVolumeImbalancePeriod)
        Me.pnlPriceVolumeImbalance.Controls.Add(Me.Label8)
        Me.pnlPriceVolumeImbalance.Controls.Add(Me.Label6)
        Me.pnlPriceVolumeImbalance.Location = New System.Drawing.Point(748, 90)
        Me.pnlPriceVolumeImbalance.Name = "pnlPriceVolumeImbalance"
        Me.pnlPriceVolumeImbalance.Size = New System.Drawing.Size(461, 50)
        Me.pnlPriceVolumeImbalance.TabIndex = 30
        '
        'txtPriceVolumeImbalanceSDMul
        '
        Me.txtPriceVolumeImbalanceSDMul.Location = New System.Drawing.Point(109, 14)
        Me.txtPriceVolumeImbalanceSDMul.Name = "txtPriceVolumeImbalanceSDMul"
        Me.txtPriceVolumeImbalanceSDMul.Size = New System.Drawing.Size(77, 22)
        Me.txtPriceVolumeImbalanceSDMul.TabIndex = 6
        '
        'nmrcPriceVolumeImbalancePeriod
        '
        Me.nmrcPriceVolumeImbalancePeriod.Location = New System.Drawing.Point(270, 14)
        Me.nmrcPriceVolumeImbalancePeriod.Name = "nmrcPriceVolumeImbalancePeriod"
        Me.nmrcPriceVolumeImbalancePeriod.Size = New System.Drawing.Size(79, 22)
        Me.nmrcPriceVolumeImbalancePeriod.TabIndex = 5
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(216, 16)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(53, 17)
        Me.Label8.TabIndex = 4
        Me.Label8.Text = "Period:"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(15, 16)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(91, 17)
        Me.Label6.TabIndex = 2
        Me.Label6.Text = "SD Multiplier:"
        '
        'pnlSupertrendConfirmation
        '
        Me.pnlSupertrendConfirmation.Controls.Add(Me.txtSupertrendConfirmationMaxRangePer)
        Me.pnlSupertrendConfirmation.Controls.Add(Me.lblSupertrendConfirmationMaxRangePer)
        Me.pnlSupertrendConfirmation.Location = New System.Drawing.Point(748, 90)
        Me.pnlSupertrendConfirmation.Name = "pnlSupertrendConfirmation"
        Me.pnlSupertrendConfirmation.Size = New System.Drawing.Size(461, 50)
        Me.pnlSupertrendConfirmation.TabIndex = 31
        '
        'txtSupertrendConfirmationMaxRangePer
        '
        Me.txtSupertrendConfirmationMaxRangePer.Location = New System.Drawing.Point(122, 14)
        Me.txtSupertrendConfirmationMaxRangePer.Name = "txtSupertrendConfirmationMaxRangePer"
        Me.txtSupertrendConfirmationMaxRangePer.Size = New System.Drawing.Size(77, 22)
        Me.txtSupertrendConfirmationMaxRangePer.TabIndex = 6
        '
        'lblSupertrendConfirmationMaxRangePer
        '
        Me.lblSupertrendConfirmationMaxRangePer.AutoSize = True
        Me.lblSupertrendConfirmationMaxRangePer.Location = New System.Drawing.Point(15, 16)
        Me.lblSupertrendConfirmationMaxRangePer.Name = "lblSupertrendConfirmationMaxRangePer"
        Me.lblSupertrendConfirmationMaxRangePer.Size = New System.Drawing.Size(103, 17)
        Me.lblSupertrendConfirmationMaxRangePer.TabIndex = 2
        Me.lblSupertrendConfirmationMaxRangePer.Text = "Max Range % :"
        '
        'pnlIchimokuSignal
        '
        Me.pnlIchimokuSignal.Controls.Add(Me.grpbxIchimokuSignalType)
        Me.pnlIchimokuSignal.Location = New System.Drawing.Point(748, 90)
        Me.pnlIchimokuSignal.Name = "pnlIchimokuSignal"
        Me.pnlIchimokuSignal.Size = New System.Drawing.Size(461, 51)
        Me.pnlIchimokuSignal.TabIndex = 32
        '
        'grpbxIchimokuSignalType
        '
        Me.grpbxIchimokuSignalType.Controls.Add(Me.rdbIchimokuSignalLaggingSpanConversionBaseLine)
        Me.grpbxIchimokuSignalType.Controls.Add(Me.rdbIchimokuSignalLaggingSpan)
        Me.grpbxIchimokuSignalType.Location = New System.Drawing.Point(11, 2)
        Me.grpbxIchimokuSignalType.Name = "grpbxIchimokuSignalType"
        Me.grpbxIchimokuSignalType.Size = New System.Drawing.Size(434, 43)
        Me.grpbxIchimokuSignalType.TabIndex = 0
        Me.grpbxIchimokuSignalType.TabStop = False
        Me.grpbxIchimokuSignalType.Text = "Signal Type"
        '
        'rdbIchimokuSignalLaggingSpanConversionBaseLine
        '
        Me.rdbIchimokuSignalLaggingSpanConversionBaseLine.AutoSize = True
        Me.rdbIchimokuSignalLaggingSpanConversionBaseLine.Location = New System.Drawing.Point(130, 18)
        Me.rdbIchimokuSignalLaggingSpanConversionBaseLine.Name = "rdbIchimokuSignalLaggingSpanConversionBaseLine"
        Me.rdbIchimokuSignalLaggingSpanConversionBaseLine.Size = New System.Drawing.Size(291, 21)
        Me.rdbIchimokuSignalLaggingSpanConversionBaseLine.TabIndex = 1
        Me.rdbIchimokuSignalLaggingSpanConversionBaseLine.TabStop = True
        Me.rdbIchimokuSignalLaggingSpanConversionBaseLine.Text = "Lagging Span, Conversion and Base Line"
        Me.rdbIchimokuSignalLaggingSpanConversionBaseLine.UseVisualStyleBackColor = True
        '
        'rdbIchimokuSignalLaggingSpan
        '
        Me.rdbIchimokuSignalLaggingSpan.AutoSize = True
        Me.rdbIchimokuSignalLaggingSpan.Location = New System.Drawing.Point(7, 18)
        Me.rdbIchimokuSignalLaggingSpan.Name = "rdbIchimokuSignalLaggingSpan"
        Me.rdbIchimokuSignalLaggingSpan.Size = New System.Drawing.Size(117, 21)
        Me.rdbIchimokuSignalLaggingSpan.TabIndex = 0
        Me.rdbIchimokuSignalLaggingSpan.TabStop = True
        Me.rdbIchimokuSignalLaggingSpan.Text = "Lagging Span"
        Me.rdbIchimokuSignalLaggingSpan.UseVisualStyleBackColor = True
        '
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1244, 690)
        Me.Controls.Add(Me.pnlIchimokuSignal)
        Me.Controls.Add(Me.pnlSupertrendConfirmation)
        Me.Controls.Add(Me.pnlPriceVolumeImbalance)
        Me.Controls.Add(Me.pnlMultiTFMultiMA)
        Me.Controls.Add(Me.pnlGraphAngle)
        Me.Controls.Add(Me.pnlLowSLFractal)
        Me.Controls.Add(Me.lblDescription)
        Me.Controls.Add(Me.btnExport)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.dgvSignal)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmMain"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Algo2Trade Signal Check"
        CType(Me.nmrcTimeFrame, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        CType(Me.dgvSignal, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlLowSLFractal.ResumeLayout(False)
        Me.pnlLowSLFractal.PerformLayout()
        Me.pnlGraphAngle.ResumeLayout(False)
        Me.pnlGraphAngle.PerformLayout()
        Me.pnlMultiTFMultiMA.ResumeLayout(False)
        Me.pnlMultiTFMultiMA.PerformLayout()
        CType(Me.nmrcMultiTFMultiMAHigherTF, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlPriceVolumeImbalance.ResumeLayout(False)
        Me.pnlPriceVolumeImbalance.PerformLayout()
        CType(Me.nmrcPriceVolumeImbalancePeriod, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlSupertrendConfirmation.ResumeLayout(False)
        Me.pnlSupertrendConfirmation.PerformLayout()
        Me.pnlIchimokuSignal.ResumeLayout(False)
        Me.grpbxIchimokuSignalType.ResumeLayout(False)
        Me.grpbxIchimokuSignalType.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents saveFile As SaveFileDialog
    Friend WithEvents btnExport As Button
    Friend WithEvents chkbHA As CheckBox
    Friend WithEvents btnCancel As Button
    Friend WithEvents nmrcTimeFrame As NumericUpDown
    Friend WithEvents lblTimeFrame As Label
    Friend WithEvents dtpckrToDate As DateTimePicker
    Friend WithEvents dtpckrFromDate As DateTimePicker
    Friend WithEvents lblToDate As Label
    Friend WithEvents lblFromDate As Label
    Friend WithEvents btnView As Button
    Friend WithEvents txtInstrumentName As TextBox
    Friend WithEvents Label4 As Label
    Friend WithEvents cmbCategory As ComboBox
    Friend WithEvents lblCategory As Label
    Friend WithEvents cmbRule As ComboBox
    Friend WithEvents lblRule As Label
    Friend WithEvents lblProgress As Label
    Friend WithEvents Panel1 As Panel
    Friend WithEvents dgvSignal As DataGridView
    Friend WithEvents txtFilePath As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents btnBrowse As Button
    Friend WithEvents opnFile As OpenFileDialog
    Friend WithEvents lblDescription As Label
    Friend WithEvents pnlLowSLFractal As Panel
    Friend WithEvents txtLowSLFractalATRMultiplier As TextBox
    Friend WithEvents lblLowSLFractalATRMultiplier As Label
    Friend WithEvents pnlGraphAngle As Panel
    Friend WithEvents dtPckrGraphAngleEndTime As DateTimePicker
    Friend WithEvents Label2 As Label
    Friend WithEvents txtGraphAngleSDMul As TextBox
    Friend WithEvents Label3 As Label
    Friend WithEvents txtGraphAngleCandlePer As TextBox
    Friend WithEvents Label5 As Label
    Friend WithEvents pnlMultiTFMultiMA As Panel
    Friend WithEvents nmrcMultiTFMultiMAHigherTF As NumericUpDown
    Friend WithEvents Label7 As Label
    Friend WithEvents pnlPriceVolumeImbalance As Panel
    Friend WithEvents nmrcPriceVolumeImbalancePeriod As NumericUpDown
    Friend WithEvents Label8 As Label
    Friend WithEvents Label6 As Label
    Friend WithEvents txtPriceVolumeImbalanceSDMul As TextBox
    Friend WithEvents pnlSupertrendConfirmation As Panel
    Friend WithEvents txtSupertrendConfirmationMaxRangePer As TextBox
    Friend WithEvents lblSupertrendConfirmationMaxRangePer As Label
    Friend WithEvents pnlIchimokuSignal As Panel
    Friend WithEvents grpbxIchimokuSignalType As GroupBox
    Friend WithEvents rdbIchimokuSignalLaggingSpanConversionBaseLine As RadioButton
    Friend WithEvents rdbIchimokuSignalLaggingSpan As RadioButton
End Class
