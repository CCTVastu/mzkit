﻿#Region "Microsoft.VisualBasic::c158aa21bf159e8da7f664dcabd07b9c, Rscript\Library\mzkit\assembly\MzWeb.vb"

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

    ' Module MzWeb
    ' 
    '     Function: GetChromatogram, loadStream, Ms1ScanPoints, Ms2ScanPeaks, Open
    '               setMzpackThumbnail, ToMzPack, writeMzpack, writeStream
    ' 
    '     Sub: WriteCache
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.Comprehensive
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.DataReader
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.MarkupData
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.MarkupData.mzXML
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.mzData
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.mzData.mzWebCache
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.ThermoRawFileReader
Imports BioNovoGene.Analytical.MassSpectrometry.Math
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.IO.netCDF
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' biodeep mzweb data viewer raw data file helper
''' </summary>
<Package("mzweb")>
Module MzWeb

    ''' <summary>
    ''' load chromatogram data from the raw file data
    ''' </summary>
    ''' <param name="scans">
    ''' the scan data object that reads from the mzXML/mzML/mzPack raw data file
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("load.chromatogram")>
    <RApiReturn(GetType(Chromatogram))>
    Public Function GetChromatogram(scans As Object, Optional env As Environment = Nothing) As Object
        If TypeOf scans Is mzPack Then
            Return DirectCast(scans, mzPack).GetChromatogram
        ElseIf Not TypeOf scans Is pipeline Then
            Return Message.InCompatibleType(GetType(mzPack), scans.GetType, env)
        Else
            Dim scanPip As pipeline = DirectCast(scans, pipeline)

            If scanPip.elementType Like GetType(mzXML.scan) Then
                Return Chromatogram.GetChromatogram(scanPip.populates(Of scan)(env))
            ElseIf scanPip.elementType Like GetType(mzML.spectrum) Then
                Return Chromatogram.GetChromatogram(scanPip.populates(Of mzML.spectrum)(env))
            Else
                Return Message.InCompatibleType(GetType(mzXML.scan), scanPip.elementType, env)
            End If
        End If
    End Function

    ''' <summary>
    ''' load the unify mzweb scan stream data from the mzml/mzxml raw scan data stream.
    ''' </summary>
    ''' <param name="scans"></param>
    ''' <param name="mzErr$"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("load.stream")>
    <RApiReturn(GetType(ScanMS1))>
    Public Function loadStream(scans As pipeline,
                               Optional mzErr$ = "da:0.1",
                               Optional env As Environment = Nothing) As pipeline

        If scans.elementType Like GetType(mzXML.scan) Then
            Return mzWebCache _
                .Load(scans.populates(Of scan)(env), mzErr) _
                .DoCall(AddressOf pipeline.CreateFromPopulator)
        ElseIf scans.elementType Like GetType(mzML.spectrum) Then
            Return mzWebCache _
                .Load(scans.populates(Of mzML.spectrum)(env), mzErr) _
                .DoCall(AddressOf pipeline.CreateFromPopulator)
        Else
            Return Message.InCompatibleType(GetType(mzXML.scan), scans.elementType, env)
        End If
    End Function

    ''' <summary>
    ''' write ASCII text format of mzweb stream
    ''' </summary>
    ''' <param name="scans"></param>
    ''' <param name="file"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("write.cache")>
    Public Function writeStream(scans As pipeline,
                                Optional file As Object = Nothing,
                                Optional env As Environment = Nothing) As Object
        Dim stream As Stream

        If file Is Nothing Then
            stream = Console.OpenStandardOutput
        ElseIf TypeOf file Is String Then
            stream = DirectCast(file, String).Open(doClear:=True)
        ElseIf TypeOf file Is Stream Then
            stream = DirectCast(file, Stream)
        Else
            Return Message.InCompatibleType(GetType(Stream), file.GetType, env)
        End If

        Call scans.populates(Of ScanMS1)(env).Write(stream)

        Return True
    End Function

    ''' <summary>
    ''' write binary format of mzweb stream data
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="cache"></param>
    <ExportAPI("packBin")>
    Public Sub WriteCache(file As String, cache As String)
        Using stream As New BinaryStreamWriter(file:=cache)
            If file.ExtensionSuffix("mzXML") Then
                For Each item In New mzXMLScans().Load(file)
                    Call stream.Write(item)
                Next
            Else
                For Each item In New mzMLScans().Load(file)
                    Call stream.Write(item)
                Next
            End If
        End Using
    End Sub

    <ExportAPI("write.mzPack")>
    Public Function writeMzpack(mzpack As mzPack, file As Object, Optional env As Environment = Nothing) As Object
        Dim filestream As [Variant](Of Stream, Message) = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Write, env)

        If filestream Like GetType(Message) Then
            Return filestream.TryCast(Of Message)
        End If

        Return mzpack.Write(filestream.TryCast(Of Stream))
    End Function

    ''' <summary>
    ''' open a raw data files in common raw data format and then returns 
    ''' in a unify mzpack data format.
    ''' </summary>
    ''' <param name="file">the ``*.mzXML``/``*.mzML``/``*.mzPack``/``*.raw`` raw data file</param>
    ''' <returns></returns>
    <ExportAPI("open.mzpack")>
    Public Function Open(file As String) As mzPack
        If file.ExtensionSuffix("mzXML", "mzML", "imzML") Then
            Return Converter.LoadRawFileAuto(xml:=file)
        ElseIf file.ExtensionSuffix("raw") Then
            Using msRaw As New MSFileReader(file)
                Return msRaw.LoadFromXRaw
            End Using
        Else
            Using stream As Stream = file.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
                Return mzPack.ReadAll(file:=stream)
            End Using
        End If
    End Function

    <ExportAPI("setThumbnail")>
    Public Function setMzpackThumbnail(mzpack As mzPack, thumb As Object) As mzPack
        If TypeOf thumb Is GraphicsData Then
            thumb = DirectCast(thumb, GraphicsData).AsGDIImage
        End If

        mzpack.Thumbnail = DirectCast(thumb, Image)
        Return mzpack
    End Function

    <ExportAPI("ms1_scans")>
    Public Function Ms1ScanPoints(mzpack As mzPack) As ms1_scan()
        Return mzpack.GetAllScanMs1.ToArray
    End Function

    <ExportAPI("ms2_peaks")>
    Public Function Ms2ScanPeaks(mzpack As mzPack) As PeakMs2()
        Return mzpack.GetMs2Peaks.ToArray
    End Function

    ''' <summary>
    ''' convert assembly file to mzpack format data object
    ''' </summary>
    ''' <param name="assembly"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("as.mzpack")>
    <RApiReturn(GetType(mzPack))>
    Public Function ToMzPack(assembly As Object, Optional env As Environment = Nothing) As Object
        If TypeOf assembly Is netCDFReader Then
            Return GC2Dimensional.ToMzPack(agilentGC:=assembly)
        Else
            Return Message.InCompatibleType(GetType(netCDFReader), assembly.GetType, env)
        End If
    End Function
End Module
