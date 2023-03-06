﻿#Region "Microsoft.VisualBasic::c4e0ccf253ae264cf3f9def982afe714, mzkit\Rscript\Library\mzkit\comprehensive\TissueMorphology.vb"

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

    '   Total Lines: 396
    '    Code Lines: 274
    ' Comment Lines: 78
    '   Blank Lines: 44
    '     File Size: 14.51 KB


    ' Module TissueMorphology
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: createCDF, createTissueData, createTissueTable, createUMAPsample, createUMAPTable
    '               gridding, loadSpatialMapping, loadTissue, loadUMAP, SplitMapping
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports BioNovoGene.Analytical.MassSpectrometry.MsImaging.TissueMorphology
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.DataMining.DensityQuery
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' spatial tissue region handler
''' 
''' tissue morphology data handler for the internal 
''' bionovogene MS-imaging analysis pipeline.
''' </summary>
<Package("TissueMorphology")>
Module TissueMorphology

    Sub New()
        Call Internal.Object.Converts.makeDataframe.addHandler(GetType(TissueRegion()), AddressOf createTissueTable)
        Call Internal.Object.Converts.makeDataframe.addHandler(GetType(UMAPPoint()), AddressOf createUMAPTable)
    End Sub

    Private Function createTissueTable(tissues As TissueRegion(), args As list, env As Environment) As dataframe
        Dim labels As String() = tissues _
            .Select(Function(i) i.label.Replicate(n:=i.nsize)) _
            .IteratesALL _
            .ToArray
        Dim colors As String() = tissues _
            .Select(Function(i) i.color.ToHtmlColor.Replicate(n:=i.nsize)) _
            .IteratesALL _
            .ToArray
        Dim x As New List(Of Integer)
        Dim y As New List(Of Integer)

        For Each region In tissues
            For Each p As Point In region.points
                x.Add(p.X)
                y.Add(p.Y)
            Next
        Next

        Return New dataframe With {
            .columns = New Dictionary(Of String, Array) From {
                {"label", labels},
                {"color", colors},
                {"x", x.ToArray},
                {"y", y.ToArray}
            }
        }
    End Function

    Private Function createUMAPTable(umap As UMAPPoint(), args As list, env As Environment) As dataframe
        Dim px As Integer() = umap.Select(Function(i) i.Pixel.X).ToArray
        Dim py As Integer() = umap.Select(Function(i) i.Pixel.Y).ToArray
        Dim x As Double() = umap.Select(Function(i) i.x).ToArray
        Dim y As Double() = umap.Select(Function(i) i.y).ToArray
        Dim z As Double() = umap.Select(Function(i) i.z).ToArray
        Dim cluster As Integer() = umap.Select(Function(i) i.class).ToArray

        Return New dataframe With {
            .columns = New Dictionary(Of String, Array) From {
                {"px", px},
                {"py", py},
                {"x", x},
                {"y", y},
                {"z", z},
                {"cluster", cluster}
            },
            .rownames = px _
                .Select(Function(xi, i) $"{xi},{py(i)}") _
                .ToArray
        }
    End Function

    ''' <summary>
    ''' create a collection of the umap sample data
    ''' </summary>
    ''' <param name="points"></param>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <param name="z"></param>
    ''' <param name="cluster"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("UMAPsample")>
    Public Function createUMAPsample(<RRawVectorArgument>
                                     points As Object,
                                     x As Double(),
                                     y As Double(),
                                     z As Double(),
                                     cluster As Integer(),
                                     Optional is_singlecells As Boolean = False,
                                     Optional env As Environment = Nothing) As UMAPPoint()

        Dim pixels As String() = CLRVector.asCharacter(points)
        Dim umap As UMAPPoint() = pixels _
            .Select(Function(pi, i)
                        Dim sample As UMAPPoint

                        If is_singlecells Then
                            sample = New UMAPPoint With {
                                .[class] = cluster(i),
                                .label = pi,
                                .x = x(i),
                                .y = y(i),
                                .z = z(i)
                            }
                        Else
                            Dim xy As Integer() = pi.Split(","c) _
                                .Select(AddressOf Integer.Parse) _
                                .ToArray

                            sample = New UMAPPoint With {
                                .[class] = cluster(i),
                                .Pixel = New Point(xy(0), xy(1)),
                                .label = pi,
                                .x = x(i),
                                .y = y(i),
                                .z = z(i)
                            }
                        End If

                        Return sample
                    End Function) _
            .ToArray

        Return umap
    End Function

    ''' <summary>
    ''' create a collection of the tissue region dataset
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <param name="labels"></param>
    ''' <param name="colorSet">
    ''' the color set schema name or a list of color data 
    ''' which can be mapping to the given <paramref name="labels"/> 
    ''' list.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("TissueData")>
    <RApiReturn(GetType(TissueRegion))>
    Public Function createTissueData(x As Integer(),
                                     y As Integer(),
                                     labels As String(),
                                     Optional colorSet As Object = "Paper",
                                     Optional env As Environment = Nothing) As Object

        Dim labelClass As String() = labels.Distinct.ToArray
        Dim colors As New Dictionary(Of String, Color)
        Dim regions As New Dictionary(Of String, List(Of Point))

        If TypeOf colorSet Is list Then
            Dim list As list = DirectCast(colorSet, list)

            For Each name As String In list.getNames
                Call colors.Add(name, RColorPalette.GetRawColor(list.getByName(name)))
            Next
        Else
            Dim colorList = Designer.GetColors(colorSet, labelClass.Length)
            Dim i As i32 = Scan0

            For Each label As String In labelClass
                Call colors.Add(label, colorList(++i))
            Next
        End If

        For Each label As String In labelClass
            Call regions.Add(label, New List(Of Point))
        Next

        For i As Integer = 0 To labels.Length - 1
            Call regions(labels(i)).Add(New Point(x(i), y(i)))
        Next

        Return regions _
            .Select(Function(r, i)
                        Return New TissueRegion With {
                            .color = colors(r.Key),
                            .label = r.Key,
                            .points = r.Value.ToArray
                        }
                    End Function) _
            .ToArray
    End Function

    ''' <summary>
    ''' export the tissue data as cdf file
    ''' </summary>
    ''' <param name="tissueMorphology"></param>
    ''' <param name="file"></param>
    ''' <param name="umap"></param>
    ''' <param name="dimension"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("writeCDF")>
    Public Function createCDF(tissueMorphology As TissueRegion(),
                              file As Object,
                              Optional umap As UMAPPoint() = Nothing,
                              Optional dimension As Size = Nothing,
                              Optional env As Environment = Nothing) As Object

        Dim saveBuf = SMRUCC.Rsharp.GetFileStream(file, IO.FileAccess.Write, env)

        If saveBuf Like GetType(Message) Then
            Return saveBuf.TryCast(Of Message)
        End If

        Using buffer As Stream = saveBuf.TryCast(Of Stream)
            Return tissueMorphology.WriteCDF(
                file:=buffer,
                umap:=umap,
                dimension:=dimension
            )
        End Using
    End Function

    ''' <summary>
    ''' load tissue region polygon data
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="id">
    ''' the region id, which could be used for load specific 
    ''' region polygon data. default nothing means load all
    ''' tissue region polygon data
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' a collection of tissue polygon region objects.
    ''' </returns>
    <ExportAPI("loadTissue")>
    <RApiReturn(GetType(TissueRegion))>
    Public Function loadTissue(<RRawVectorArgument>
                               file As Object,
                               Optional id As String = "*",
                               Optional env As Environment = Nothing) As Object

        Dim readBuf = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Read, env)

        If readBuf Like GetType(Message) Then
            Return readBuf.TryCast(Of Message)
        End If

        Using buffer As Stream = readBuf.TryCast(Of Stream)
            If id.StringEmpty OrElse id = "*" Then
                Return buffer _
                    .ReadTissueMorphology _
                    .ToArray
            Else
                Return buffer _
                    .ReadTissueMorphology _
                    .Where(Function(r) r.label = id) _
                    .ToArray
            End If
        End Using
    End Function

    ''' <summary>
    ''' load UMAP data
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("loadUMAP")>
    <RApiReturn(GetType(UMAPPoint))>
    Public Function loadUMAP(<RRawVectorArgument> file As Object, Optional env As Environment = Nothing) As Object
        Dim readBuf = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Read, env)

        If readBuf Like GetType(Message) Then
            Return readBuf.TryCast(Of Message)
        End If

        Using buffer As Stream = readBuf.TryCast(Of Stream)
            Return buffer.ReadUMAP
        End Using
    End Function

    ''' <summary>
    ''' read spatial mapping data of STdata mapping to SMdata
    ''' </summary>
    ''' <param name="file">
    ''' the file path of the spatial mapping xml dataset file 
    ''' </param>
    ''' <param name="remove_suffix">
    ''' removes of the numeric suffix of the STdata barcode?
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("read.spatialMapping")>
    <RApiReturn(GetType(SpatialMapping))>
    Public Function loadSpatialMapping(file As String, Optional remove_suffix As Boolean = False, Optional env As Environment = Nothing) As Object
        Dim mapping = file.LoadXml(Of SpatialMapping)(throwEx:=False)

        If mapping Is Nothing Then
            Return Internal.debug.stop({
                $"the required spatial mapping data which is loaded from the file location ({file}) is nothing, this could be some reasons:",
                $"file is exists on location: {file}",
                $"or invalid xml file format"
            }, env)
        ElseIf remove_suffix Then
            mapping = New SpatialMapping With {
                .label = mapping.label,
                .transform = mapping.transform,
                .spots = mapping.spots _
                    .Select(Function(f)
                                Return New SpotMap With {
                                    .barcode = f.barcode.StringReplace("[-]\d+", ""),
                                    .flag = f.flag,
                                    .physicalXY = f.physicalXY,
                                    .SMX = f.SMX,
                                    .SMY = f.SMY,
                                    .spotXY = f.spotXY,
                                    .STX = f.STX,
                                    .STY = f.STY
                                }
                            End Function) _
                    .ToArray
            }
        End If

        Return mapping
    End Function

    <ExportAPI("splitMapping")>
    <RApiReturn(GetType(list))>
    Public Function SplitMapping(mapping As SpatialMapping) As Object
        Dim list As New Dictionary(Of String, Object)
        Dim groups = mapping.spots _
            .GroupBy(Function(r) Strings.Trim(r.TissueMorphology)) _
            .ToArray

        For Each group In groups
            list(group.Key) = New SpatialMapping With {
                .color = mapping.color,
                .label = group.Key,
                .transform = mapping.transform,
                .spots = group.ToArray
            }
        Next

        Return New list With {
            .slots = list
        }
    End Function

    ''' <summary>
    ''' create a spatial grid for the spatial spot data
    ''' </summary>
    ''' <param name="mapping"></param>
    ''' <param name="gridSize"></param>
    ''' <param name="label">
    ''' the parameter value will overrides the internal
    ''' label of the mapping if this parameter string 
    ''' value is not an empty string.
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("gridding")>
    Public Function gridding(mapping As SpatialMapping,
                             Optional gridSize As Integer = 6,
                             Optional label As String = Nothing) As Object

        Dim spotGrid As Grid(Of SpotMap) = Grid(Of SpotMap).Create(mapping.spots)
        Dim blocks = spotGrid.WindowSize(gridSize, gridSize).Gridding.ToArray
        Dim grids As New list With {.slots = New Dictionary(Of String, Object)}
        Dim tag As String = mapping.label

        If Not label.StringEmpty Then
            tag = label
        End If
        If tag.StringEmpty Then
            tag = label
        End If
        If tag.StringEmpty Then
            tag = "block"
        End If

        For i As Integer = 0 To blocks.Length - 1
            If blocks(i).Length > 0 Then
                Call grids.add($"{tag}_{i + 1}", blocks(i))
            End If
        Next

        Return grids
    End Function

End Module
