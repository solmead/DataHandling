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

Public Module Extensions

    Public VideoList() As String = {"FLV", "MOD", "AVI", "MPG", "MPEG", "MOV", "WMV", "VOB", "VRO", "MTS", "QT", "SWF", "MP4", "M4V", "OGV"}
    Public ImageList() As String = {"JPG", "JPEG", "PNG", "BMP", "TGA", "GIF"}
    Public AudioList() As String = {"MP3", "WAV", "OGG", "AAC", "WMA", "M4A"}

    <Extension()> _
    Public Function RenderContentPage(Page As WebViewPage, Content As Object) As MvcHtmlString

        If Page.ViewBag.ShowCleanVersion Then
            Page.Layout = "~/Views/Shared/_LayoutClean.cshtml"
        End If
        Page.ViewBag.Page = Content
        Page.ViewBag.Title = Content.Title
        Return Page.Html.Partial("~/Views/Page/_Details.cshtml", Content)
    End Function
    <Extension()> _
    Public Function RenderContentPage(Html As HtmlHelper, Content As Object) As MvcHtmlString
        Dim View As WebViewPage = System.Web.WebPages.WebPageContext.Current.Page

        'Dim T = System.Web.Mvc.ViewPage

        Return View.RenderContentPage(Content)
    End Function

    Public Function CalcCRC32(ByVal File As System.IO.FileInfo) As String

        Dim f = File.OpenRead
        Dim sha1 As New System.Security.Cryptography.SHA1CryptoServiceProvider()
        sha1.ComputeHash(f)


        Dim hash = sha1.Hash
        Dim buff = New StringBuilder()
        For Each hashByte As Byte In hash

            buff.Append(String.Format("{0:X1}", hashByte))
        Next
        f.Close()
        Return buff.ToString
    End Function

    Public Function MaxDateIf(ByVal IfValue As Boolean, ByVal D1 As Nullable(Of Date), ByVal D2 As Nullable(Of Date)) As Nullable(Of Date)
        If Not IfValue Then Return D1
        If D1.HasValue AndAlso Not D2.HasValue Then
            Return D1
        ElseIf Not D1.HasValue AndAlso D2.HasValue Then
            Return D2
        ElseIf Not D1.HasValue AndAlso Not D2.HasValue Then
            Return Nothing
        End If

        If D1.Value > D2.Value Then
            Return D1
        Else
            Return D2
        End If
    End Function
    Public Function MaxDate(ByVal D1 As Nullable(Of Date), ByVal D2 As Nullable(Of Date)) As Nullable(Of Date)

        If D1.HasValue AndAlso Not D2.HasValue Then
            Return D1
        ElseIf Not D1.HasValue AndAlso D2.HasValue Then
            Return D2
        ElseIf Not D1.HasValue AndAlso Not D2.HasValue Then
            Return Nothing
        End If
        If D1 > D2 Then
            Return D1
        Else
            Return D2
        End If
    End Function
    Public Function CleanPhoneNumber(ByVal Number As String) As String
        If Number Is Nothing OrElse Number = "" Then Return ""
        Dim tstr = Replace(Replace(Replace(Replace(Replace(Number, "(", ""), ")", ""), "-", ""), " ", ""), ".", "")
        If tstr = Nothing Then tstr = ""
        tstr = tstr.Trim()
        If tstr = Nothing Then tstr = ""
        If tstr = "" Then Return tstr
        If Val(tstr) = 0 Then Return ""
        While tstr.Length < 10
            tstr = "0" & tstr
        End While
        Return "(" & Left(tstr, 3) & ") " & Mid(tstr, 4, 3) & "-" & Mid(tstr, 7)
    End Function

    <Extension()> _
    Public Function ShortDate(ByVal D As Nullable(Of Date)) As String
        Dim D1 As Date = Nothing
        If Not D.HasValue OrElse D.Value = D1 Then
            Return ""
        Else
            Return D.Value.ToShortDateString
        End If
    End Function
    <Extension()> _
    Public Function ShortDate(ByVal D As String) As String
        If Not IsDate(D) OrElse D = "" Then
            Return ""
        End If
        Dim RD As Nullable(Of Date) = CDate(D)
        Dim D1 As Date = Nothing
        If Not RD.HasValue OrElse RD.Value = D1 Then
            Return ""
        Else
            Return RD.Value.ToShortDateString
        End If
    End Function
    <Extension()> _
    Public Function ShortTime(ByVal D As Nullable(Of Date)) As String
        Dim D1 As Date = Nothing
        If Not D.HasValue OrElse D.Value = D1 Then
            Return ""
        Else
            Return D.Value.ToShortTimeString
        End If
    End Function
    <Extension()> _
    Public Function ShortenString(ByVal Str As String, ByVal len As Integer) As String
        If Str Is Nothing Then Str = ""
        Dim S = StripHTML(Str)
        If S.Length > len Then
            S = S.Substring(0, len)
            Dim a = InStrRev(S, " ")
            If a > 0 Then
                S = Mid(S, 1, a - 1)
            End If
            S = S & "..."
        End If
        Return S

    End Function
    <Extension()> _
    Public Function StripHTML(ByVal Content As String) As String
        Return RemoveBetween(Content, "<", ">").Replace("&nbsp;", " ")
    End Function
    <Extension()> _
    Public Function RemoveBetween(ByVal Content As String, ByVal BeginChar As String, ByVal EndChar As String) As String
        Dim i As Integer = Content.IndexOf(BeginChar)
        Dim cnt As Integer = 0
        While i >= 0 AndAlso cnt < 10000
            Dim E = Content.IndexOf(EndChar, i + BeginChar.Length)
            If E > i + BeginChar.Length Then
                Content = Content.Substring(0, i) & Content.Substring(E + EndChar.Length)
            Else
                cnt = 10001
            End If
            i = Content.IndexOf(BeginChar)
        End While
        Return Content
    End Function

    <Extension()> _
    Public Function GetNextBetween(ByVal Content As String, ByVal BeginChar As String, ByVal EndChar As String, Optional StartPos As Integer = 0) As String
        Dim i As Integer = Content.IndexOf(BeginChar, StartPos)
        Dim cnt As Integer = 0
        While i >= 0 AndAlso cnt < 10000
            Dim E = Content.IndexOf(EndChar, i + BeginChar.Length)
            If E > i + BeginChar.Length Then
                i = i + BeginChar.Length
                Return Content.Substring(i, E - i)
            Else
                cnt = 10001
            End If
            i = Content.IndexOf(BeginChar)
        End While
        Return ""
    End Function
    <Extension()> _
    Public Function ConvertToCollection(Ob As Object) As FormCollection
        Dim F As New FormCollection
        Try
            'Dim KeyName = GetKeyProperty()
            Dim Tp As Type = Ob.GetType()
            Dim props = Tp.GetProperties((BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.FlattenHierarchy))
            For Each p In props
                If p.CanWrite Then

                    Dim V As Object = Nothing
                    Try
                        V = p.GetValue(Ob, Nothing)
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

    <Extension()>
    Public Function Age(Birthdate As Date, Optional ReferenceDate As Nullable(Of Date) = Nothing) As Integer
        If Not ReferenceDate.HasValue Then
            ReferenceDate = Now
        End If
        Dim RDate As Date = ReferenceDate.Value
        Dim BD = Birthdate
        Dim TD = New Date(RDate.Year, BD.Month, BD.Day)
        If TD <= RDate Then
            Return RDate.Year - BD.Year
        Else
            Return (RDate.Year - BD.Year) - 1
        End If
    End Function
    <Extension()>
    Public Function Age(Birthdate As Nullable(Of Date), Optional ReferenceDate As Nullable(Of Date) = Nothing) As Integer
        If Not ReferenceDate.HasValue Then
            ReferenceDate = Now
        End If
        Dim RDate As Date = ReferenceDate.Value
        If Not Birthdate.HasValue Then
            Birthdate = RDate
        End If
        Dim BD = Birthdate.Value
        Dim TD As Date
        Try
            TD = New Date(RDate.Year, BD.Month, BD.Day)
        Catch ex As Exception
            TD = New Date(RDate.Year, BD.Month, BD.Day - 1)
        End Try

        If TD <= RDate Then
            Return RDate.Year - BD.Year
        Else
            Return (RDate.Year - BD.Year) - 1
        End If
    End Function

    <Extension()> _
    Public Function ToDictionary(ByVal Collection As System.Web.Mvc.FormCollection) As Dictionary(Of String, String)
        Dim Dic As New Dictionary(Of String, String)
        For Each C As String In Collection.Keys
            Dic.Add(C, Collection(C))
        Next

        Return Dic

    End Function
    <Extension()> _
    Public Function ContentType(ByVal File As System.IO.FileInfo) As String
        Dim Extension As String = File.Extension.Replace(".", "").ToUpper

        Dim mContentType = "application/octet-stream"
        If VideoList.Contains(Extension) Then
            mContentType = "video/mpeg"
        End If
        Select Case Extension
            Case "JPG"
                mContentType = "image/jpeg"
            Case "GIF"
                mContentType = "image/gif"
            Case "BMP"
                mContentType = "image/bmp"
            Case "MP3"
                mContentType = "audio/mp3"
            Case "WAV"
                mContentType = "audio/wav"
            Case "M4A"
                mContentType = "audio/x-m4a"
            Case "MP4"
                mContentType = "video/mp4"
            Case "M4V"
                mContentType = "video/x-m4v"
            Case "MOV"
                mContentType = "video/quicktime"
            Case "PDF"
                mContentType = "application/pdf"
            Case "OGV"
                mContentType = "video/ogg"
            Case "WEBM"
                mContentType = "video/webm"
        End Select

        Return mContentType
    End Function
    <Extension()> _
    Public Function GetKey(Of T0, T1)(ByVal Dic As Dictionary(Of T0, T1), ByVal Item As T1) As T0
        For Each K In Dic.Keys
            If Dic(K).Equals(Item) Then
                Return K
            End If
        Next
        Return Nothing
    End Function


    Public Function GetTheKey(Of T0, T1)(ByVal Dic As Dictionary(Of T0, T1), ByVal Item As T1) As T0
        For Each K In Dic.Keys
            If Dic(K).Equals(Item) Then
                Return K
            End If
        Next
        Return Nothing
    End Function

    <Extension()> _
    Public Function IsEmail(ByVal email As String, Optional ByVal Required As Boolean = False) As Boolean
        If String.IsNullOrEmpty(email) Then Return Not Required
        email = email.ToLower
        'regular expression pattern for valid email
        'addresses, allows for the following domains:
        'com,edu,info,gov,int,mil,net,org,biz,name,museum,coop,aero,pro,tv
        Dim pattern As String = "^[_a-z0-9-]+(.[a-z0-9-]+)*@[-.a-zA-Z0-9]+(\.[-.a-zA-Z0-9]+)*\." & _
        "(com|edu|info|gov|int|mil|net|org|biz|name|museum|coop|aero|pro|tv|[a-zA-Z]{2})$"

        'pattern = "^[-a-z0-9-]+(-.[a-z0-9-]+)*@[-.a-zA-Z0-9]+(\.[-.a-zA-Z0-9]+)*\." & _
        '"(com|edu|info|gov|int|mil|net|org|biz|name|museum|coop|aero|pro|tv|[a-zA-Z]{2})$"
        'Regular expression object
        Dim check As New Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace)
        'boolean variable to return to calling method
        Dim valid As Boolean = False

        'make sure an email address was provided
        If String.IsNullOrEmpty(email) Then
            valid = Not Required
        Else
            'use IsMatch to validate the address
            valid = check.IsMatch(email)
        End If
        'return the value to the calling method

        If Not valid Then
            Dim i As Integer = 1

        End If
        If (From C In email Where C = "@" Select C).Count > 1 Then
            valid = False
        End If
        'Return True
        Return valid
    End Function
    <Extension()> _
    Public Function RemoveIllegalCharacters(ByVal name As String) As String
        Dim tname = name.Trim.ToUpper
        Dim List2 = {"?", "~", "|", ","} '
        For Each I In List2
            tname = tname.Replace(I, "")
        Next

        Return tname
    End Function
    <Extension()> _
    Public Function ContainsInvalidWords(ByVal name As String) As Boolean
        Dim List = {"Child", "Girl", "Boy", "Female", "Male", "Baby", "Husband", "Wife", "Jane Doe", "John Doe"}
        Dim tname = name.Trim.ToUpper
        For Each I In List
            I = I.ToUpper
            Dim tstr() = I.Split(" ")
            Dim fnd = True
            For Each S In tstr
                fnd = fnd AndAlso (tname.StartsWith(S & " ") OrElse tname.Contains(" " & S & " ") OrElse tname.EndsWith(" " & S))
            Next
            If fnd Then Return True
        Next
        Dim List2 = {"?", "(", ")", "~", "`", "!", "@", "#", "$", "%", "^", "&", "*", "_", "+", "=", "{", "}", "[", "]", ":", ";", """", "<", ">", "/", "|", "\", ","} '
        For Each I In List2
            I = I.ToUpper
            Dim tstr() = I.Split(" ")
            Dim fnd = tname.Contains(I)
            If fnd Then Return True
        Next
        

        Return False
    End Function
    <Extension()> _
    Public Function ContainsTwoNames(ByVal name As String) As Boolean
        If name.Contains("&") OrElse name.Contains("/") OrElse name.ToUpper.Contains(" AND ") OrElse name.ToUpper.Contains(" OR ") Then ' OrElse name.Trim.Contains(" ") Then
            Return True
        Else
            Return False
        End If
    End Function

    <Extension()> _
    Public Function IsMobile(ByVal helper As HtmlHelper) As Boolean
        Dim Context = helper.ViewContext.RequestContext
        Dim UAgent = Context.HttpContext.Request.UserAgent
        Return Context.HttpContext.Request.Browser.IsMobileDevice OrElse helper.IsIPad
    End Function
    <Extension()> _
    Public Function IsIPad(ByVal helper As HtmlHelper) As Boolean
        Dim Context = helper.ViewContext.RequestContext
        Dim UAgent = Context.HttpContext.Request.UserAgent
            Return UAgent.Contains("iPad")
    End Function
    <Extension()> _
    Public Function IsIPhone(ByVal helper As HtmlHelper) As Boolean
        Dim Context = helper.ViewContext.RequestContext
        Dim UAgent = Context.HttpContext.Request.UserAgent
            Return UAgent.Contains("iPhone")
    End Function
    <Extension()> _
    Public Function IsIPod(ByVal helper As HtmlHelper) As Boolean
        Dim Context = helper.ViewContext.RequestContext
        Dim UAgent = Context.HttpContext.Request.UserAgent
            Return UAgent.Contains("iPod")
    End Function
    <Extension()> _
    Public Function ViewDataList(Of T)(ByVal helper As HtmlHelper, ByVal name As String) As IEnumerable(Of T)
        If (helper.ViewData(name) IsNot Nothing) Then
            Return CType(helper.ViewData(name), IEnumerable(Of T))
        End If
        Return New List(Of T)
    End Function

    <Extension()> _
    Public Function ViewDataSingle(Of T)(ByVal helper As HtmlHelper, ByVal name As String) As T
        If (helper.ViewData(name) IsNot Nothing) Then
            Return CType(helper.ViewData(name), T)
        End If
        Return Nothing
    End Function

    <Extension()> _
    Public Function FileNameWithoutExtension(ByVal File As System.IO.FileInfo) As String
        Return Left(File.Name, Len(File.Name) - (Len(File.Extension)))
    End Function
    <Extension()> _
    Public Function BaseURL(ByVal source As UrlHelper) As String
        Try

            Dim U = source.RequestContext.HttpContext.Request.Url
            Dim S As String = U.AbsoluteUri
            Dim HTs = Mid(S, 1, InStr(S, "://") + 3 - 1)
            S = Mid(S, InStr(S, "://") + 3)
            If InStr(S, "/") > 0 Then
                S = Mid(S, 1, InStr(S, "/") - 1)
            End If
            Return HTs & S

        Catch ex As Exception

        End Try
        Return ""
    End Function
    <Extension()> _
    Public Function ImageLink(ByVal source As HtmlHelper, Title As String, ByVal ImageSource As String, URL As String, ByVal htmlAttributes As Object, ByVal imgAttributes As Object) As MvcHtmlString
        '<a href="<%=Url.VideoDetails(item.ID)%>"><img src="<%=item.WebImagePathAndName%>" alt="" class="video-thumbnail" /></a>
        'Dim URL As New UrlHelper(source.ViewContext.RequestContext)
        Dim postAction = URL ' URL.RouteUrl(RouteName, routeValues)

        'Dim SB As New StringBuilder
        Dim aTag = New TagBuilder("a")
        aTag.MergeAttribute("href", postAction)
        aTag.MergeAttribute("title", Title)
        If htmlAttributes IsNot Nothing Then
            Dim htmlRVD = New RouteValueDictionary(htmlAttributes)
            aTag.MergeAttributes(htmlRVD, True)
        End If
        Dim imgTag = New TagBuilder("img")
        If (ImageSource.StartsWith("~/")) Then
            ImageSource = source.ViewContext.RequestContext.HttpContext.Request.ApplicationPath + ImageSource.Substring(2)
        End If
        imgTag.MergeAttribute("src", ImageSource)
        imgTag.MergeAttribute("alt", Title)
        If imgAttributes IsNot Nothing Then
            Dim imgRVD = New RouteValueDictionary(imgAttributes)
            imgTag.MergeAttributes(imgRVD, True)
        End If
        aTag.InnerHtml = imgTag.ToString(TagRenderMode.SelfClosing)
        Return New MvcHtmlString(aTag.ToString(TagRenderMode.Normal))

    End Function
    <Extension()> _
    Public Function ImageRouteLink(ByVal source As HtmlHelper, Title As String, ByVal ImageSource As String, ByVal RouteName As String, ByVal routeValues As Object, ByVal htmlAttributes As Object, ByVal imgAttributes As Object) As MvcHtmlString
        '<a href="<%=Url.VideoDetails(item.ID)%>"><img src="<%=item.WebImagePathAndName%>" alt="" class="video-thumbnail" /></a>
        Dim URL As New UrlHelper(source.ViewContext.RequestContext)
        Dim postAction = URL.RouteUrl(RouteName, routeValues)

        'Dim SB As New StringBuilder
        Dim aTag = New TagBuilder("a")
        aTag.MergeAttribute("href", postAction)
        aTag.MergeAttribute("title", Title)
        If htmlAttributes IsNot Nothing Then
            Dim htmlRVD = New RouteValueDictionary(htmlAttributes)
            aTag.MergeAttributes(htmlRVD, True)
        End If
        Dim imgTag = New TagBuilder("img")
        If (ImageSource.StartsWith("~/")) Then
            ImageSource = source.ViewContext.RequestContext.HttpContext.Request.ApplicationPath + ImageSource.Substring(2)
        End If
        imgTag.MergeAttribute("src", ImageSource)
        imgTag.MergeAttribute("alt", Title)
        If imgAttributes IsNot Nothing Then
            Dim imgRVD = New RouteValueDictionary(imgAttributes)
            imgTag.MergeAttributes(imgRVD, True)
        End If
        aTag.InnerHtml = imgTag.ToString(TagRenderMode.SelfClosing)
        Return New MvcHtmlString(aTag.ToString(TagRenderMode.Normal))

    End Function
    '<Extension()> _
    'Public Function DeleteActionLink(ByVal source As HtmlHelper, ByVal linkText As String, ByVal actionName As String, ByVal routeValues As Object, ByVal htmlAttributes As Object) As String
    '    Dim URL As New UrlHelper(source.ViewContext.RequestContext)

    '    Dim postAction = URL.Action(actionName, routeValues)

    '    Dim SB As New StringBuilder

    '    SB.AppendLine("<form action=""" & postAction & """ method=""post"">")
    '    SB.AppendLine(source.ActionLink(linkText, actionName, routeValues, htmlAttributes))
    '    SB.AppendLine("</form>")

    '    Return SB.ToString

    'End Function

    <Extension()> _
    Public Function PostRouteLink(ByVal source As HtmlHelper, ByVal linkText As String, routeName As String, ByVal routeValues As Object, Optional formAttributes As Object = Nothing, Optional htmlAttributes As Object = Nothing) As MvcHtmlString
        Dim URL As New UrlHelper(source.ViewContext.RequestContext)
        Dim postAction = URL.RouteUrl(routeName, routeValues)

        'Dim SB As New StringBuilder

        Dim formTag = New TagBuilder("form")
        formTag.MergeAttribute("action", postAction)
        formTag.MergeAttribute("method", "post")
        Dim formRVD = New RouteValueDictionary(formAttributes)
        formTag.MergeAttributes(formRVD, True)

        Dim btnTag = New TagBuilder("input")

        Dim htmlRVD = New RouteValueDictionary(htmlAttributes)
        btnTag.MergeAttribute("type", "submit")
        btnTag.MergeAttribute("value", linkText)
        btnTag.MergeAttributes(htmlRVD, True)
        'SB.AppendLine(source.ActionLink(linkText, actionName, routeValues, htmlAttributes))
        '<input type="submit" value="Save" class="button" /> 

        formTag.InnerHtml = btnTag.ToString(TagRenderMode.SelfClosing)
        'SB.AppendLine(btnTag.ToString(TagRenderMode.SelfClosing))
        'SB.AppendLine("</form>")

        Return New MvcHtmlString(formTag.ToString(TagRenderMode.Normal))

    End Function

    <Extension()> _
    Public Function PostActionLink(ByVal source As HtmlHelper, ByVal linkText As String, ByVal actionName As String, ByVal controllerName As String, ByVal routeValues As Object, Optional formAttributes As Object = Nothing, Optional htmlAttributes As Object = Nothing) As MvcHtmlString
        Dim URL As New UrlHelper(source.ViewContext.RequestContext)
        Dim postAction = URL.Action(actionName, controllerName, routeValues)

        Dim formTag = New TagBuilder("form")
        formTag.MergeAttribute("action", postAction)
        formTag.MergeAttribute("method", "post")
        Dim formRVD = New RouteValueDictionary(formAttributes)
        formTag.MergeAttributes(formRVD, True)

        Dim btnTag = New TagBuilder("input")

        Dim htmlRVD = New RouteValueDictionary(htmlAttributes)
        btnTag.MergeAttribute("type", "submit")
        btnTag.MergeAttribute("value", linkText)
        btnTag.MergeAttributes(htmlRVD, True)
        'SB.AppendLine(source.ActionLink(linkText, actionName, routeValues, htmlAttributes))
        '<input type="submit" value="Save" class="button" /> 
        formTag.InnerHtml = btnTag.ToString(TagRenderMode.SelfClosing)
        'SB.AppendLine(btnTag.ToString(TagRenderMode.SelfClosing))
        'SB.AppendLine("</form>")

        Return New MvcHtmlString(formTag.ToString(TagRenderMode.Normal))


    End Function

    <Extension()> _
    Public Function DeleteRouteLink(ByVal source As HtmlHelper, ByVal linkText As String, routeName As String, ByVal routeValues As Object, ByVal htmlAttributes As Object) As MvcHtmlString

        Return source.PostRouteLink(linkText, routeName, routeValues, New With {.class = "DeleteForm"}, htmlAttributes)

        'Dim URL As New UrlHelper(source.ViewContext.RequestContext)
        'Dim postAction = URL.RouteUrl(routeName, routeValues)

        'Dim SB As New StringBuilder


        'SB.AppendLine("<form action=""" & postAction & """ method=""post"" class='DeleteForm'>")

        'Dim btnTag = New TagBuilder("input")

        'Dim htmlRVD = New RouteValueDictionary(htmlAttributes)
        'btnTag.MergeAttribute("type", "submit")
        'btnTag.MergeAttribute("value", linkText)
        'btnTag.MergeAttributes(htmlRVD, True)
        ''SB.AppendLine(source.ActionLink(linkText, actionName, routeValues, htmlAttributes))
        ''<input type="submit" value="Save" class="button" /> 
        'SB.AppendLine(btnTag.ToString(TagRenderMode.SelfClosing))
        'SB.AppendLine("</form>")

        'Return New MvcHtmlString(SB.ToString)

    End Function

    <Extension()> _
    Public Function DeleteActionLink(ByVal source As HtmlHelper, ByVal linkText As String, ByVal actionName As String, ByVal controllerName As String, ByVal routeValues As Object, ByVal htmlAttributes As Object) As MvcHtmlString

        Return source.PostActionLink(linkText, actionName, controllerName, routeValues, New With {.class = "DeleteForm"}, htmlAttributes)

        'Dim URL As New UrlHelper(source.ViewContext.RequestContext)
        'Dim postAction = URL.Action(actionName, controllerName, routeValues)

        'Dim SB As New StringBuilder


        'SB.AppendLine("<form action=""" & postAction & """ method=""post"" class='DeleteForm'>")

        'Dim btnTag = New TagBuilder("input")

        'Dim htmlRVD = New RouteValueDictionary(htmlAttributes)
        'btnTag.MergeAttribute("type", "submit")
        'btnTag.MergeAttribute("value", linkText)
        'btnTag.MergeAttributes(htmlRVD, True)
        ''SB.AppendLine(source.ActionLink(linkText, actionName, routeValues, htmlAttributes))
        ''<input type="submit" value="Save" class="button" /> 
        'SB.AppendLine(btnTag.ToString(TagRenderMode.SelfClosing))
        'SB.AppendLine("</form>")

        'Return New MvcHtmlString(SB.ToString)

    End Function

    Public Function Shell(ByVal Command As String, ByVal Args As String, Optional ByVal DontWait As Boolean = False, Optional ByVal CallBack As Action(Of String) = Nothing) As String
        Dim p = New System.Diagnostics.Process()
        Dim pStart = New System.Diagnostics.ProcessStartInfo()
        pStart.UseShellExecute = False
        pStart.WindowStyle = ProcessWindowStyle.Normal ' System.Diagnostics.ProcessWindowStyle.Hidden
        pStart.Arguments = Args
        pStart.FileName = Command
        pStart.CreateNoWindow = True ' True
        pStart.RedirectStandardOutput = True
        pStart.RedirectStandardError = True


        p.StartInfo = pStart

        Dim sTimeOut = 60 * 60 * 2 'System.Configuration.ConfigurationManager.AppSettings("TranscodeTimeoutSeconds")
        Dim timeOut = Integer.Parse(sTimeOut)

        'DebugMsg("Starting Process Command:[" & Command & "] Args:[" & Args & "]")
        Dim success = p.Start()
        p.PriorityClass = ProcessPriorityClass.BelowNormal
        If DontWait Then
            Return ""
        End If
        'DebugMsg("Process Started")
        Dim SR2 = p.StandardError
        Dim S2 As String = ""
        If (success) Then
            'DebugMsg("Process Success")

            Dim elapsedTime = System.Diagnostics.Stopwatch.StartNew()

            'DebugMsg("Waiting till process exiting")
            While (Not p.HasExited)
                Dim SS = SR2.ReadLine()
                'DebugMsg(SS)
                If CallBack IsNot Nothing Then
                    CallBack(SS)
                End If
                S2 = S2 & SS & vbCrLf
                If (elapsedTime.Elapsed.Seconds > timeOut) Then

                    p.Kill()
                    p.Close()
                    p.Dispose()
                    p = Nothing

                    elapsedTime.Stop()
                    elapsedTime = Nothing
                    Exit While
                End If
            End While
            'DebugMsg("Process Completed")
        End If
        'DebugMsg("Getting Output")

        Dim SR = p.StandardOutput
        Dim S = SR.ReadToEnd

        S2 = S2 & SR2.ReadToEnd
        Return S2
    End Function




    Public Function GetAsTimeSpan(ByVal Length As Double) As TimeSpan
        Dim S As Double = Length
        Dim M As Double = S / 60
        Dim H As Double = M / 60
        Dim D As Double = H / 24
        H = (D - Int(D)) * 24
        M = (H - Int(H)) * 60
        S = (M - Int(M)) * 60
        D = Int(D)
        H = Int(H)
        M = Int(M)


        Dim ts As New TimeSpan(D, H, M, S)
        Return ts

    End Function



    '<Extension()> _
    'Public Function UploadFileActionLinkSilverlight(ByVal source As HtmlHelper, ByVal ID As String, Optional ByVal AllowVideos As Boolean = False, Optional ByVal AllowImages As Boolean = False, Optional ByVal AllowAudios As Boolean = False) As MvcHtmlString
    '    Dim URL As New UrlHelper(source.ViewContext.RequestContext)
    '    Dim FileGUID As String = source.ViewData("FileGUID")
    '    If FileGUID Is Nothing OrElse FileGUID = "" Then
    '        FileGUID = Guid.NewGuid.ToString
    '    End If
    '    Dim FileGUIDText = ID & "_" & FileGUID
    '    Dim postAction = URL.RouteUrl("Default", New With {.Controller = "UploadedFile", .Action = "AsyncUpload", .FileGUID = FileGUIDText})
    '    Dim UploadChunkSize = -1
    '    Dim MaximumUpload = -1

    '    Dim Filter = ""
    '    ' "Images (*.jpg;*.gif)|*.jpg;*.gif|All Files (*.*)|*.*"
    '    Dim FExt = ""
    '    Dim Fst = True
    '    If AllowImages Then
    '        Fst = True
    '        FExt = ""
    '        For Each St In ImageList
    '            If Not Fst Then FExt = FExt & ";"
    '            FExt = FExt & "*." & St
    '            Fst = False
    '        Next
    '        If Filter.Length > 0 Then Filter = Filter & "|"
    '        Filter = Filter & "Images (" & FExt & ")|" & FExt
    '    End If
    '    If AllowVideos Then
    '        Fst = True
    '        FExt = ""
    '        For Each St In VideoList
    '            If Not Fst Then FExt = FExt & ";"
    '            FExt = FExt & "*." & St
    '            Fst = False
    '        Next
    '        If Filter.Length > 0 Then Filter = Filter & "|"
    '        Filter = Filter & "Videos (" & FExt & ")|" & FExt
    '    End If
    '    If AllowAudios Then
    '        Fst = True
    '        FExt = ""
    '        For Each St In AudioList
    '            If Not Fst Then FExt = FExt & ";"
    '            FExt = FExt & "*." & St
    '            Fst = False
    '        Next
    '        If Filter.Length > 0 Then Filter = Filter & "|"
    '        Filter = Filter & "Audio Files (" & FExt & ")|" & FExt
    '    End If
    '    If Filter.Length > 0 Then Filter = Filter & "|"
    '    Filter = Filter & "All Files (*.*)|*.*"

    '    ' display files in the uploader
    '    Dim AllowThumbnail = "false"
    '    ' javascript function to call when all files have uploaded
    '    Dim JavascriptCompleteFunction = "UploadComplete"
    '    Dim MaxNumberToUpload = -1


    '    Dim args = "UploadPage=" & URL.Encode(postAction) & ",UploadChunkSize=" & UploadChunkSize & ",MaximumUpload=" & MaximumUpload & ",Filter=" & Filter & ",AllowThumbnail=" & AllowThumbnail & ",JavascriptCompleteFunction=" & URL.Encode(JavascriptCompleteFunction) & ",MaxNumberToUpload=" & MaxNumberToUpload & ",FileGUID=" & FileGUIDText + ",JavascriptStartFunction=" + URL.Encode("")


    '    Dim SB As New StringBuilder

    '    SB.AppendLine("<object id=""" & ID & """ name=""" & ID & """ data=""data:application/x-silverlight-2,"" type=""application/x-silverlight-2"" width=""105px"" height=""50px"">")
    '    SB.AppendLine("	<param name=""source"" value=""/ClientBin/FileUploader.xap""/>")
    '    SB.AppendLine("	<param name=""onError"" value=""onSilverlightError"" />")
    '    SB.AppendLine("	<param name=""background"" value=""white"" />")
    '    SB.AppendLine("	<param name=""minRuntimeVersion"" value=""4.0.50401.0"" />")
    '    SB.AppendLine("	<param name=""autoUpgrade"" value=""true"" />")
    '    SB.AppendLine("	<param name=""initParams"" value=""" & args & """ />")
    '    SB.AppendLine("	<a href=""http://go.microsoft.com/fwlink/?LinkID=149156&v=4.0.50401.0"" style=""text-decoration: none;"">")
    '    SB.AppendLine("		<img src=""http://go.microsoft.com/fwlink/?LinkId=161376"" alt=""Get Microsoft Silverlight"" style=""border-style: none""/>")
    '    SB.AppendLine("	</a>")
    '    SB.AppendLine("</object><iframe id=""_sl_historyFrame"" style=""visibility:hidden;height:0px;width:0px;border:0px""></iframe>")

    '    '    <object data="data:application/x-silverlight-2," type="application/x-silverlight-2" width="100%" height="100%">
    '    '<param name="source" value="ClientBin/FileUploader.xap"/>
    '    '<param name="onError" value="onSilverlightError" />
    '    '<param name="background" value="white" />
    '    '<param name="minRuntimeVersion" value="4.0.50401.0" />
    '    '<param name="autoUpgrade" value="true" />
    '    '<a href="http://go.microsoft.com/fwlink/?LinkID=149156&v=4.0.50401.0" style="text-decoration:none">
    '    '  <img src="http://go.microsoft.com/fwlink/?LinkId=161376" alt="Get Microsoft Silverlight" style="border-style:none"/>
    '    '</a>
    '    ' </object><iframe id="_sl_historyFrame" style="visibility:hidden;height:0px;width:0px;border:0px"></iframe>


    '    Return New MvcHtmlString(SB.ToString)
    'End Function

    <Extension()> _
    Public Function UploadFileActionLink(ByVal source As HtmlHelper, ByVal ID As String, Optional callbackFunction As String = "") As MvcHtmlString
        Dim URL As New UrlHelper(source.ViewContext.RequestContext)
        Dim FileGUID As String = source.ViewData("FileGUID")
        If FileGUID Is Nothing OrElse FileGUID = "" Then
            FileGUID = Guid.NewGuid.ToString
        End If
        Dim FileGUIDText = ID & "_" & FileGUID
        'Dim postAction = URL.RouteUrl("Default", New With {.Controller = "UploadedFile", .Action = "AsyncUpload", .FileGUID = FileGUIDText})

        Dim SB As New StringBuilder
        SB.AppendLine("<input type=""file"" id=""" & ID & """ name=""" & ID & """ />")
        SB.AppendLine("")
        SB.AppendLine(source.Hidden(ID & "_FileGUID_Actual", FileGUID).ToString)
        SB.AppendLine(source.Hidden(ID & "_FileGUID", ID & "_" & FileGUID).ToString)
        SB.AppendLine("<script type=""text/javascript"">")
        SB.AppendLine("    $(document).ready(function() {")
        SB.AppendLine("         var uploader = System.FileUploader('#" & ID & "', '" + ID + "_" + FileGUID + "'" + IIf(String.IsNullOrWhiteSpace(callbackFunction), "", ", " + callbackFunction) + ");")
        SB.AppendLine("    });")
        SB.AppendLine("</script>")

        Return New MvcHtmlString(SB.ToString)
    End Function

    Public Function CheckAndReturn(ByVal Value1 As Object, ByVal Value2 As Object) As Object
        If Value1 Is Nothing Then
            Return Value2
        Else
            Return Value1
        End If
    End Function
    Public Function CheckAndReturn(ByVal Value1 As Nullable(Of Integer), ByVal Value2 As Nullable(Of Integer)) As Nullable(Of Integer)
        If Not Value1.HasValue OrElse Value1.Value = 0 Then
            Return Value2
        Else
            Return Value1
        End If
    End Function
    Public Function CheckAndReturn(ByVal Value1 As String, ByVal Value2 As String) As String
        If Value1 = "" Then
            Return Value2
        Else
            Return Value1
        End If

    End Function
    Public Function CheckAndReturn(ByVal Value1 As Nullable(Of Date), ByVal Value2 As Nullable(Of Date)) As Nullable(Of Date)
        If Not Value1.HasValue OrElse Value1.Value = Nothing Then
            Return Value2
        Else
            Return Value1
        End If

    End Function

End Module
