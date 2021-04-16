﻿#Region "Microsoft.VisualBasic::51c63fddeeb02fc49846f557a505b613, src\assembly\assembly\MarkupData\mzML\XML\index.vb"

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

'     Class indexList
' 
'         Properties: index
' 
'         Function: GetOffsets, ParseIndexList
' 
'     Class index
' 
'         Properties: name, offsets
' 
'         Function: ToString
' 
'     Class offset
' 
'         Properties: idRef, value
' 
'         Function: ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Text.Xml.Linq

Namespace MarkupData.mzML

    Public Class indexList : Inherits List

        <XmlElement(NameOf(index))>
        Public Property index As index()

        Friend Shared Function ParseIndexList(bin As BinaryDataReader, offset As Long) As indexList
            Dim text As New StreamReader(bin.BaseStream)
            Dim source As String
            Dim indexList As indexList

            bin.Seek(offset, SeekOrigin.Begin)
            source = text.IterateArrayNodes("indexList").FirstOrDefault
            indexList = Data.CreateNodeObject(Of indexList)(source)

            Return indexList
        End Function

        Public Iterator Function GetOffsets() As IEnumerable(Of NamedValue(Of Long))
            For Each group As index In index
                For Each offset As offset In group.offsets
                    Yield New NamedValue(Of Long) With {
                        .Description = group.name,
                        .Name = offset.idRef,
                        .Value = offset.value
                    }
                Next
            Next
        End Function

    End Class

    Public Class index

        <XmlAttribute>
        Public Property name As String

        <XmlElement(NameOf(offset))>
        Public Property offsets As offset()

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return name
        End Function

    End Class

    Public Class offset

        <XmlAttribute>
        Public Property idRef As String

        <XmlText> Public Property value As Long

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"{idRef}: {value}"
        End Function
    End Class
End Namespace
