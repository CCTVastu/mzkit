﻿#Region "Microsoft.VisualBasic::06e67a0a2ee9045148505b92aaaf7e70, mzkit\src\assembly\mzPackExtensions\MSImaging\imzXMLWriter.vb"

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

    '   Total Lines: 42
    '    Code Lines: 28
    ' Comment Lines: 7
    '   Blank Lines: 7
    '     File Size: 1.34 KB


    ' Module imzXMLWriter
    ' 
    '     Function: WriteXML
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.MarkupData
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.mzData.mzWebCache
Imports Microsoft.VisualBasic.Imaging.Math2D

Public Module imzXMLWriter

    ''' <summary>
    ''' write mzpack file data as imzML file
    ''' </summary>
    ''' <param name="mzpack"></param>
    ''' <param name="output"></param>
    ''' <returns></returns>
    Public Function WriteXML(mzpack As mzPack, output As String) As Boolean
        Dim writer As imzML.mzPackWriter = imzML.mzPackWriter.OpenOutput(output)
        Dim polygon As New Polygon2D(mzpack.MS.Select(Function(p) p.GetMSIPixel))
        Dim dimsize As Size

        If polygon.length = 0 Then
            dimsize = New Size
        Else
            dimsize = New Size With {
                .Width = polygon.xpoints.Max,
                .Height = polygon.ypoints.Max
            }
        End If

        ' config of the writer
        Call writer _
            .SetMSImagingParameters(dimsize, 17) _
            .SetSpectrumParameters(1) _
            .SetSourceLocation(mzpack.source)

        For Each scan As ScanMS1 In mzpack.MS
            Call writer.WriteScan(scan)
        Next

        Call writer.Dispose()

        Return True
    End Function
End Module
