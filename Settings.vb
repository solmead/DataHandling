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

Imports System.Configuration
Imports System.Web.Configuration
Imports System.Web.Configuration.WebConfigurationManager
Imports System.Net
Imports System.Net.Mail

Public Class Settings
    Private Shared mconfig As Configuration = Nothing

    Public Shared ReadOnly Property FileLocation As String
        Get
            Dim fl = Settings.ConfigProperty("FileLocation")
            If fl.StartsWith("/") Then
                fl = System.Web.HttpContext.Current.Server.MapPath(fl)
            End If
            Return fl
        End Get
    End Property

    Public Shared ReadOnly Property AdminEmail As MailAddress
        Get
            Dim fl = Settings.ConfigProperty("AdminEmailAddress")
            If String.IsNullOrWhiteSpace(fl) Then
                fl = "none@none.com"
                Settings.ConfigProperty("AdminEmailAddress") = fl
            End If
            Return New MailAddress(fl, fl)
        End Get
    End Property
    Public Shared ReadOnly Property ConnStringName() As String
        Get
            Return "MainDBConnString"
        End Get
    End Property
    Public Shared ReadOnly Property DoesDatabaseExist() As Boolean
        Get
            Try
                Dim MyConn As New SqlClient.SqlConnection(ConnectionString)
                MyConn.Open()
                MyConn.Close()
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Get
    End Property
    Public Shared Function GetConfiguration() As Configuration
        If mconfig Is Nothing Then
            Try
                mconfig = WebConfigurationManager.OpenWebConfiguration("~")
            Catch ex As Exception
                Try
                    mconfig = ConfigurationManager.OpenExeConfiguration("")
                Catch ex2 As Exception
                    mconfig = WebConfigurationManager.OpenWebConfiguration(Nothing)
                End Try
                'config = WebConfigurationManager.OpenWebConfiguration("c:\")
            End Try
        End If
        Return mconfig
    End Function
    Private Shared Property NamedConnString(ByVal ConnName As String) As String
        Get
            Dim config = GetConfiguration()
            Dim CSS As ConnectionStringSettings = config.ConnectionStrings.ConnectionStrings(ConnName)
            If CSS IsNot Nothing Then
                Return CSS.ConnectionString
            End If
            Return ""
        End Get
        Set(ByVal value As String)
            Dim config = GetConfiguration()
            Dim CSS As ConnectionStringSettings = config.ConnectionStrings.ConnectionStrings(ConnName)
            If CSS Is Nothing Then
                CSS = New ConnectionStringSettings(ConnName, value, "System.Data.SqlClient")
                config.ConnectionStrings.ConnectionStrings.Add(CSS)
            Else
                CSS.ConnectionString = value
            End If
            config.Save()
        End Set
    End Property
    Public Shared Property ConfigProperty(ByVal Name As String) As String
        Get
            Dim config = GetConfiguration()
            Dim kvce As KeyValueConfigurationElement = config.AppSettings.Settings(Name)

            If kvce IsNot Nothing Then
                Return kvce.Value
            End If
            kvce = New KeyValueConfigurationElement(Name, "")
            config.AppSettings.Settings.Add(kvce)
            Try
                config.Save()
            Catch ex As Exception

            End Try
            Return ""
        End Get
        Set(ByVal value As String)
            Dim config = GetConfiguration()
            Dim kvce As KeyValueConfigurationElement = config.AppSettings.Settings(Name)
            If kvce Is Nothing Then
                kvce = New KeyValueConfigurationElement(Name, value)
                config.AppSettings.Settings.Add(kvce)
            Else
                kvce.Value = value
            End If
            Try
                config.Save()
            Catch ex As Exception

            End Try
        End Set
    End Property
    Public Shared Function GetConfigProperty(Name As String) As String
        Return ConfigProperty(Name)
    End Function
    Public Shared Sub SetConfigProperty(Name As String, Value As String)
        ConfigProperty(Name) = Value
    End Sub
    'NotificationEmails

    Public Shared Property NotificationEmails() As String
        Get

            If Settings.ConfigProperty("NotificationEmails") = "" Then
                Settings.ConfigProperty("NotificationEmails") = "website@7hillschurch.tv"
            End If
            Return ConfigProperty("NotificationEmails")
        End Get
        Set(ByVal value As String)
            ConfigProperty("NotificationEmails") = value
        End Set
    End Property
    Public Shared Property SiteName() As String
        Get
            Return ConfigProperty("SiteName")
        End Get
        Set(ByVal value As String)
            ConfigProperty("SiteName") = value
        End Set
    End Property

    Public Shared ReadOnly Property ConnectionString() As String
        Get
            Return NamedConnString(ConnStringName)
        End Get
    End Property

    Public Shared Property DB_ServerName() As String
        Get
            Return ConfigProperty("DB_ServerName")
        End Get
        Set(ByVal value As String)
            ConfigProperty("DB_ServerName") = value
            NamedConnString(ConnStringName) = "Data Source=" & DB_ServerName & ";Initial Catalog=" & DB_Name & ";Persist Security Info=True;User ID=" & DB_UserName & ";Password=" & DB_Password & ""
        End Set
    End Property
    Public Shared Property DB_Name() As String
        Get
            Return ConfigProperty("DB_Name")
        End Get
        Set(ByVal value As String)
            ConfigProperty("DB_Name") = value
            NamedConnString(ConnStringName) = "Data Source=" & DB_ServerName & ";Initial Catalog=" & DB_Name & ";Persist Security Info=True;User ID=" & DB_UserName & ";Password=" & DB_Password & ""
        End Set
    End Property
    Public Shared Property DB_UserName() As String
        Get
            Return ConfigProperty("DB_UserName")
        End Get
        Set(ByVal value As String)
            ConfigProperty("DB_UserName") = value
            NamedConnString(ConnStringName) = "Data Source=" & DB_ServerName & ";Initial Catalog=" & DB_Name & ";Persist Security Info=True;User ID=" & DB_UserName & ";Password=" & DB_Password & ""
        End Set
    End Property
    Public Shared Property DB_Password() As String
        Get
            Return ConfigProperty("DB_Password")
        End Get
        Set(ByVal value As String)
            ConfigProperty("DB_Password") = value
            NamedConnString(ConnStringName) = "Data Source=" & DB_ServerName & ";Initial Catalog=" & DB_Name & ";Persist Security Info=True;User ID=" & DB_UserName & ";Password=" & DB_Password & ""
        End Set
    End Property
End Class



