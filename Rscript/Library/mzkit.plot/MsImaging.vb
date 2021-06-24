﻿#Region "Microsoft.VisualBasic::8c035a4594c14f6a830bcaa00e41768c, Rscript\Library\mzkit.plot\MsImaging.vb"

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

' Module MsImaging
' 
'     Function: flatten, layer, LoadPixels, viewer
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.MarkupData.imzML
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports BioNovoGene.Analytical.MassSpectrometry.MsImaging
Imports BioNovoGene.Analytical.MassSpectrometry.MsImaging.IndexedCache
Imports BioNovoGene.Analytical.MassSpectrometry.MsImaging.Pixel
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' Visual MS imaging data(*.imzML)
''' </summary>
<Package("MsImaging")>
Module MsImaging

    <ExportAPI("write.MSI_XIC")>
    <RApiReturn(GetType(XICWriter))>
    Public Function WriteXICCache(<RRawVectorArgument> pixels As Object, ibd As ibdReader, Optional env As Environment = Nothing) As Object
        Dim pixelData As pipeline = pipeline.TryCreatePipeline(Of ScanData)(pixels, env)

        If pixelData.isError Then
            Return pixelData.getError
        End If

        Dim allPixels As ScanData() = pixelData.populates(Of ScanData)(env).ToArray
        Dim width As Integer = Aggregate p In allPixels Into Max(p.x)
        Dim height As Integer = Aggregate p In allPixels Into Max(p.y)
        Dim cache As New XICWriter(width, height, sourceName:=ibd.fileName Or "n/a".AsDefault)

        For Each pixel As ScanData In allPixels
            Call cache.WritePixels(New ibdPixel(ibd, pixel))
        Next

        Call cache.Flush()

        Return cache
    End Function

    <ExportAPI("write.MSI")>
    Public Function writeIndexCacheFile(cache As XICWriter, file As Object, Optional env As Environment = Nothing) As Object
        If file Is Nothing Then
            Return Internal.debug.stop("the required target file can not be nothing!", env)
        End If

        Dim stream As Stream
        Dim autoClose As Boolean = False

        If TypeOf file Is String Then
            stream = DirectCast(file, String).Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
            autoClose = True
        ElseIf TypeOf file Is Stream Then
            stream = file
        Else
            Return Message.InCompatibleType(GetType(Stream), file.GetType, env)
        End If

        Call XICIndex.WriteIndexFile(cache, stream)
        Call stream.Flush()

        If autoClose Then
            Call stream.Close()
        End If

        Return True
    End Function

    ''' <summary>
    ''' load imzML data into the ms-imaging render
    ''' </summary>
    ''' <param name="imzML"></param>
    ''' <returns></returns>
    <ExportAPI("viewer")>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function viewer(imzML As String) As Drawer
        Return New Drawer(imzML)
    End Function

    ''' <summary>
    ''' load the raw pixels data from imzML file 
    ''' </summary>
    ''' <param name="mz">a collection of ion m/z value for rendering on one image</param>
    ''' <param name="tolerance">m/z tolerance error for get layer data</param>
    ''' <param name="skip_zero"></param>
    ''' <returns></returns>
    <ExportAPI("pixels")>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <RApiReturn(GetType(PixelData))>
    Public Function LoadPixels(imzML As Drawer, mz As Double(),
                               Optional tolerance As Object = "da:0.1",
                               Optional skip_zero As Boolean = True,
                               Optional env As Environment = Nothing) As pipeline

        Dim errors As [Variant](Of Tolerance, Message) = Math.getTolerance(tolerance, env)

        If errors Like GetType(Message) Then
            Return errors.TryCast(Of Message)
        End If

        Return imzML _
            .LoadPixels(mz, errors, skip_zero) _
            .DoCall(AddressOf pipeline.CreateFromPopulator)
    End Function

    ''' <summary>
    ''' render a ms-imaging layer by a specific ``m/z`` scan.
    ''' </summary>
    ''' <param name="viewer"></param>
    ''' <param name="mz"></param>
    ''' <param name="threshold"></param>
    ''' <param name="pixelSize"></param>
    ''' <param name="tolerance"></param>
    ''' <param name="color$"></param>
    ''' <param name="levels%"></param>
    ''' <returns></returns>
    <ExportAPI("layer")>
    <RApiReturn(GetType(Bitmap))>
    Public Function layer(viewer As Drawer, mz As Double(),
                          Optional threshold As Double = 0.05,
                          <RRawVectorArgument>
                          Optional pixelSize As Object = "5,5",
                          Optional tolerance As Object = "da:0.1",
                          Optional color$ = "YlGnBu:c8",
                          Optional levels% = 30,
                          Optional env As Environment = Nothing) As Object

        Dim errors As [Variant](Of Tolerance, Message) = Math.getTolerance(tolerance, env)

        If errors Like GetType(Message) Then
            Return errors.TryCast(Of Message)
        End If

        If mz.IsNullOrEmpty Then
            Return Nothing
        ElseIf mz.Length = 1 Then
            Return viewer.DrawLayer(
                mz:=mz(Scan0),
                threshold:=threshold,
                pixelSize:=InteropArgumentHelper.getSize(pixelSize, "5,5"),
                toleranceErr:=errors.TryCast(Of Tolerance).GetScript,
                colorSet:=color,
                mapLevels:=levels
            )
        Else
            Return viewer.DrawLayer(
                mz:=mz,
                threshold:=threshold,
                pixelSize:=InteropArgumentHelper.getSize(pixelSize, "5,5"),
                toleranceErr:=errors.TryCast(Of Tolerance).GetScript,
                colorSet:=color,
                mapLevels:=levels
            )
        End If
    End Function

    ''' <summary>
    ''' flatten image layers
    ''' </summary>
    ''' <param name="layers">
    ''' layer bitmaps should be all in equal size
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("flatten")>
    Public Function flatten(layers As Bitmap(), Optional bg$ = "white") As Bitmap
        Using g As Graphics2D = New Bitmap(layers(Scan0).Width, layers(Scan0).Height)
            If Not bg.StringEmpty Then
                Call g.Clear(bg.GetBrush)
            End If

            ' 在这里是反向叠加图层的
            ' 向量中最开始的图层表示为最上层的图层，即最后进行绘制的图层
            For Each layer As Bitmap In layers.Reverse
                Call g.DrawImageUnscaled(layer, New Point)
            Next

            Return g.ImageResource
        End Using
    End Function
End Module
