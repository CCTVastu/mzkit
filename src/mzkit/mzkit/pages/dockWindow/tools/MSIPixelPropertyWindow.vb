﻿Imports System.Drawing.Drawing2D
Imports BioNovoGene.Analytical.MassSpectrometry.MsImaging.Pixel
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Shapes
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Quantile
Imports Task

Public Class MSIPixelPropertyWindow

    Public Sub SetPixel(pixel As PixelScan)
        PropertyGrid1.SelectedObject = New PixelProperty(pixel)

        Dim q As QuantileEstimationGK = pixel.GetMs.Select(Function(i) i.intensity).GKQuantile
        Dim serial As New SerialData With {
            .color = Color.SteelBlue,
            .lineType = DashStyle.Dash,
            .pointSize = 10,
            .shape = LegendStyles.Triangle,
            .title = "Intensity",
            .width = 5,
            .pts = seq(0, 1, 0.1) _
                .Select(Function(lv)
                            Return New PointData(lv, q.Query(lv))
                        End Function) _
                .ToArray
        }
        Dim Q2line As New Line(New PointF(0, q.Query(0.5)), New PointF(1, q.Query(0.5)), New Pen(Color.Red, 10))

        If DirectCast(PropertyGrid1.SelectedObject, PixelProperty).NumOfIons = 0 Then
            PictureBox1.BackgroundImage = Nothing
        Else
            PictureBox1.BackgroundImage = {serial}.Plot(
                size:="1800,1200",
                padding:="padding:50px 50px 100px 200px;",
                fill:=True,
                ablines:={Q2line},
                YtickFormat:="G2"
            ).AsGDIImage
        End If
    End Sub

    Private Sub MSIPixelPropertyWindow_Load(sender As Object, e As EventArgs) Handles Me.Load
        TabText = "MSI Pixel Properties"
        PictureBox1.BackgroundImageLayout = ImageLayout.Zoom
    End Sub
End Class