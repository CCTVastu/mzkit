﻿#Region "Microsoft.VisualBasic::2a75ecca65b456df60d55b7402f1dd92, src\mzkit\mzkit\application\ApplicationEvents.vb"

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

    '     Class MyApplication
    ' 
    '         Properties: featureExplorer, fileExplorer, host, LogForm, mzkitRawViewer
    '                     PlotParameters, REngine, ribbon, RtermPage
    ' 
    '         Function: GetSplashScreen
    ' 
    '         Sub: doRunScript, doRunScriptWithSpecialCommand, ExecuteRScript, handleResult, InitializeREngine
    '              InstallPackageRelease, LogText, MyApplication_UnhandledException, RegisterConsole, RegisterHost
    '              RegisterOutput, RegisterPlot
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Windows.Forms
Imports mzkit.DockSample
Imports RDev
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports Task

Namespace My

    ' The following events are available for MyApplication:
    ' Startup: Raised when the application starts, before the startup form is created.
    ' Shutdown: Raised after all application forms are closed.  This event is not raised if the application terminates abnormally.
    ' UnhandledException: Raised if the application encounters an unhandled exception.
    ' StartupNextInstance: Raised when launching a single-instance application and the application is already active. 
    ' NetworkAvailabilityChanged: Raised when the network connection is connected or disconnected.
    Partial Friend Class MyApplication

        Public Shared ReadOnly Property host As frmMain
        Public Shared ReadOnly Property LogForm As OutputWindow
        Public Shared ReadOnly Property REngine As RInterpreter

        Shared WithEvents console As Console
        Shared Rtask As Thread
        Shared cancel As New ManualResetEvent(initialState:=False)

        Public Shared ReadOnly TaskQueue As New ThreadQueue

#Region "mzkit"
        Public Shared ReadOnly Property mzkitRawViewer As PageMzkitTools
            Get
                Return _host.mzkitTool
            End Get
        End Property
