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

Public Class NullToEmptyStringModelBinder
    Inherits DefaultModelBinder

    Protected Overrides Sub SetProperty(ByVal controllerContext As System.Web.Mvc.ControllerContext, ByVal bindingContext As System.Web.Mvc.ModelBindingContext, ByVal propertyDescriptor As System.ComponentModel.PropertyDescriptor, ByVal value As Object)
        If value Is Nothing AndAlso propertyDescriptor.PropertyType Is GetType(String) Then
            value = ""
        End If
        If propertyDescriptor.PropertyType Is GetType(Nullable(Of Integer)) AndAlso Val(value) = 0 Then
            value = Nothing
        End If

        If value Is Nothing AndAlso propertyDescriptor.PropertyType Is GetType(Integer) Then
            value = 0
        End If


        MyBase.SetProperty(controllerContext, bindingContext, propertyDescriptor, value)
    End Sub

End Class
