﻿#Region "Microsoft.VisualBasic::2d8d926ce80e4462f75265573efe8c14, mzkit\src\assembly\assembly\test\read_imzML.vb"

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

    '   Total Lines: 28
    '    Code Lines: 20
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 882.00 B


    ' Module read_imzML
    ' 
    '     Sub: Main, readmzML, testReadIbd
    ' 
    ' /********************************************************************************/

#End Region

Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.MarkupData
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.MarkupData.imzML

Module read_imzML

    Sub Main()
        Call readmzML()
        Call testReadIbd()
    End Sub

    Sub readmzML()
        Dim scans = imzML.XML.LoadScans("E:\demo\HR2MSI mouse urinary bladder S096.imzML").First
        Dim ibd As New ibdReader("E:\demo\HR2MSI mouse urinary bladder S096.ibd".Open([readOnly]:=True, doClear:=False), Format.Processed)
        Dim data = ibd.GetMSMS(scans)


        Pause()
    End Sub

    Sub testReadIbd()
        Dim ibd As New ibdReader("E:\demo\HR2MSI mouse urinary bladder S096.ibd".Open([readOnly]:=True, doClear:=False), Format.Processed)

        Dim testMzArray = ibd.ReadArray(16, 9032, 1129)
        Dim testIntoArray = ibd.ReadArray(9048, 4516, 1129)

        Pause()
    End Sub
End Module
