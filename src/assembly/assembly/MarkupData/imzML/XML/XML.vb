﻿#Region "Microsoft.VisualBasic::97634435c23d8aa1b58431026c730e19, src\assembly\assembly\MarkupData\imzML\XML\XML.vb"

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

    '     Class XML
    ' 
    '         Properties: version
    ' 
    '         Function: LoadScanData, LoadScans
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra

Namespace MarkupData.imzML

    <XmlType("indexedmzML", [Namespace]:=mzML.indexedmzML.xmlns)>
    Public Class XML

        <XmlAttribute>
        Public Property version As String

        ''' <summary>
        ''' just load the scan meta from the imzML file
        ''' </summary>
        ''' <param name="file"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function LoadScans(file As String) As IEnumerable(Of ScanData)
            Return mzML.indexedmzML.LoadScans(file).Select(Function(scan) New ScanData(scan))
        End Function

        Public Shared Iterator Function LoadScanData(imzML As String) As IEnumerable(Of ScanReader)
            Dim ibd As ibdReader = ibdReader.Open(imzML.ChangeSuffix("ibd"))

            For Each scan As ScanData In LoadScans(file:=imzML)
                Yield New ScanReader(scan, ibd)
            Next
        End Function
    End Class
End Namespace
