﻿#Region "Microsoft.VisualBasic::325ada35cacf2e0921047c02a31fc6c0, mzkit\src\mzmath\ms2_math-core\Chromatogram\TICPoint.vb"

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

'   Total Lines: 17
'    Code Lines: 12
' Comment Lines: 0
'   Blank Lines: 5
'     File Size: 562 B


'     Class TICPoint
' 
'         Properties: intensity, mz, time
' 
'         Function: ToString
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports stdNum = System.Math

Namespace Chromatogram

    Public Class TICPoint : Implements ITimeSignal

        Public Property mz As Double
        Public Property time As Double Implements ITimeSignal.time
        Public Property intensity As Double Implements ITimeSignal.intensity

        Public Overrides Function ToString() As String
            Return $"Dim [{mz.ToString("F3")}, {stdNum.Round(time, 1)}] = {intensity.ToString("G4")}"
        End Function

    End Class
End Namespace