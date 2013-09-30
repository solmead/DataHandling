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
Imports System.Reflection
Imports System.ComponentModel.DataAnnotations

Namespace CodeFirst

    Public MustInherit Class ActiveRecord(Of TT As Class)
        Inherits IEntity
        Implements IValidatableObject

        Public Shared LastIdSaved As Object
        Public Overridable Sub HandleDeleteBefore(db As ActiveRecordContext)

        End Sub
        Public Overridable Sub HandleDeleteAfter(db As ActiveRecordContext)

        End Sub
        Public Overridable Sub HandleSaveBefore(db As ActiveRecordContext)

        End Sub
        Public Overridable Sub HandleSaveAfter(db As ActiveRecordContext)

        End Sub
        Protected Overridable Sub HandleDeleteCalledStart(ByVal db As ActiveRecordContext)

        End Sub
        Protected Overridable Sub HandleSaveCalledStart(ByVal db As ActiveRecordContext)

        End Sub
        Protected Overridable Sub HandleDeleteCalledEnd(ByVal db As ActiveRecordContext)

        End Sub
        Protected Overridable Sub HandleSaveCalledEnd(ByVal db As ActiveRecordContext)

        End Sub
        Protected Overridable Sub HandleLoadCalledEnd(ByVal db As ActiveRecordContext)

        End Sub
        Public Function GetKeyName(db As ActiveRecordContext) As String
            Dim objectContext = CType(db, System.Data.Entity.Infrastructure.IObjectContextAdapter).ObjectContext
            Dim objectSet As System.Data.Objects.ObjectSet(Of TT) = objectContext.CreateObjectSet(Of TT)()
            Dim keyName = (From m In objectSet.EntitySet.ElementType.KeyMembers() Select m.Name).ToList().First()
            Return keyName
        End Function
        Public Function GetKeyValue(db As ActiveRecordContext) As Object
            Dim name = GetKeyName(db)

            Dim tp As Type = Me.GetType()
            Dim props = tp.GetProperties((BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.FlattenHierarchy))
            Return (From p In props Where p.Name = name Select p).FirstOrDefault.GetValue(Me, Nothing)
        End Function


        'Private Shared Function Convert(ByVal entity As ActiveRecord(Of TT)) As TT
        '    Return CType(CType(entity, Object), TT)
        'End Function
        'Private Shared Function Convert(ByVal entity As TT) As ActiveRecord(Of TT)
        '    Return CType(CType(entity, Object), ActiveRecord(Of TT))
        'End Function
        Public Shared Function GetList(ByVal db As ActiveRecordContext) As IQueryable(Of TT)
            If db IsNot Nothing Then
                Dim l = From I In db.Set(GetType(TT)) Select CType(I, TT)
                Return l
            Else
                Dim l As New List(Of TT)
                Return l.AsQueryable
            End If
        End Function
        Public Shared Function Load(ByVal db As ActiveRecordContext, ByVal id As Object) As TT
            Dim tp As Type = GetType(TT)
            If db IsNot Nothing Then
                'Dim L As New List(Of TT)
                Dim l2 = db.Set(tp).Find({id})
                If l2 IsNot Nothing Then
                    Return l2
                Else
                    Dim o As Object = tp.Assembly.CreateInstance(tp.FullName, True)

                    Return CType(o, TT)
                End If
            Else
                Dim o As Object = tp.Assembly.CreateInstance(tp.FullName, True)

                Return CType(o, TT)
            End If
        End Function
        Public Sub DeletePartial(ByVal db As ActiveRecordContext)
            If db IsNot Nothing Then
                db.ChangeTracker.DetectChanges()
                HandleDeleteCalledStart(db)
                Dim tp As Type = Me.GetType()
                Try
                    If db.Entry(Me).State <> EntityState.Deleted Then
                        db.Entry(Me).State = EntityState.Deleted
                        'db.Set(tp).Remove(Me)
                    End If
                Catch ex As Exception

                End Try
            End If
        End Sub

        Public Sub SavePartial(ByVal db As ActiveRecordContext)
            If db IsNot Nothing Then
                db.ChangeTracker.DetectChanges()
                HandleSaveCalledStart(db)
                Dim id As Object = GetKeyValue(db)
                If ((TypeOf (id) Is Integer AndAlso id = 0) OrElse (id Is Nothing) OrElse (TypeOf (id) Is Guid AndAlso id = Guid.Empty)) Then
                    Dim tp As Type = Me.GetType()
                    If db.Entry(Me).State = EntityState.Detached Then
                        db.Set(tp).Add(Me)
                    End If
                    'DB.GetTable(Tp).InsertOnSubmit(Me)
                Else
                    Try
                        Dim tp As Type = Me.GetType()
                        If db.Entry(Me).State = EntityState.Detached Then
                            db.Set(tp).Attach(Me)
                        End If
                    Catch ex As Exception
                        'Dim a As Integer = 0
                    End Try
                End If
            End If
        End Sub

        Public Sub Save(ByVal db As ActiveRecordContext)
            If db IsNot Nothing Then
                CType(db, Object).HAS.clear()
                SavePartial(db)
                db.SaveChanges()
                LastIdSaved = GetKeyValue(db)
                HandleSaveCalledEnd(db)
                For Each i In db.Has
                    i.HandleAfterSave(db)
                Next
            Else
                'If Not Me.IsValid Then Throw New Exception("Rule violations prevent saving")
            End If
        End Sub

        Public Sub Delete(ByVal db As ActiveRecordContext)
            If db IsNot Nothing Then
                'Try
                DeletePartial(db)
                db.SaveChanges()
                HandleDeleteCalledEnd(db)
                'Catch ex As Exception
                '    Dim a As Integer = 1
                'End Try
            End If
        End Sub


        Public Function AsCollection() As FormCollection
            Dim f As New FormCollection
            Try
                'Dim KeyName = GetKeyProperty()
                Dim tp As Type = Me.GetType()
                Dim props = tp.GetProperties((BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.FlattenHierarchy))
                For Each p In props
                    If p.CanWrite Then

                        Dim v As Object = Nothing
                        Try
                            v = p.GetValue(Me, Nothing)
                        Catch ex As Exception
                            'Debug.WriteLine("Copy Into Error Prop:" & p.Name)
                        End Try
                        Dim name As String = ""
                        Dim btName As String = ""
                        Try
                            name = p.PropertyType.FullName
                            If p.PropertyType.BaseType IsNot Nothing Then
                                btName = p.PropertyType.BaseType.FullName
                            End If
                        Catch ex As Exception
                            'WriteDebug(ex.ToString)
                        End Try
                        'WriteDebug("Type = [" & TName & "]")
                        If v Is Nothing Then
                            'Dim a As Integer = 1
                        End If
                        If Not name.Contains("EntitySet") AndAlso Not (name.Contains("ActiveRecord") OrElse btName.Contains("ActiveRecord")) AndAlso v IsNot Nothing Then
                            If v IsNot Nothing Then
                                Try
                                    'WriteDebug(p.Name & "=""" & V & """")
                                Catch ex As Exception
                                    'WriteDebug(p.Name & "=[Unknown]")
                                End Try
                            Else
                                'WriteDebug(p.Name & "= Nothing")
                            End If
                            Try
                                f(p.Name) = v.ToString
                            Catch ex As Exception
                                'Dim a As Integer = 1
                            End Try
                        Else
                            'WriteDebug(p.Name & " was an entity.")
                        End If
                    End If
                Next
            Catch ex As Exception
                'Dim a As Integer = 1
            End Try
            Return f
        End Function

        Public Overridable Function GetPropertyType(PropertyName As String) As Type
            Dim tp As Type = Me.GetType()
            Dim prop = tp.GetProperty(PropertyName)

            If (prop IsNot Nothing) Then
                Return prop.PropertyType
            End If
            Return GetType(String)
        End Function
        Public Overridable Sub SetValue(PropertyName As String, Value As Object)

            Dim tp As Type = Me.GetType()
            Dim prop = tp.GetProperty(PropertyName)

            If (prop IsNot Nothing) Then
                prop.SetValue(Me, Value, Nothing)
            End If
        End Sub
        Public Overridable Function GetValue(PropertyName As String) As Object
            Dim retVal As Object = Nothing
            Dim tp As Type = Me.GetType()
            Dim prop = tp.GetProperty(PropertyName)
            If (prop IsNot Nothing) Then
                retVal = prop.GetValue(Me, Nothing)
            End If

            Return retVal
        End Function
        Public Overridable Function DoesPropertyExist(PropertyName As String) As Boolean
            Dim retVal As Object = Nothing
            Dim tp As Type = Me.GetType()
            Dim prop = tp.GetProperty(PropertyName)
            Return (prop IsNot Nothing)
        End Function
        Public Overridable Function GetPropertyNames() As List(Of String)
            Dim tp As Type = Me.GetType()
            Dim props = tp.GetProperties((BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.FlattenHierarchy))
            Return (From p In props Where p.CanWrite Select p.Name).ToList
        End Function
        Public Overrides Function ToString() As String
            Dim sb As New Text.StringBuilder
            Try
                'Dim KeyName = GetKeyProperty()
                Dim tp As Type = Me.GetType()
                Dim props = tp.GetProperties((BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.FlattenHierarchy))
                For Each p In props
                    If p.CanWrite Then 'AndAlso p.Name.ToUpper <> KeyName Then

                        Dim v As Object = Nothing
                        Try
                            v = p.GetValue(Me, Nothing)
                        Catch ex As Exception
                            'Debug.WriteLine("Copy Into Error Prop:" & p.Name)
                        End Try
                        Dim name As String = ""
                        Dim btName As String = ""
                        Try
                            name = p.PropertyType.FullName
                            If p.PropertyType.BaseType IsNot Nothing Then
                                btName = p.PropertyType.BaseType.FullName
                            End If
                        Catch ex As Exception
                            'WriteDebug(ex.ToString)
                        End Try
                        'WriteDebug("Type = [" & TName & "]")
                        If v Is Nothing Then
                            'Dim a As Integer = 1
                        End If
                        If Not name.Contains("EntitySet") AndAlso Not (name.Contains("ActiveRecord") OrElse btName.Contains("ActiveRecord")) AndAlso v IsNot Nothing AndAlso p.CanWrite Then
                            If v IsNot Nothing Then
                                Try
                                    'WriteDebug(p.Name & "=""" & V & """")
                                Catch ex As Exception
                                    'WriteDebug(p.Name & "=[Unknown]")
                                End Try
                            Else
                                'WriteDebug(p.Name & "= Nothing")
                            End If
                            Try
                                sb.AppendLine(p.Name & ": " & v.ToString)
                            Catch ex As Exception
                                'Dim a As Integer = 1
                            End Try
                        Else
                            'WriteDebug(p.Name & " was an entity.")
                        End If
                    End If
                Next
            Catch ex As Exception
                'Dim a As Integer = 1
            End Try
            Return sb.ToString
        End Function
        Public Overridable Function ValidateObject(ByVal validationContext As ValidationContext) As IEnumerable(Of ValidationResult)
            Dim vrList As New List(Of ValidationResult)

            'For Each RV In GetRuleViolations()
            '    VRList.Add(New ValidationResult(RV.ErrorMessage, {RV.PropertyName}.ToList))
            'Next

            Return vrList
        End Function
        Public Function Validate(ByVal validationContext As ValidationContext) As IEnumerable(Of ValidationResult) Implements IValidatableObject.Validate
            Return ValidateObject(validationContext)
        End Function
    End Class


End Namespace