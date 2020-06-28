Imports Algo2TradeBLL
Imports Utilities.DAL
Imports System.IO
Imports System.Threading
Public Class StockSelection

#Region "Events/Event handlers"
    Public Event DocumentDownloadComplete()
    Public Event DocumentRetryStatus(ByVal currentTry As Integer, ByVal totalTries As Integer)
    Public Event Heartbeat(ByVal msg As String)
    Public Event WaitingFor(ByVal elapsedSecs As Integer, ByVal totalSecs As Integer, ByVal msg As String)
    'The below functions are needed to allow the derived classes to raise the above two events
    Protected Overridable Sub OnDocumentDownloadComplete()
        RaiseEvent DocumentDownloadComplete()
    End Sub
    Protected Overridable Sub OnDocumentRetryStatus(ByVal currentTry As Integer, ByVal totalTries As Integer)
        RaiseEvent DocumentRetryStatus(currentTry, totalTries)
    End Sub
    Protected Overridable Sub OnHeartbeat(ByVal msg As String)
        RaiseEvent Heartbeat(msg)
    End Sub
    Protected Overridable Sub OnWaitingFor(ByVal elapsedSecs As Integer, ByVal totalSecs As Integer, ByVal msg As String)
        RaiseEvent WaitingFor(elapsedSecs, totalSecs, msg)
    End Sub
#End Region

    Private _cts As CancellationTokenSource
    Private ReadOnly _category As String
    Private ReadOnly _fileName As String
    Private ReadOnly _cmn As Common = Nothing
    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal stockCategory As String, ByVal cmn As Common, ByVal fileName As String)
        _cts = canceller
        _category = stockCategory
        _cmn = cmn
        _fileName = fileName
    End Sub
    Public Async Function GetStockList(ByVal tradingDate As Date) As Task(Of List(Of String))
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As List(Of String) = Nothing
        _cts.Token.ThrowIfCancellationRequested()
        If _fileName IsNot Nothing AndAlso File.Exists(_fileName) Then
            Dim dt As DataTable = Nothing
            Using csvHelper As New CSVHelper(_fileName, ",", _cts)
                dt = csvHelper.GetDataTableFromCSV(1)
            End Using
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                Dim dateList As List(Of Date) = Nothing
                For i = 0 To dt.Rows.Count - 1
                    Dim rowDate As Date = dt.Rows(i).Item("Date")

                    If dateList Is Nothing Then dateList = New List(Of Date)
                    If Not dateList.Contains(rowDate.Date) Then dateList.Add(rowDate.Date)
                Next

                If dateList IsNot Nothing AndAlso dateList.Count > 0 Then
                    Dim dateToCheck As Date = tradingDate.Date
                    If Not dateList.Contains(tradingDate.Date) Then
                        dateToCheck = dateList.Max(Function(x)
                                                       If x.Date <= tradingDate.Date Then
                                                           Return x.Date
                                                       Else
                                                           Return Date.MinValue
                                                       End If
                                                   End Function)
                    End If
                    If dateToCheck.Date <> Date.MinValue Then
                        For i = 0 To dt.Rows.Count - 1
                            Dim rowDate As Date = dt.Rows(i).Item("Date")
                            If rowDate.Date = dateToCheck.Date Then
                                If ret Is Nothing Then ret = New List(Of String)
                                Dim tradingSymbol As String = dt.Rows(i).Item("Trading Symbol")
                                Dim instrumentName As String = Nothing
                                If tradingSymbol.Contains("FUT") Then
                                    instrumentName = tradingSymbol.Remove(tradingSymbol.Count - 8)
                                Else
                                    instrumentName = tradingSymbol
                                End If
                                ret.Add(instrumentName)
                            End If
                        Next
                    End If
                End If
            End If
        Else
            Throw New ApplicationException("Instrument File not available")
        End If
        Return ret
    End Function
End Class
