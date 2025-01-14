Imports System.IO
Imports Microsoft.VisualBasic.ValueTypes

Public Class read_WDF

    ''' <summary>
    ''' Convert the Windows 64bit timestamp to human readable format.
    ''' </summary>
    ''' <param name="t">
    ''' timestamp in W64 format (default for .wdf files)
    ''' </param>
    ''' <returns>
    ''' string formatted to suit local settings
    ''' </returns>
    Public Function convert_time(t As Double) As String
        'time_of_spectrum_recording =
        '  [convert_time(x) for x in origins.iloc[:,4]]

        'should give you the list with the times on which
        'each specific spectrum was recorded
        Return DateTimeHelper.FromUnixTimeStamp(t).ToString
    End Function

    ''' <summary>
    ''' Read data from the binary .wdf file.
    ''' </summary>
    ''' <param name="filename">
    ''' The complete (relative or absolute) path to the file
    ''' </param>
    ''' <param name="verbose"></param>
    ''' <returns>
    ''' The data is returned in form of five variables.
    ''' </returns>
    Public Function read_WDF(filename As String, Optional verbose As Boolean = False) As WDF
        Dim DATA_TYPES = {"Arbitrary",
        "Spectral",
        "Intensity",
        "SpatialX",
        "SpatialY",
        "SpatialZ",
        "SpatialR",
        "SpatialTheta",
        "SpatialPhi",
        "Temperature",
        "Pressure",
        "Time",
        "Derived",
        "Polarization",
        "FocusTrack",
        "RampRate",
        "Checksum",
        "Flags",
        "ElapsedTime",
        "Frequency",
        "MpWellSpatialX",
        "MpWellSpatialY",
        "MpLocationIndex",
        "MpWellReference",
        "PAFZActual",
        "PAFZError",
        "PAFSignalUsed",
        "ExposureTime",
        "EndMarker"}
        Dim DATA_UNITS = {"Arbitrary",
        "RamanShift",
        "Wavenumber",
        "Nanometre",
        "ElectronVolt",
        "Micron",
        "Counts",
        "Electrons",
        "Millimetres",
        "Metres",
        "Kelvin",
        "Pascal",
        "Seconds",
        "Milliseconds",
        "Hours",
        "Days",
        "Pixels",
        "Intensity",
        "RelativeIntensity",
        "Degrees",
        "Radians",
        "Celcius",
        "Farenheit",
        "KelvinPerMinute",
        "FileTime",
        "Microseconds",
        "EndMarker"}
        Dim SCAN_TYPES = {"Unspecified",
        "Static",
        "Continuous",
        "StepRepeat",
        "FilterScan",
        "FilterImage",
        "StreamLine",
        "StreamLineHR",
        "Point",
        "MultitrackDiscrete",
        "LineFocusMapping"}

        Dim MAP_TYPES As New Dictionary(Of Long, String) From {{0, "RandomPoints"},
{1, "ColumnMajor"},
{2, "Alternating2"},
{3, "LineFocusMapping"},
{4, "InvertedRows"},
{5, "InvertedColumns"},
{6, "SurfaceProfile"},
{7, "XyLine"},
{64, "LiveTrack"},  ' added as it seemed fit
{66, "StreamLine"},
{68, "InvertedRows"},  ' Remember to check this 68
{128, "Slice"}}

        Dim MEASUREMENT_TYPES = {"Unspecified",
        "Single",
        "Series",
        "Map"}

        Dim WDF_FLAGS As New Dictionary(Of Long, String) From {{0, "WdfXYXY"},
              {1, "WdfChecksum"},
{2, "WdfCosmicRayRemoval"},
{3, "WdfMultitrack"},
{4, "WdfSaturation"},
{5, "WdfFileBackup"},
{6, "WdfTemporary"},
{7, "WdfSlice"},
{8, "WdfPQ"},
{16, "UnknownFlag (check in WiRE?)"}}

        Dim f = filename.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
        Dim filesize As Long = filename.FileLength

        If verbose Then
            Call println($"Reading the file: '{filename.FileName}")
        End If
    End Function
End Class

Public Class WDF

    ''' <summary>
    ''' all the recorded spectra
    ''' </summary>
    ''' <returns></returns>
    Public Property spectra
    ''' <summary>
    ''' the raman shifts
    ''' </summary>
    ''' <returns></returns>
    Public Property x_values
    ''' <summary>
    ''' dictionary containing measurement parameters
    ''' </summary>
    ''' <returns></returns>
    Public Property params As Dictionary(Of String, String)
    ''' <summary>
    ''' dictionary containing map parameters
    ''' </summary>
    ''' <returns></returns>
    Public Property map_params As Dictionary(Of String, String)
    ''' <summary>
    ''' the spatio-temporal coordinates of each recording.
    ''' Note that it has triple column names (label, data type, data units)
    ''' </summary>
    ''' <returns></returns>
    Public Property origins

End Class