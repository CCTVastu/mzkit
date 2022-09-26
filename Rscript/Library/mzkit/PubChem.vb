﻿#Region "Microsoft.VisualBasic::86bef1e93dbd834281582480ee924277, mzkit\Rscript\Library\mzkit\PubChem.vb"

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

'   Total Lines: 155
'    Code Lines: 118
' Comment Lines: 13
'   Blank Lines: 24
'     File Size: 6.09 KB


' Module PubChemToolKit
' 
'     Constructor: (+1 Overloads) Sub New
'     Function: CID, ImageFlyGetImages, pubchemUrl, pugView, queryPubChem
'               ReadSIDMap, SIDMapTable
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Threading
Imports BioNovoGene.BioDeep.Chemistry.MetaLib.Models
Imports BioNovoGene.BioDeep.Chemistry.NCBI
Imports BioNovoGene.BioDeep.Chemistry.NCBI.MeSH
Imports BioNovoGene.BioDeep.Chemistry.NCBI.PubChem
Imports BioNovoGene.BioDeep.Chemistry.NCBI.PubChem.Graph
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.genomics.Analysis.HTS.GSEA
Imports SMRUCC.Rsharp
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("pubchem_kit")>
Module PubChemToolKit

    Sub New()
        Call Internal.Object.Converts.makeDataframe.addHandler(GetType(SIDMap()), AddressOf SIDMapTable)
    End Sub

    Private Function SIDMapTable(maps As SIDMap(), args As list, env As Environment) As Rdataframe
        Dim data As New Rdataframe With {
            .columns = New Dictionary(Of String, Array)
        }

        data.columns(NameOf(SIDMap.SID)) = maps.Select(Function(m) m.SID).ToArray
        data.columns(NameOf(SIDMap.sourceName)) = maps.Select(Function(m) m.sourceName).ToArray
        data.columns(NameOf(SIDMap.registryIdentifier)) = maps.Select(Function(m) m.registryIdentifier).ToArray
        data.columns(NameOf(SIDMap.CID)) = maps.Select(Function(m) m.CID).ToArray

        Return data
    End Function

    <ExportAPI("image_fly")>
    Public Function ImageFlyGetImages(<RRawVectorArgument>
                                      cid As Object,
                                      <RRawVectorArgument>
                                      Optional size As Object = "500,500",
                                      Optional ignoresInvalidCid As Boolean = False,
                                      Optional env As Environment = Nothing) As Object

        Dim ids As String() = REnv.asVector(Of String)(cid)
        Dim invalids = ids.Where(Function(id) Not id.IsPattern("\d+")).ToArray
        Dim images As New list
        Dim sizeVector As Double()

        If TypeOf size Is String OrElse TypeOf size Is String() Then
            With DirectCast(REnv.asVector(Of String)(size), String()).First.SizeParser
                sizeVector = { .Width, .Height}
            End With
        ElseIf TypeOf size Is Double() Then
            sizeVector = size
        Else
            Return Internal.debug.stop(Message.InCompatibleType(GetType(Double), size.GetType, env), env)
        End If

        Dim img As Image

        For Each id As String In ids
            img = ImageFly.GetImage(id, sizeVector(0), sizeVector(1), doBgTransparent:=False)

            Call Thread.Sleep(1000)
            Call images.slots.Add(id, img)
        Next

        Return images
    End Function

    ''' <summary>
    ''' query of the pathways, taxonomy and reaction 
    ''' data from the pubchem database.
    ''' </summary>
    ''' <param name="cid"></param>
    ''' <param name="cache"></param>
    ''' <param name="interval">
    ''' the sleep time interval in ms
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("query.external")>
    Public Function queryExternalMetadata(cid As String,
                                          Optional cache$ = "./pubchem/",
                                          Optional interval As Integer = -1) As list

        Dim query As New QueryPathways(cache, interval:=interval)
        Dim result As New list With {
            .slots = New Dictionary(Of String, Object)
        }

        Call result.add("pathways", query.QueryCacheText(New NamedValue(Of Types)(cid, Types.pathways), cacheType:=".json"))
        Call result.add("taxonomy", query.QueryCacheText(New NamedValue(Of Types)(cid, Types.taxonomy), cacheType:=".json"))
        Call result.add("reaction", query.QueryCacheText(New NamedValue(Of Types)(cid, Types.reaction), cacheType:=".json"))

        Return result
    End Function

    ''' <summary>
    ''' query cid from pubchem database
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="cache"></param>
    ''' <param name="offline"></param>
    ''' <param name="interval">
    ''' the time sleep interval in ms
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("CID")>
    Public Function CID(name As String,
                        Optional cache$ = "./.pubchem",
                        Optional offline As Boolean = False,
                        Optional interval As Integer = -1) As String()

        Return Query.QueryCID(
            name:=name,
            cacheFolder:=cache,
            offlineMode:=offline,
            hitCache:=Nothing,
            interval:=interval
        )
    End Function

    <ExportAPI("pubchem_url")>
    Public Function pubchemUrl(cid As String) As String
        Return WebQuery.pugViewApi(cid)
    End Function

    <ExportAPI("query.knowlegde_graph")>
    Public Function QueryKnowledgeGraph(cid As String, Optional cache As String = "./graph_kb") As list
        Dim geneSet = WebGraph.Query(cid, PubChem.Graph.Types.ChemicalGeneSymbolNeighbor, cache)
        Dim diseaseSet = WebGraph.Query(cid, PubChem.Graph.Types.ChemicalDiseaseNeighbor, cache)
        Dim metaboliteSet = WebGraph.Query(cid, PubChem.Graph.Types.ChemicalNeighbor, cache)

        Return New list With {
            .slots = New Dictionary(Of String, Object) From {
                {"genes", geneSet},
                {"disease", diseaseSet},
                {"compounds", metaboliteSet}
            }
        }
    End Function

    ''' <summary>
    ''' query of the pubchem database
    ''' </summary>
    ''' <param name="id"></param>
    ''' <param name="cache$"></param>
    ''' <returns></returns>
    <ExportAPI("query")>
    Public Function queryPubChem(<RRawVectorArgument> id As Object, Optional cache$ = "./", Optional env As Environment = Nothing) As list
        Dim ids As String() = REnv.asVector(Of String)(id)
        Dim cid As String()
        Dim query As New Dictionary(Of String, PugViewRecord)
        Dim result As New list With {
            .slots = New Dictionary(Of String, Object)
        }
        Dim meta As Dictionary(Of String, MetaLib)

        For Each term As String In ids.Distinct.ToArray
            query = PubChem.QueryPugViews(term, cacheFolder:=cache)
            cid = query.Keys.ToArray
            meta = query _
                .ToDictionary(Function(a) a.Key,
                              Function(a)
                                  If a.Value Is Nothing Then
                                      Return Nothing
                                  Else
                                      Return a.Value.GetMetaInfo
                                  End If
                              End Function)

            Call result.slots.Add(term, meta)
        Next

        Return result
    End Function

    <ExportAPI("pugView")>
    Public Function pugView(<RRawVectorArgument> cid As Object,
                            Optional cacheFolder$ = "./pubchem_cache",
                            Optional offline As Boolean = False,
                            Optional env As Environment = Nothing) As Object

        Dim api As WebQuery = $"{cacheFolder}/pugViews/".GetQueryHandler(Of WebQuery)(offline)
        Dim result = env.EvaluateFramework(Of String, PugViewRecord)(
            x:=cid,
            eval:=Function(id)
                      Return api.Query(Of PugViewRecord)(id)
                  End Function)

        Return result
    End Function

    <ExportAPI("SID_map")>
    Public Function ReadSIDMap(sidMapText As String, Optional skipNoCID As Boolean = True, Optional dbfilter$ = Nothing) As SIDMap()
        Dim ls As SIDMap() = SIDMap _
            .GetMaps(handle:=sidMapText, skipNoCID:=skipNoCID) _
            .ToArray

        If Not dbfilter.StringEmpty Then
            ls = ls _
                .Where(Function(map) map.sourceName = dbfilter) _
                .ToArray
        End If

        Return ls
    End Function

    ''' <summary>
    ''' read xml text and then parse as pugview record data object
    ''' </summary>
    ''' <param name="file">
    ''' the file path or the xml text content
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("read.pugView")>
    Public Function readPugViewXml(file As String) As PugViewRecord
        If file.FileExists Then
            Return file.LoadXml(Of PugViewRecord)
        Else
            Return file.LoadFromXml(Of PugViewRecord)
        End If
    End Function

    <ExportAPI("metadata.pugView")>
    Public Function GetMetaInfo(pugView As PugViewRecord) As MetaLib
        Return pugView.GetMetaInfo
    End Function

    <ExportAPI("read.mesh_tree")>
    Public Function ParseMeshTree(<RRawVectorArgument> file As Object, Optional env As Environment = Nothing) As Object
        Dim stream = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Read, env)

        If stream Like GetType(Message) Then
            Return stream.TryCast(Of Message)
        End If

        Return MeSH.ParseTree(New StreamReader(stream.TryCast(Of Stream)))
    End Function

    <ExportAPI("mesh_background")>
    Public Function MeshBackground(mesh As Tree(Of Term)) As Background
        Dim bg = mesh.ImportsTree(
            Function(term)
                Return New BackgroundGene With {
                    .accessionID = term.term,
                    .[alias] = {term.term},
                    .locus_tag = New NamedValue With {
                        .name = term.term,
                        .text = term.term
                    },
                    .name = term.term,
                    .term_id = {term.term}
                }
            End Function)

        bg = New Background With {
            .build = Now,
            .clusters = bg.clusters _
                .GroupBy(Function(c) c.ID) _
                .Select(Function(c)
                            If c.Count = 1 Then
                                Return c.First
                            Else
                                Return New Cluster With {
                                    .description = c.Select(Function(i) i.description).Distinct.JoinBy("; "),
                                    .ID = c.Key,
                                    .names = c.Select(Function(i) i.names).Distinct.JoinBy("; "),
                                    .members = c.Select(Function(i) i.members) _
                                        .IteratesALL _
                                        .GroupBy(Function(g) g.accessionID) _
                                        .Select(Function(a) a.First) _
                                        .ToArray
                                }
                            End If
                        End Function) _
                .ToArray
        }

        Return bg
    End Function
End Module
