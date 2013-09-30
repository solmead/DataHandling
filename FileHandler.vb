
Imports System.Web

Public Class FileHandler
    Implements System.Web.IHttpHandler

    Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        'context.Response.ContentType = "text/plain"
        'context.Response.Write("Hello World!")
        Dim IStr = context.Request.InputStream

        Dim Server = context.Server
        Dim request = context.Request

        Dim FileGUID = request.QueryString("FileGUID")
        Dim First = CBool(request.QueryString("First"))

        Dim Complete = CBool(request.QueryString("Complete"))
        Dim Resp = <Response><Error>True</Error><Description>Initial Value</Description></Response>

        If Not Complete Then
            Dim Data() As Byte
            ReDim Data(IStr.Length - 1)
            IStr.Read(Data, 0, IStr.Length)
            'Dim Data As String = System.Text.Encoding.UTF8.GetString(Barr)


            Debug.WriteLine(Now.ToString & " - Asyncupload")
            Try
                Dim tGUID As String = FileGUID
                If tGUID = "" Then
                    tGUID = "Generated_" & Guid.NewGuid().ToString
                End If

                tGUID = Replace(tGUID, ".", "")
                tGUID = Replace(tGUID, "\", "")
                tGUID = Replace(tGUID, "/", "")
                If tGUID.Length > 100 Then tGUID = tGUID.Substring(0, 100)
                Dim FI As New System.IO.FileInfo(Settings.FileLocation & "/AsyncUploads/" & tGUID & ".tmp")
                If Not FI.Directory.Exists Then FI.Directory.Create()
                If First Then
                    If FI.Exists Then FI.Delete()
                End If
                Debug.WriteLine("Saving file:" & FI.FullName)
                'Dim I = f.InputStream
                Debug.WriteLine("data.length=" & Data.Length)

                'Dim Buf() As Byte = System.Convert.FromBase64String(Data)
                Dim W = FI.OpenWrite
                W.Seek(0, IO.SeekOrigin.End)
                W.Write(Data, 0, Data.Length)
                W.Close()
                FI.Refresh()
                Debug.WriteLine("File Saved - " & FI.Length)
                Debug.WriteLine(Now.ToString & " - Finishing Asyncupload")
                Resp = <Response><Error>False</Error><Description></Description></Response>
            Catch ex As Exception
                Dim I As Integer = 0
                Resp = <Response><Error>True</Error><Description><%= ex.ToString %></Description></Response>
            End Try

        Else
            Try

                Dim CRC32 = request.QueryString("CRC32")
                Dim T_CRC32 = "No File"
                Dim tGUID As String = FileGUID
                If tGUID = "" Then
                    tGUID = "Generated_" & Guid.NewGuid().ToString
                End If

                tGUID = Replace(tGUID, ".", "")
                tGUID = Replace(tGUID, "\", "")
                tGUID = Replace(tGUID, "/", "")
                If tGUID.Length > 100 Then tGUID = tGUID.Substring(0, 100)
                Dim FI As New System.IO.FileInfo(Settings.FileLocation & "/AsyncUploads/" & tGUID & ".tmp")
                If Not FI.Directory.Exists Then FI.Directory.Create()
                If FI.Exists Then
                    T_CRC32 = DataHandling.CalcCRC32(FI)
                End If

                If CRC32.ToUpper <> T_CRC32.ToUpper Then
                    Resp = <Response><Error>True</Error><Description>CRC does not match. Sent:<%= CRC32 %> current file: <%= T_CRC32 %></Description></Response>
                Else
                    Resp = <Response><Error>False</Error><Description>CRC Matches</Description></Response>
                End If
            Catch ex As Exception
                Dim I As Integer = 0
                Resp = <Response><Error>True</Error><Description><%= ex.ToString %></Description></Response>
            End Try
        End If

        Dim objResponse As HttpResponse = context.Response
        objResponse.Write(Resp.ToString)
    End Sub

    ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class
