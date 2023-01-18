﻿Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.DataStorage.netCDF
Imports Microsoft.VisualBasic.DataStorage.netCDF.Components
Imports Microsoft.VisualBasic.DataStorage.netCDF.Data
Imports Microsoft.VisualBasic.DataStorage.netCDF.DataVector
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports any = Microsoft.VisualBasic.Scripting
Imports CDFDimension = Microsoft.VisualBasic.DataStorage.netCDF.Components.Dimension

''' <summary>
''' the cdf file data handler
''' </summary>
Public Module CDF

    <Extension>
    Public Function IsTissueMorphologyCDF(cdf As netCDFReader) As Boolean
        Static attrNames As Index(Of String) = {"regions", "umap_sample"}
        Static varNames As String() = {
            "sampleX", "sampleY", "cluster", "umapX", "umapY", "umapZ"
        }

        Return cdf.globalAttributes.All(Function(a) a.name Like attrNames) AndAlso
            varNames.All(Function(name) cdf.dataVariableExists(name))
    End Function

    <Extension>
    Public Function GetDimension(tissueMorphology As TissueRegion()) As Size
        Dim allPixels As Point() = tissueMorphology _
            .Select(Function(t) t.points) _
            .IteratesALL _
            .ToArray

        If allPixels.IsNullOrEmpty Then
            Return Nothing
        End If

        Dim w = Aggregate p In allPixels Into Max(p.X)
        Dim h = Aggregate p In allPixels Into Max(p.Y)

        Return New Size(w, h)
    End Function

    <Extension>
    Public Function GetDimension(cdf As netCDFReader) As Size
        If cdf.attributeExists("scan_x") AndAlso cdf.attributeExists("scan_y") Then
            Dim scan_x As Integer = any.ToString(cdf("scan_x")).ParseInteger
            Dim scan_y As Integer = any.ToString(cdf("scan_y")).ParseInteger

            Return New Size(scan_x, scan_y)
        Else
            Return cdf _
                .ReadTissueMorphology _
                .ToArray _
                .GetDimension
        End If
    End Function

    <Extension>
    Public Function WriteCDF(tissueMorphology As TissueRegion(), file As Stream,
                             Optional dimension As Size = Nothing,
                             Optional umap As UMAPPoint() = Nothing) As Boolean

        Using cdf As New CDFWriter(file)
            Dim attrs As New List(Of attribute)
            Dim pixels As New List(Of Integer)
            Dim i As i32 = 1
            Dim vec As integers
            Dim dims As Dimension
            Dim uniqueId As String

            If umap Is Nothing Then
                umap = {}
            End If
            If dimension.IsEmpty Then
                dimension = tissueMorphology.GetDimension
            End If

            attrs.Add(New attribute With {.name = "scan_x", .type = CDFDataTypes.INT, .value = dimension.Width})
            attrs.Add(New attribute With {.name = "scan_y", .type = CDFDataTypes.INT, .value = dimension.Height})
            attrs.Add(New attribute With {.name = "regions", .type = CDFDataTypes.INT, .value = tissueMorphology.Length})
            attrs.Add(New attribute With {.name = "umap_sample", .type = CDFDataTypes.INT, .value = umap.Length})
            cdf.GlobalAttributes(attrs.PopAll)

            ' write region data
            For Each region As TissueRegion In tissueMorphology
                attrs.Add(New attribute With {.name = "label", .type = CDFDataTypes.CHAR, .value = region.label})
                attrs.Add(New attribute With {.name = "color", .type = CDFDataTypes.CHAR, .value = region.color.ToHtmlColor})
                attrs.Add(New attribute With {.name = "size", .type = CDFDataTypes.INT, .value = region.nsize})

                For Each p As Point In region.points
                    Call pixels.Add(p.X)
                    Call pixels.Add(p.Y)
                Next

                vec = New integers(pixels.PopAll)
                uniqueId = $"region_{++i}"
                dims = New Dimension With {.name = $"sizeof_{uniqueId}", .size = vec.Length}
                cdf.AddVariable(uniqueId, vec, dims, attrs.PopAll)
            Next

            ' write umap sample data
            Dim sampleX As Integer() = umap.Select(Function(p) p.Pixel.X).ToArray
            Dim sampleY As Integer() = umap.Select(Function(p) p.Pixel.Y).ToArray
            Dim umapX As Double() = umap.Select(Function(p) p.x).ToArray
            Dim umapY As Double() = umap.Select(Function(p) p.y).ToArray
            Dim umapZ As Double() = umap.Select(Function(p) p.z).ToArray
            Dim clusters As Integer() = umap.Select(Function(p) p.class).ToArray
            Dim umapsize As New Dimension With {.name = "umap_size", .size = umap.Length}
            Dim labels As String() = umap.Select(Function(p) Strings.Trim(p.label)).ToArray

            Call cdf.AddVariable("sampleX", New integers(sampleX), umapsize)
            Call cdf.AddVariable("sampleY", New integers(sampleY), umapsize)
            Call cdf.AddVariable("cluster", New integers(clusters), umapsize)

            Call cdf.AddVariable("umapX", New doubles(umapX), umapsize)
            Call cdf.AddVariable("umapY", New doubles(umapY), umapsize)
            Call cdf.AddVariable("umapZ", New doubles(umapZ), umapsize)

            ' cells labels is optional
            If Not labels.All(Function(s) s = "") Then
                Dim chrs As New chars(labels)
                Dim chrSize As Dimension = CDFDimension.FromVector(chrs)

                Call cdf.AddVariable("spot_labels", chrs, [dim]:=chrSize)
            End If
        End Using

        Return True
    End Function

    <Extension>
    Public Iterator Function ReadUMAP(cdf As netCDFReader) As IEnumerable(Of UMAPPoint)
        Dim sampleX As integers = cdf.getDataVariable("sampleX")
        Dim sampleY As integers = cdf.getDataVariable("sampleY")
        Dim umapX As doubles = cdf.getDataVariable("umapX")
        Dim umapY As doubles = cdf.getDataVariable("umapY")
        Dim umapZ As doubles = cdf.getDataVariable("umapZ")
        Dim clusters As integers = cdf.getDataVariable("cluster")
        Dim labels As String() = {}

        ' label is optional for make data compatibability
        If cdf.dataVariableExists("spot_labels") Then
            labels = DirectCast(cdf.getDataVariable("spot_labels"), chars).LoadJSON(Of String())
        End If

        For i As Integer = 0 To clusters.Length - 1
            Yield New UMAPPoint With {
                .[class] = clusters(i),
                .Pixel = New Point(sampleX(i), sampleY(i)),
                .x = umapX(i),
                .y = umapY(i),
                .z = umapZ(i),
                .label = labels.ElementAtOrDefault(i)
            }
        Next
    End Function

    <Extension>
    Public Function ReadUMAP(file As Stream) As UMAPPoint()
        Using cdf As New netCDFReader(file)
            Return cdf.ReadUMAP.ToArray
        End Using
    End Function

    <Extension>
    Public Iterator Function ReadTissueMorphology(cdf As netCDFReader) As IEnumerable(Of TissueRegion)
        Dim regions As Integer = any.ToString(cdf.getAttribute("regions")).ParseInteger

        For i As Integer = 1 To regions
            Dim refId As String = $"region_{i}"
            Dim var As variable = cdf.getDataVariableEntry(refId)
            Dim name As String = var.FindAttribute("label").value
            Dim color As String = var.FindAttribute("color").value
            Dim nsize As Integer = var.FindAttribute("size").value.ParseInteger
            Dim data As integers = cdf.getDataVariable(var)
            Dim pixels As Point() = data _
                .Split(2) _
                .Select(Function(p)
                            Return New Point With {
                                .X = p(Scan0),
                                .Y = p(1)
                            }
                        End Function) _
                .ToArray

            Yield New TissueRegion With {
                .color = color.TranslateColor,
                .label = name,
                .points = pixels
            }
        Next
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' target file stream will be close automatically
    ''' </remarks>
    <Extension>
    Public Function ReadTissueMorphology(file As Stream) As TissueRegion()
        Using cdf As New netCDFReader(file)
            ' 20220825 由于在这使用了using进行文件资源的自动释放
            ' 所以在这里不可以使用迭代器进行数据返回
            ' 否则文件读取模块会因为using语句自动释放资源导致报错
            Return cdf.ReadTissueMorphology.ToArray
        End Using
    End Function

End Module
