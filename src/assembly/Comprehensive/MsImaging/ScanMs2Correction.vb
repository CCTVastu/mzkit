﻿
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.mzData.mzWebCache
Imports Microsoft.VisualBasic.Linq

Namespace MsImaging

    ''' <summary>
    ''' processing the MSI raw data contains with ms2 scan data
    ''' </summary>
    Public Class ScanMs2Correction : Inherits Correction

        Dim scans As ScanMS1()
        Dim scanId As Dictionary(Of String, Integer)
        Dim rtPixel As ScanTimeCorrection

        Sub New(totalTime As Double, pixels As Integer)
            rtPixel = New ScanTimeCorrection(totalTime, pixels)
        End Sub

        Public Sub SetMs1Scans(scans As IEnumerable(Of ScanMS1))
            Me.scans = scans.OrderBy(Function(i) i.rt).ToArray
            Me.scanId = Me.scans _
                .SeqIterator _
                .ToDictionary(Function(scan) scan.value.scan_id,
                              Function(scan)
                                  Return scan.i
                              End Function)
        End Sub

        Public Overrides Function GetPixelRowX(scanMs1 As ScanMS1) As Integer
            Dim i As Integer = scanId(scanMs1.scan_id)
            Dim nMs1 As Integer = -1
            Dim total As Integer = GetTotalScanNumbers(index:=i, ms1Count:=nMs1)
            Dim skipMs2 As Integer = total - nMs1

            Return rtPixel.GetPixelRow(scanMs1.rt) - skipMs2
        End Function

        Public Function GetTotalScanNumbers(index As Integer, Optional ByRef ms1Count As Integer = -1) As Integer
            If index = 0 Then
                Return 0
            End If

            Dim before As ScanMS1() = scans _
                .Take(index) _
                .ToArray

            ms1Count = before.Length

            Return Aggregate scan As ScanMS1
                   In before
                   Let total As Integer = GetTotalScanNumbers(scan)
                   Into Sum(total)
        End Function

        Public Shared Function GetTotalScanNumbers(scan As ScanMS1) As Integer
            If scan.products.IsNullOrEmpty Then
                Return 1
            Else
                Return scan.products.Length + 1
            End If
        End Function

        Public Shared Function GetTotalScanNumbers(raw As mzPack) As Integer
            Return Aggregate scan As ScanMS1
                   In raw.MS
                   Let total As Integer = GetTotalScanNumbers(scan)
                   Into Sum(total)
        End Function
    End Class
End Namespace