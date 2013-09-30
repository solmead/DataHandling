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
Imports System.Web.Mvc
Imports System.IO

Public Enum ReturnFormat
    HTML
    PartialHTML
    CleanHTML
    JSON
    XML
    XLS
    CSV
    Pdf
    Other
End Enum

Public MustInherit Class BaseController
    Inherits Controller
    Public Property Format As ReturnFormat = ReturnFormat.HTML
    Public Property Filename = ""
    Protected Property FormatResultCalled As Boolean = False



    Protected Overridable Sub OnShowView(filterContext As System.Web.Mvc.ActionExecutedContext)

    End Sub

    Protected Overrides Sub OnActionExecuted(filterContext As System.Web.Mvc.ActionExecutedContext)
        MyBase.OnActionExecuted(filterContext)
        'System.Diagnostics.Debug.WriteLine("BaseController - OnActionExecuted")
        If Not FormatResultCalled AndAlso Format <> ReturnFormat.HTML AndAlso Format <> ReturnFormat.CleanHTML Then
            Dim obj = filterContext.Controller.ViewData.Model
            Dim hasError = (filterContext.Exception IsNot Nothing)
            Dim errorMessage = ""
            If hasError Then
                errorMessage = filterContext.Exception.Message
            End If
            If obj IsNot Nothing Then
                Dim tp As Type = obj.GetType()
                If tp.ToString.ToUpper.Contains("PAGINATEDLIST") Then
                    obj = obj.MySource()
                End If
            End If
            filterContext.Result = JSON_Result(obj, hasError, errorMessage)
        ElseIf Format = ReturnFormat.HTML OrElse Format = ReturnFormat.PartialHTML OrElse Format = ReturnFormat.CleanHTML Then
            OnShowView(filterContext)
        End If
    End Sub
    Protected Overrides Sub OnActionExecuting(ByVal filterContext As ActionExecutingContext)
        MyBase.OnActionExecuting(filterContext)
        'System.Diagnostics.Debug.WriteLine("BaseController - OnActionExecuting")

        Dim routeValues = filterContext.RouteData.Values
        Const formatKey As String = "Format"
        If routeValues.ContainsKey(formatKey) Then
            Dim requestedFormat As String = routeValues(formatKey).ToString.ToLower
            Format = ReturnFormat.HTML
            Try
                For Each rf As ReturnFormat In [Enum].GetValues(GetType(ReturnFormat))
                    If rf.ToString.ToUpper = requestedFormat.ToUpper Then
                        Format = rf
                        Exit For
                    End If
                Next
            Catch ex As Exception
            End Try
        Else
            Dim requestedFormat As String = filterContext.HttpContext.Request(formatKey)
            If requestedFormat Is Nothing Then requestedFormat = ""
            requestedFormat = requestedFormat.ToString.ToLower
            Format = ReturnFormat.HTML
            Try
                For Each rf As ReturnFormat In [Enum].GetValues(GetType(ReturnFormat))
                    If rf.ToString.ToUpper = requestedFormat.ToUpper Then
                        Format = rf
                        Exit For
                    End If
                Next
                'Format = [Enum].Parse(GetType(ReturnFormat), requestedFormat)
            Catch ex As Exception
            End Try
        End If
        If routeValues.ContainsKey("Action") AndAlso routeValues.ContainsKey("Controller") Then
            Filename = routeValues("Controller").ToString & "_" & routeValues("Action").ToString
        End If

        ViewBag.ShowCleanVersion = False
    End Sub
    Public Sub New()
    End Sub
    Public Sub AddRuleViolations(ByVal errors As IEnumerable(Of RuleViolation))
        For Each issue In errors
            ModelState.AddModelError(issue.PropertyName, issue.ErrorMessage)
        Next
    End Sub
    'ByVal IsError As Boolean, Optional ByVal ErrorMessage As String = ""

    Protected Function GetUploadedFile(ByVal origItem As FileInteface, ByVal newItem As FileInteface, ByVal baseName As String, ByVal itemName As String) As FileInteface
        Dim item As FileInteface
        If origItem IsNot Nothing Then
            item = origItem
        Else
            item = newItem
        End If
        Dim guid As String = Request.Form(baseName & "_guid")
        Dim fname As String = Request.Form(baseName & "_filename")
        'Response.Write("GUID=" & GUID & "<br/>")
        'Response.Flush()
        'Response.Write("Fname=" & Fname & "<br/>")
        'Response.Flush()
        If Request.Files(baseName) IsNot Nothing AndAlso Request.Files(baseName).ContentLength > 1000 Then
            'Response.Write("Request.Files is long enough")
            'Response.Flush()
            item.SetInitialFileFromUploadedFile(Request.Files(baseName))
            item.Name = itemName
            item.PostedTimeStamp = Now
            item.SpecialHandling()
            'SendingEmail.SendEmail(New System.Net.Mail.MailAddress("solmead@gmail.com", "Chris"), MySettings.SiteNAme & " " & "New Video Uploaded", "A New Video Was uploaded #" & ItemName)
            'P.Video.IsReady = True
            Return item
        ElseIf fname IsNot Nothing AndAlso fname.Length > 0 Then
            'If guid = "" Then guid = Session("FileGUID")
            guid = Request.Form(baseName & "_FileGUID")
            Dim fi As New FileInfo(Settings.FileLocation & "/AsyncUploads/" & guid & ".tmp")
            If fi.Exists Then
                item.SetIntialFileDataFromFile(fi, fname)
                item.Name = itemName
                item.PostedTimeStamp = Now
                item.SpecialHandling()
                'P.BackImage.IsReady = True
                Return item
            Else
                Throw New Exception("Temp file not found on server. FileName=[" & fname & "] TempName=[" & fi.FullName & "]")
            End If
        End If
        Return origItem
    End Function

    'Protected Function FormatResult(Of T)(ByVal viewModel As LinqToSql.IEntity(Of T), ByVal IsError As Boolean, ByVal ErrorMessage As String, Optional ByVal UseRazor As Boolean = False, Optional ByVal ForAdmin As Boolean = True) As ActionResult
    '    Return FormatResult(viewModel, IsError, ErrorMessage, "", UseRazor:=UseRazor, ForAdmin:=ForAdmin)
    'End Function
    'Protected Function FormatResult(Of T)(ByVal viewModel As LinqToSql.IEntity(Of T), ByVal IsError As Boolean, ByVal ErrorMessage As String, ByVal Name As String, Optional ByVal UseRazor As Boolean = False, Optional ByVal ForAdmin As Boolean = True) As ActionResult
    '    Return FormatResult(CType(viewModel, Object), IsError, ErrorMessage, Name, UseRazor:=UseRazor, ForAdmin:=ForAdmin)
    'End Function
    Protected Function Redirect_Result(ByVal viewModel As Object, ByVal htmlRoute As ActionResult) As ActionResult
        If Format = ReturnFormat.CleanHTML Then
            ViewBag.ShowCleanVersion = True
        End If
        If Format = ReturnFormat.HTML OrElse Format = ReturnFormat.CleanHTML OrElse Format = ReturnFormat.PartialHTML Then
            FormatResultCalled = True
            'If Format <> ReturnFormat.HTML Then
            '    htmlRoute.
            'End If
            Return htmlRoute
        End If
        Return JSON_Result(viewModel, False, "")
    End Function
    Protected Function Error_Result(ByVal viewModel As Object, ByVal exception As Exception) As ActionResult
        Return Error_Result("", viewModel, exception)
    End Function
    Protected Function Error_Result(ByVal pageName As String, ByVal viewModel As Object, ByVal exception As Exception) As ActionResult
        ModelState.AddModelError("Error", exception.Message)

        'Dim Msg = ""
        'For Each C In Collection.Keys
        '    Msg = Msg & "[" & C & "] = [" & Collection(C) & "]" & vbCrLf
        'Next

        Return JSON_Result(pageName, viewModel, True, exception.Message & vbCrLf & "<span style='display:none;'>" & exception.ToString & "</span>")
    End Function
    Protected Function Error_Result(ByVal viewModel As Object, ByVal exception As Exception, ByVal htmlRoute As ActionResult) As ActionResult
        ModelState.AddModelError("Error", exception.Message)
        If Format = ReturnFormat.CleanHTML Then
            ViewBag.ShowCleanVersion = True
        End If
        If Format = ReturnFormat.HTML OrElse Format = ReturnFormat.CleanHTML Then
            FormatResultCalled = True
            Return htmlRoute
        End If
        Return JSON_Result(viewModel, True, exception.Message & vbCrLf & "<span style='display:none;'>" & exception.ToString & "</span>", "")
    End Function
    'Protected Function Error_Result(ByVal viewModel As Object, ByVal Exception As String) As ActionResult
    '    Return Error_Result("", viewModel, Exception)
    'End Function
    'Protected Function Error_Result(ByVal PageName As String, ByVal viewModel As Object, ByVal Exception As String) As ActionResult
    '    Return JSON_Result(viewModel, True, Exception, PageName)
    'End Function
    'Protected Function Error_Result(ByVal viewModel As Object, ByVal Exception As String, ByVal HTMLRoute As ActionResult) As ActionResult
    '    If Format = ReturnFormat.HTML Then
    '        Return HTMLRoute
    '    End If
    '    Return JSON_Result(viewModel, True, Exception, "")
    'End Function
    Protected Function JSON_Result(ByVal viewModel As Object, ByVal message As String, ByVal HTMLRoute As ActionResult) As ActionResult
        If Format = ReturnFormat.CleanHTML Then
            ViewBag.ShowCleanVersion = True
        End If
        If Format = ReturnFormat.HTML OrElse Format = ReturnFormat.CleanHTML Then
            FormatResultCalled = True
            Return HTMLRoute
        End If
        Return JSON_Result(viewModel, False, message, "")
    End Function
    Protected Function JSON_Result(ByVal viewModel As Object) As ActionResult
        Return JSON_Result("", viewModel, False, "")
    End Function
    Protected Function JSON_Result(ByVal viewModel As Object, ByVal message As String) As ActionResult
        Return JSON_Result("", viewModel, False, message)
    End Function
    Protected Function JSON_Result(pageName As String, ByVal viewModel As Object, ByVal message As String) As ActionResult
        Return JSON_Result(pageName, viewModel, False, message)
    End Function
    Private Function JSON_Result(ByVal viewModel As Object, ByVal IsError As Boolean, ByVal errorMessage As String, ByVal htmlRoute As ActionResult) As ActionResult
        If Format = ReturnFormat.CleanHTML Then
            ViewBag.ShowCleanVersion = True
        End If
        If Format = ReturnFormat.HTML OrElse Format = ReturnFormat.CleanHTML Then
            FormatResultCalled = True
            Return htmlRoute
        End If
        Return JSON_Result("", viewModel, IsError, errorMessage)
    End Function
    Private Function JSON_Result(ByVal viewModel As Object, ByVal IsError As Boolean, ByVal errorMessage As String) As ActionResult
        Return JSON_Result("", viewModel, IsError, errorMessage)
    End Function
    Private Function JSON_Result(pageName As String, ByVal viewModel As Object, ByVal IsError As Boolean, ByVal errorMessage As String) As ActionResult

        FormatResultCalled = True
        Dim list As New List(Of Object)
        Dim tstr As String = ""
        Try
            Dim listViolations = viewModel.GetRuleViolations
            AddRuleViolations(listViolations)
            For Each RV As RuleViolation In listViolations
                tstr = tstr & RV.ErrorMessage & "<br/>"
                list.Add(New With {.PropertyName = RV.PropertyName, .Message = RV.ErrorMessage})
            Next
        Catch ex As Exception
        End Try
        tstr = tstr & errorMessage
        ModelState.AddModelError("_FORM", errorMessage)
        ViewData("ErrorMessage") = errorMessage

        If Format = ReturnFormat.CleanHTML Then
            ViewBag.ShowCleanVersion = True
        End If
        If Format = ReturnFormat.HTML OrElse Format = ReturnFormat.CleanHTML Then
            If pageName <> "" Then
                Return View(pageName, viewModel)
            Else
                Return View(viewModel)
            End If
        ElseIf Format = ReturnFormat.PartialHTML Then
            If pageName <> "" Then
                Return PartialView(pageName, viewModel)
            Else
                Return PartialView(viewModel)
            End If
        End If

        Dim p As Object = Nothing

        If TypeOf viewModel Is IEnumerable Then
            Dim modelList As IEnumerable = viewModel
            If Format <> ReturnFormat.JSON Then
                Return Format_Return(modelList, pageName)
            End If
            Try
                modelList = (From m As IEntity In modelList Select m.GetJSON).ToList
            Catch ex As Exception

            End Try
            p = New With {.IsError = IsError, .Message = tstr, .Item = modelList, .FieldList = list}
            Return Format_Return(p, pageName)
        Else
            Dim model As Object = viewModel
            If Format <> ReturnFormat.JSON Then
                Return Format_Return(model, pageName)
            End If
            Try
                model = viewModel.GetJSon
            Catch ex As Exception
            End Try
            p = New With {.IsError = IsError, .Message = tstr, .Item = model, .FieldList = list}
            Return Format_Return(p, pageName)
        End If


    End Function
    Private Function Format_Return(ByVal viewModel As Object) As ActionResult
        Return Format_Return(viewModel, "")
    End Function
    Private Function Format_Return(ByVal viewModel As Object, ByVal name As String) As ActionResult
        Dim jsonModel As Object = viewModel
        Try
            jsonModel = viewModel.getJSON
        Catch ex As Exception
        End Try
        FormatResultCalled = True
        If Format = ReturnFormat.CleanHTML Then
            ViewBag.ShowCleanVersion = True
        End If

        Select Case Format
            Case ReturnFormat.HTML, ReturnFormat.CleanHTML
                If name <> "" Then
                    Return View(name, viewModel)
                Else
                    Return View(viewModel)
                End If
            Case ReturnFormat.PartialHTML
                If name <> "" Then
                    Return PartialView(name, viewModel)
                Else
                    Return PartialView(viewModel)
                End If
            Case ReturnFormat.XML
                Return New XmlResult(jsonModel)
            Case ReturnFormat.JSON
                Return Json(jsonModel, JsonRequestBehavior.AllowGet)
            Case ReturnFormat.XLS
                If name = "" Then
                    name = "List"
                End If
                If Filename = "" Then
                    Filename = name
                End If
                Dim List As New List(Of Object)
                List.Add(jsonModel)
                Dim EE As New ExcelExport(name, List.ToDataTable)
                Response.AddHeader("Content-Disposition", "inline; filename=" & Filename & ".xls")
                Return File(System.Text.Encoding.UTF8.GetBytes(EE.GetExcelData), "application/excel")

            Case ReturnFormat.CSV
                If name = "" Then
                    name = "List"
                End If
                If Filename = "" Then
                    Filename = name
                End If
                Dim list As New List(Of Object)
                list.Add(jsonModel)
                Dim csv = CSVFile.LoadFromDataTable(list.ToDataTable)
                Response.AddHeader("Content-Disposition", "inline; filename=" & Filename & ".csv")
                Return File(System.Text.Encoding.UTF8.GetBytes(csv.GetAsCSV), "application/excel")
            Case ReturnFormat.Pdf
                Return Nothing
            Case Else
                Return Nothing
        End Select
        Return Nothing
    End Function
    Private Function Format_Return(ByVal viewModel As IEnumerable) As ActionResult
        Return Format_Return(viewModel, "")
    End Function
    Private Function Format_Return(ByVal viewModel As IEnumerable, ByVal name As String) As ActionResult
        FormatResultCalled = True
        If Format = ReturnFormat.CleanHTML Then
            ViewBag.ShowCleanVersion = True
        End If
        Dim jsonList As New List(Of Object)
        For Each i In viewModel
            jsonList.Add(i)
        Next
        Select Case Format
            Case ReturnFormat.HTML, ReturnFormat.CleanHTML
                If name <> "" Then
                    Return View(name, viewModel)
                Else
                    Return View(viewModel)
                End If
            Case ReturnFormat.PartialHTML
                If name <> "" Then
                    Return PartialView(name, viewModel)
                Else
                    Return PartialView(viewModel)
                End If
            Case ReturnFormat.XML
                'Dim jsonList As IEnumerable = viewModel
                Try
                    jsonList = (From p As IEntity In jsonList Select p.GetJSON).ToList
                Catch ex As Exception
                End Try
                Return New XmlResult(jsonList)
            Case ReturnFormat.JSON
                'dim jsonList As IEnumerable = viewModel
                Try
                    jsonList = (From p As IEntity In jsonList Select p.GetJSON).ToList
                Catch ex As Exception
                End Try
                Return Json(jsonList, JsonRequestBehavior.AllowGet)
            Case ReturnFormat.XLS
                If name = "" Then
                    name = "List"
                End If
                If Filename = "" Then
                    Filename = name
                End If
                'Dim jsonList As IEnumerable = viewModel
                Try
                    jsonList = (From p As IEntity In jsonList Select p.GetJSONForDataTable).ToList
                Catch ex As Exception
                End Try
                Dim EE As New ExcelExport(name, jsonList.ToDataTable)
                Response.AddHeader("Content-Disposition", "inline; filename=" & Filename & ".xls")
                Return File(System.Text.Encoding.UTF8.GetBytes(EE.GetExcelData), "application/excel")

            Case ReturnFormat.CSV
                If name = "" Then
                    name = "List"
                End If
                If Filename = "" Then
                    Filename = name
                End If
                'Dim jsonList As IEnumerable = viewModel
                Try
                    jsonList = (From p As IEntity In jsonList Select p.GetJSONForDataTable).ToList
                Catch ex As Exception
                End Try
                Dim cSv = CSVFile.LoadFromDataTable(jsonList.ToDataTable)
                Response.AddHeader("Content-Disposition", "inline; filename=" & Filename & ".csv")
                Return File(System.Text.Encoding.UTF8.GetBytes(cSv.GetAsCSV), "application/excel")
            Case ReturnFormat.Pdf
                Return Nothing
            Case Else
                Return Nothing
        End Select
        'Return Nothing
    End Function

    'Protected Function Format_Return(ByVal viewModel As Object, ByVal IsError As Boolean, ByVal ErrorMessage As String, ByVal HTMLRoute As ActionResult) As ActionResult
    '    Dim List As New List(Of Object)
    '    Dim Tstr As String = ""
    '    Dim ListViolations As New List(Of RuleViolation)
    '    If viewModel IsNot Nothing Then
    '        Try
    '            ListViolations = viewModel.GetRuleViolations
    '            For Each RV In ListViolations
    '                Tstr = Tstr & RV.ErrorMessage & "<br/>"
    '                List.Add(New With {.PropertyName = RV.PropertyName, .Message = RV.ErrorMessage})
    '            Next
    '        Catch ex As Exception
    '        End Try
    '    End If
    '    Tstr = Tstr & ErrorMessage
    '    Dim O As Object = Nothing
    '    If viewModel IsNot Nothing Then
    '        Try
    '            O = viewModel.GetJSON
    '        Catch ex As Exception
    '            O = viewModel
    '        End Try
    '    End If
    '    Dim p As Object = New With {.IsError = IsError, .Message = Tstr, .Item = O, .FieldList = List}
    '    AddRuleViolations(ListViolations)
    '    ModelState.AddModelError("_FORM", ErrorMessage)
    '    ViewData("ErrorMessage") = ErrorMessage
    '    Return FormatResult(p, HTMLRoute)
    'End Function
    'Private Function Format_Return(ByVal viewModel As Object, ByVal HTMLRoute As ActionResult) As ActionResult
    '    Dim Name As String = ""
    '    Dim JSONModel As Object = viewModel
    '    Try
    '        JSONModel = viewModel.getJSON
    '    Catch ex As Exception
    '    End Try
    '    Select Case Format
    '        Case ReturnFormat.HTML
    '            Return HTMLRoute
    '        Case ReturnFormat.XML
    '            'Return New XmlResult(JSONModel)
    '        Case ReturnFormat.JSON
    '            Return Json(JSONModel, JsonRequestBehavior.AllowGet)
    '        Case ReturnFormat.XLS
    '            If Name = "" Then
    '                Name = "List"
    '            End If
    '            Dim List As New List(Of Object)
    '            List.Add(JSONModel)
    '            Dim EE As New ExcelExport(Name, List.ToDataTable)
    '            Return File(System.Text.Encoding.UTF8.GetBytes(EE.GetExcelData), "")

    '        Case ReturnFormat.CSV
    '            If Name = "" Then
    '                Name = "List"
    '            End If
    '            Dim List As New List(Of Object)
    '            List.Add(JSONModel)
    '            Dim CSv = CSVFile.LoadFromDataTable(List.ToDataTable)
    '            Response.AddHeader("Content-Disposition", "inline; filename=" & Name & ".csv")
    '            Return File(System.Text.Encoding.UTF8.GetBytes(CSv.GetAsCSV), "application/excel")
    '        Case ReturnFormat.Pdf
    '            Return Nothing
    '        Case Else
    '            Return Nothing
    '    End Select
    '    Return Nothing
    'End Function
    'Protected Function FormatResult(Of T)(ByVal viewModel As PaginatedList(Of T), ByVal HTMLRoute As ActionResult) As ActionResult
    '    Dim name As String = ""
    '    Select Case Format
    '        Case ReturnFormat.HTML
    '            Return HTMLRoute
    '        Case ReturnFormat.XML
    '            Dim List = From p As Object In viewModel.ToList Select p.GetJSON
    '            'Return New XmlResult(List)
    '        Case ReturnFormat.JSON
    '            Dim List = From p As Object In viewModel.ToList Select p.GetJSON
    '            Return Json(List, JsonRequestBehavior.AllowGet)
    '        Case ReturnFormat.XLS
    '            If name = "" Then
    '                name = "List"
    '            End If
    '            Try
    '                Dim PL = New PaginatedList(Of T)(viewModel.MySource, Nothing, viewModel.MySource.Count + 10, "", "")
    '                viewModel = PL
    '            Catch ex As Exception
    '            End Try
    '            Dim List = From p As Object In viewModel.ToList Select p.GetJSONForDataTable
    '            Dim EE As New ExcelExport(name, List.ToDataTable)
    '            Response.AddHeader("Content-Disposition", "inline; filename=" & name & ".xls")
    '            Return File(System.Text.Encoding.UTF8.GetBytes(EE.GetExcelData), "application/excel")

    '        Case ReturnFormat.CSV
    '            If name = "" Then
    '                name = "List"
    '            End If
    '            Try
    '                Dim PL = New PaginatedList(Of T)(viewModel.MySource, Nothing, viewModel.MySource.Count + 10, "", "")
    '                viewModel = PL
    '            Catch ex As Exception
    '            End Try
    '            Dim List = From p As Object In viewModel.ToList Select p.GetJSONForDataTable
    '            Dim CSv = CSVFile.LoadFromDataTable(List.ToDataTable)
    '            Response.AddHeader("Content-Disposition", "inline; filename=" & name & ".csv")
    '            Return File(System.Text.Encoding.UTF8.GetBytes(CSv.GetAsCSV), "application/excel")
    '        Case ReturnFormat.Pdf
    '            Return Nothing
    '        Case Else
    '            Return Nothing
    '    End Select
    '    Return Nothing
    'End Function


    'Public Format As ReturnFormat = ReturnFormat.HTML

    'Public Property Filename = ""


    'Protected Overrides Sub OnActionExecuting(ByVal filterContext As System.Web.Mvc.ActionExecutingContext)
    '    MyBase.OnActionExecuting(filterContext)


    '    Dim routeValues = filterContext.RouteData.Values
    '    Dim formatKey = "Format"
    '    If routeValues.ContainsKey(formatKey) Then
    '        Dim requestedFormat As String = routeValues(formatKey).ToString.ToLower
    '        Format = ReturnFormat.HTML
    '        Try
    '            For Each RF As ReturnFormat In [Enum].GetValues(GetType(ReturnFormat))
    '                If RF.ToString.ToUpper = requestedFormat.ToUpper Then
    '                    Format = RF
    '                    Exit For
    '                End If
    '            Next
    '        Catch ex As Exception

    '        End Try
    '    Else
    '        Dim requestedFormat As String = filterContext.HttpContext.Request(formatKey)
    '        If requestedFormat Is Nothing Then requestedFormat = ""
    '        requestedFormat = requestedFormat.ToString.ToLower
    '        Format = ReturnFormat.HTML
    '        Try
    '            For Each RF As ReturnFormat In [Enum].GetValues(GetType(ReturnFormat))
    '                If RF.ToString.ToUpper = requestedFormat.ToUpper Then
    '                    Format = RF
    '                    Exit For
    '                End If
    '            Next
    '            'Format = [Enum].Parse(GetType(ReturnFormat), requestedFormat)
    '        Catch ex As Exception

    '        End Try
    '    End If

    '    If routeValues.ContainsKey("Action") AndAlso routeValues.ContainsKey("Controller") Then
    '        Filename = routeValues("Controller").ToString & "_" & routeValues("Action").ToString
    '    End If
    'End Sub

    'Public Sub New()

    'End Sub

    'Public Sub AddRuleViolations(ByVal errors As IEnumerable(Of RuleViolation))
    '    For Each issue In errors
    '        ModelState.AddModelError(issue.PropertyName, issue.ErrorMessage)
    '    Next
    'End Sub
    ''ByVal IsError As Boolean, Optional ByVal ErrorMessage As String = ""


    'Protected Function GetUploadedFile(ByVal OrigItem As FileInteface, ByVal NewItem As FileInteface, ByVal BaseName As String, ByVal ItemName As String) As FileInteface
    '    Dim Item As FileInteface
    '    If OrigItem IsNot Nothing Then
    '        Item = OrigItem
    '    Else
    '        Item = NewItem
    '    End If
    '    Dim GUID As String = Request.Form(BaseName & "_guid")
    '    Dim Fname As String = Request.Form(BaseName & "_filename")
    '    'Response.Write("GUID=" & GUID & "<br/>")
    '    'Response.Flush()
    '    'Response.Write("Fname=" & Fname & "<br/>")
    '    'Response.Flush()

    '    If Request.Files(BaseName) IsNot Nothing AndAlso Request.Files(BaseName).ContentLength > 1000 Then
    '        Item.SetInitialFileFromUploadedFile(Request.Files(BaseName))
    '        Item.Name = ItemName
    '        Item.PostedTimeStamp = Now
    '        Item.SpecialHandling()
    '        Return Item
    '    ElseIf Fname IsNot Nothing AndAlso Fname.Length > 0 Then
    '        If GUID = "" Then GUID = Session("FileGUID")
    '        GUID = Request.Form(BaseName & "_FileGUID")
    '        Dim FI As New System.IO.FileInfo(Settings.FileLocation & "/AsyncUploads/" & GUID & ".tmp")
    '        If FI.Exists Then
    '            Item.SetIntialFileDataFromFile(FI, Fname)
    '            Item.Name = ItemName
    '            Item.PostedTimeStamp = Now
    '            Item.SpecialHandling()
    '            'P.BackImage.IsReady = True

    '            Return Item
    '        Else
    '            Throw New Exception("Temp file not found on server. FileName=[" & Fname & "] TempName=[" & FI.FullName & "]")
    '        End If
    '    End If
    '    Return OrigItem
    'End Function


    'Protected Function FormatResult(Of T)(ByVal viewModel As LinqToSql.IEntity(Of T), ByVal IsError As Boolean, ByVal ErrorMessage As String, Optional ByVal UseRazor As Boolean = False, Optional ByVal ForAdmin As Boolean = True) As ActionResult
    '    Return FormatResult(viewModel, IsError, ErrorMessage, "", UseRazor:=UseRazor, ForAdmin:=ForAdmin)
    'End Function

    'Protected Function FormatResult(Of T)(ByVal viewModel As LinqToSql.IEntity(Of T), ByVal IsError As Boolean, ByVal ErrorMessage As String, ByVal Name As String, Optional ByVal UseRazor As Boolean = False, Optional ByVal ForAdmin As Boolean = True) As ActionResult

    '    Return FormatResult(CType(viewModel, Object), IsError, ErrorMessage, Name, UseRazor:=UseRazor, ForAdmin:=ForAdmin)

    'End Function

    'Protected Function FormatResult(ByVal viewModel As Object, ByVal IsError As Boolean, ByVal ErrorMessage As String, Optional ByVal UseRazor As Boolean = False, Optional ByVal ForAdmin As Boolean = True) As ActionResult
    '    Return FormatResult(viewModel, IsError, ErrorMessage, "", UseRazor:=UseRazor, ForAdmin:=ForAdmin)
    'End Function

    'Protected Function FormatResult(ByVal viewModel As Object, ByVal IsError As Boolean, ByVal ErrorMessage As String, ByVal Name As String, Optional ByVal UseRazor As Boolean = False, Optional ByVal ForAdmin As Boolean = True) As ActionResult

    '    Dim List As New List(Of Object)
    '    Dim Tstr As String = ""
    '    Try
    '        Dim ListViolations = viewModel.GetRuleViolations
    '        AddRuleViolations(ListViolations)
    '        For Each RV In ListViolations
    '            Tstr = Tstr & RV.ErrorMessage & "<br/>"
    '            List.Add(New With {.PropertyName = RV.PropertyName, .Message = RV.ErrorMessage})
    '        Next
    '    Catch ex As Exception

    '    End Try
    '    Tstr = Tstr & ErrorMessage
    '    Dim Ob = viewModel
    '    Try
    '        Ob = viewModel.GetJSon
    '    Catch ex As Exception

    '    End Try


    '    Dim p As Object = New With {.IsError = IsError, .Message = Tstr, .Item = Ob, .FieldList = List}

    '    ModelState.AddModelError("_FORM", ErrorMessage)
    '    ViewData("ErrorMessage") = ErrorMessage
    '    If Format = ReturnFormat.HTML Then
    '        Return FormatResult(viewModel, Name, UseRazor:=UseRazor, ForAdmin:=ForAdmin)
    '    Else
    '        Return FormatResult(p, Name, UseRazor:=UseRazor, ForAdmin:=ForAdmin)
    '    End If

    'End Function
    'Protected Function FormatResult(Of T)(ByVal viewModel As LinqToSql.IEntity(Of T), Optional ByVal UseRazor As Boolean = False, Optional ByVal ForAdmin As Boolean = True) As ActionResult

    '    Return FormatResult(CType(viewModel, Object), UseRazor:=UseRazor, ForAdmin:=ForAdmin)
    'End Function
    'Protected Function FormatResult(Of T)(ByVal viewModel As LinqToSql.IEntity(Of T), ByVal Name As String, Optional ByVal UseRazor As Boolean = False, Optional ByVal ForAdmin As Boolean = True) As ActionResult
    '    Return FormatResult(CType(viewModel, Object), Name, UseRazor:=UseRazor, ForAdmin:=ForAdmin)
    'End Function
    'Protected Function FormatResult(ByVal viewModel As Object, Optional ByVal UseRazor As Boolean = False, Optional ByVal ForAdmin As Boolean = True) As ActionResult
    '    Return FormatResult(viewModel, "", UseRazor:=UseRazor, ForAdmin:=ForAdmin)
    'End Function
    'Protected Function FormatResult(ByVal viewModel As Object, ByVal Name As String, Optional ByVal UseRazor As Boolean = False, Optional ByVal ForAdmin As Boolean = True) As ActionResult
    '    Dim JSONModel As Object = viewModel
    '    Try
    '        JSONModel = viewModel.getJSON
    '    Catch ex As Exception

    '    End Try
    '    Select Case Format
    '        Case ReturnFormat.HTML
    '            If UseRazor Then
    '                If Name <> "" Then
    '                    Return RazorView(Name, viewModel, ForAdmin:=ForAdmin)
    '                Else
    '                    Return RazorView(viewModel, ForAdmin:=ForAdmin)
    '                End If
    '            Else

    '                If Name <> "" Then
    '                    Return View(Name, viewModel)
    '                Else
    '                    Return View(viewModel)
    '                End If
    '            End If

    '        Case ReturnFormat.XML
    '            'Return New XmlResult(JSONModel)

    '        Case ReturnFormat.JSON
    '            Return Json(JSONModel, JsonRequestBehavior.AllowGet)

    '        Case ReturnFormat.XLS
    '            If Name = "" Then
    '                Name = "List"
    '            End If
    '            If Filename = "" Then
    '                Filename = Name
    '            End If
    '            Dim List As New List(Of Object)
    '            List.Add(JSONModel)
    '            Dim EE As New ExcelExport(Name, List.ToDataTable)
    '            Response.AddHeader("Content-Disposition", "inline; filename=" & Filename & ".xls")
    '            Return File(System.Text.Encoding.UTF8.GetBytes(EE.GetExcelData), "application/excel")


    '        Case ReturnFormat.CSV
    '            If Name = "" Then
    '                Name = "List"
    '            End If
    '            If Filename = "" Then
    '                Filename = Name
    '            End If
    '            Dim List As New List(Of Object)
    '            List.Add(JSONModel)
    '            Dim CSv = CSVFile.LoadFromDataTable(List.ToDataTable)
    '            Response.AddHeader("Content-Disposition", "inline; filename=" & Filename & ".csv")
    '            Return File(System.Text.Encoding.UTF8.GetBytes(CSv.GetAsCSV), "application/excel")

    '        Case ReturnFormat.Pdf
    '            Return Nothing

    '        Case Else
    '            Return Nothing

    '    End Select
    '    Return Nothing
    'End Function
    'Protected Function FormatResult(Of T)(ByVal viewModel As List(Of T), Optional ByVal UseRazor As Boolean = False, Optional ByVal ForAdmin As Boolean = True) As ActionResult
    '    Return FormatResult(viewModel, "", UseRazor:=UseRazor, ForAdmin:=ForAdmin)
    'End Function
    'Protected Function FormatResult(Of T)(ByVal viewModel As List(Of T), ByVal Name As String, Optional ByVal UseRazor As Boolean = False, Optional ByVal ForAdmin As Boolean = True) As ActionResult
    '    Dim PL = New PaginatedList(Of T)(viewModel.AsQueryable, Nothing, viewModel.Count + 10, "", "")
    '    Return FormatResult(PL, Name, UseRazor:=UseRazor, ForAdmin:=ForAdmin)
    'End Function
    'Protected Function FormatResult(Of T)(ByVal viewModel As PaginatedList(Of T), Optional ByVal UseRazor As Boolean = False, Optional ByVal ForAdmin As Boolean = True) As ActionResult
    '    Return FormatResult(viewModel, "", UseRazor:=UseRazor, ForAdmin:=ForAdmin)
    'End Function
    'Protected Function FormatResult(Of T)(ByVal viewModel As PaginatedList(Of T), ByVal Name As String, Optional ByVal UseRazor As Boolean = False, Optional ByVal ForAdmin As Boolean = True) As ActionResult
    '    Select Case Format
    '        Case ReturnFormat.HTML
    '            If UseRazor Then
    '                If Name <> "" Then
    '                    Return RazorView(Name, viewModel, ForAdmin:=ForAdmin)
    '                Else
    '                    Return RazorView(viewModel, ForAdmin:=ForAdmin)
    '                End If
    '            Else

    '                If Name <> "" Then
    '                    Return View(Name, viewModel)
    '                Else
    '                    Return View(viewModel)
    '                End If
    '            End If

    '        Case ReturnFormat.XML
    '            Dim List = From p As Object In viewModel.ToList Select p.GetJSON
    '            'Return New XmlResult(List)

    '        Case ReturnFormat.JSON
    '            Dim List = From p As Object In viewModel.ToList Select p.GetJSON
    '            Return Json(List, JsonRequestBehavior.AllowGet)

    '        Case ReturnFormat.XLS
    '            If Name = "" Then
    '                Name = "List"
    '            End If
    '            If Filename = "" Then
    '                Filename = Name
    '            End If
    '            Try
    '                Dim PL = New PaginatedList(Of T)(viewModel.MySource, Nothing, viewModel.MySource.Count + 10, "", "")
    '                viewModel = PL

    '            Catch ex As Exception

    '            End Try
    '            Dim List = From p As Object In viewModel.ToList Select p.GetJSONForDataTable
    '            Dim EE As New ExcelExport(Name, List.ToDataTable)
    '            Response.AddHeader("Content-Disposition", "inline; filename=" & Filename & ".xls")
    '            Return File(System.Text.Encoding.UTF8.GetBytes(EE.GetExcelData), "application/excel")


    '        Case ReturnFormat.CSV
    '            If Name = "" Then
    '                Name = "List"
    '            End If
    '            If Filename = "" Then
    '                Filename = Name
    '            End If
    '            Try
    '                Dim PL = New PaginatedList(Of T)(viewModel.MySource, Nothing, viewModel.MySource.Count + 10, "", "")
    '                viewModel = PL

    '            Catch ex As Exception

    '            End Try
    '            Dim List = From p As Object In viewModel.ToList Select p.GetJSONForDataTable
    '            Dim CSv = CSVFile.LoadFromDataTable(List.ToDataTable)
    '            Response.AddHeader("Content-Disposition", "inline; filename=" & Filename & ".csv")
    '            Return File(System.Text.Encoding.UTF8.GetBytes(CSv.GetAsCSV), "application/excel")

    '        Case ReturnFormat.Pdf
    '            Return Nothing

    '        Case Else
    '            Return Nothing

    '    End Select
    '    Return Nothing
    'End Function













    'Protected Function FormatResult(Of T)(ByVal viewModel As LinqToSql.IEntity(Of T), ByVal IsError As Boolean, ByVal ErrorMessage As String, ByVal HTMLRoute As ActionResult) As ActionResult
    '    Return FormatResult(CType(viewModel, Object), IsError, ErrorMessage, HTMLRoute)
    'End Function


    'Protected Function FormatResult(ByVal viewModel As Object, ByVal IsError As Boolean, ByVal ErrorMessage As String, ByVal HTMLRoute As ActionResult) As ActionResult

    '    Dim List As New List(Of Object)
    '    Dim Tstr As String = ""
    '    Dim ListViolations As New List(Of RuleViolation)
    '    If viewModel IsNot Nothing Then
    '        Try

    '            ListViolations = viewModel.GetRuleViolations
    '            For Each RV In ListViolations
    '                Tstr = Tstr & RV.ErrorMessage & "<br/>"
    '                List.Add(New With {.PropertyName = RV.PropertyName, .Message = RV.ErrorMessage})
    '            Next
    '        Catch ex As Exception

    '        End Try
    '    End If
    '    Tstr = Tstr & ErrorMessage
    '    Dim O As Object = Nothing
    '    If viewModel IsNot Nothing Then
    '        Try
    '            O = viewModel.GetJSON
    '        Catch ex As Exception
    '            O = viewModel
    '        End Try
    '    End If

    '    Dim p As Object = New With {.IsError = IsError, .Message = Tstr, .Item = O, .FieldList = List}

    '    AddRuleViolations(ListViolations)
    '    ModelState.AddModelError("_FORM", ErrorMessage)
    '    ViewData("ErrorMessage") = ErrorMessage

    '    Return FormatResult(p, HTMLRoute)
    'End Function
    'Protected Function FormatResult(Of T)(ByVal viewModel As LinqToSql.IEntity(Of T), ByVal HTMLRoute As ActionResult) As ActionResult
    '    Return FormatResult(viewModel, HTMLRoute)
    'End Function
    'Protected Function FormatResult(ByVal viewModel As Object, ByVal HTMLRoute As ActionResult) As ActionResult
    '    Dim Name As String = ""
    '    Dim JSONModel As Object = viewModel
    '    Try
    '        JSONModel = viewModel.getJSON
    '    Catch ex As Exception

    '    End Try
    '    Select Case Format
    '        Case ReturnFormat.HTML
    '            Return HTMLRoute

    '        Case ReturnFormat.XML
    '            'Return New XmlResult(JSONModel)

    '        Case ReturnFormat.JSON
    '            Return Json(JSONModel, JsonRequestBehavior.AllowGet)

    '        Case ReturnFormat.XLS
    '            If Name = "" Then
    '                Name = "List"
    '            End If
    '            Dim List As New List(Of Object)
    '            List.Add(JSONModel)
    '            Dim EE As New ExcelExport(Name, List.ToDataTable)
    '            Return File(System.Text.Encoding.UTF8.GetBytes(EE.GetExcelData), "")


    '        Case ReturnFormat.CSV
    '            If Name = "" Then
    '                Name = "List"
    '            End If
    '            Dim List As New List(Of Object)
    '            List.Add(JSONModel)
    '            Dim CSv = CSVFile.LoadFromDataTable(List.ToDataTable)
    '            Response.AddHeader("Content-Disposition", "inline; filename=" & Name & ".csv")
    '            Return File(System.Text.Encoding.UTF8.GetBytes(CSv.GetAsCSV), "application/excel")

    '        Case ReturnFormat.Pdf
    '            Return Nothing

    '        Case Else
    '            Return Nothing

    '    End Select
    '    Return Nothing
    'End Function
    'Protected Function FormatResult(Of T)(ByVal viewModel As List(Of T), ByVal HTMLRoute As ActionResult) As ActionResult
    '    Dim PL = New PaginatedList(Of T)(viewModel.AsQueryable, Nothing, viewModel.Count + 10, "", "")
    '    Return FormatResult(PL, HTMLRoute)
    'End Function
    'Protected Function FormatResult(Of T)(ByVal viewModel As PaginatedList(Of T), ByVal HTMLRoute As ActionResult) As ActionResult
    '    Dim name As String = ""
    '    Select Case Format
    '        Case ReturnFormat.HTML
    '            Return HTMLRoute

    '        Case ReturnFormat.XML
    '            Dim List = From p As Object In viewModel.ToList Select p.GetJSON
    '            'Return New XmlResult(List)

    '        Case ReturnFormat.JSON
    '            Dim List = From p As Object In viewModel.ToList Select p.GetJSON
    '            Return Json(List, JsonRequestBehavior.AllowGet)

    '        Case ReturnFormat.XLS
    '            If name = "" Then
    '                name = "List"
    '            End If
    '            Try
    '                Dim PL = New PaginatedList(Of T)(viewModel.MySource, Nothing, viewModel.MySource.Count + 10, "", "")
    '                viewModel = PL

    '            Catch ex As Exception

    '            End Try
    '            Dim List = From p As Object In viewModel.ToList Select p.GetJSONForDataTable
    '            Dim EE As New ExcelExport(name, List.ToDataTable)
    '            Response.AddHeader("Content-Disposition", "inline; filename=" & name & ".xls")
    '            Return File(System.Text.Encoding.UTF8.GetBytes(EE.GetExcelData), "application/excel")


    '        Case ReturnFormat.CSV
    '            If name = "" Then
    '                name = "List"
    '            End If
    '            Try
    '                Dim PL = New PaginatedList(Of T)(viewModel.MySource, Nothing, viewModel.MySource.Count + 10, "", "")
    '                viewModel = PL

    '            Catch ex As Exception

    '            End Try
    '            Dim List = From p As Object In viewModel.ToList Select p.GetJSONForDataTable
    '            Dim CSv = CSVFile.LoadFromDataTable(List.ToDataTable)
    '            Response.AddHeader("Content-Disposition", "inline; filename=" & name & ".csv")
    '            Return File(System.Text.Encoding.UTF8.GetBytes(CSv.GetAsCSV), "application/excel")

    '        Case ReturnFormat.Pdf
    '            Return Nothing

    '        Case Else
    '            Return Nothing

    '    End Select
    '    Return Nothing
    'End Function


End Class
