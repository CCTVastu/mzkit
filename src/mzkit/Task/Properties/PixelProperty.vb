﻿Imports System.ComponentModel
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports BioNovoGene.Analytical.MassSpectrometry.MsImaging.Pixel
Imports Microsoft.VisualBasic.Math.Quantile
Imports stdNum = System.Math

Public Class PixelProperty

    Public ReadOnly Property TopIonMz As Double
    Public ReadOnly Property MaxIntensity As Double
    Public ReadOnly Property MinIntensity As Double
    Public ReadOnly Property NumOfIons As Integer
    <Category("Intensity")> Public ReadOnly Property Q1 As Double
    <Category("Intensity")> Public ReadOnly Property Q2 As Double
    <Category("Intensity")> Public ReadOnly Property Q3 As Double
    <Category("Intensity")> Public ReadOnly Property Q1Count As Integer
    <Category("Intensity")> Public ReadOnly Property Q2Count As Integer
    <Category("Intensity")> Public ReadOnly Property Q3Count As Integer

    Public ReadOnly Property AverageIons As Double
    Public ReadOnly Property TotalIon As Double
    <Category("Pixel")> Public ReadOnly Property X As Integer
    <Category("Pixel")> Public ReadOnly Property Y As Integer

    Sub New(pixel As PixelScan)
        Dim ms As ms2() = pixel.GetMs
        Dim into As Double() = ms.Select(Function(mz) mz.intensity).ToArray

        X = pixel.X
        Y = pixel.Y

        If into.Length = 0 Then
        Else
            NumOfIons = ms.Length
            TopIonMz = stdNum.Round(ms.OrderByDescending(Function(i) i.intensity).First.mz, 4)
            MaxIntensity = stdNum.Round(into.Max)
            MinIntensity = stdNum.Round(into.Min)
            TotalIon = stdNum.Round(into.Sum)
            AverageIons = stdNum.Round(into.Average)

            Dim quartile = into.Quartile

            Q1 = stdNum.Round(quartile.Q1)
            Q2 = stdNum.Round(quartile.Q2)
            Q3 = stdNum.Round(quartile.Q3)
            Q1Count = into.Where(Function(i) i <= quartile.Q1).Count
            Q2Count = into.Where(Function(i) i <= quartile.Q2).Count
            Q3Count = into.Where(Function(i) i <= quartile.Q3).Count
        End If
    End Sub

End Class