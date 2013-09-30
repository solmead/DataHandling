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

Public Class PaginatedList(Of T)
    Inherits List(Of T)

    Private m_PageIndex As Integer = 1
    Private m_PageSize As Integer = 2 '5
    Private m_TotalCount As Integer = 0
    Private m_TotalPages As Integer = 0
    Private m_PagingRoute As String = ""
    Private m_Controller As String = ""
    Private m_MySource As IQueryable(Of T)
    Public ReadOnly Property MySource() As IQueryable(Of T)
        Get
            Return m_MySource
        End Get
    End Property

    Public Property Controller() As String
        Get
            Return m_Controller
        End Get
        Set(ByVal Value As String)
            m_Controller = Value
        End Set
    End Property



    Public Property PagingRoute() As String
        Get
            Return m_PagingRoute
        End Get
        Set(ByVal Value As String)
            m_PagingRoute = Value
        End Set
    End Property


    Public ReadOnly Property PageIndex() As Integer
        Get
            Return m_PageIndex
        End Get
    End Property

    Public ReadOnly Property PageSize() As Integer
        Get
            Return m_PageSize
        End Get
    End Property

    Public ReadOnly Property TotalCount() As Integer
        Get
            Return m_TotalCount
        End Get
    End Property

    Public ReadOnly Property TotalPages() As Integer
        Get
            Return m_TotalPages
        End Get
    End Property
    Public Sub New(ByVal source As IQueryable(Of T), ByVal pageIndex As Nullable(Of Integer), ByVal Controller As String)
        m_MySource = source
        Me.Controller = Controller
        Me.PagingRoute = "ClientPaging"
        If Not pageIndex.HasValue Then pageIndex = 1
        If pageIndex <= 0 Then pageIndex = 1
        m_PageIndex = pageIndex
        m_TotalCount = source.Count()

        m_TotalPages = Math.Ceiling(TotalCount / PageSize)
        If pageIndex > TotalPages Then pageIndex = TotalPages
        If TotalCount > 0 Then
            AddRange(source.Skip((pageIndex - 1) * PageSize).Take(PageSize))
        End If
    End Sub
    Public Sub New(ByVal source As IQueryable(Of T), ByVal pageIndex As Nullable(Of Integer), ByVal pageSize As Integer, ByVal Controller As String)
        m_MySource = source
        Me.Controller = Controller
        Me.PagingRoute = "ClientPaging"
        If Not pageIndex.HasValue Then pageIndex = 1
        If pageIndex < 0 Then pageIndex = 1
        m_PageIndex = pageIndex
        m_PageSize = pageSize
        m_TotalCount = source.Count()

        m_TotalPages = Math.Ceiling(TotalCount / pageSize)
        If pageIndex > TotalPages Then pageIndex = TotalPages
        If TotalCount > 0 Then
            AddRange(source.Skip((pageIndex - 1) * pageSize).Take(pageSize))
        End If
    End Sub
    Public Sub New(ByVal source As IQueryable(Of T), ByVal pageIndex As Nullable(Of Integer), ByVal Controller As String, ByVal PagingRoute As String)
        m_MySource = source
        Me.Controller = Controller
        Me.PagingRoute = PagingRoute
        If Not pageIndex.HasValue Then pageIndex = 1
        If pageIndex <= 0 Then pageIndex = 1
        m_PageIndex = pageIndex
        m_TotalCount = source.Count()

        m_TotalPages = Math.Ceiling(TotalCount / PageSize)
        If pageIndex > TotalPages Then pageIndex = TotalPages
        If TotalCount > 0 Then
            AddRange(source.Skip((pageIndex - 1) * PageSize).Take(PageSize))
        End If
    End Sub
    Public Sub New(ByVal source As IQueryable(Of T), ByVal pageIndex As Nullable(Of Integer), ByVal pageSize As Integer, ByVal Controller As String, ByVal PagingRoute As String)
        m_MySource = source
        Me.Controller = Controller
        Me.PagingRoute = PagingRoute
        If Not pageIndex.HasValue Then pageIndex = 1
        If pageIndex < 0 Then pageIndex = 1
        m_PageIndex = pageIndex
        m_PageSize = pageSize
        m_TotalCount = source.Count()

        m_TotalPages = Math.Ceiling(TotalCount / pageSize)
        If pageIndex > TotalPages Then pageIndex = TotalPages
        If TotalCount > 0 Then
            AddRange(source.Skip((pageIndex - 1) * pageSize).Take(pageSize))
        End If
    End Sub

    Public Sub New(ByVal source As IQueryable(Of T), ByVal pageIndex As Nullable(Of Integer))
        m_MySource = source
        'source = source.ToList.AsQueryable
        If Not pageIndex.HasValue Then pageIndex = 1
        If pageIndex <= 0 Then pageIndex = 1
        m_PageIndex = pageIndex
        m_TotalCount = source.Count()

        m_TotalPages = Math.Ceiling(TotalCount / PageSize)
        If pageIndex > TotalPages Then pageIndex = TotalPages
        If TotalCount > 0 Then
            AddRange(source.Skip((pageIndex - 1) * PageSize).Take(PageSize))
        End If
    End Sub
    Public Sub New(ByVal source As IQueryable(Of T), ByVal pageIndex As Nullable(Of Integer), ByVal pageSize As Integer)
        m_MySource = source
        If Not pageIndex.HasValue Then pageIndex = 1
        If pageIndex < 0 Then pageIndex = 1
        m_PageIndex = pageIndex
        m_PageSize = pageSize
        m_TotalCount = source.Count()

        m_TotalPages = Math.Ceiling(TotalCount / pageSize)
        If pageIndex > TotalPages Then pageIndex = TotalPages
        If TotalCount > 0 Then
            AddRange(source.Skip((pageIndex - 1) * pageSize).Take(pageSize))
        End If
    End Sub
    Public ReadOnly Property HasPreviousPage()
        Get
            Return (PageIndex > 1)
        End Get
    End Property
    Public ReadOnly Property HasNextPage()
        Get
            Return (PageIndex + 1 <= TotalPages)
        End Get
    End Property

End Class
