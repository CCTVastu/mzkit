﻿Imports System.IO
Imports System.IO.Compression
Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1.PrecursorType
Imports Microsoft.VisualBasic.Data.IO.MessagePack
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.SecurityString
Imports stdNum = System.Math

Public Class Writer : Implements IDisposable

    Dim disposedValue As Boolean
    Dim file As ZipArchive
    Dim index As New List(Of DynamicIndex)
    Dim mzcalc As Dictionary(Of String, MzCalculator)

    Private Class DynamicIndex
        Public mz As Double
        Public keys As New List(Of String)
    End Class

    Const IndexPath As String = ".metadata/index"

    Sub New(file As String, Optional truncated As Boolean = False)
        Call Me.New(file.Open(FileMode.OpenOrCreate, doClear:=False, [readOnly]:=True), truncated)
    End Sub

    Sub New(file As Stream, Optional truncated As Boolean = False)
        Call Me.New(New ZipArchive(file, ZipArchiveMode.Update), truncated)
    End Sub

    Sub New(file As ZipArchive, Optional truncated As Boolean = False)
        Me.file = file

        If Not truncated Then
            Me.LoadIndex()
        Else
            For Each item As ZipArchiveEntry In file.Entries.ToArray
                Call item.Delete()
            Next
        End If
    End Sub

    Private Sub LoadIndex()
        Dim list = From file As ZipArchiveEntry
                   In Me.file.Entries
                   Where file.FullName.StartsWith(IndexPath)

        For Each i As ZipArchiveEntry In list
            Dim tmp As MassIndex = MsgPackSerializer.Deserialize(Of MassIndex)(i.Open)
            Dim target As New DynamicIndex With {
                .mz = tmp.mz,
                .keys = tmp.referenceIds.AsList
            }

            Call index.Add(target)
        Next
    End Sub

    Private Sub writeIndex()
        For Each target As DynamicIndex In index
            Dim mass As New MassIndex With {
                .mz = target.mz,
                .referenceIds = target.keys.ToArray
            }
            Dim bytes = BitConverter.GetBytes(mass.mz)
            Dim hash As String = bytes.GetMd5Hash
            Dim fullName As String = $"{IndexPath}/{hash.Substring(0, 2)}/{hash}.msgpack"
            Dim pack As ZipArchiveEntry = getSection(fullName, Nothing)

            Using buffer As Stream = pack.Open
                Call MsgPackSerializer.SerializeObject(mass, buffer)
            End Using
        Next
    End Sub

    Private Function getSection(fullName As String, ByRef missing As Boolean) As ZipArchiveEntry
        Dim pack As ZipArchiveEntry = file.Entries.Where(Function(i) i.FullName = fullName).FirstOrDefault

        If pack Is Nothing Then
            pack = file.CreateEntry(fullName)
            missing = True
        Else
            missing = False
        End If

        Return pack
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub AddReference(ref As Metabolite)
        Dim key As String = AddIndex(ref)
        Dim fullName As String = $"{key.Substring(0, 2)}/{key}.dat"
        Dim missing As Boolean = False
        Dim pack As ZipArchiveEntry = getSection(fullName, missing)

        If Not missing Then
            Dim buffer = pack.Open
            Dim current As Metabolite = MsgPackSerializer.Deserialize(Of Metabolite)(buffer)

            ' union two object
            ref = New Metabolite With {
                .annotation = ref.annotation,
                .precursors = ref.precursors.JoinIterates(current.precursors).ToArray,
                .spectrums = ref.spectrums.JoinIterates(current.spectrums).ToArray
            }

            Call buffer.Close()
        End If

        Call MsgPackSerializer.SerializeObject(ref, pack.Open, closeFile:=True)
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="precursor"></param>
    ''' <returns>mz value round to 4 digits</returns>
    Private Function getMz(exactMass As Double, precursor As PrecursorData) As Double
        If Not mzcalc.ContainsKey(precursor.ion) Then
            mzcalc.Add(precursor.ion, Parser.ParseMzCalculator(precursor.ion, ionMode:=If(precursor.charge > 0, "+", "-")))
        End If

        Return stdNum.Round(mzcalc(precursor.ion).CalcMZ(exactMass), 4)
    End Function

    Private Function AddIndex(ref As Metabolite) As String
        Dim exactMass As Double = ref.exactMass
        Dim key As String = ref.Id.MD5

        For Each precursor As PrecursorData In ref.precursors
            Dim mz As Double = getMz(exactMass, precursor)
            Dim index As DynamicIndex = Me.index _
                .Where(Function(i)
                           Return stdNum.Abs(i.mz - mz) < 0.0001
                       End Function) _
                .FirstOrDefault

            If index Is Nothing Then
                index = New DynamicIndex With {
                    .mz = mz,
                    .keys = New List(Of String)
                }

                Call Me.index.Add(index)
            End If

            Call index.keys.Add(key)
        Next

        Return key
    End Function

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects)
                Call writeIndex()
                Call file.Dispose()
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
            ' TODO: set large fields to null
            disposedValue = True
        End If
    End Sub

    ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
    ' Protected Overrides Sub Finalize()
    '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
    '     Dispose(disposing:=False)
    '     MyBase.Finalize()
    ' End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub
End Class