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

Imports System.Net
Imports Krystalware.UploadHelper
Imports System.Xml

Public Class WebRequest

    Public Event PercentDone(ByVal Area As String, ByVal Percent As Double)
    Public Event DebugMsg(ByVal Msg As String)

    Public Enum Method_Enum
        [Get]
        Post
    End Enum
    Public Cookies As New System.Net.CookieContainer()
    Public Credentials As System.Net.ICredentials = System.Net.CredentialCache.DefaultCredentials()

    Public Sub PerDone(ByVal Area As String, ByVal Percent As Double)
        'Me.Area = Area
        'CurrentPercentage = Percent
        RaiseEvent PercentDone(Area, Percent)
    End Sub

    Public Sub DebugMessage(ByVal Msg As String)


        RaiseEvent DebugMsg(Msg)


    End Sub
    Private Function ProcessReturn(ByVal xml As String) As String
        Dim XMLD As XElement = XElement.Load(New System.IO.StringReader(xml))



        'XMLDoc.LoadXml(xml)
        '<Response><Error>False</Error><Description></Description></Response>
        Dim IsError = XMLD.<Error>.Value
        'CBool(XMLDoc.SelectSingleNode("Response\Error").Value)
        Dim Description = XMLD.<Description>.Value
        ' XMLDoc.SelectSingleNode("Response\Description").Value
        If IsError Then
            Throw New Exception(Description)
        End If
        Return Description
    End Function

    Private Function BytesString(ByVal ByteCount As Long) As String
        Try
            Dim R As Double = ByteCount
            Dim Rates = {"b", "kb", "mb", "gb", "tb", "pb"}
            Dim cnt = 0
            While R > 1024
                R = R / 1024
                cnt += 1
            End While

            Return (Int(R * 100)) / 100 & " " & Rates(cnt)
        Catch ex As Exception
            Return ""
        End Try
    End Function
    Private Function Rate(ByVal BytesUploaded As Long, ByVal Time As TimeSpan) As String
        Try
            Dim TM = Time.TotalMilliseconds / 1000
            If TM = 0 Then TM = 0.001

            Dim R As Double = ((BytesUploaded) / (TM)) * 8


            Return BytesString(R) & "ps"

        Catch ex As Exception

        End Try
        Return ""
    End Function

    Public Function RequestChunked(ByVal PostURL As String, ByVal PostItems As NameValueCollection, ByVal File As System.IO.FileInfo) As String
        Try
            File.Refresh()
            Dim First = True
            Dim TempURL = ""
            Dim Resp = ""
            Dim BArr() As Byte
            'Dim TempURL = PostURL & "filename=" & File.Name & "&StartByte=" & BytesUploaded & "&Complete=" & complete & "&FileGUID=" & FileGUID & "&First=false"
            Dim CS As Double = File.Length / 100
            If CS < 4096 * 2 Then
                CS = 4096 * 2
            ElseIf CS > 512 * 1024 Then
                CS = 512 * 1024
            End If

            For a = 0 To 64
                If 2 ^ a > CS Then
                    CS = 2 ^ (a - 1)
                    Exit For
                End If
            Next
            Dim ChunkSize = CS
            Dim cc As Long = 0
            Dim cnt As Long = File.Length
            Dim STim As Date = Now
            PerDone("Upload File:", 0)
            DebugMessage("Upload File:" & File.Name)
            DebugMessage("Upload File Exists:" & File.Exists)
            DebugMessage("Upload File length:" & File.Length)

            PostItems("First") = First
            PostItems("Chunked") = True
            PostItems("filename") = File.Name
            Dim Fs = File.OpenRead()
            While Fs.Position < Fs.Length
                Dim BytesUploaded = Fs.Position
                cc = BytesUploaded
                If cc = 0 Then cc = 1
                Dim TE = Now.AddSeconds((Now.Subtract(STim).TotalSeconds / cc) * (cnt - cc))
                PerDone(":" & TE.ToShortDateString & " " & TE.ToLongTimeString & ", " & Rate(cc, Now.Subtract(STim)) & " Up " & BytesString(cc) & " - " & BytesString(Fs.Length), cc / cnt)

                ReDim BArr(ChunkSize - 1)
                Dim Bytes = Fs.Read(BArr, 0, BArr.Length)
                If Bytes < BArr.Length Then
                    ReDim Preserve BArr(Bytes - 1)
                End If
                Dim MS As New System.IO.MemoryStream
                MS.Write(BArr, 0, BArr.Length)
                MS.Seek(0, IO.SeekOrigin.Begin)
                'Dim Files2 = {New Krystalware.UploadHelper.UploadFile(MS, "File1", File.Name, "application/octet-stream")}
                'Dim Resp2 = Request(PostURL, PostItems, Files2.ToList)

                PostItems("StartByte") = BytesUploaded
                PostItems("Complete") = False
                TempURL = PostURL & "?T=T"
                For Each Item In PostItems.Keys
                    TempURL = TempURL & "&" & Item & "=" & PostItems(Item)
                Next


                Resp = Request(TempURL, Method_Enum.Post, MS.ToArray)
                Resp = ProcessReturn(Resp)
                First = False
                PostItems("First") = First
            End While
            Fs.Close()
            Dim CRC32 = DataHandling.CalcCRC32(File)

            PerDone("", 0)
            DebugMessage("Doing CRC Check")
            PostItems("Complete") = True
            PostItems("CRC32") = CRC32
            TempURL = PostURL & "?T=T"
            For Each Item In PostItems.Keys
                TempURL = TempURL & "&" & Item & "=" & PostItems(Item)
            Next
            ReDim BArr(0)
            Resp = Request(TempURL, Method_Enum.Post, BArr)
            Resp = ProcessReturn(Resp)
            '<Response><Error>False</Error><Description></Description></Response>



            Return Resp

        Catch ex As Exception
            DebugMessage("Error on upload:" & ex.ToString)
            Throw ex
        End Try
        Return ""
    End Function

    'Public Function RequestChunked(ByVal PostURL As String, ByVal PostItems As NameValueCollection, ByVal File As System.IO.MemoryStream)

    'End Function

    'Public Function RequestChunked(ByVal PostURL As String, ByVal PostItems As NameValueCollection, ByVal File As Byte())

    'End Function

    Public Function Request(ByVal URL As String, ByVal PostItems As NameValueCollection, ByVal files As List(Of Krystalware.UploadHelper.UploadFile)) As String
        Dim WebReq As System.Net.HttpWebRequest = System.Net.WebRequest.Create(URL)
        WebReq.KeepAlive = False
        WebReq.SendChunked = True
        WebReq.Timeout = Integer.MaxValue
        WebReq.CookieContainer = Cookies
        WebReq.Credentials = Credentials
        WebReq.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)"

        Dim WebResp = HttpUploadHelper.Upload(WebReq, files.ToArray, PostItems)
        Dim Strm = New System.IO.StreamReader(WebResp.GetResponseStream())

        Dim XMLSTr = Strm.ReadToEnd()
        WebResp.Close()
        Return XMLSTr
    End Function

    Public Function Request(ByVal URL As String, ByVal Method As Method_Enum, Optional ByVal PostItems As NameValueCollection = Nothing) As String
        Dim webreq As System.Net.HttpWebRequest = System.Net.WebRequest.Create(URL)
        Dim Str As String = ""
        If PostItems IsNot Nothing Then
            For Each i In PostItems.Keys
                If Str.Length > 0 Then Str = Str & "&"
                Str = Str & i & "=" & PostItems(i)
            Next
        ElseIf Method = Method_Enum.Post Then
            Str = Str & "I=1"
        End If

        Dim postdata As Byte() = System.Text.Encoding.UTF8.GetBytes(Str)

        Return Request(URL, Method, postdata)

        'webreq.KeepAlive = True
        'webreq.SendChunked = True
        'webreq.Timeout = Integer.MaxValue
        'webreq.CookieContainer = Cookies
        'webreq.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)"
        'webreq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"

        'webreq.Method = Method.ToString.ToUpper
        'webreq.Credentials = Credentials
        'webreq.Proxy = GlobalProxySelection.GetEmptyWebProxy


        'Dim webstream As System.IO.Stream
        'If postdata.Length > 0 AndAlso Method = Method_Enum.Post Then
        '    webreq.ContentType = "application/x-www-form-urlencoded"
        '    webreq.SendChunked = True
        '    webreq.ContentLength = postdata.Length
        '    webstream = webreq.GetRequestStream()
        '    webstream.Write(postdata, 0, postdata.Length)
        '    webstream.Close()
        'End If

        'Dim response As System.Net.WebResponse = webreq.GetResponse()
        'Dim Status = CType(response, System.Net.HttpWebResponse).StatusDescription
        ''Debug.WriteLine(Status)

        'webstream = response.GetResponseStream()
        'Dim reader As New System.IO.StreamReader(webstream)
        'Dim responseFromServer = reader.ReadToEnd()


        'reader.Close()
        'webstream.Close()
        'response.Close()
        'Return responseFromServer
    End Function

    Public Function Request(ByVal URL As String, ByVal Method As Method_Enum, ByVal PostData As Byte()) As String
        Dim webreq As System.Net.HttpWebRequest = System.Net.WebRequest.Create(URL)
        'Dim Str As String = ""
        'If PostItems IsNot Nothing Then
        '    For Each i In PostItems.Keys
        '        If Str.Length > 0 Then Str = Str & "&"
        '        Str = Str & i & "=" & PostItems(i)
        '    Next
        'ElseIf Method = Method_Enum.Post Then
        '    Str = Str & "I=1"
        'End If

        'Dim postdata As Byte() = System.Text.Encoding.UTF8.GetBytes(Str)

        webreq.KeepAlive = True
        webreq.SendChunked = True
        webreq.Timeout = Integer.MaxValue
        webreq.CookieContainer = Cookies
        webreq.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)"
        webreq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"

        webreq.Method = Method.ToString.ToUpper
        webreq.Credentials = Credentials
        webreq.Proxy = Nothing 'GlobalProxySelection.GetEmptyWebProxy


        Dim webstream As System.IO.Stream
        If PostData.Length > 0 AndAlso Method = Method_Enum.Post Then
            webreq.ContentType = "application/x-www-form-urlencoded"
            webreq.SendChunked = True
            webreq.ContentLength = PostData.Length
            webstream = webreq.GetRequestStream()
            webstream.Write(PostData, 0, PostData.Length)
            webstream.Close()
        End If

        Dim response As System.Net.WebResponse = webreq.GetResponse()
        Dim Status = CType(response, System.Net.HttpWebResponse).StatusDescription
        'Debug.WriteLine(Status)

        webstream = response.GetResponseStream()
        Dim reader As New System.IO.StreamReader(webstream)
        Dim responseFromServer = reader.ReadToEnd()


        reader.Close()
        webstream.Close()
        response.Close()
        Return responseFromServer
    End Function
End Class
