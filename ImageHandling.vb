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

Imports System.Drawing.Imaging
Imports System.IO


Public Class ImageHandling

    Public Enum ImageType
        None
        JPG
        PNG
        BMP
        GIF
    End Enum
    Public Enum HAlignment_Enum
        Center
        Left
        Right
    End Enum
    Public Enum VAlignment_Enum
        Center
        Top
        Bottom
    End Enum
    'Public Shared Function HandleFileRequest(ByVal V As UploadedFile, ByVal Width As Nullable(Of Integer), ByVal Height As Nullable(Of Integer), ByVal Stretch As Nullable(Of Boolean), ByVal ImageType As Nullable(Of ImageType), ByVal FillArea As Nullable(Of Boolean), ByVal HAlignment As Nullable(Of HAlignment_Enum), ByVal VAlignment As Nullable(Of VAlignment_Enum)) As System.IO.FileInfo
    '    Dim FI As New System.IO.FileInfo(V.ActualPathAndName)




    '    Dim FI2 As System.IO.FileInfo

    '    Dim Fname As String = FI.FileNameWithoutExtension()

    '    Dim Extension = FI.Extension

    '    If V.IsImage Then

    '        If Width.HasValue Then
    '            If Width > 1024 Then Width = 1024
    '            Fname = Fname & "_" & Width
    '        ElseIf Height.HasValue Then
    '            If Height > 1024 Then Height = 1024
    '            Fname = Fname & "_0_" & Height
    '        End If

    '        If Height.HasValue AndAlso Width.HasValue Then
    '            If Height > 1024 Then Height = 1024
    '            Fname = Fname & "_" & Height
    '        End If
    '        If Not Stretch.HasValue Then
    '            Stretch = False
    '        End If
    '        If Stretch Then
    '            Fname = Fname & "_Stretch"
    '        End If
    '        If Not FillArea.HasValue Then
    '            FillArea = False
    '        End If
    '        If FillArea Then
    '            Fname = Fname & "_FillArea"
    '        End If
    '        If Not HAlignment.HasValue Then HAlignment = HAlignment_Enum.Center
    '        If HAlignment <> HAlignment_Enum.Center Then
    '            Fname = Fname & "_H_" & HAlignment.ToString
    '        End If
    '        If Not VAlignment.HasValue Then VAlignment = VAlignment_Enum.Center
    '        If VAlignment <> VAlignment_Enum.Center Then
    '            Fname = Fname & "_V_" & VAlignment.ToString
    '        End If

    '        If ImageType.HasValue Then
    '            Extension = "." & ImageType.ToString
    '        End If
    '    End If
    '    'image/jpeg
    '    'image/gif
    '    'Dim FPR As FilePathResult
    '    'Dim DName As String = ""
    '    FI2 = New System.IO.FileInfo(FI.DirectoryName & "/Temp/" & Fname & Extension)
    '    'DName = FI2.Name
    '    If (FI2.Exists AndAlso (FI.LastWriteTime <= FI2.LastWriteTime OrElse FI.FullName.ToUpper = FI2.FullName.ToUpper)) Then
    '        Return FI2
    '    Else
    '        If V.IsImage Then
    '            ResizeImage(FI, FI2, Width, Height, Stretch, FillArea, HAlignment, VAlignment)
    '            Return FI2
    '        Else
    '            Return FI
    '        End If
    '    End If
    'End Function
    Public Shared Function WebPathAndNameCached(BaseFile As FileInfo, ByVal Width As Integer, ByVal Height As Integer, ByVal Stretch As Boolean, ByVal ImageType As ImageType, ByVal FillArea As Boolean, ByVal HAlignment As HAlignment_Enum, ByVal VAlignment As VAlignment_Enum) As System.IO.FileInfo
        Dim FI2 As System.IO.FileInfo
        Dim FI = BaseFile

        Dim Fname As String = FI.FileNameWithoutExtension()

        Dim Extension = FI.Extension

        If ImageType <> ImageType.None Then
            Extension = ImageType.ToString
        Else
            Try
                ImageType = [Enum].Parse(GetType(ImageType), Extension.Replace(".", "").ToUpper)
            Catch exp As Exception

            End Try
        End If

        If ImageType = ImageType.None Then
            ImageType = ImageType.JPG
        End If

        If Width > 0 Then
            If Width > 1024 Then Width = 1024
            Fname = Fname & "_" & Width
        ElseIf Height > 0 Then
            If Height > 1024 Then Height = 1024
            Fname = Fname & "_0_" & Height
        End If

        If Height > 0 AndAlso Width > 0 Then
            If Height > 1024 Then Height = 1024
            Fname = Fname & "_" & Height
        End If
        'If Not Stretch.HasValue Then
        '    Stretch = False
        'End If
        If Stretch Then
            Fname = Fname & "_Stretch"
        End If
        If FillArea Then
            Fname = Fname & "_FillArea"
        End If
        If HAlignment <> HAlignment_Enum.Center Then
            Fname = Fname & "_H_" & HAlignment.ToString
        End If
        If VAlignment <> VAlignment_Enum.Center Then
            Fname = Fname & "_V_" & VAlignment.ToString
        End If

        If ImageType <> ImageType.None Then
            Extension = "." & ImageType.ToString
        End If

        FI2 = New System.IO.FileInfo(FI.DirectoryName & "/Temp/" & Fname & Extension)
        If Not FI2.Directory.Exists Then
            FI2.Directory.Create()
        End If
        If Not (FI2.Exists AndAlso (FI.LastWriteTime <= FI2.LastWriteTime OrElse FI.FullName.ToUpper = FI2.FullName.ToUpper)) Then
            ResizeImage(FI, FI2, IIf(Width > 0, Width, Nothing), IIf(Height > 0, Height, Nothing), Stretch, FillArea, HAlignment, VAlignment)
            FI2.Refresh()
        End If

        Return FI2
    End Function
    Private Shared Sub ResizeImage(ByVal FromFile As System.IO.FileInfo, ByVal ToFile As System.IO.FileInfo, ByVal Width As Nullable(Of Integer), ByVal Height As Nullable(Of Integer), ByVal Stretch As Boolean, ByVal FillArea As Boolean, ByVal HAlignment As HAlignment_Enum, ByVal VAlignment As VAlignment_Enum)
        Try

            If ToFile.Exists Then ToFile.Delete()
            Dim Extension = ToFile.Extension.Replace(".", "")
            Dim UseTransparent As Boolean = False
            If Extension.ToUpper = "PNG" Then
                UseTransparent = True
            End If

            If Not Width.HasValue AndAlso Not Height.HasValue Then
                FromFile.CopyTo(ToFile.FullName)
                Exit Sub
            End If

            Dim Ti = New System.Drawing.Bitmap(FromFile.FullName)
            Dim BothSet As Boolean = True
            If Width.HasValue And Not Height.HasValue Then
                Height = Ti.Height / Ti.Width * Width
                BothSet = False
            End If
            If Height.HasValue And Not Width.HasValue Then
                Width = Ti.Width / Ti.Height * Height
                BothSet = False
            End If
            Dim PF = Ti.PixelFormat
            If UseTransparent Then
                PF = Drawing.Imaging.PixelFormat.Format32bppArgb
            End If
            Dim FT As New System.Drawing.Bitmap(Width, Height, PF)
            Dim Gr = System.Drawing.Graphics.FromImage(FT)
            If UseTransparent Then Gr.Clear(Drawing.Color.Transparent)
            Gr.Dispose()

            If BothSet Then
                Dim NWidth = Width
                Dim NHeight = Height


                Dim dx = Width / Ti.Width
                Dim dy = Height / Ti.Height
                Dim Comp As Boolean = (dx < dy)
                Dim Ratio = Ti.Width / Ti.Height
                'If Ratio > 1 Then Ratio = 1 / Ratio





                If Not Stretch Then
                    If FillArea Then Comp = Not Comp
                    If Comp Then
                        Dim y = Int(dx * Ti.Height)
                        Dim T = Int((Height - y) / 2)
                        If VAlignment = VAlignment_Enum.Top Then
                            T = 0
                        End If
                        If VAlignment = VAlignment_Enum.Bottom Then
                            T = Height - y
                        End If
                        Gr = System.Drawing.Graphics.FromImage(FT)
                        Gr.DrawImage(Ti, New Drawing.Rectangle(0, T, Width, y))
                        Gr.Dispose()
                        Ti = FT
                    Else
                        Dim x = Int(dy * Ti.Width)
                        Dim L = Int((Width - x) / 2)
                        If HAlignment = HAlignment_Enum.Left Then
                            L = 0
                        End If
                        If HAlignment = HAlignment_Enum.Right Then
                            L = Width - x
                        End If
                        Gr = System.Drawing.Graphics.FromImage(FT)
                        Gr.DrawImage(Ti, New Drawing.Rectangle(L, 0, x, Height))
                        'Gr.DrawImageUnscaled(Ti, New Drawing.Point(0, 0))
                        Gr.Dispose()
                        Ti = FT
                    End If
                Else
                    Gr = System.Drawing.Graphics.FromImage(FT)
                    Gr.DrawImage(Ti, New Drawing.Rectangle(0, 0, Width, Height))
                    Gr.Dispose()
                    Ti = FT
                End If
            Else
                Gr = System.Drawing.Graphics.FromImage(FT)
                Gr.DrawImage(Ti, New Drawing.Rectangle(0, 0, Width, Height))
                Gr.Dispose()
                Ti = FT
            End If

            If Extension.ToUpper = "JPG" Or Extension.ToUpper = "JPEG" Then

                ' Encoder parameter for image quality
                Dim qualityParam As New EncoderParameter(Encoder.Quality, 100)
                ' Jpeg image codec 
                Dim jpegCodec As ImageCodecInfo = GetEncoderInfo("image/jpeg")

                Dim encoderParams As New EncoderParameters(1)
                encoderParams.Param(0) = qualityParam
                Ti.Save(ToFile.FullName, jpegCodec, encoderParams)
                'Ti.Save(ToFile.FullName, Drawing.Imaging.ImageFormat.Jpeg)
            End If
            If Extension.ToUpper = "PNG" Then
                Ti.Save(ToFile.FullName, Drawing.Imaging.ImageFormat.Png)
            End If
            If Extension.ToUpper = "BMP" Then
                Ti.Save(ToFile.FullName, Drawing.Imaging.ImageFormat.Bmp)
            End If
            If Extension.ToUpper = "GIF" Then
                Ti.Save(ToFile.FullName, Drawing.Imaging.ImageFormat.Gif)
            End If
            If Extension.ToUpper = "TGA" Then
                Ti.Save(ToFile.FullName, Drawing.Imaging.ImageFormat.Jpeg)
            End If
        Catch ex As Exception
            Dim i As Integer = 1
        End Try

    End Sub

    ' Returns the image codec with the given mime type 
    Private Shared Function GetEncoderInfo(ByVal mimeType As String) As ImageCodecInfo
        ' Get image codecs for all image formats 
        Dim codecs As ImageCodecInfo() = ImageCodecInfo.GetImageEncoders()

        ' Find the correct image codec 
        For i As Integer = 0 To codecs.Length - 1
            If (codecs(i).MimeType = mimeType) Then
                Return codecs(i)
            End If
        Next i

        Return Nothing
    End Function
End Class
