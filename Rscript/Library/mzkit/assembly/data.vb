﻿#Region "Microsoft.VisualBasic::07c5d180ad0b2b4189c9b87a32871c34, mzkit\Rscript\Library\mzkit\assembly\data.vb"

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


' Code Statistics:

'   Total Lines: 366
'    Code Lines: 251
' Comment Lines: 54
'   Blank Lines: 61
'     File Size: 13.91 KB


' Module data
' 
'     Function: createPeakMs2, getIntensity, getScantime, libraryMatrix, LibraryTable
'               makeROInames, nfragments, readMatrix, RtSlice, XIC
'               XICGroups, XICTable
' 
'     Sub: Main
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly
Imports BioNovoGene.Analytical.MassSpectrometry.Math
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Chromatogram
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' m/z data operator module
''' </summary>
<Package("data")>
Module data

    <RInitialize>
    Sub Main()
        Call Internal.Object.Converts.makeDataframe.addHandler(GetType(ms1_scan()), AddressOf XICTable)
        Call Internal.Object.Converts.makeDataframe.addHandler(GetType(PeakMs2()), AddressOf getIonsSummaryTable)
        Call Internal.Object.Converts.makeDataframe.addHandler(GetType(LibraryMatrix), AddressOf LibraryTable)
        Call Internal.Object.Converts.makeDataframe.addHandler(GetType(ChromatogramTick()), AddressOf TICTable)
    End Sub

    Private Function TICTable(TIC As ChromatogramTick(), args As list, env As Environment) As dataframe
        Dim table As New dataframe With {.columns = New Dictionary(Of String, Array)}

        table.columns("time") = TIC.Select(Function(t) t.Time).ToArray
        table.columns("intensity") = TIC.Select(Function(t) t.Intensity).ToArray

        Return table
    End Function

    Private Function LibraryTable(matrix As LibraryMatrix, args As list, env As Environment) As dataframe
        Dim table As New dataframe With {.columns = New Dictionary(Of String, Array)}

        table.columns("mz") = matrix.Select(Function(m) m.mz).ToArray
        table.columns("intensity") = matrix.Select(Function(m) m.intensity).ToArray
        table.columns("info") = matrix.Select(Function(m) m.Annotation).ToArray

        Return table
    End Function

    Private Function XICTable(XIC As ms1_scan(), args As list, env As Environment) As dataframe
        Dim table As New dataframe With {.columns = New Dictionary(Of String, Array)}

        table.columns("mz") = XIC.Select(Function(a) a.mz).ToArray
        table.columns("scan_time") = XIC.Select(Function(a) a.scan_time).ToArray
        table.columns("intensity") = XIC.Select(Function(a) a.intensity).ToArray

        Return table
    End Function

    Private Function getIonsSummaryTable(peaks As PeakMs2(), args As list, env As Environment) As dataframe
        Dim df As New dataframe With {
            .columns = New Dictionary(Of String, Array)
        }

        Call df.add(NameOf(PeakMs2.lib_guid), From i In peaks Select i.lib_guid)
        Call df.add(NameOf(PeakMs2.scan), From i In peaks Select i.scan)
        Call df.add(NameOf(PeakMs2.file), From i In peaks Select i.file)
        Call df.add(NameOf(PeakMs2.mz), From i In peaks Select i.mz)
        Call df.add(NameOf(PeakMs2.rt), From i In peaks Select i.rt)
        Call df.add(NameOf(PeakMs2.precursor_type), From i In peaks Select i.precursor_type)
        Call df.add(NameOf(PeakMs2.collisionEnergy), From i In peaks Select i.collisionEnergy)
        Call df.add(NameOf(PeakMs2.activation), From i In peaks Select i.activation)
        Call df.add(NameOf(PeakMs2.fragments), From i In peaks Select i.fragments)
        Call df.add(NameOf(PeakMs2.intensity), From i In peaks Select i.intensity)
        Call df.add(NameOf(PeakMs2.Ms2Intensity), From i In peaks Select i.Ms2Intensity)

        Dim ionsMs2 = peaks _
            .Select(Function(i)
                        Return i.mzInto _
                            .OrderByDescending(Function(m) m.intensity) _
                            .ToArray
                    End Function) _
            .ToArray

        Call df.add("basePeak", From i In ionsMs2 Select toString(i(Scan0)))
        Call df.add("top2", From i In ionsMs2 Select toString(i.ElementAtOrNull(1)))
        Call df.add("top3", From i In ionsMs2 Select toString(i.ElementAtOrNull(2)))
        Call df.add("top4", From i In ionsMs2 Select toString(i.ElementAtOrNull(3)))
        Call df.add("top5", From i In ionsMs2 Select toString(i.ElementAtOrNull(4)))

        Return df
    End Function

    Private Function toString(i As ms2) As String
        If i Is Nothing Then
            Return ""
        Else
            Return $"{i.mz.ToString("F4")}:{i.intensity.ToString("G3")}"
        End If
    End Function

    <ExportAPI("unionPeaks")>
    <RApiReturn(GetType(PeakMs2), GetType(LibraryMatrix))>
    Public Function unionPeaks(peaks As PeakMs2(),
                               Optional matrix As Boolean = False,
                               Optional massDiff As Double = 0.1) As Object

        Dim fragments As ms2() = peaks _
            .Select(Function(i) i.mzInto) _
            .IteratesALL _
            .GroupBy(Function(i) i.mz, offsets:=massDiff) _
            .Select(Function(i)
                        Dim mz As Double = i.OrderByDescending(Function(x) x.intensity).First.mz
                        Dim into = i.Max(Function(x) x.intensity)

                        Return New ms2 With {
                            .mz = mz,
                            .intensity = into
                        }
                    End Function) _
            .ToArray

        If matrix Then
            Return New LibraryMatrix With {
                .ms2 = fragments,
                .centroid = True,
                .parentMz = peaks _
                    .OrderByDescending(Function(i) i.intensity) _
                    .First _
                    .mz
            }
        Else
            Return New PeakMs2 With {
                .file = peaks.Select(Function(i) i.file).Distinct.JoinBy("; "),
                .intensity = peaks.Sum(Function(i) i.intensity),
                .mzInto = fragments,
                .rt = peaks.Average(Function(i) i.rt)
            }
        End If
    End Function

    ''' <summary>
    ''' get the size of the target ms peaks
    ''' </summary>
    ''' <param name="matrix"></param>
    ''' <returns></returns>
    <ExportAPI("nsize")>
    <RApiReturn(GetType(Integer))>
    Public Function nfragments(matrix As Object, Optional env As Environment = Nothing) As Object
        If matrix Is Nothing Then
            Return 0
        ElseIf TypeOf matrix Is LibraryMatrix Then
            Return DirectCast(matrix, LibraryMatrix).Length
        ElseIf TypeOf matrix Is PeakMs2 Then
            Return DirectCast(matrix, PeakMs2).fragments
        Else
            Return Message.InCompatibleType(GetType(LibraryMatrix), matrix.GetType, env)
        End If
    End Function

    ''' <summary>
    ''' create a new ms2 peaks data object
    ''' </summary>
    ''' <param name="precursor"></param>
    ''' <param name="rt"></param>
    ''' <param name="mz"></param>
    ''' <param name="into"></param>
    ''' <param name="totalIons"></param>
    ''' <param name="file"></param>
    ''' <param name="meta"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("peakMs2")>
    Public Function createPeakMs2(precursor As Double, rt As Double, mz As Double(), into As Double(),
                                  Optional totalIons As Double = 0,
                                  Optional file As String = Nothing,
                                  <RListObjectArgument>
                                  Optional meta As list = Nothing,
                                  Optional env As Environment = Nothing) As PeakMs2

        Return New PeakMs2 With {
            .mz = precursor,
            .intensity = totalIons,
            .mzInto = mz _
                .Select(Function(mzi, i)
                            Return New ms2 With {
                                .mz = mzi,
                                .intensity = into(i)
                            }
                        End Function) _
                .ToArray,
            .rt = rt,
            .file = file,
            .meta = meta.AsGeneric(Of String)(env)
        }
    End Function

    ''' <summary>
    ''' Create a library matrix object
    ''' </summary>
    ''' <param name="matrix">
    ''' for a dataframe object, should contains column data:
    ''' mz, into and annotation.
    ''' </param>
    ''' <param name="title"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("libraryMatrix")>
    Public Function libraryMatrix(<RRawVectorArgument> matrix As Object,
                                  Optional title$ = "MS Matrix",
                                  Optional parentMz As Double = -1,
                                  Optional centroid As Boolean = False,
                                  Optional env As Environment = Nothing) As Object
        Dim MS As ms2()

        If TypeOf matrix Is dataframe Then
            Dim ms2 As dataframe = DirectCast(matrix, dataframe)
            Dim mz As Double() = ms2.getVector(Of Double)("mz", "m/z")
            Dim into As Double() = ms2.getVector(Of Double)("into", "intensity")
            Dim annotation As String() = ms2.getVector(Of String)("annotation")

            If annotation Is Nothing Then
                annotation = {}
            End If

            MS = mz _
                .Select(Function(mzi, i)
                            Return New ms2 With {
                                .mz = mzi,
                                .intensity = into(i),
                                .Annotation = annotation.ElementAtOrDefault(i)
                            }
                        End Function) _
                .ToArray
        Else
            Dim data As pipeline = pipeline.TryCreatePipeline(Of ms2)(matrix, env)

            If data.isError Then
                Return data.getError
            End If

            MS = data.populates(Of ms2)(env).ToArray
        End If

        Return New LibraryMatrix With {
            .name = title,
            .ms2 = MS,
            .parentMz = parentMz,
            .centroid = centroid
        }
    End Function

    ''' <summary>
    ''' grouping of the ms1 scan points by m/z data
    ''' </summary>
    ''' <param name="ms1"></param>
    ''' <param name="tolerance">
    ''' the m/z diff tolerance value for grouping ms1 scan point 
    ''' based on its ``m/z`` value
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("XIC_groups")>
    Public Function XICGroups(<RRawVectorArgument>
                              ms1 As Object,
                              Optional tolerance As Object = "ppm:20",
                              Optional env As Environment = Nothing) As Object

        Dim mzErr = Math.getTolerance(tolerance, env)
        Dim mzdiff As Tolerance

        If mzErr Like GetType(Message) Then
            Return mzErr.TryCast(Of Message)
        Else
            mzdiff = mzErr.TryCast(Of Tolerance)
        End If

        If TypeOf ms1 Is mzPack Then
            Return DirectCast(ms1, mzPack).getRawXICSet(mzdiff)
        ElseIf TypeOf ms1 Is mzPack() Then
            Dim list As mzPack() = DirectCast(ms1, mzPack())

            If list.Length = 1 Then
                Return list(Scan0).getRawXICSet(mzdiff)
            Else
                Dim output As New list With {
                    .slots = New Dictionary(Of String, Object)
                }

                For i As Integer = 0 To list.Length - 1
                    Call output.add(list(i).source Or $"#{i + 1}".AsDefault, list(i).getRawXICSet(mzdiff))
                Next

                Return output
            End If
        Else
            Return pipeline _
                .TryCreatePipeline(Of IMs1)(ms1, env, suppress:=True) _
                .populates(Of IMs1)(env) _
                .getXICPoints(mzdiff)
        End If
    End Function

    <Extension>
    Private Function getXICPoints(Of T As IMs1)(ms1_scans As IEnumerable(Of T), mzdiff As Tolerance) As list
        Dim mzgroups = ms1_scans.GroupBy(Function(x) x.mz, mzdiff).ToArray
        Dim xic As New list With {.slots = New Dictionary(Of String, Object)}

        For Each mzi As NamedCollection(Of T) In mzgroups
            xic.add(Val(mzi.name).ToString("F4"), mzi.OrderBy(Function(ti) ti.rt).ToArray)
        Next

        Return xic
    End Function

    <Extension>
    Private Function getRawXICSet(raw As mzPack, tolerance As Tolerance) As list
        Dim allPoints = raw.GetAllScanMs1().ToArray
        Dim pack = allPoints.getXICPoints(tolerance)

        Return pack
    End Function

    ''' <summary>
    ''' get chromatogram data for a specific metabolite with given m/z from the ms1 scans data.
    ''' </summary>
    ''' <param name="ms1">a sequence data of ms1 scans</param>
    ''' <param name="mz">target mz value</param>
    ''' <param name="tolerance">
    ''' tolerance value in unit ``ppm`` or ``da`` for 
    ''' extract ``m/z`` data from the given ms1 ion 
    ''' scans.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("XIC")>
    <RApiReturn(GetType(ms1_scan), GetType(ChromatogramTick))>
    Public Function XIC(<RRawVectorArgument> ms1 As Object, mz#,
                        Optional tolerance As Object = "ppm:20",
                        Optional env As Environment = Nothing) As Object

        Dim ms1_scans As pipeline = pipeline.TryCreatePipeline(Of IMs1)(ms1, env, suppress:=True)
        Dim mzErr = Math.getTolerance(tolerance, env)

        If mzErr Like GetType(Message) Then
            Return mzErr.TryCast(Of Message)
        End If

        Dim mzdiff As Tolerance = mzErr.TryCast(Of Tolerance)

        If ms1_scans.isError Then
            If TypeOf ms1 Is mzPack Then
                Return DirectCast(ms1, mzPack).rawXIC(mz, mzdiff)
            ElseIf TypeOf ms1 Is mzPack() Then
                Dim all = DirectCast(ms1, mzPack())

                If all.Length = 1 Then
                    Return all(Scan0).rawXIC(mz, mzdiff)
                Else
                    Dim list As New list With {.slots = New Dictionary(Of String, Object)}

                    For i As Integer = 0 To all.Length - 1
                        Call list.add($"#{i + 1}", all(i).rawXIC(mz, mzdiff))
                    Next

                    Return list
                End If
            Else
                Return ms1_scans.getError
            End If
        Else
        End If

        Dim xicFilter As Array = ms1_scans _
            .populates(Of IMs1)(env) _
            .Where(Function(pt) mzdiff(pt.mz, mz)) _
            .OrderBy(Function(a) a.rt) _
            .Select(Function(a) CObj(a)) _
            .ToArray

        Return REnv.TryCastGenericArray(xicFilter, env)
    End Function

    <Extension>
    Private Function rawXIC(ms1 As mzPack, mz As Double, mzdiff As Tolerance) As ChromatogramTick()
        Return ms1.MS _
            .Select(Function(scan)
                        Dim i As Double = scan.GetIntensity(mz, mzdiff)
                        Dim tick As New ChromatogramTick With {
                            .Intensity = i,
                            .Time = scan.rt
                        }

                        Return tick
                    End Function) _
            .ToArray
    End Function

    ''' <summary>
    ''' slice a region of ms1 scan data by a given rt window.
    ''' </summary>
    ''' <param name="ms1">a sequence of ms1 scan data.</param>
    ''' <param name="rtmin"></param>
    ''' <param name="rtmax"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("rt_slice")>
    <RApiReturn(GetType(ms1_scan))>
    Public Function RtSlice(<RRawVectorArgument>
                            ms1 As Object,
                            rtmin#, rtmax#,
                            Optional env As Environment = Nothing) As Object

        Dim ms1_scans As pipeline = pipeline.TryCreatePipeline(Of IMs1)(ms1, env)

        If ms1_scans.isError Then
            Return ms1_scans.getError
        End If

        Dim xicFilter As Array = ms1_scans _
            .populates(Of IMs1)(env) _
            .Where(Function(pt) pt.rt >= rtmin AndAlso pt.rt <= rtmax) _
            .OrderBy(Function(a) a.rt) _
            .Select(Function(a) CObj(a)) _
            .ToArray

        Return REnv.TryCastGenericArray(xicFilter, env)
    End Function

    ''' <summary>
    ''' get intensity value from the ion scan points
    ''' </summary>
    ''' <param name="ticks"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("intensity")>
    <RApiReturn(GetType(Double))>
    Public Function getIntensity(<RRawVectorArgument>
                                 ticks As Object,
                                 Optional mz As Double = -1,
                                 Optional mzdiff As Object = "da:0.3",
                                 Optional env As Environment = Nothing) As Object

        Dim scans As pipeline = pipeline.TryCreatePipeline(Of ms1_scan)(ticks, env, suppress:=True)
        Dim tolerance = Math.getTolerance(mzdiff, env)

        If scans.isError Then
            scans = pipeline.TryCreatePipeline(Of PeakMs2)(ticks, env)

            If scans.isError Then
                Return scans.getError
            End If

            If mz > 0 AndAlso tolerance Like GetType(Message) Then
                Return tolerance.TryCast(Of Message)
            End If

            Dim mzErr As Tolerance = tolerance.TryCast(Of Tolerance)

            Return scans _
                .populates(Of PeakMs2)(env) _
                .Select(Function(x)
                            If mz > 0 Then
                                Return x.GetIntensity(mz, mzErr)
                            Else
                                Return x.intensity
                            End If
                        End Function) _
                .DoCall(AddressOf vector.asVector)
        End If

        Return scans _
            .populates(Of ms1_scan)(env) _
            .Select(Function(x) x.intensity) _
            .DoCall(AddressOf vector.asVector)
    End Function

    ''' <summary>
    ''' get scan time value from the ion scan points
    ''' </summary>
    ''' <param name="ticks"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("scan_time")>
    <RApiReturn(GetType(Double))>
    Public Function getScantime(<RRawVectorArgument>
                                ticks As Object,
                                Optional env As Environment = Nothing) As Object

        Dim scans As pipeline = pipeline.TryCreatePipeline(Of ms1_scan)(ticks, env)

        If scans.isError Then
            scans = pipeline.TryCreatePipeline(Of PeakMs2)(ticks, env)

            If scans.isError Then
                Return scans.getError
            Else
                Return scans.populates(Of PeakMs2)(env) _
                    .Select(Function(x) x.rt) _
                    .DoCall(AddressOf vector.asVector)
            End If
        End If

        Return scans.populates(Of ms1_scan)(env) _
            .Select(Function(x) x.scan_time) _
            .DoCall(AddressOf vector.asVector)
    End Function

    ''' <summary>
    ''' makes xcms_id format liked ROI unique id
    ''' </summary>
    ''' <param name="ROIlist"></param>
    ''' <param name="name_chrs">
    ''' just returns the ROI names character?
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("make.ROI_names")>
    <RApiReturn(GetType(PeakMs2))>
    Public Function makeROInames(<RRawVectorArgument> ROIlist As Object,
                                 Optional name_chrs As Boolean = False,
                                 Optional env As Environment = Nothing) As Object

        If TypeOf ROIlist Is list AndAlso {"mz", "rt"}.All(AddressOf DirectCast(ROIlist, list).hasName) Then
            Dim mz As Double() = DirectCast(ROIlist, list).getValue(Of Double())("mz", env)
            Dim rt As Double() = DirectCast(ROIlist, list).getValue(Of Double())("rt", env)

            Return xcms_id(mz, rt)
        End If

        Dim dataList As pipeline = pipeline.TryCreatePipeline(Of PeakMs2)(ROIlist, env)

        If dataList.isError Then
            Return dataList.getError
        End If

        Dim allData As PeakMs2() = dataList.populates(Of PeakMs2)(env).ToArray
        Dim uniques As String() = xcms_id(
            mz:=allData.Select(Function(p) p.mz).ToArray,
            rt:=allData.Select(Function(p) p.rt).ToArray
        )

        If name_chrs Then
            Return uniques
        Else
            For i As Integer = 0 To allData.Length - 1
                allData(i).lib_guid = uniques(i)
            Next

            Return allData
        End If
    End Function

    <ExportAPI("read.MsMatrix")>
    Public Function readMatrix(file As String) As Spectra.Library()
        Return file.LoadCsv(Of Spectra.Library)
    End Function
End Module
