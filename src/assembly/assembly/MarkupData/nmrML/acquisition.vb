﻿#Region "Microsoft.VisualBasic::a5aeaf70a63ac71740c5ad39e02e23e1, src\assembly\assembly\MarkupData\nmrML\acquisition.vb"

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

'     Class acquisition
' 
'         Properties: acquisition1D, acquisitionMultiD
' 
'         Function: ParseMatrix
' 
'     Class acquisitionMultiD
' 
'         Properties: fidData
' 
'         Function: ParseMatrix
' 
'     Class fidData
' 
'         Properties: base64, byteFormat, compressed, encodedLength
' 
' 
' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace MarkupData.nmrML

    Public Class acquisition

        Public Property acquisitionMultiD As acquisitionMultiD
        Public Property acquisition1D As acquisitionMultiD

        Public Function ParseMatrix() As fidComplex()
            If acquisition1D Is Nothing Then
                Return acquisitionMultiD.ParseMatrix
            Else
                Return acquisition1D.ParseMatrix
            End If
        End Function

    End Class

    Public Class acquisitionMultiD

        ''' <summary>
        ''' Free Induction Decay
        ''' </summary>
        ''' <returns></returns>
        Public Property fidData As fidData

        Public Function ParseMatrix() As fidComplex()
            Dim rawComplex As New List(Of ms2)
            Dim vec As Double() = fidData.DecodeBytes
            Dim rawR As Double() = New Double(vec.Length / 2 - 1) {}
            Dim rawI As Double() = New Double(vec.Length / 2 - 1) {}
            Dim j As i32 = Scan0

            For i As Integer = 0 To vec.Length - 1 Step 2
                rawR(j) = vec(i)
                rawI(++j) = vec(i + 1)
            Next

            Dim index As Integer() = seq(from:=3 * rawR.Length / 4, [to]:=rawR.Length, by:=1) _
                .Select(Function(d) CInt(d) - 1) _
                .ToArray
            Dim mediaR = rawR.AsVector()(index).Average
            Dim mediaI = -rawI.AsVector()(index).Average

            rawR = rawR.AsVector - mediaR
            rawI = rawI.AsVector - mediaI

            Return rawR _
                .Select(Function(r, i)
                            Return New fidComplex With {
                                .real = r,
                                .imaging = rawI(i)
                            }
                        End Function) _
                .ToArray
        End Function

    End Class

    Public Class fidComplex

        Public Property real As Double
        Public Property imaging As Double

    End Class

    ''' <summary>
    ''' The signal we detect is called a Free Induction Decay (FID). 
    ''' The FID is produced by the macroscopic magnetization after 
    ''' the pulse. The magnetization will undergo several processes 
    ''' as it returns to equilibrium.
    ''' </summary>
    Public Class fidData

        <XmlAttribute> Public Property byteFormat As String
        <XmlAttribute> Public Property compressed As String
        <XmlAttribute> Public Property encodedLength As Integer

        <XmlText>
        Public Property base64 As String

    End Class
End Namespace