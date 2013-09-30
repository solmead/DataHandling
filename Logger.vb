'/*
' * Copyright (C) 2009-2012 Solmead Productions
' *
' * == BEGIN LICENSE ==
' *
' * Licensed under the terms of any of the following licenses at your
' * choice:
' *
' *  - GNU General Public License Version 2 or later (the "GPL")
' *    http://www.gnu.org/licenses/gpl.html
' *
' *  - GNU Lesser General Public License Version 2.1 or later (the "LGPL")
' *    http://www.gnu.org/licenses/lgpl.html
' *
' *  - Mozilla Public License Version 1.1 or later (the "MPL")
' *    http://www.mozilla.org/MPL/MPL-1.1.html
' *
' * == END LICENSE ==
' */

Public Interface LogEnabled
    Event DebugMsg(Msg As String, SendEmail As Boolean)
    Event TraceMsg(Msg As String)
    Event ErrorMsg(Msg As String)
    Event PercentDone(ByVal Area As String, ByVal Percent As Double)
    Sub SetLog(NewLog As Logger)
End Interface

Public MustInherit Class Loggable
    Implements LogEnabled


    Public Event DebugMsg(Msg As String, SendEmail As Boolean) Implements DataHandling.LogEnabled.DebugMsg
    Public Event ErrorMsg(Msg As String) Implements DataHandling.LogEnabled.ErrorMsg
    Public Event TraceMsg(Msg As String) Implements LogEnabled.TraceMsg
    Public Event PercentDone(ByVal Area As String, ByVal Percent As Double) Implements DataHandling.LogEnabled.PercentDone

    Private m_Log As Logger

    Public Overridable Sub LogChanged(OldLog As Logger, NewLog As Logger)

    End Sub

    Public Property MainLog As Logger
        Get
            Return m_Log
        End Get
        Set(value As Logger)

            If m_Log IsNot Nothing AndAlso m_Log IsNot value Then
                Dim List = m_Log.get_LoggedObjsList
                For Each I In List
                    m_Log.RemoveLoggedObj(I)
                    If value IsNot Nothing Then
                        value.AddLoggedObj(I)
                    End If
                Next
            End If
            Call LogChanged(m_Log, value)
            m_Log = value
            value.AddLoggedObj(Me)
        End Set
    End Property

    Public Sub AddLoggedObj(Obj As Loggable)
        If MainLog Is Nothing Then
            MainLog = New Logger
        End If
        If MainLog IsNot Nothing Then
            MainLog.AddLoggedObj(Obj)
            'Obj.Log = Log
        End If
    End Sub
    Public Sub RemoveLoggedObj(Obj As Loggable)
        If MainLog IsNot Nothing Then
            MainLog.RemoveLoggedObj(Obj)
            'Obj.Log = Nothing
        End If
    End Sub

    Public Sub PercentageDone(ByVal Area As String, ByVal Percent As Double)
        RaiseEvent PercentDone(Area, Percent)
    End Sub

    Public Sub DebugMessage(ByVal Msg As String, Optional SendEmail As Boolean = False)
        RaiseEvent DebugMsg(Msg, SendEmail)
    End Sub

    Public Sub TraceMessage(ByVal Msg As String)
        RaiseEvent TraceMsg(Msg)
    End Sub
    Public Sub ErrorMessage(ByVal Msg As String)
        RaiseEvent ErrorMsg(Msg)
    End Sub

    Public Sub SetLog(NewLog As Logger) Implements LogEnabled.SetLog
        MainLog = NewLog
    End Sub
End Class


Public Class Logger
    Implements LogEnabled

    Public Event DebugMsg(Msg As String, SendEmail As Boolean) Implements DataHandling.LogEnabled.DebugMsg
    Public Event ErrorMsg(Msg As String) Implements DataHandling.LogEnabled.ErrorMsg
    Public Event TraceMsg(Msg As String) Implements DataHandling.LogEnabled.TraceMsg
    Public Event PercentDone(ByVal Area As String, ByVal Percent As Double) Implements DataHandling.LogEnabled.PercentDone

    Public Property Tracing As Boolean
    Public Property LogFile As New System.IO.FileInfo("c:\console_log.log")

    Private Property MsgList As New List(Of String)
    Public Property Name As String = ""

    Private LoggedObjs As New List(Of LogEnabled)
    Private WithEvents Obj As LogEnabled

    Public Property LastArea As String = ""
    Public Property LastPercent As Double = 0

    Private Shared _GlobalLog As Logger

    Public Shared ReadOnly Property GlobalLog As Logger
        Get
            If _GlobalLog Is Nothing Then
                _GlobalLog = New Logger("GlobalLog")
            End If
            Return _GlobalLog
        End Get
    End Property

    Private Function GetDateTimeSerial() As String
        Return Now.Year & Now.Month.ToString("00") & Now.Day.ToString("00") & Now.Hour.ToString("00") & Now.Minute.ToString("00") & Now.Second.ToString("00")
    End Function

    Public Function get_LoggedObjsList() As List(Of LogEnabled)
        Return LoggedObjs.ToList
    End Function

    Public Sub New()
        Me.New("", "")
    End Sub
    Public Sub New(SubName As String)
        Me.New(SubName, "")
    End Sub

    Public Sub New(SubName As String, Directory As String)
        Name = SubName
        If Name = "" Then
            Name = "Temp"
        End If
        If Directory = "" Then
            Directory = AppDomain.CurrentDomain.BaseDirectory
        End If
        Dim DI As New System.IO.DirectoryInfo(Directory)


        LogFile = New System.IO.FileInfo(DI.FullName & "\Logs\" & Name & "_" & GetDateTimeSerial() & ".log")
        If Not LogFile.Directory.Exists Then
            LogFile.Directory.Create()
        End If
        DebugMessage(LogFile.FullName)

    End Sub
    Public Sub AddLoggedObj(Obj As LogEnabled)
        If Not LoggedObjs.Contains(Obj) Then
            LoggedObjs.Add(Obj)
            AddHandler Obj.DebugMsg, AddressOf Me.Obj_DebugMsg
            AddHandler Obj.TraceMsg, AddressOf Me.Obj_TraceMsg
            AddHandler Obj.ErrorMsg, AddressOf Me.Obj_ErrorMsg
            AddHandler Obj.PercentDone, AddressOf Me.Obj_PercentageDone
            Obj.SetLog(Me)
        End If
    End Sub
    Public Sub RemoveLoggedObj(Obj As LogEnabled)
        If LoggedObjs.Contains(Obj) Then
            LoggedObjs.Remove(Obj)
            RemoveHandler Obj.DebugMsg, AddressOf Me.Obj_DebugMsg
            RemoveHandler Obj.TraceMsg, AddressOf Me.Obj_TraceMsg
            RemoveHandler Obj.ErrorMsg, AddressOf Me.Obj_ErrorMsg
            RemoveHandler Obj.PercentDone, AddressOf Me.Obj_PercentageDone

        End If

    End Sub
    Public Function GetMsgList() As List(Of String)
        Dim TL As New List(Of String)
        SyncLock MsgList
            TL.InsertRange(0, MsgList)
            MsgList.Clear()
        End SyncLock
        Return TL
    End Function

    Public Sub PercentageDone(ByVal Area As String, ByVal Percent As Double)
        LastArea = Area
        LastPercent = Percent

        Console.Write(Area & " - " & (Percent * 100).ToString("0.00") & "%          " & vbCr)
        RaiseEvent PercentDone(Area, Percent)
    End Sub
    Public Sub ErrorMessage(ByVal Msg As String)
        Dim Msg2 As String = Now.ToLongTimeString & " Error: " & Msg
        SyncLock MsgList
            MsgList.Add(Msg2)
        End SyncLock

        Console.WriteLine(Msg2)
        Debug.WriteLine(Msg2)
        RaiseEvent ErrorMsg(Msg)


        'Console.WriteLine(Msg)
        'Debug.WriteLine(Msg)
        Try
            Dim FW = LogFile.AppendText
            FW.WriteLine(Msg2)
            FW.Close()
        Catch ex As Exception

        End Try
        Dim Emails As New List(Of System.Net.Mail.MailAddress)
        Dim Emls = Split(Settings.NotificationEmails, ",")
        Emails.Add(New System.Net.Mail.MailAddress("chris@7hillschurch.tv", "Chris"))
        For Each eml In Emls
            Emails.Add(New System.Net.Mail.MailAddress(eml, eml))
        Next

        SendingEmail.SendEmail(Emails, Name & " Error Message", Msg)
    End Sub
    Public Sub DebugMessage(ByVal Msg As String, Optional SendEmail As Boolean = False)
        Dim Msg2 As String = Now.ToLongTimeString & " " & Msg
        SyncLock MsgList
            MsgList.Add(Msg2)
        End SyncLock
        Console.WriteLine(Msg2)
        Debug.WriteLine(Msg2)
        RaiseEvent DebugMsg(Msg, SendEmail)


        'Console.WriteLine(Msg)
        'Debug.WriteLine(Msg)
        Try
            Dim FW = LogFile.AppendText
            FW.WriteLine(Msg2)
            FW.Close()
        Catch ex As Exception

        End Try
        If SendEmail Then
            Dim Emails As New List(Of System.Net.Mail.MailAddress)
            Dim Emls = Split(Settings.NotificationEmails, ",")
            Emails.Add(New System.Net.Mail.MailAddress("chris@7hillschurch.tv", "Chris"))
            For Each eml In Emls
                Emails.Add(New System.Net.Mail.MailAddress(eml, eml))
            Next

            SendingEmail.SendEmail(Emails, Name & " Message", Msg)
        End If
    End Sub


    Public Sub TraceMessage(ByVal Msg As String)
        'SyncLock DList
        '    DList.Add(Msg)
        'End SyncLock
        If Not Tracing Then Return
        RaiseEvent TraceMsg(Msg)

        Msg = Now.ToLongTimeString & " " & Msg
        'Console.WriteLine(Msg)
        'Debug.WriteLine(Msg)
        Try
            Dim FW = LogFile.AppendText
            FW.WriteLine(Msg)
            FW.Close()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub Obj_DebugMsg(Msg As String, SendEmail As Boolean) Handles Obj.DebugMsg
        Me.DebugMessage(Msg)
    End Sub

    Private Sub Obj_ErrorMsg(Msg As String) Handles Obj.ErrorMsg
        Me.ErrorMessage(Msg)
    End Sub

    Private Sub Obj_TraceMsg(Msg As String) Handles Obj.TraceMsg
        Me.TraceMessage(Msg)
    End Sub
    Private Sub Obj_PercentageDone(ByVal Area As String, ByVal Percent As Double) Handles Obj.PercentDone
        Me.PercentageDone(Area, Percent)
    End Sub

    Public Sub SetLog(NewLog As Logger) Implements LogEnabled.SetLog

    End Sub
End Class
