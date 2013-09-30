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

Public Class IEntity

    'Protected Overridable Function GetKeyValue() As Object
    '    Return Nothing
    'End Function

    Public Function GetSubJSON() As Object
        Return Me.GetJSON
    End Function

    Public Overridable Function GetJSON() As Object
        Dim Obj = New With {.ID = ""}

        Return Obj
    End Function
    Public Overridable Function GetJSONForDataTable() As Object
        Return GetJSON()
    End Function


End Class
