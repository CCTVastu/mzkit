﻿#Region "Microsoft.VisualBasic::92325367fed627da200914861ad7c132, src\assembly\assembly\mzPack\Binary\BinaryStreamWriter.vb"

    ' Author:
    ' 
    '       xieguigang (gg.xie@bionovogene.com, BioNovoGene Co., LTD.)
    ' 
    ' Copyright (c) 2018 gg.xie@bionovogene.com, BioNovoGene Co., LTD.
    ' 
    ' 
    ' MIT License
    ' 
    ' 
    ' Permission is hereby granted, free of charge, to any person obtaining a copy
    ' of this software and associated documentation files (the "Software"), to deal
    ' in the Software without restriction, including without limitation the rights
    ' to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    ' copies of the Software, and to permit persons to whom the Software is
    ' furnished to do so, subject to the following conditions:
    ' 
    ' The above copyright notice and this permission notice shall be included in all
    ' copies or substantial portions of the Software.
    ' 
    ' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    ' IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    ' FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    ' AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    ' LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    ' OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    ' SOFTWARE.



    ' /********************************************************************************/

    ' Summaries:

    '     Class BinaryStreamWriter
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Sub: (+2 Overloads) Dispose, (+2 Overloads) Write, (+2 Overloads) WriteBuffer, writeIndex
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Text

Namespace mzData.mzWebCache

    Public Class BinaryStreamWriter : Implements IDisposable

        Protected file As BinaryDataWriter

        Dim disposedValue As Boolean
        Dim scanIndex As New Dictionary(Of String, Long)
        ''' <summary>
        ''' a cache list of the meta data for each scan ms1
        ''' </summary>
        Dim scanMetaData As New Dictionary(Of String, Dictionary(Of String, String))

        Dim mzmin As Double = Integer.MaxValue
        Dim mzmax As Double = Integer.MinValue
        Dim rtmin As Double = Integer.MaxValue
        Dim rtmax As Double = Integer.MinValue

        Public Const Magic As String = "BioNovoGene/mzWebStream"

        Sub New(file As String)
            Call Me.New(file.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False))
        End Sub

        Sub New(file As Stream)
            Me.file = New BinaryDataWriter(file, encoding:=Encodings.ASCII)
            Me.file.Write(Magic, BinaryStringFormat.NoPrefixOrTermination)
            Me.file.Write(New Double() {0, 0, 0, 0})
            Me.file.Write(0&)
            Me.file.ByteOrder = ByteOrder.LittleEndian
            Me.file.Flush()
        End Sub

        ''' <summary>
        ''' write MS scan and MS/MS product scans
        ''' </summary>
        ''' <param name="scan"></param>
        Public Sub Write(scan As ScanMS1)
            Dim start As Long = file.Position

            If rtmin > scan.rt Then
                rtmin = scan.rt
            End If
            If rtmax < scan.rt Then
                rtmax = scan.rt
            End If

            ' add index data
            Call scanIndex.Add(scan.scan_id, start&)

            ' write MS1 scan information
            ' this first zero int32 is a 
            ' placeholder for indicate the byte size
            ' of this ms1 data region
            Call file.Write(0)
            Call file.Write(scan.scan_id, BinaryStringFormat.ZeroTerminated)
            Call file.Write(scan.rt)
            Call file.Write(scan.BPC)
            Call file.Write(scan.TIC)
            Call file.Write(scan.mz.Length)
            Call file.Write(scan.mz)
            Call file.Write(scan.into)
            Call file.Flush()

            Dim size As Integer = file.Position - start
            Dim products As ScanMS2()

            If scan.products Is Nothing Then
                products = {}
            Else
                products = scan.products
            End If

            Call file.Write(products.Length)

            For Each product As ScanMS2 In products
                Call Write(product)
            Next

            Call file.Flush()

            Using file.TemporarySeek(start, SeekOrigin.Begin)
                ' write data size offset of ms1 
                Call file.Write(size)
            End Using

            Call file.Flush()
        End Sub

        Private Sub Write(scan As ScanMS2)
            If mzmin > scan.parentMz Then
                mzmin = scan.parentMz
            End If
            If mzmax < scan.parentMz Then
                mzmax = scan.parentMz
            End If

            Call WriteBuffer(scan, file)
        End Sub

        Public Shared Sub WriteBuffer(scan As ScanMS2, buffer As Stream)
            Call WriteBuffer(scan, New BinaryDataWriter(buffer))
        End Sub

        Private Shared Sub WriteBuffer(scan As ScanMS2, file As BinaryDataWriter)
            Call file.Write(scan.scan_id, BinaryStringFormat.ZeroTerminated)
            Call file.Write(scan.parentMz)
            Call file.Write(scan.rt)
            Call file.Write(scan.intensity)
            Call file.Write(scan.polarity)
            Call file.Write(scan.charge)
            Call file.Write(scan.activationMethod)
            Call file.Write(scan.collisionEnergy)
            Call file.Write(CByte(If(scan.centroided, 1, 0)))
            Call file.Write(scan.mz.Length)
            Call file.Write(scan.mz)
            Call file.Write(scan.into)
        End Sub

        Protected Overridable Sub writeIndex()
            Dim indexPos As Long

            file.Flush()

            indexPos = file.Position

            file.Seek(Magic.Length, SeekOrigin.Begin)
            file.Write({mzmin, mzmax, rtmin, rtmax})
            file.Write(indexPos)
            file.Seek(indexPos, SeekOrigin.Begin)
            file.Write(scanIndex.Count)

            For Each entry In scanIndex
                Call file.Write(entry.Value)
                Call file.Write(entry.Key, BinaryStringFormat.ZeroTerminated)
            Next
        End Sub

        Protected Sub writeMetaData()

        End Sub

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    Call writeIndex()

                    ' TODO: 释放托管状态(托管对象)
                    Call file.Flush()
                    Call file.Dispose()
                End If

                ' TODO: 释放未托管的资源(未托管的对象)并替代终结器
                ' TODO: 将大型字段设置为 null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
        ' Protected Overrides Sub Finalize()
        '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace
