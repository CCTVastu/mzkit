﻿#Region "Microsoft.VisualBasic::e38f644fe4ee15e5c89ddbfcb8451176, mzkit\src\mzmath\ms2_math-core\Spectra\Models\Xml\AlignmentOutput.vb"

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

    '   Total Lines: 62
    '    Code Lines: 49
    ' Comment Lines: 0
    '   Blank Lines: 13
    '     File Size: 2.23 KB


    '     Class AlignmentOutput
    ' 
    '         Properties: alignments, forward, jaccard, mirror, nhits
    '                     query, reference, reverse
    ' 
    '         Function: CreateLinearMatrix, GetAlignmentMirror, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Linq
Imports stdNum = System.Math

Namespace Spectra.Xml

    Public Class AlignmentOutput

        <XmlAttribute> Public Property forward As Double
        <XmlAttribute> Public Property reverse As Double
        <XmlAttribute> Public Property jaccard As Double
        <XmlAttribute> Public Property entropy As Double

        Public ReadOnly Property mirror As Double
            Get
                If _alignments.IsNullOrEmpty Then
                    Return 0.0
                Else
                    Dim nq As Integer = alignments.Where(Function(x) x.query > 0).Count
                    Dim nr As Integer = alignments.Where(Function(x) x.ref > 0).Count

                    Return jaccard * (stdNum.Min(nq, nr) / stdNum.Max(nq, nr))
                End If
            End Get
        End Property

        Public Property query As Meta
        Public Property reference As Meta

        <XmlArray("alignments")>
        Public Property alignments As SSM2MatrixFragment()

        Public ReadOnly Property nhits As Integer
            Get
                Return alignments _
                    .Where(Function(a)
                               If a.da = "NA" Then
                                   Return False
                               Else
                                   Return Not Double.Parse(a.da).IsNaNImaginary
                               End If
                           End Function) _
                    .Count
            End Get
        End Property

        Public Function GetAlignmentMirror() As (query As LibraryMatrix, ref As LibraryMatrix)
            With New Ms2AlignMatrix(alignments)
                Dim q = .GetQueryMatrix.With(Sub(a) a.name = query.id)
                Dim r = .GetReferenceMatrix.With(Sub(a) a.name = reference.id)

                Return (q, r)
            End With
        End Function

        Public Overrides Function ToString() As String
            Return $"{query} vs {reference}"
        End Function

        Public Shared Iterator Function CreateLinearMatrix(matrix As IEnumerable(Of SSM2MatrixFragment)) As IEnumerable(Of String)
            For Each line As SSM2MatrixFragment In matrix
                Yield $"{line.mz.ToString("F4")}:{line.query.ToString("G4")},{line.ref.ToString("G4")}"
            Next
        End Function

    End Class
End Namespace
