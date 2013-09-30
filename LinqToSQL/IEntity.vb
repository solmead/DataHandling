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

Namespace LinqToSql

    Public MustInherit Class IEntity(Of t)
        Inherits IEntity
        Implements IValidatableObject

        Public Shared LastIDSaved As Object

        'Private Shared m_InternalDB As DataContext
        'Private Shared Function DataContext() As DataContext
        '    If m_InternalDB Is Nothing Then
        '        m_InternalDB = BaseData.CVP_DataContext
        '    End If
        '    Return m_InternalDB
        'End Function
        Public Property OverrideValidation As Boolean

        Public ReadOnly Property IsValid() As Boolean
            Get
                If OverrideValidation Then
                    Return True
                End If
                Dim List = Me.GetRuleViolations
                Dim Tp As Type = GetType(t)
                For Each I In List
                    Debug.WriteLine("Type:" & Tp.Name & " Prop:" & I.PropertyName & " Prob:" & I.ErrorMessage)
                Next
                Return List.Count = 0
            End Get
        End Property

        Public Overridable Function GetRuleViolations() As List(Of RuleViolation)
            Return New List(Of RuleViolation)
        End Function

        Private Shared Function GetKeyProperty() As String
            Dim Tp As Type = GetType(t)

            Tp.GetProperties((BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.FlattenHierarchy))
            Dim MainCA = (From p In Tp.GetProperties((BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.FlattenHierarchy)) From CA In p.GetCustomAttributes(False) Where CA.GetType Is GetType(System.Data.Linq.Mapping.ColumnAttribute) AndAlso CType(CA, System.Data.Linq.Mapping.ColumnAttribute).IsPrimaryKey Select CA, p.Name).FirstOrDefault

            Dim IDString = MainCA.Name
            If IDString = "" Then IDString = "ID"
            Return IDString
        End Function

        Protected Function GetKeyValue() As Object
            Dim Obj As Object = Me.GetType.GetProperty(GetKeyProperty).GetValue(Me, Nothing)
            Return Obj
        End Function
        Public Overridable Sub HandleBeforeDelete(DB As System.Data.Linq.DataContext)

        End Sub
        Public Overridable Sub HandleAfterDelete(DB As System.Data.Linq.DataContext)

        End Sub
        Public Overridable Sub HandleBeforeSave(DB As System.Data.Linq.DataContext)

        End Sub
        Public Overridable Sub HandleAfterSave(DB As System.Data.Linq.DataContext)

        End Sub

        Protected Overridable Sub HandleLocalDelete(ByVal DB As DataContext)

        End Sub
        Protected Overridable Sub HandleLocalSave(ByVal DB As DataContext)

        End Sub
        Protected Overridable Sub HandleLocalAfterDelete(ByVal DB As DataContext)

        End Sub
        Protected Overridable Sub HandleLocalAfterSave(ByVal DB As DataContext)

        End Sub
        Protected Overridable Sub HandleLocalAfterLoad(ByVal DB As DataContext)

        End Sub

        Private Shared Function Convert(ByVal Entity As IEntity(Of t)) As t
            Return CType(CType(Entity, Object), t)
        End Function
        Private Shared Function Convert(ByVal Entity As t) As IEntity(Of t)
            Return CType(CType(Entity, Object), IEntity(Of t))
        End Function
        'Public Shared Function GetList() As IQueryable(Of t)
        '    Return GetList(DataContext)
        'End Function
        Public Shared Function GetList(ByVal DB As DataContext) As IQueryable(Of t)
            If DB IsNot Nothing Then
                Return From I As t In DB.GetTable(GetType(t)) Select I
            Else
                Dim L As New List(Of t)
                Return L.AsQueryable
            End If
        End Function
        'Public Shared Function Load(ByVal ID As Integer) As t

        'End Function
        Public Shared Function Load(ByVal DB As DataContext, ByVal ID As Object) As t
            Dim Tp As Type = GetType(t)
            If DB IsNot Nothing Then
                'Dim p = Tp.GetProperty("ID")
                Dim ColName As String = GetKeyProperty()
                Dim TableName As String = ""
                'Dim PropCAs = From CA In p.GetCustomAttributes(False) Where CA.GetType Is GetType(System.Data.Linq.Mapping.ColumnAttribute) Select CA
                'If PropCAs.Count > 0 Then
                ' Dim ca As System.Data.Linq.Mapping.ColumnAttribute = PropCAs.First
                'ColName = ca.Name
                'End If
                'If ColName = "" Then
                '    ColName = "ID"
                'End If


                Dim ObjCAs = From CA In Tp.GetCustomAttributes(False) Where CA.GetType Is GetType(System.Data.Linq.Mapping.TableAttribute) Select CA
                If ObjCAs.Count > 0 Then
                    Dim ca As System.Data.Linq.Mapping.TableAttribute = ObjCAs.First
                    TableName = ca.Name
                End If

                Dim L As New List(Of t)

                'Dim st As String = List.ToString & " where [t0].[" & ColName & "]={0}"
                'Dim st As String = List.ToString & " where [t0].[ID]={0}"
                Try

                    Dim st As String = "Select * from " & TableName & " where " & ColName & "={0}"
                    Dim L2 = DB.ExecuteQuery(Of t)(st, ID.ToString)
                    L.AddRange(L2)

                Catch ex As Exception

                End Try
                If L.Count > 0 Then
                    Return L.First
                Else
                    Dim O As Object = Tp.Assembly.CreateInstance(Tp.FullName, True)

                    Return CType(O, t)
                End If
            Else

                Dim O As Object = Tp.Assembly.CreateInstance(Tp.FullName, True)

                Return CType(O, t)
            End If
        End Function
        'Public Shared Sub Save(ByVal entity As t)

        'End Sub
        Public Shared Sub Save(ByVal DB As DataContext, ByVal entity As t)
            Dim EOrg As IEntity(Of t) = Convert(entity)
            Dim ID As Object = EOrg.GetKeyValue()
            If ((TypeOf (ID) Is Integer AndAlso ID = 0) OrElse (ID Is Nothing)) Then
                EOrg.Save(DB)
            Else
                Dim E1 As t = Load(DB, ID)
                Dim E2 As IEntity(Of t) = Convert(E1)
                EOrg.CopyInto(E1)
                E2.Save(DB)
            End If
        End Sub
        'Public Sub DeletePartial()

        'End Sub
        Public Sub DeletePartial(ByVal DB As DataContext)
            If DB IsNot Nothing Then
                Dim Tp As Type = Me.GetType()
                Try
                    DB.GetTable(Tp).Attach(Me, True)
                Catch ex As Exception

                End Try
                DB.GetTable(Tp).DeleteOnSubmit(Me)
                Me.HandleLocalDelete(DB)
            End If
        End Sub
        'Public Sub SavePartial()

        'End Sub
        Public Sub SavePartial(ByVal DB As DataContext)
            If DB IsNot Nothing Then
                Dim ID As Object = Me.GetKeyValue()
                If ((TypeOf (ID) Is Integer AndAlso ID = 0) OrElse (ID Is Nothing) OrElse (TypeOf (ID) Is Guid AndAlso ID = Guid.Empty)) Then
                    Dim Tp As Type = Me.GetType()
                    DB.GetTable(Tp).InsertOnSubmit(Me)
                Else
                    Try
                        Dim Tp As Type = Me.GetType()
                        DB.GetTable(Tp).Attach(Me)
                    Catch ex As Exception
                        Dim a As Integer = 0
                    End Try
                End If
                Me.HandleLocalSave(DB)
            End If
        End Sub
        'Public Shared Sub Delete(ByVal entity As t)

        'End Sub
        Public Shared Sub Delete(ByVal DB As DataContext, ByVal entity As t)
            Dim ID As Integer = CInt(entity.GetType.GetProperty("ID").GetValue(entity, Nothing))
            Dim E As IEntity(Of t) = Convert(Load(DB, ID))
            E.Delete(DB)
        End Sub
        'Public Sub Save()

        'End Sub
        Public Sub Save(ByVal DB As DataContext)
            If DB IsNot Nothing Then
                CType(DB, Object).HAS.clear()
                SavePartial(DB)
                DB.SubmitChanges()
                LastIDSaved = Me.GetKeyValue()
                HandleLocalAfterSave(DB)
                For Each i As Object In CType(DB, Object).HAS
                    i.HandleAfterSave(DB)
                Next
            Else
                If Not Me.IsValid Then Throw New Exception("Rule violations prevent saving")
            End If
        End Sub
        'Public Sub Delete()

        'End Sub
        Public Sub Delete(ByVal DB As DataContext)
            If DB IsNot Nothing Then
                'Try
                DeletePartial(DB)
                DB.SubmitChanges()
                HandleLocalAfterDelete(DB)
                'Catch ex As Exception
                '    Dim a As Integer = 1
                'End Try
            End If
        End Sub
        Public Function Clone() As t
            Dim Tp As Type = Me.GetType()
            Dim NewItem As t = CType(Tp.Assembly.CreateInstance(Tp.FullName), t)
            Me.CopyInto(NewItem)
            Return NewItem
        End Function
        Public Sub CloneFrom(ByVal NewItem As t)
            Dim NewItem2 As IEntity(Of t) = Convert(NewItem)
            NewItem2.CopyInto(Convert(Me))
        End Sub
        Public Sub CopyInto(ByVal NewItem As t)
            Try
                Dim KeyName = GetKeyProperty()
                Dim Tp As Type = Me.GetType()
                Dim props = Tp.GetProperties((BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.FlattenHierarchy))
                For Each p In props
                    If p.CanWrite AndAlso p.Name.ToUpper <> KeyName Then

                        Dim V As Object = Nothing
                        Try
                            V = p.GetValue(Me, Nothing)
                        Catch ex As Exception
                            'Debug.WriteLine("Copy Into Error Prop:" & p.Name)
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
                        'WriteDebug("Type = [" & TName & "]")
                        If V Is Nothing Then
                            Dim a As Integer = 1
                        End If
                        If Not TName.Contains("EntitySet") AndAlso Not (TName.Contains("IEntity") OrElse BTName.Contains("IEntity")) AndAlso V IsNot Nothing AndAlso p.CanWrite Then
                            If V IsNot Nothing Then
                                Try
                                    'WriteDebug(p.Name & "=""" & V & """")
                                Catch ex As Exception
                                    'WriteDebug(p.Name & "=[Unknown]")
                                End Try
                            Else
                                'WriteDebug(p.Name & "= Nothing")
                            End If
                            Try
                                Tp.GetProperty(p.Name).SetValue(NewItem, V, Nothing)
                            Catch ex As Exception
                                Dim a As Integer = 1
                            End Try
                        Else
                            'WriteDebug(p.Name & " was an entity.")
                        End If
                    End If
                Next
            Catch ex As Exception
                Dim a As Integer = 1
            End Try
        End Sub

        Public Function AsCollection() As FormCollection
            Dim F As New FormCollection
            Try
                Dim KeyName = GetKeyProperty()
                Dim Tp As Type = Me.GetType()
                Dim props = Tp.GetProperties((BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.FlattenHierarchy))
                For Each p In props
                    If p.CanWrite AndAlso p.Name.ToUpper <> KeyName Then

                        Dim V As Object = Nothing
                        Try
                            V = p.GetValue(Me, Nothing)
                        Catch ex As Exception
                            'Debug.WriteLine("Copy Into Error Prop:" & p.Name)
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
                        'WriteDebug("Type = [" & TName & "]")
                        If V Is Nothing Then
                            Dim a As Integer = 1
                        End If
                        If Not TName.Contains("EntitySet") AndAlso Not (TName.Contains("IEntity") OrElse BTName.Contains("IEntity")) AndAlso V IsNot Nothing Then
                            If V IsNot Nothing Then
                                Try
                                    'WriteDebug(p.Name & "=""" & V & """")
                                Catch ex As Exception
                                    'WriteDebug(p.Name & "=[Unknown]")
                                End Try
                            Else
                                'WriteDebug(p.Name & "= Nothing")
                            End If
                            Try
                                F(p.Name) = V.ToString
                            Catch ex As Exception
                                Dim a As Integer = 1
                            End Try
                        Else
                            'WriteDebug(p.Name & " was an entity.")
                        End If
                    End If
                Next
            Catch ex As Exception
                Dim a As Integer = 1
            End Try
            Return F
        End Function
        Public Overrides Function ToString() As String
            Dim SB As New System.Text.StringBuilder
            Try
                Dim KeyName = GetKeyProperty()
                Dim Tp As Type = Me.GetType()
                Dim props = Tp.GetProperties((BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.FlattenHierarchy))
                For Each p In props
                    If p.CanWrite AndAlso p.Name.ToUpper <> KeyName Then

                        Dim V As Object = Nothing
                        Try
                            V = p.GetValue(Me, Nothing)
                        Catch ex As Exception
                            'Debug.WriteLine("Copy Into Error Prop:" & p.Name)
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
                        'WriteDebug("Type = [" & TName & "]")
                        If V Is Nothing Then
                            Dim a As Integer = 1
                        End If
                        If Not TName.Contains("EntitySet") AndAlso Not (TName.Contains("IEntity") OrElse BTName.Contains("IEntity")) AndAlso V IsNot Nothing AndAlso p.CanWrite Then
                            If V IsNot Nothing Then
                                Try
                                    'WriteDebug(p.Name & "=""" & V & """")
                                Catch ex As Exception
                                    'WriteDebug(p.Name & "=[Unknown]")
                                End Try
                            Else
                                'WriteDebug(p.Name & "= Nothing")
                            End If
                            Try
                                SB.AppendLine(p.Name & ": " & V.ToString)
                            Catch ex As Exception
                                Dim a As Integer = 1
                            End Try
                        Else
                            'WriteDebug(p.Name & " was an entity.")
                        End If
                    End If
                Next
            Catch ex As Exception
                Dim a As Integer = 1
            End Try
            Return SB.ToString
        End Function
        Public Overridable Function ValidateObject(ByVal validationContext As System.ComponentModel.DataAnnotations.ValidationContext) As System.Collections.Generic.IEnumerable(Of System.ComponentModel.DataAnnotations.ValidationResult)
            Dim VRList As New List(Of ValidationResult)

            For Each RV In GetRuleViolations()
                VRList.Add(New ValidationResult(RV.ErrorMessage, {RV.PropertyName}.ToList))
            Next

            Return VRList
        End Function
        Public Function Validate(ByVal validationContext As System.ComponentModel.DataAnnotations.ValidationContext) As System.Collections.Generic.IEnumerable(Of System.ComponentModel.DataAnnotations.ValidationResult) Implements System.ComponentModel.DataAnnotations.IValidatableObject.Validate
            Return ValidateObject(validationContext)
        End Function
    End Class

End Namespace