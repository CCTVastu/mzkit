﻿#Region "Microsoft.VisualBasic::e07dfe046d2e103cce8320d2d14f0c07, mzkit\src\assembly\assembly\mzPack\mzWebCache\mzMLScans.vb"

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

    '   Total Lines: 33
    '    Code Lines: 27
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 1.38 KB


    '     Class mzMLScans
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: dataReader, GetUVScans, loadScans
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.DataReader
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.MarkupData.mzML
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Math.SignalProcessing

Namespace mzData.mzWebCache

    Public Class mzMLScans : Inherits ScanPopulator(Of spectrum)

        Public Sub New(Optional mzErr$ = "da:0.1", Optional intocutoff As Double = 0.0001)
            MyBase.New(mzErr, intocutoff)
        End Sub

        Public Iterator Function GetUVScans(instrumentConfigurationId As String) As IEnumerable(Of GeneralSignal)
            For Each scan In invalidScans
                If Not scan.cvParams.KeyItem(UVScanType) Is Nothing Then
                    Yield scan.CreateGeneralSignal(instrumentConfigurationId)
                End If
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Protected Overrides Function loadScans(rawfile As String) As IEnumerable(Of spectrum)
            Return indexedmzML.LoadScans(rawfile)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Protected Overrides Function dataReader() As MsDataReader(Of spectrum)
            Return New mzMLScan()
        End Function
    End Class
End Namespace
