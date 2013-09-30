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

Imports System
Imports System.Diagnostics
Imports System.Globalization
Imports System.IO
Imports System.Text

Public Class SQLLogger
    Inherits TextWriter

    Public Property isOpen As Boolean
    Public Property level As Integer
    Public Property category As String = ""
    Public mEncoding As UnicodeEncoding

    Public Sub New()
        Me.New(0, Debugger.DefaultCategory)
    End Sub
    Public Sub New(ByVal level As Integer, ByVal category As String)
        Me.new(level, category, CultureInfo.CurrentCulture)
    End Sub
    Public Sub New(ByVal level As Integer, ByVal category As String, ByVal formatProvider As IFormatProvider)
        MyBase.New(formatProvider)

        Me.level = level
        Me.category = category
        Me.isOpen = True

    End Sub
    Public Overrides Sub write(ByVal value As Char)
        If (Not isOpen) Then

            Throw New ObjectDisposedException(Nothing)
        End If
        Debugger.Log(level, category, value.ToString())
    End Sub
    Public Overrides Sub write(ByVal value As String)
        If (Not isOpen) Then

            Throw New ObjectDisposedException(Nothing)
        End If
        If (value IsNot Nothing) Then

            Debugger.Log(level, category, value)
        End If
    End Sub
    Public Overrides Sub write(ByVal buffer As Char(), ByVal index As Integer, ByVal count As Integer)
        If (Not isOpen) Then

            Throw New ObjectDisposedException(Nothing)
        End If
        If (buffer Is Nothing OrElse index < 0 OrElse count < 0 OrElse buffer.Length - index < count) Then

            MyBase.Write(buffer, index, count)
        End If
        Debugger.Log(level, category, New String(buffer, index, count))
    End Sub

    Public Overrides ReadOnly Property Encoding As System.Text.Encoding
        Get
            If (mEncoding Is Nothing) Then
                mEncoding = New UnicodeEncoding(False, False)
            End If
            Return mEncoding
        End Get
    End Property
End Class
