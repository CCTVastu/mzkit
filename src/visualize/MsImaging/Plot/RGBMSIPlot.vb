﻿#Region "Microsoft.VisualBasic::17c96e82e1b55df2f2e4c2782b86e25e, src\visualize\MsImaging\Plot\RGBMSIPlot.vb"

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

    ' Class RGBMSIPlot
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: PlotInternal
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports BioNovoGene.Analytical.MassSpectrometry.MsImaging.Imaging
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.MIME.Html.CSS

Public Class RGBMSIPlot : Inherits Plot

    ReadOnly R, G, B As SingleIonLayer
    ReadOnly dimensionSize As Size
    ReadOnly pixelDrawer As Boolean
    ReadOnly maxCut As Double = 0.75

    Public Sub New(R As SingleIonLayer, G As SingleIonLayer, B As SingleIonLayer, pixelDrawer As Boolean, theme As Theme)
        MyBase.New(theme)

        Me.pixelDrawer = pixelDrawer
        Me.R = R
        Me.G = G
        Me.B = B
        Me.dimensionSize = R.DimensionSize
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Dim Xtick As Double() = New DoubleRange({0, dimensionSize.Width}).CreateAxisTicks()
        Dim Ytick As Double() = New DoubleRange({0, dimensionSize.Height}).CreateAxisTicks
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
        Dim engine As Renderer = If(pixelDrawer, New PixelRender, New RectangleRender)
        Dim iR = Me.R.MSILayer
        Dim iG = Me.G?.MSILayer
        Dim iB = Me.B?.MSILayer
        Dim qr As DoubleRange = {0, Renderer.AutoCheckCutMax(iR.Select(Function(p) p.intensity).ToArray, maxCut)}
        Dim qg As DoubleRange = {0, Renderer.AutoCheckCutMax(iG.SafeQuery.Select(Function(p) p.intensity).ToArray, maxCut)}
        Dim qb As DoubleRange = {0, Renderer.AutoCheckCutMax(iB.SafeQuery.Select(Function(p) p.intensity).ToArray, maxCut)}

        MSI = engine.ChannelCompositions(Me.R.MSILayer, Me.G?.MSILayer, Me.B?.MSILayer, dimensionSize, cut:=(qr, qg, qb), background:=theme.background)
        MSI = Drawer.ScaleLayer(MSI, rect.Width, rect.Height, InterpolationMode.Bilinear)

        Call g.DrawAxis(canvas, scale, showGrid:=False, xlabel:=xlabel, ylabel:=ylabel, XtickFormat:="F0", YtickFormat:="F0", htmlLabel:=False)
        Call g.DrawImageUnscaled(MSI, rect)

        ' draw ion m/z
        Dim labelFont As Font = CSSFont.TryParse(theme.legendLabelCSS).GDIObject(g.Dpi)
        Dim labelSize As SizeF = g.MeasureString(Me.R.IonMz.ToString("F4"), labelFont)
        Dim pos As New Point(rect.Right + canvas.Padding.Right * 0.05, rect.Top + labelSize.Height)
        Dim mzR As New LegendObject With {
            .color = "red",
            .fontstyle = theme.legendLabelCSS,
            .style = LegendStyles.Square,
            .title = Me.R.IonMz.ToString("F4")
        }
        Dim mzG As LegendObject = Nothing
        Dim mzB As LegendObject = Nothing

        If Not Me.G Is Nothing Then
            mzG = New LegendObject With {
                .color = "green",
                .fontstyle = theme.legendLabelCSS,
                .style = LegendStyles.Square,
                .title = Me.G.IonMz.ToString("F4")
            }
        End If
        If Not Me.B Is Nothing Then
            mzB = New LegendObject With {
                .color = "blue",
                .fontstyle = theme.legendLabelCSS,
                .style = LegendStyles.Square,
                .title = Me.B.IonMz.ToString("F4")
            }
        End If

        Dim legends As LegendObject() = {mzR, mzG, mzB}.Where(Function(a) Not a Is Nothing).ToArray

        Call Legend.DrawLegends(g, pos, legends, $"{labelSize.Height},{labelSize.Height}")
    End Sub
End Class