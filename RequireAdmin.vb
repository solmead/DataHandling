
<AttributeUsage(AttributeTargets.Class Or AttributeTargets.Method, Inherited:=True)>
Public Class RequireAdmin
    Inherits ActionFilterAttribute


    Public Overrides Sub OnActionExecuting(ByVal filterContext As ActionExecutingContext)
        System.Diagnostics.Debug.WriteLine("RequireAdmin - OnActionExecuting")
        'filterContext.HttpContext.Trace.Write("(Logging Filter)Action Executing: " + _
        '        filterContext.ActionDescriptor.ActionName)
        Dim url = filterContext.HttpContext.Request.RawUrl
        Dim path = filterContext.HttpContext.Request.Path
        If (Not path.ToUpper().Contains("/ADMIN")) Then
            Dim newURL = url.Replace(path, "/Admin" + path)
            filterContext.Result = New RedirectResult(newURL)
        End If


        MyBase.OnActionExecuting(filterContext)
    End Sub

    Public Overrides Sub OnActionExecuted(ByVal filterContext As ActionExecutedContext)
        'System.Diagnostics.Debug.WriteLine("RequireAdmin - OnActionExecuted")
        'If Not filterContext.Exception Is Nothing Then
        '    filterContext.HttpContext.Trace.Write("(Logging Filter)Exception thrown")
        'End If
        If (TypeOf filterContext.Result Is ViewResult) Then
            Dim r = CType(filterContext.Result, ViewResult)
            r.MasterName = "~/Views/Shared/_LayoutAdmin.cshtml"
        End If
        MyBase.OnActionExecuted(filterContext)
    End Sub

End Class
