﻿#Region "Microsoft.VisualBasic::417bc1378fbef78e1b4af06287cd7054, src\metadna\metaDNA\Models\KEGGCompound.vb"

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

    ' Structure KEGGCompound
    ' 
    '     Properties: ExactMass, kegg_id
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports BioNovoGene.BioDeep.Chemoinformatics
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports SMRUCC.genomics.Assembly.KEGG.DBGET.bGetObject

''' <summary>
''' object model wrapper for the KEGG compound in order to apply of the generic ms search engine
''' </summary>
Public Structure KEGGCompound : Implements IReadOnlyId, IExactmassProvider

    Public ReadOnly Property ExactMass As Double Implements IExactmassProvider.ExactMass
        Get
            Return KEGG.exactMass
        End Get
    End Property

    Public ReadOnly Property kegg_id As String Implements IReadOnlyId.Identity
        Get
            Return KEGG.entry
        End Get
    End Property

    Dim KEGG As Compound

    Public Overrides Function ToString() As String
        Return KEGG.ToString
    End Function

End Structure
