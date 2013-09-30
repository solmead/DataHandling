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

Public Interface FileInteface
    Property Name() As String
    Property FileName() As String
    Property PostedTimeStamp() As Date
    Sub SpecialHandling()
    Sub SetIntialFileDataFromFile(ByVal Fi As System.IO.FileInfo, Optional ByVal Filename As String = "")
    Sub SetIntialFileDataFromStream(ByVal gStream As System.IO.Stream, Optional ByVal Filename As String = "")
    Sub SetInitialFileFromUploadedFile(ByVal hpf As System.Web.HttpPostedFileBase, Optional ByVal Filename As String = "")
End Interface
