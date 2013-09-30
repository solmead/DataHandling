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
Imports System.Linq
Imports System.Runtime.Serialization
Imports System.Text
Imports System.Web
Imports System.Web.Mvc
Imports System.Xml
Imports System.Xml.Linq
Imports System.Xml.Serialization
Imports System.Reflection
Imports System.Data
Imports System.Data.Linq

Public Class XmlResult
    Inherits ActionResult

    Public Property ContentType As String
    Public Property ContentEncoding As Encoding
    Public Property Data As Object


    Public Sub New()

    End Sub
    Public Sub New(data As Object)
        Me.Data = data
    End Sub


    Public Overrides Sub ExecuteResult(context As System.Web.Mvc.ControllerContext)
        If (context Is Nothing) Then
            Throw New ArgumentNullException("context")
        End If

        Dim response As HttpResponseBase = context.HttpContext.Response
        If (Not String.IsNullOrEmpty(Me.ContentType)) Then
            response.ContentType = Me.ContentType
        Else
            response.ContentType = "text/xml"
        End If

        If (Me.ContentEncoding IsNot Nothing) Then
            response.ContentEncoding = Me.ContentEncoding
        End If

        If (Me.Data IsNot Nothing) Then

            If (TypeOf Me.Data Is XmlNode) Then
                response.Write((CType(Data, XmlNode).OuterXml))
            ElseIf (TypeOf Me.Data Is XNode) Then
                response.Write((CType(Data, XNode).ToString()))
            Else
                Dim dataType = Me.Data.GetType()
                Dim List As List(Of Object) = Nothing
                If TypeOf Me.Data Is IEnumerable Then
                    List = (From I In CType(Me.Data, IEnumerable).AsQueryable Select I).ToList
                Else
                    List = New List(Of Object)
                    List.Add(Me.Data)
                End If

                'Dim D3 = List.ToDataTable
                Dim XmlDoc As New XmlDocument
                Dim BaseElem = XmlDoc.CreateElement("Response")
                XmlDoc.AppendChild(BaseElem)
                For Each Obj In List
                    BaseElem.AppendChild(GetXMLFromObj(XmlDoc, Obj))
                Next
                'For Each Row In D3.Rows
                '    Dim CurElem = XmlDoc.CreateElement("Object")
                '    BaseElem.AppendChild(CurElem)

                '    For Each col As DataColumn In D3.Columns

                '        Dim CurCol = XmlDoc.CreateElement(col.ColumnName)
                '        CurElem.AppendChild(CurCol)
                '        CurCol.InnerText = Row(col.ColumnName)
                '    Next
                'Next
                XmlDoc.Save(response.OutputStream)
            End If
        End If
    End Sub


    Private Function GetXMLFromObj(XMLDoc As XmlDocument, Obj As Object, Optional ObjectName As String = "Object")
        Dim CurElem = XMLDoc.CreateElement(ObjectName)

        Dim Tp As Type = Obj.GetType()
        CurElem.SetAttribute("Type", Tp.ToString)
        Dim props = Tp.GetProperties((BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.FlattenHierarchy))
        For Each p In props

            Dim V As Object = Nothing
            Try
                V = p.GetValue(Obj, Nothing)
            Catch ex As Exception
                'Debug.WriteLine("Copy Into Error Prop:" & p.Name)
                V = ""
            End Try
            Dim TName As String = ""
            Dim BTName As String = ""
            Try
                TName = p.PropertyType.FullName
                If p.PropertyType.BaseType IsNot Nothing Then
                    BTName = p.PropertyType.BaseType.FullName
                End If
            Catch ex As Exception
                'WriteDebug(ex.ToString)
            End Try
            If V Is Nothing Then
                V = "{Nothing}"
            End If
            System.Diagnostics.Debug.WriteLine("[" & p.Name & "]=[" & V.ToString & "]")
            System.Diagnostics.Debug.WriteLine("Type = [" & TName & "]")
            System.Diagnostics.Debug.WriteLine("BType = [" & BTName & "]")
            If V Is Nothing Then
                Dim a As Integer = 1
            End If
            ' If Not TName.Contains("EntitySet") AndAlso Not (TName.Contains("IEntity") OrElse BTName.Contains("IEntity")) AndAlso V IsNot Nothing Then
            'If V IsNot Nothing Then
            '    Try
            '        'WriteDebug(p.Name & "=""" & V & """")
            '    Catch ex As Exception
            '        'WriteDebug(p.Name & "=[Unknown]")
            '    End Try
            'Else
            '    'WriteDebug(p.Name & "= Nothing")
            'End If
            Try
                

                If TName.Contains("String") OrElse BTName.Contains("ValueType") Then
                    Dim CurCol = XMLDoc.CreateElement(p.Name)
                    CurElem.AppendChild(CurCol)
                    CurCol.SetAttribute("Type", TName)
                    CurCol.SetAttribute("BaseType", BTName)
                    CurCol.InnerText = V.ToString
                ElseIf TypeOf V Is Integer OrElse TypeOf V Is String Then
                    Dim CurCol = XMLDoc.CreateElement(p.Name)
                    CurElem.AppendChild(CurCol)
                    CurCol.SetAttribute("Type", TName)
                    CurCol.SetAttribute("BaseType", BTName)
                    CurCol.InnerText = V.ToString
                ElseIf TypeOf V Is IEnumerable Then
                    Dim CurCol = XMLDoc.CreateElement(p.Name)
                    CurElem.AppendChild(CurCol)
                    CurCol.SetAttribute("Type", TName)
                    CurCol.SetAttribute("BaseType", BTName)
                    For Each It In V
                        CurCol.AppendChild(GetXMLFromObj(XMLDoc, It, p.Name & "_Item"))
                    Next
                Else
                    CurElem.AppendChild(GetXMLFromObj(XMLDoc, V, p.Name))
                    'CurCol.InnerText = V.ToString
                End If
            Catch ex As Exception
                Dim a As Integer = 1
            End Try
            'Else
            'WriteDebug(p.Name & " was an entity.")
            'End If
        Next

        Return CurElem
    End Function
End Class
