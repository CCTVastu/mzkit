﻿#Region "Microsoft.VisualBasic::968c987ee5edc81e1fa824ee68916d3e, mzkit\src\mzmath\MoleculeNetworking\Networking.vb"

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

    '   Total Lines: 91
    '    Code Lines: 82
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 3.50 KB


    ' Module Networking
    ' 
    '     Function: RepresentativeSpectrum, Tree
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports Microsoft.VisualBasic.DataMining.BinaryTree
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Serialization.JSON

Public Module Networking

    <Extension>
    Public Function Tree(ions As IEnumerable(Of PeakMs2),
                         Optional mzdiff As Double = 0.3,
                         Optional intocutoff As Double = 0.05,
                         Optional equals As Double = 0.85) As ClusterTree

        Dim align As New MSScore(New CosAlignment(Tolerance.DeltaMass(mzdiff), New RelativeIntensityCutoff(intocutoff)), ions, equals, equals)
        Dim clustering As New ClusterTree

        For Each ion As PeakMs2 In align.Ions
            Call ClusterTree.Add(clustering, ion.lib_guid, align, threshold:=equals)
        Next

        Return clustering
    End Function

    <Extension>
    Public Function RepresentativeSpectrum(cluster As PeakMs2(),
                                           tolerance As Tolerance,
                                           zero As RelativeIntensityCutoff,
                                           Optional key As String = Nothing) As PeakMs2
        Dim union As ms2() = cluster _
            .Select(Function(i)
                        Dim maxinto As Double = i.mzInto _
                            .Select(Function(mzi) mzi.intensity) _
                            .Max

                        Return i.mzInto _
                            .Select(Function(mzi)
                                        Return New ms2 With {
                                            .mz = mzi.mz,
                                            .intensity = mzi.intensity / maxinto
                                        }
                                    End Function)
                    End Function) _
            .IteratesALL _
            .ToArray _
            .Centroid(tolerance, cutoff:=zero) _
            .ToArray
        Dim rt As Double = cluster _
            .Select(Function(c) c.rt) _
            .TabulateBin _
            .Average
        Dim mz1 As Double
        Dim metadata = cluster _
            .Select(Function(c) c.meta) _
            .IteratesALL _
            .GroupBy(Function(t) t.Key) _
            .ToDictionary(Function(t) t.Key,
                            Function(t)
                                Return t _
                                    .Select(Function(ti) ti.Value) _
                                    .Distinct _
                                    .JoinBy("; ")
                            End Function)

        If cluster.Length = 1 Then
            mz1 = cluster(Scan0).mz
        Else
            mz1 = 0
            metadata("mz1") = cluster _
                .Select(Function(c) c.mz) _
                .ToArray _
                .GetJson
        End If

        Return New PeakMs2 With {
            .rt = rt,
            .activation = "NA",
            .collisionEnergy = 0,
            .file = key,
            .intensity = cluster.Sum(Function(c) c.intensity),
            .lib_guid = key,
            .mz = mz1,
            .mzInto = union,
            .precursor_type = "NA",
            .scan = "NA",
            .meta = metadata
        }
    End Function
End Module

