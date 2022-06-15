﻿#Region "Microsoft.VisualBasic::e171128c53298d13f6c81ca09e8dc8da, mzkit\src\visualize\MsImaging\SingleIonLayer.vb"

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

    '   Total Lines: 150
    '    Code Lines: 104
    ' Comment Lines: 23
    '   Blank Lines: 23
    '     File Size: 5.13 KB


    ' Class SingleIonLayer
    ' 
    '     Properties: DimensionSize, hasZeroPixels, IonMz, maxinto, MSILayer
    ' 
    '     Function: GetIntensity, (+3 Overloads) GetLayer, GetQuartile, IntensityCutoff, MeasureUninSize
    '               Take, (+2 Overloads) ToString, Trim
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports BioNovoGene.Analytical.MassSpectrometry.MsImaging.Reader
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Quantile

Public Class SingleIonLayer

    Public Property IonMz As String
    Public Property MSILayer As PixelData()

    ''' <summary>
    ''' the canvas size of the MSI plot output
    ''' </summary>
    ''' <returns></returns>
    Public Property DimensionSize As Size

    Public ReadOnly Property hasZeroPixels As Boolean
        Get
            Return DimensionSize.Width * DimensionSize.Height > MSILayer.Length
        End Get
    End Property

    Public ReadOnly Property maxinto As Double
        Get
            Return Aggregate p As PixelData In MSILayer Into Max(p.intensity)
        End Get
    End Property

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="xy">x,y</param>
    ''' <returns></returns>
    Public ReadOnly Property Item(xy As String()) As SingleIonLayer
        Get
            Dim xyIndex As Index(Of String) = xy.Indexing
            Dim pixels As PixelData() = MSILayer _
                .Where(Function(p)
                           Return $"{p.x},{p.y}" Like xyIndex
                       End Function) _
                .ToArray

            Return New SingleIonLayer With {
                .DimensionSize = DimensionSize,
                .IonMz = IonMz,
                .MSILayer = pixels
            }
        End Get
    End Property

    ''' <summary>
    ''' Removes pixels which relative intensity value is 
    ''' less than the given <paramref name="intocutoff"/> 
    ''' threshold.
    ''' </summary>
    ''' <param name="intocutoff">
    ''' relative intensity cutoff value in range ``[0,1]``.
    ''' </param>
    ''' <returns></returns>
    Public Function IntensityCutoff(intocutoff As Double) As SingleIonLayer
        Dim maxinto As Double = Me.maxinto

        Return New SingleIonLayer With {
            .DimensionSize = DimensionSize,
            .IonMz = IonMz,
            .MSILayer = MSILayer _
                .Where(Function(p)
                           Return p.intensity / maxinto >= intocutoff
                       End Function) _
                .ToArray
        }
    End Function

    Public Overrides Function ToString() As String
        Return $"({MSILayer.Length} pixels) {ToString(Me)}"
    End Function

    Public Overloads Shared Function ToString(ion As SingleIonLayer) As String
        Return If(ion.IonMz.IsNumeric, $"m/z {Double.Parse(ion.IonMz).ToString("F4")}", ion.IonMz)
    End Function

    Public Function MeasureUninSize(sampling As Integer) As Size
        Return New Size(DimensionSize.Width / sampling, DimensionSize.Height / sampling)
    End Function

    ''' <summary>
    ''' remove a polygon region from the MSI render raw data
    ''' </summary>
    ''' <param name="polygon"></param>
    ''' <param name="unionSize"></param>
    ''' <returns></returns>
    Public Function Trim(polygon As Polygon2D, unionSize As Size) As SingleIonLayer
        Dim takes As PixelData() = MSILayer _
            .TrimRegion(polygon, unionSize) _
            .Distinct _
            .ToArray

        Return New SingleIonLayer With {
            .IonMz = IonMz,
            .DimensionSize = DimensionSize,
            .MSILayer = takes
        }
    End Function

    ''' <summary>
    ''' take part of the pixels array from the current layer with given region polygon data.
    ''' </summary>
    ''' <param name="region"></param>
    ''' <param name="unionSize"></param>
    ''' <returns></returns>
    Public Function Take(region As Polygon2D, unionSize As Size) As SingleIonLayer
        Dim takes As PixelData() = MSILayer _
            .TakeRegion(region, unionSize) _
            .Distinct _
            .ToArray

        Return New SingleIonLayer With {
            .IonMz = IonMz,
            .DimensionSize = DimensionSize,
            .MSILayer = takes
        }
    End Function

    Public Function GetIntensity() As Double()
        Return MSILayer.Select(Function(p) p.intensity).ToArray
    End Function

    Public Function GetQuartile() As DataQuartile
        Return GetIntensity.Quartile
    End Function

    Public Shared Function GetLayer(mz As Double(), viewer As PixelReader, mzErr As Tolerance) As SingleIonLayer
        Dim pixels As PixelData() = viewer _
            .LoadPixels(mz, mzErr) _
            .ToArray

        Return New SingleIonLayer With {
            .IonMz = mz.FirstOrDefault.ToString("F4"),
            .DimensionSize = viewer.dimension,
            .MSILayer = pixels
        }
    End Function

    Public Shared Function GetLayer(mz As Double, viewer As PixelReader, mzErr As Tolerance) As SingleIonLayer
        Dim pixels As PixelData() = viewer _
            .LoadPixels({mz}, mzErr) _
            .ToArray

        Return New SingleIonLayer With {
            .IonMz = mz,
            .DimensionSize = viewer.dimension,
            .MSILayer = pixels
        }
    End Function

    Public Shared Function GetLayer(mz As Double(), viewer As Drawer, mzErr As Tolerance) As SingleIonLayer
        Dim pixels As PixelData() = viewer _
            .LoadPixels(mz, mzErr) _
            .ToArray

        Return New SingleIonLayer With {
            .IonMz = If(mz.Length = 1, mz(0), mz.Select(Function(d) d.ToString("F4")).JoinBy("+")),
            .DimensionSize = viewer.dimension,
            .MSILayer = pixels
        }
    End Function

    Public Shared Narrowing Operator CType(ion As SingleIonLayer) As PixelData()
        Return If(ion Is Nothing, Nothing, ion.MSILayer)
    End Operator
End Class
