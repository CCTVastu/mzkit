﻿#Region "Microsoft.VisualBasic::08773540053cd775c56efed86cdbf09f, src\mzkit\mzkit\forms\frmLogin.vb"

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

' Class frmLogin
' 
'     Sub: Button1_Click, Button2_Click, LinkLabel1_LinkClicked
' 
' /********************************************************************************/

#End Region

Imports System.Collections.Specialized
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.My
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.SecurityString
Imports Task

Public Class frmLogin

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Call Process.Start("http://passport.biodeep.cn/register?lang=enUS")
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Call Close()
    End Sub

    ''' <summary>
    ''' login
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim account As String = TextBox1.Text
        Dim password As String = TextBox2.Text.MD5
        Dim post As New NameValueCollection

        If TextBox1.Text.StringEmpty OrElse TextBox2.Text.StringEmpty Then
            Call MessageBox.Show("Account or password could not be empty!", "BioDeep Login", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        Call post.Add("account", account)
        Call post.Add("password", password)

        Dim result As WebResponseResult = $"http://passport.biodeep.cn/passport/verify.vbs".POST(params:=post)
        Dim json As JsonObject = New JsonParser().OpenJSON(result.html)

        If json!code.AsString <> 0 Then
            Call MessageBox.Show("Account not found or incorrect password...", "BioDeep Login", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Else
            ' session_id
            ' cookie_name
            json = json!debug

            SingletonHolder(Of BioDeepSession).Instance.cookieName = json!cookie_name.AsString
            SingletonHolder(Of BioDeepSession).Instance.ssid = json!session_id.AsString

            Call saveSettings()
            Call Close()
        End If
    End Sub

    Private Function saveSettings() As String
        Dim random As String = Globals.Settings.random

        If random.StringEmpty Then
            random = RandomASCIIString(8)
            Globals.Settings.random = random
            Globals.Settings.Save()
        End If

        Dim SHA256 As New SHA256("mzkit_win32", random)
        Dim password As String = SHA256.EncryptData(TextBox2.Text)

        Return $"{TextBox1.Text}|{password}"
    End Function

    Private Function loadSettings() As NamedValue(Of String)
        Dim biodeep As NamedValue(Of String) = Globals.Settings.biodeep.GetTagValue("|")
        Dim random As String = Globals.Settings.random

        If random.StringEmpty Then
            random = RandomASCIIString(8)
            Globals.Settings.random = random
            Globals.Settings.Save()
        End If

        Dim SHA256 As New SHA256("mzkit_win32", random)
        Dim password As String = SHA256.DecryptString(biodeep.Value)

        Return New NamedValue(Of String)(biodeep.Name, biodeep.Value)
    End Function

    Private Sub frmLogin_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not Globals.Settings.biodeep.StringEmpty Then
            Dim biodeep As NamedValue(Of String) = loadSettings()

            TextBox1.Text = biodeep.Name
            TextBox2.Text = biodeep.Value
        End If
    End Sub
End Class
