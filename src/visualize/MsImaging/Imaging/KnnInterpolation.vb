﻿Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Math.Distributions

Namespace Imaging

    Public Module KnnInterpolation

        <Extension>
        Public Function KnnFill(layer As SingleIonLayer, Optional resolution As Integer = 10) As SingleIonLayer
            Dim graph As Grid(Of PixelData) = Grid(Of PixelData).Create(layer.MSILayer)
            Dim size As Size = layer.DimensionSize
            Dim dx As Integer = size.Width / resolution
            Dim dy As Integer = size.Height / resolution
            Dim pixels As New List(Of PixelData)
            Dim point As PixelData
            Dim deltaSize As New Size(dx, dy)

            For i As Integer = 1 To size.Width
                For j As Integer = 1 To size.Height
                    point = graph.GetData(i, j)

                    If point Is Nothing Then
                        point = graph.KnnInterpolation(i, j, deltaSize)

                        If Not point Is Nothing Then
                            Call graph.Add(point)
                        End If
                    End If

                    If Not point Is Nothing Then
                        Call pixels.Add(point)
                    End If
                Next
            Next

            Return New SingleIonLayer With {
                .DimensionSize = layer.DimensionSize,
                .IonMz = layer.IonMz,
                .MSILayer = pixels.ToArray
            }
        End Function

        <Extension>
        Private Function KnnInterpolation(graph As Grid(Of PixelData), x As Integer, y As Integer, deltaSize As Size) As PixelData
            Dim query As PixelData() = graph.Query(x, y, deltaSize).ToArray

            If query.IsNullOrEmpty Then
                Return Nothing
            End If

            Dim intensity As Double() = query.Select(Function(p) p.intensity).TabulateBin
            Dim mean As Double = intensity.Average

            Return New PixelData With {
                .intensity = mean,
                .level = 0,
                .mz = 0,
                .x = x,
                .y = y
            }
        End Function

    End Module
End Namespace