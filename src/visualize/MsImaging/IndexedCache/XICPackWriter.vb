﻿Imports System.Drawing
Imports System.IO
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports BioNovoGene.Analytical.MassSpectrometry.MsImaging.Pixel
Imports BioNovoGene.Analytical.MassSpectrometry.MsImaging.Reader
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math

Namespace IndexedCache

    Public Class XICPackWriter : Implements IDisposable

        ReadOnly stream As StreamPack

        Dim disposedValue As Boolean

        Sub New(file As String)
            Call Me.New(file.Open(FileMode.OpenOrCreate, doClear:=False, [readOnly]:=False))
        End Sub

        Sub New(file As Stream)
            stream = New StreamPack(file,, meta_size:=64 * 1024 * 1024)
        End Sub

        Public Sub SetAttribute(dims As Size, mzdiff As Double, spares As Double)
            Call stream.globalAttributes.Add("dims", {dims.Width, dims.Height})
            Call stream.globalAttributes.Add("mzdiff", mzdiff)
            Call stream.globalAttributes.Add("spares", spares)
        End Sub

        Public Sub AddLayer(layer As MatrixXIC)
            Dim filename As String = $"/layers/{layer.GetType.Name}/{layer.mz}.dat"

            Using buffer As Stream = stream.OpenBlock(filename)
                Call layer.Serialize(buffer)
            End Using

            Dim obj = stream.GetObject(filename)

            obj.attributes.Add("mz", layer.mz)
            obj.attributes.Add("type", If(TypeOf layer Is PointXIC, 1, 0))
        End Sub

        Public Sub AddMsCache(scan As PixelScan)
            Dim filename As String = $"/msdata/{scan.Y}/{scan.scanId}.ms"
            Dim ms1 = scan.GetMs

            Using buffer As Stream = stream.OpenBlock(filename),
                bin As New BinaryDataWriter(buffer) With {
                    .ByteOrder = ByteOrder.BigEndian
            }
                bin.Write(ms1.Length)
                bin.Write(ms1.Select(Function(a) a.mz).ToArray)
                bin.Write(ms1.Select(Function(a) a.intensity).ToArray)
                bin.Flush()
            End Using

            Dim obj = stream.GetObject(filename)

            obj.attributes.Add("x", scan.X)
            obj.attributes.Add("y", scan.Y)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="raw"></param>
        ''' <param name="file">the target file to write</param>
        Public Shared Sub IndexRawData(raw As PixelReader, file As Stream,
                                       Optional da As Double = 0.01,
                                       Optional spares As Double = 0.2)
            Dim ionList = raw.AllPixels _
                .Select(Function(i)
                            Dim pt As New Point(i.X, i.Y)
                            Dim ions = i.GetMsPipe.Select(Function(ms) (pt, ms))

                            Return ions
                        End Function) _
                .IteratesALL _
                .ToArray
            Dim mzgroups = ionList _
                .GroupBy(Function(mzi)
                             Return mzi.ms.mz
                         End Function, offsets:=da)
            Dim dims As Size = raw.dimension
            Dim total As Integer = dims.Area
            Dim data As MatrixXIC

            Using pack As New XICPackWriter(file)
                Call pack.SetAttribute(
                    dims:=raw.dimension,
                    mzdiff:=da,
                    spares:=spares
                )

                For Each layer In mzgroups
                    data = getLayer(layer, total, spares, dims)
                    pack.AddLayer(layer:=data)
                Next

                For Each pixel As PixelScan In raw.AllPixels
                    Call pack.AddMsCache(scan:=pixel)
                Next
            End Using
        End Sub

        Private Shared Function getLayer(layer As NamedCollection(Of (Point, ms2)), total As Integer, spares As Double, dims As Size) As MatrixXIC
            Dim pixels = layer _
                .GroupBy(Function(p)
                             Dim pt = p.Item1
                             Return $"{pt.X},{pt.Y}"
                         End Function) _
                .ToArray

            If pixels.Length / total > spares Then
                ' matrix
                Dim matrix = Grid(Of IGrouping(Of String, (Point, ms2))) _
                    .Create(
                        data:=pixels,
                        getPixel:=Function(p)
                                      Dim t As String() = p.Key.Split(","c)
                                      Dim pt As New Point(t(0), t(1))

                                      Return pt
                                  End Function)
                Dim intensity As New List(Of Double)
                Dim hit As Boolean

                For i As Integer = 1 To dims.Width
                    For j As Integer = 1 To dims.Height
                        Dim p = matrix.GetData(i, j, hit)

                        If hit Then
                            intensity.Add(p.Average(Function(a) a.Item2.intensity))
                        Else
                            intensity.Add(0)
                        End If
                    Next
                Next

                Return New MatrixXIC With {
                    .mz = Val(layer.name),
                    .intensity = intensity.ToArray
                }
            Else
                ' point
                Return New PointXIC With {
                    .x = pixels.Select(Function(p) p.First.Item1.X).ToArray,
                    .y = pixels.Select(Function(p) p.First.Item1.Y).ToArray,
                    .mz = Val(layer.name),
                    .intensity = pixels _
                        .Select(Function(p)
                                    Return p.Average(Function(a) a.Item2.intensity)
                                End Function) _
                        .ToArray
                }
            End If
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                    Call stream.Dispose()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
                ' TODO: set large fields to null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
        ' Protected Overrides Sub Finalize()
        '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace