﻿
Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra.Xml
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace PoolData

    ''' <summary>
    ''' the cluster tree flesystem
    ''' </summary>
    Public MustInherit Class PoolFs : Implements IDisposable

        Dim disposedValue As Boolean
        ''' <summary>
        ''' due to the reason of contains in-memory cache system
        ''' inside this module, so we should share this object between
        ''' multiple pool object
        ''' </summary>
        Dim score As AlignmentProvider

        Protected m_level As Double
        Protected m_split As Integer

        ''' <summary>
        ''' the score threshold for assign the given spectrum as current cluster member
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property level As Double
            Get
                Return m_level
            End Get
        End Property
        Public ReadOnly Property split As Integer
            Get
                Return m_split
            End Get
        End Property
        Public ReadOnly Property splitDelta As Double

        Public MustOverride Function CheckExists(spectral As PeakMs2) As Boolean
        Public MustOverride Function GetTreeChilds(path As String) As IEnumerable(Of String)
        Public MustOverride Function LoadMetadata(path As String) As MetadataProxy
        Public MustOverride Sub CommitMetadata(path As String, data As MetadataProxy)
        Public MustOverride Function FindRootId(path As String) As String
        Public MustOverride Sub SetRootId(path As String, id As String)
        Public MustOverride Function ReadSpectrum(p As Metadata) As PeakMs2
        Public MustOverride Function WriteSpectrum(spectral As PeakMs2) As Metadata

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetScore(x As PeakMs2, y As PeakMs2) As AlignmentOutput
            Return score.CreateAlignment(x.mzInto, y.mzInto)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Sub SetScore(da As Double, intocutoff As Double)
            score = AlignmentProvider.Cosine(Tolerance.DeltaMass(da), New RelativeIntensityCutoff(intocutoff))
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="level">
        ''' the score threshold for assign the given spectrum as current cluster member
        ''' </param>
        ''' <param name="split">split into n parts</param>
        Friend Overridable Sub SetLevel(level As Double, split As Integer)
            m_level = level
            m_split = split
            _splitDelta = level / split
        End Sub

        Public Shared Function OpenAuto(link As String, model_id As String) As PoolFs
            Dim linkStr As String = link.ToLower

            If linkStr.StartsWith("http://") OrElse linkStr.StartsWith("https://") Then
                ' web services based
                Dim pool As New HttpTreeFs(link, model_id)
                Return pool
            Else
                Return New TreeFs(link)
            End If
        End Function

        Public Shared Function CreateAuto(link As String,
                                          level As Double,
                                          split As Integer,
                                          name As String,
                                          desc As String) As PoolFs

            Dim linkStr As String = link.ToLower

            If linkStr.StartsWith("http://") OrElse linkStr.StartsWith("https://") Then
                ' web services based
                Dim info = HttpTreeFs.CreateModel(link, name, desc, level, split)
                Dim pool As New HttpTreeFs(link, info.model_id)

                Return pool
            Else
                Dim local As New TreeFs(link)
                Dim params As New Dictionary(Of String, String) From {
                    {"level", level},
                    {"split", split}
                }

                local.WriteText(name, "/.metadata/name.txt")
                local.WriteText(desc, "/.metadata/note.txt")
                local.WriteText(params.GetJson, "/.metadata/params.json")

                Return local
            End If
        End Function

        Protected MustOverride Sub Close()

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    Call Close()
                End If

                ' TODO: 释放未托管的资源(未托管的对象)并重写终结器
                ' TODO: 将大型字段设置为 null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
        ' Protected Overrides Sub Finalize()
        '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace