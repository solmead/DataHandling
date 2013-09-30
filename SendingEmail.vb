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

Imports System.Net
Imports System.Net.Mail


Public Class SendingEmail
    Public Shared Log As Logger = Logger.GlobalLog

    Public Shared Sub SendEmail(ByVal ToUser As MailAddress, ByVal FromAddress As MailAddress, ByVal Subject As String, ByVal Body As String, Optional ByVal CCAddress As MailAddress = Nothing, Optional ByVal ReplyTo As MailAddress = Nothing)
        Dim Message As New MailMessage()
        Message.To.Add(ToUser)
        If CCAddress IsNot Nothing Then Message.CC.Add(CCAddress)
        Message.Bcc.Add(Settings.AdminEmail)
        If ReplyTo IsNot Nothing Then
            Message.ReplyToList.Add(ReplyTo)
        End If
        Message.From = FromAddress
        Message.Subject = Subject
        Message.Body = Body
        Message.IsBodyHtml = True
        Dim Client = New SmtpClient
        Try
            Client.Send(Message)
        Catch ex As Exception
            If Log Is Nothing Then
                Log = New Logger("SendingEmail")
            End If
            Dim Tstr = "" & "<br/>" & vbCrLf
            Tstr = Tstr & "To=" & ToUser.Address & "<br/>" & vbCrLf
            Tstr = Tstr & "FromAddress=" & FromAddress.Address & "<br/>" & vbCrLf
            Tstr = Tstr & "Subject=" & Subject & "<br/>" & vbCrLf
            Tstr = Tstr & "Body=" & Body & "<br/>" & vbCrLf
            If CCAddress IsNot Nothing Then
                Tstr = Tstr & "CCAddress=" & CCAddress.Address & "<br/>" & vbCrLf
            End If
            If ReplyTo IsNot Nothing Then
                Tstr = Tstr & "ReplyTo=" & ReplyTo.Address & "<br/>" & vbCrLf
            End If

            Log.DebugMessage("SendEmail Error:" & Tstr & ex.ToString)
        End Try
    End Sub
    Public Shared Sub SendEmail(ByVal ToUser As MailAddress, ByVal Subject As String, ByVal Body As String, Optional ByVal CCAddress As MailAddress = Nothing, Optional ByVal ReplyTo As MailAddress = Nothing)
        Dim Message As New MailMessage()
        Message.To.Add(ToUser)
        If CCAddress IsNot Nothing Then Message.CC.Add(CCAddress)
        Message.Bcc.Add(Settings.AdminEmail)
        If ReplyTo IsNot Nothing Then
            Message.ReplyToList.Add(ReplyTo)
        End If
        Message.Subject = Subject
        Message.Body = Body
        Message.IsBodyHtml = True
        Dim Client = New SmtpClient
        Try
            Client.Send(Message)
        Catch ex As Exception
            If Log Is Nothing Then
                Log = New Logger("SendingEmail")
            End If
            Dim Tstr = "" & "<br/>" & vbCrLf
            Tstr = Tstr & "To=" & ToUser.Address & "<br/>" & vbCrLf
            Tstr = Tstr & "Subject=" & Subject & "<br/>" & vbCrLf
            Tstr = Tstr & "Body=" & Body & "<br/>" & vbCrLf
            If CCAddress IsNot Nothing Then
                Tstr = Tstr & "CCAddress=" & CCAddress.Address & "<br/>" & vbCrLf
            End If
            If ReplyTo IsNot Nothing Then
                Tstr = Tstr & "ReplyTo=" & ReplyTo.Address & "<br/>" & vbCrLf
            End If

            Log.DebugMessage("SendEmail Error:" & Tstr & ex.ToString)
        End Try
    End Sub
    Public Shared Sub SendEmail(ByVal ToUserList As List(Of MailAddress), ByVal Subject As String, ByVal Body As String, Optional ByVal CCAddress As MailAddress = Nothing, Optional ByVal ReplyTo As MailAddress = Nothing)
        Dim Message As New MailMessage()
        For Each U In ToUserList
            Message.To.Add(U)
        Next
        If CCAddress IsNot Nothing Then Message.CC.Add(CCAddress)
        Message.Bcc.Add(Settings.AdminEmail)
        If ReplyTo IsNot Nothing Then
            Message.ReplyToList.Add(ReplyTo)
        End If
        Message.Subject = Subject
        Message.Body = Body
        Message.IsBodyHtml = True
        Dim Client = New SmtpClient
        Try
            Client.Send(Message)
        Catch ex As Exception
            If Log Is Nothing Then
                Log = New Logger("SendingEmail")
            End If
            Dim Tstr = "" & "<br/>" & vbCrLf
            Tstr = Tstr & "To List<br/>" & vbCrLf
            For Each U In ToUserList
                Tstr = Tstr & "To:" & U.Address & "<br/>" & vbCrLf
            Next
            Tstr = Tstr & "Subject=" & Subject & "<br/>" & vbCrLf
            Tstr = Tstr & "Body=" & Body & "<br/>" & vbCrLf
            If CCAddress IsNot Nothing Then
                Tstr = Tstr & "CCAddress=" & CCAddress.Address & "<br/>" & vbCrLf
            End If
            If ReplyTo IsNot Nothing Then
                Tstr = Tstr & "ReplyTo=" & ReplyTo.Address & "<br/>" & vbCrLf
            End If

            Log.DebugMessage("SendEmail Error:" & Tstr & ex.ToString)
        End Try
    End Sub
End Class
