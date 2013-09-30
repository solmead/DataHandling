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

Imports System.Collections.Generic
Imports System.Text
Imports System.Linq
Imports System.Linq.Expressions
Imports System.Reflection
Imports System.Reflection.Emit
Imports System.Threading
Imports System.Runtime.CompilerServices

Module ControllerExtensions
    '<Extension()> _
    'Public Function RazorView(ByVal Controller As Controller, Optional ByVal ForAdmin As Boolean = True) As ViewResult

    '    Return RazorView(Controller, Nothing, Nothing, ForAdmin:=ForAdmin)
    'End Function

    '<Extension()> _
    'Public Function RazorView(ByVal Controller As Controller, ByVal model As Object, Optional ByVal ForAdmin As Boolean = True) As ViewResult

    '    Return RazorView(Controller, Nothing, model, ForAdmin:=ForAdmin)
    'End Function

    '<Extension()> _
    'Public Function RazorView(ByVal Controller As Controller, ByVal viewName As String, Optional ByVal ForAdmin As Boolean = True) As ViewResult

    '    Return RazorView(Controller, viewName, Nothing, ForAdmin:=ForAdmin)
    'End Function

    '<Extension()> _
    'Public Function RazorView(ByVal Controller As Controller, ByVal viewName As String, ByVal model As Object, Optional ByVal ForAdmin As Boolean = True) As ViewResult

    '    If (model IsNot Nothing) Then
    '        Controller.ViewData.Model = model
    '    End If

    '    Controller.ViewBag._ViewName = GetViewName(Controller, viewName)

    '    Return New ViewResult With {
    '        .ViewName = IIf(ForAdmin, "RazorViewAdmin", "RazorView"),
    '        .ViewData = Controller.ViewData,
    '        .TempData = Controller.TempData
    '    }
    'End Function

    '<Extension()> _
    'Public Function GetViewName(ByVal Controller As Controller, ByVal viewName As String) As String

    '    If Not String.IsNullOrEmpty(viewName) Then
    '        Return viewName
    '    Else
    '        Return Controller.RouteData.GetRequiredString("action")
    '    End If
    'End Function
End Module
