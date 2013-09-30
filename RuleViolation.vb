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

Imports System.Reflection
Imports System.Data
Imports System.Data.Linq

Public Class RuleViolation
    Private m_ErrorMessage As String = ""
    Private m_PropertyName As String = ""
    Public Property ErrorMessage() As String
    	Get
    		Return m_ErrorMessage
    	End Get
    	Set(ByVal Value As String)
    		m_ErrorMessage = Value
    	End Set
    End Property

    Public Property PropertyName() As String
    	Get
    		Return m_PropertyName
    	End Get
    	Set(ByVal Value As String)
    		m_PropertyName = Value
    	End Set
    End Property

    Public Sub New()

    End Sub
    Public Sub New(ByVal errorMessage As String)
        Me.ErrorMessage = errorMessage
    End Sub
    Public Sub New(ByVal errorMessage As String, ByVal propertyName As String)
        Me.ErrorMessage = errorMessage
        Me.PropertyName = propertyName
    End Sub

End Class
