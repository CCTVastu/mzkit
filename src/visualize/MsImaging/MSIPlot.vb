﻿Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.MIME.Html.CSS

Public Class MSIPlot : Inherits Plot

    ReadOnly ion As SingleIonLayer

    Public Sub New(ion As SingleIonLayer, theme As Theme)
        Call MyBase.New(theme)

        Me.ion = ion
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Dim Xtick As Double() = New DoubleRange({0, ion.DimensionSize.Width}).CreateAxisTicks()
        Dim Ytick As Double() = New DoubleRange({0, ion.DimensionSize.Height}).CreateAxisTicks
        Dim rect As Rectangle = canvas.PlotRegion
        Dim scaleX = d3js.scale.linear.domain(Xtick).range(New Double() {rect.Left, rect.Right})
        Dim scaleY = d3js.scale.linear.domain(Ytick).range(New Double() {rect.Top, rect.Bottom})
        Dim scale As New DataScaler With {
            .AxisTicks = (Xtick.AsVector, Ytick.AsVector),
            .region = rect,
            .X = scaleX,
            .Y = scaleY
        }

        Dim MSI As Image

        MSI = Drawer.RenderPixels(ion.MSILayer, ion.DimensionSize, Nothing, colorSet:=theme.colorSet)
        MSI = Drawer.ScaleLayer(MSI, rect.Width, rect.Height, InterpolationMode.Bilinear)

        Call g.DrawAxis(canvas, scale, showGrid:=False, xlabel:=xlabel, ylabel:=ylabel, XtickFormat:="F0", YtickFormat:="F0", htmlLabel:=False)
        Call g.DrawImageUnscaled(MSI, rect)

        ' draw ion m/z
        Dim labelFont As Font = CSSFont.TryParse(theme.legendLabelCSS)
        Dim labelSize As SizeF = g.MeasureString(ion.IonMz.ToString("F4"), labelFont)
        Dim pos As New Point(rect.Right + canvas.Padding.Right * 0.05, rect.Top + labelSize.Height)
        Dim mzLegend As New LegendObject With {
            .color = "black",
            .fontstyle = theme.legendLabelCSS,
            .style = LegendStyles.Square,
            .title = ion.IonMz.ToString("F4")
        }

        Call Legend.DrawLegends(g, pos, {mzLegend}, $"{labelSize.Height},{labelSize.Height}")

        Dim colors = Designer.GetColors(theme.colorSet, 30).Select(Function(c) New SolidBrush(c)).ToArray
        Dim intensityTicks As Double() = New DoubleRange(ion.GetIntensity).CreateAxisTicks
        Dim layout As New Rectangle(pos.X, pos.Y + labelSize.Height * 2, canvas.Padding.Right * 0.8, rect.Height * 0.7)
        Dim tickFont As Font = CSSFont.TryParse(theme.legendTickCSS)
        Dim tickPen As Pen = Stroke.TryParse(theme.legendTickAxisStroke)

        Call g.ColorMapLegend(layout, colors, intensityTicks, labelFont, "Intensity", tickFont, tickPen, format:="G3")
    End Sub
End Class