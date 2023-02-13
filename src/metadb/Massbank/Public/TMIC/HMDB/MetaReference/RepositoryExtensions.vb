﻿#Region "Microsoft.VisualBasic::99d36d5c3b3c5cd7dbe8fdc8aba3742f, mzkit\src\metadb\Massbank\Public\TMIC\HMDB\MetaReference\RepositoryExtensions.vb"

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

    '   Total Lines: 37
    '    Code Lines: 31
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 1.48 KB


    '     Module RepositoryExtensions
    ' 
    '         Function: EnumerateNames, GetMetabolite, PopulateHMDBMetaData
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports XmlLinq = Microsoft.VisualBasic.Text.Xml.Linq.Data

Namespace TMIC.HMDB.Repository

    <HideModuleName>
    Public Module RepositoryExtensions

        ReadOnly web As New Dictionary(Of String, WebQuery)

        Public Function GetMetabolite(id As String, Optional cache$ = "./hmdb/", Optional offline As Boolean = False) As metabolite
            Dim engine As WebQuery = web.ComputeIfAbsent(cache, lazyValue:=Function() New WebQuery(cache,, offline))
            engine.offlineMode = offline
            Return engine.Query(Of metabolite)(id)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function PopulateHMDBMetaData(Xml As String) As IEnumerable(Of MetaReference)
            Return XmlLinq.LoadXmlDataSet(Of MetaReference)(
                XML:=Xml,
                typeName:=NameOf(metabolite),
                xmlns:="http://www.hmdb.ca",
                forceLargeMode:=True
            )
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function EnumerateNames(metabolite As MetaReference) As IEnumerable(Of String)
            Return {metabolite.name}.AsList +
                metabolite.synonyms.synonym +
                metabolite.iupac_name
        End Function
    End Module
End Namespace
