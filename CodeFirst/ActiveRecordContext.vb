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
Imports System.Data.Entity

Namespace CodeFirst

    Public Class ActiveRecordContext
        Inherits DbContext

        Public Has As New List(Of Object)

        Public Sub New()
            MyBase.New()
        End Sub
        Public Sub New(nameOrConnectionString As String)
            MyBase.New(nameOrConnectionString)
        End Sub


        Private Function CheckChanges(savedObjs As List(Of Object), deletedObjs As List(Of Object)) As Boolean
            Dim changed = False
            ChangeTracker.DetectChanges()
            Dim changedList = ChangeTracker.Entries().ToList
            For Each EntityEntry In changedList
                Dim item As Object = TryCast(EntityEntry.Entity, IEntity)
                If item IsNot Nothing Then
                    If (EntityEntry.State = EntityState.Added OrElse EntityEntry.State = EntityState.Modified) AndAlso Not savedObjs.Contains(item) Then
                        item.HandleSaveBefore(Me)
                        savedObjs.Add(item)
                        changed = True
                    ElseIf EntityEntry.State = EntityState.Deleted AndAlso Not deletedObjs.Contains(item) Then
                        EntityEntry.State = EntityState.Modified
                        item.HandleDeleteBefore(Me)
                        EntityEntry.State = EntityState.Deleted
                        deletedObjs.Add(item)
                        changed = True
                    End If
                End If
            Next
            Return changed
        End Function


        Public Overrides Function SaveChanges() As Integer
            ChangeTracker.DetectChanges()
            Dim I As Integer
            Dim savedObjs As New List(Of Object)
            Dim deletedObjs As New List(Of Object)

            While CheckChanges(savedObjs, deletedObjs)

            End While
            Try
                'Logger.GlobalLog.DebugMessage("SaveChanges: Saving changes count=" & savedObjs.Count)
                I = MyBase.SaveChanges()
                'Logger.GlobalLog.DebugMessage("SaveChanges: Saved changes")
            Catch ex As Exception
                'Logger.GlobalLog.DebugMessage("SaveChanges: Error")
                For Each e In GetValidationErrors()
                    For Each se In e.ValidationErrors
                        Debug.WriteLine("[" & se.PropertyName & "] - [" & se.ErrorMessage & "]")
                    Next
                Next
                'Dim i2 As Integer = 1

                Throw
            End Try
            For Each item In savedObjs
                item.HandleSaveAfter(Me)
            Next
            For Each item In deletedObjs
                item.HandleDeleteAfter(Me)
            Next

            Return I
        End Function
    End Class
End Namespace