#End Region

        Public Shared Sub RegisterPlot(plot As Action(Of PlotProperty),
                                       Optional width% = 2048,
                                       Optional height% = 1600,
                                       Optional padding$ = g.DefaultUltraLargePadding,
                                       Optional bg$ = "white",
                                       Optional title$ = "mzkit data plot",
                                       Optional showLegend As Boolean = True,
                                       Optional showGrid As Boolean = True,
                                       Optional gridFill$ = "white",
                                       Optional xlab$ = "X",
                                       Optional ylab$ = "Y")

            Dim margin As Padding = padding

            With WindowModules.plotParams.params
                .width = width
                .height = height
                .background = bg.TranslateColor
                .title = title
                .xlabel = xlab
                .ylabel = ylab
                .gridFill = gridFill.TranslateColor

                .show_legend = showLegend
                .show_grid = showGrid

                .padding_top = margin.Top
                .padding_right = margin.Right
                .padding_left = margin.Left
                .padding_bottom = margin.Bottom
            End With

            WindowModules.plotParams.draw = plot
            WindowModules.plotParams.draw()(WindowModules.plotParams.params)
        End Sub

        Public Shared Sub RegisterOutput(log As OutputWindow)
            _LogForm = log

            Microsoft.VisualBasic.My.Log4VB.redirectError =
                Sub(header, message, level)
                    log.Invoke(Sub() log.AppendMessage($"[{header} {level.ToString}] {message}"))
                End Sub
            Microsoft.VisualBasic.My.Log4VB.redirectWarning =
                Sub(header, message, level)
                    log.Invoke(Sub() log.AppendMessage($"[{header} {level.ToString}] {message}"))
                End Sub
        End Sub

        Public Shared Sub RegisterConsole(console As Console)
            _console = console
        End Sub

        Public Shared Function GetSplashScreen() As frmSplashScreen
            For i As Integer = 0 To Application.OpenForms.Count - 1
                If TypeOf Application.OpenForms(i) Is frmSplashScreen Then
                    Return DirectCast(Application.OpenForms(i), frmSplashScreen)
                End If
            Next

            Return Nothing
        End Function

        Public Shared Sub ExecuteRScript(scriptText As String, isFile As Boolean, writeLine As Action(Of String))
            Dim result As Object

            Using buffer As New MemoryStream
                Using writer As New StreamWriter(buffer)
                    MyApplication.REngine.RedirectOutput(writer, OutputEnvironments.Html)

                    If isFile Then
                        result = MyApplication.REngine.Source(scriptText)
                    Else
                        result = MyApplication.REngine.Evaluate(scriptText)
                    End If

                    Call REngine.Print(RInterpreter.lastVariableName)

                    writer.Flush()
                    writeLine(Encoding.UTF8.GetString(buffer.ToArray))
                End Using
            End Using

            Call handleResult(result, writeLine)
        End Sub

        Private Shared Sub handleResult(result As Object, writeLine As Action(Of String))
            If TypeOf result Is Message AndAlso DirectCast(result, Message).level = MSG_TYPES.ERR Then
                Dim err As Message = result

                writeLine(err.ToString & vbCrLf)

                For i As Integer = 0 To err.message.Length - 1
                    writeLine((i + 1) & ". " & err.message(i))
                Next

                writeLine(vbCrLf)

                For Each stack In err.environmentStack
                    writeLine(stack.ToString)
                Next

                writeLine(vbCrLf)
            End If
        End Sub

        Public Shared Sub InitializeREngine()
            Dim Rcore = GetType(RInterpreter).Assembly.FromAssembly
            Dim framework = GetType(App).Assembly.FromAssembly

            Call console.WriteLine($"
   , __           | 
  /|/  \  |  |    | Documentation: https://r_lang.dev.SMRUCC.org/
   |___/--+--+--  |
   | \  --+--+--  | Version {Rcore.AssemblyVersion} ({Rcore.BuiltTime.ToString})
   |  \_/ |  |    | sciBASIC.NET Runtime: {framework.AssemblyVersion}         
                  
Welcome to the R# language
")
            Call console.WriteLine("Type 'demo()' for some demos, 'help()' for on-line help, or
'help.start()' for an HTML browser interface to help.
Type 'q()' to quit R.
")

            _REngine = New RInterpreter

            _REngine.strict = False

            _REngine.LoadLibrary("base")
            _REngine.LoadLibrary("utils")
            _REngine.LoadLibrary("grDevices")
            _REngine.LoadLibrary("stats")

            _REngine.Evaluate("imports 'gtk' from 'Rstudio';")

            _REngine.LoadLibrary(GetType(MyApplication))

            AddHandler console.CancelKeyPress,
                Sub()
                    ' ctrl + C just break the current executation
                    ' not exit program running
                    cancel.Set()

                    If Not Rtask Is Nothing Then
                        Rtask.Abort()
                    End If
                End Sub

            Call New Thread(AddressOf New Shell(New PS1("> "), AddressOf doRunScriptWithSpecialCommand, dev:=console) With {.Quite = "!.R#::quit" & Rnd()}.Run).Start()
            Call DescriptionTooltip.SetEngine(REngine)
        End Sub

        Private Shared Sub doRunScriptWithSpecialCommand(script As String)
            Select Case script
                Case "CLS"
                    Call console.Clear()
                Case Else
                    If Not script.StringEmpty Then
                        Rtask = New Thread(Sub() Call doRunScript(script))
                        Rtask.Start()

                        ' block the running task thread at here
                        cancel.Reset()
                        cancel.WaitOne()
                    Else
                        console.WriteLine()
                    End If
            End Select
        End Sub

        Private Shared Sub doRunScript(script As String)
            Call ExecuteRScript(script, isFile:=False, AddressOf console.WriteLine)
            Call cancel.Set()
        End Sub

        Public Overloads Shared Sub LogText(msg As String)
            LogForm.AppendMessage(msg)
        End Sub

        Public Shared Sub RegisterHost(host As frmMain)
            MyApplication._host = host
        End Sub

        Private Sub MyApplication_UnhandledException(sender As Object, e As UnhandledExceptionEventArgs) Handles Me.UnhandledException
            Call App.LogException(e.Exception)
            Call MessageBox.Show(e.Exception.ToString, "Unknown Error!", MessageBoxButtons.OK, MessageBoxIcon.Error)

            Try
                Call host.SaveSettings()
                Call WindowModules.fileExplorer.SaveFileCache(Sub()
                                                                  ' do nothing
                                                              End Sub)
            Catch ex As Exception

            End Try

            End
        End Sub

        Public Shared Sub InstallPackageRelease()
            Dim file As String = $"{App.HOME}/Rstudio/mzkit.zip"

            If Not file.FileExists Then
                file = $"{App.HOME}/../../src/mzkit/setup/mzkit.zip"
            End If

            If file.FileExists Then
                Call REngine.Invoke("install.packages", {file}, REngine.globalEnvir)
            Else
                host.showStatusMessage("missing R# package release file: mzkit.zip!", My.Resources.StatusAnnotations_Warning_32xLG_color)
            End If
        End Sub
    End Class
End Namespace
