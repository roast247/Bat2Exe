Imports System.CodeDom.Compiler
Public Class Form1
    Dim fichier, icone As String
    Dim Source As String =
    "Module BtE" & vbNewLine &
    "Sub Main()" & vbNewLine &
    "If System.IO.File.Exists(System.Environment.GetEnvironmentVariable(" & Chr(34) & "TMP" & Chr(34) & ") & " & Chr(34) & "\cmd.bat" & Chr(34) & ") Then" & vbNewLine &
    "System.IO.File.Delete(System.Environment.GetEnvironmentVariable(" & Chr(34) & "TMP" & Chr(34) & ") & " & Chr(34) & "\cmd.bat" & Chr(34) & ")" & vbNewLine &
    "End If" & vbNewLine &
    "System.IO.File.WriteAllText(System.Environment.GetEnvironmentVariable(" & Chr(34) & "TMP" & Chr(34) & ") & " & Chr(34) & "\cmd.bat" & Chr(34) & ", {BATCH})" & vbNewLine &
    "System.Diagnostics.Process.Start(System.Environment.GetEnvironmentVariable(" & Chr(34) & "TMP" & Chr(34) & ") & " & Chr(34) & "\cmd.bat" & Chr(34) & ")" & vbNewLine &
    "End Sub" & vbNewLine &
    "End Module"

    Private draggable As Boolean
    Private mouseY As Integer
    Private mouseX As Integer
    Private drag As Boolean

    Public Sub GenerateExecutable(ByVal Output As String, ByVal Source As String)
        Try
            Dim Param As New CompilerParameters()
            Dim Res As CompilerResults
            Param.GenerateExecutable = True
            Param.IncludeDebugInformation = False
            Param.CompilerOptions = "/target:winexe /optimize+ /filealign:512"
            Param.OutputAssembly = Output
            Res = New VBCodeProvider(New Dictionary(Of String, String)() From {{"Version", "v2"}}).CompileAssemblyFromSource(Param, Source)
            If Res.Errors.Count <> 0 Then
                For Each [Error] In Res.Errors
                    MessageBox.Show("Error: " & [Error].ErrorText, "", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Next
            End If
        Catch ex As Exception
            MsgBox("Error: " & ex.ToString)
        End Try
    End Sub
    Function TobString(ByVal b As String)
        Dim ret As String = ""
        For i As Integer = 0 To Split(b, vbNewLine).Count - 1
            Dim line As String = Split(b, vbNewLine)(i).Replace(Chr(34), Chr(34) & " & System.Text.Encoding.ASCII.GetString(New Byte() {34}) & " & Chr(34))
            ret &= Chr(34) & line & Chr(34) & " & System.Environment.NewLine & _" & vbNewLine
        Next
        Return ret.Remove(ret.Length - 6)
    End Function
    Sub ConvertBtE(ByVal Output As String)
        UpdateStatus("Reading data...", 50)
        Dim batch As String
        If (Button3.Text = "From the textbox") Then
            batch = IO.File.ReadAllText(TextBox1.Text)
        Else
            batch = TextBox3.Text
        End If
        UpdateStatus("Preparing code...", 25)
        Dim code As String = Source
        '        UpdateStatus("", 50)
        code = code.Replace("{BATCH}", TobString(batch))
        UpdateStatus("Compiling...", 50)
        GenerateExecutable(Output, code)
    End Sub


    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        If (Button3.Text = "From the textbox") And (Not IO.File.Exists(TextBox1.Text)) Then
            MsgBox("Please select a valid batch file!", MsgBoxStyle.Critical, "Error")
            Exit Sub
        End If
        Dim sfd As New SaveFileDialog
        sfd.Filter = "Executables|*.exe"
        If sfd.ShowDialog <> vbOK Then
            Exit Sub
        End If

        Button1.Enabled = False
        Button2.Enabled = False
        Button3.Enabled = False
        Button4.Enabled = False
        TextBox1.Enabled = False
        TextBox2.Enabled = False
        TextBox3.Enabled = False
        TextBox4.Enabled = False

        TextBox2.Text = sfd.FileName
        Dim tConvert As New Threading.Thread(AddressOf ConvertBtE)
        tConvert.Start(sfd.FileName)

        If TextBox4.Text = "" Then
            Dim msg As String
            Dim title As String
            Dim style As MsgBoxStyle
            Dim response As MsgBoxResult
            msg = "Do you want to add an Icon?"
            title = "Add an Icon"
            style = MsgBoxStyle.Question + MsgBoxStyle.YesNo
            response = MsgBox(msg, style, title)
            If response = MsgBoxResult.Yes Then


                Dim ofd As New OpenFileDialog
                ofd.Title = "Select an icon"
                ofd.Filter = "Icon (*.ico)|*.ico"
                If ofd.ShowDialog <> vbOK Then
                    Exit Sub
                End If
                TextBox4.Text = ofd.FileName
                PictureBox1.SizeMode = PictureBoxSizeMode.StretchImage
                PictureBox1.ImageLocation = ofd.FileName
                ChangeIcon()
                UpdateStatus("Injecting icon...", 75)
            End If
        End If
        If TextBox4.Text = "" Then
            ChangeIcon()
            UpdateStatus("Injecting icon...", 75)
        End If

        UpdateStatus("Done", 100)

        Button1.Enabled = True
        Button2.Enabled = True
        Button3.Enabled = True
        Button4.Enabled = True
        TextBox1.Enabled = True
        TextBox2.Enabled = True
        TextBox3.Enabled = True
        TextBox4.Enabled = True

    End Sub

    Delegate Sub delUpdateStatus(ByVal lbl As String, ByVal prgs As Integer)
    Sub UpdateStatus(ByVal lbl As String, ByVal prgs As Integer)
        If InvokeRequired Then
            Invoke(New delUpdateStatus(AddressOf UpdateStatus), New Object() {lbl, prgs})
        Else
            If lbl <> "" Then Label1.Text = "Status: " + lbl
            ProgressBar1.Value = prgs
        End If
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        If Button3.Text = "Use text instead" Then
            Button3.Text = "Use batch file"
            Button1.Enabled = False
            TextBox3.Enabled = True
        Else
            Button3.Text = "Use text instead"
            Button1.Enabled = True
            TextBox3.Enabled = False
        End If
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Dim ofd As New OpenFileDialog
        ofd.Title = "Select an icon"
        ofd.Filter = "Icon (*.ico)|*.ico"
        If ofd.ShowDialog <> vbOK Then
            Exit Sub
        End If
        TextBox4.Text = ofd.FileName
        PictureBox1.SizeMode = PictureBoxSizeMode.StretchImage
        PictureBox1.ImageLocation = ofd.FileName
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        UpdateStatus("Inactive", 0)
        Dim ofd As New OpenFileDialog
        ofd.Filter = "Batch files|*.bat"
        If ofd.ShowDialog <> vbOK Then
            Exit Sub
        End If
        TextBox1.Text = ofd.FileName
    End Sub

    Private Sub closeApp_Click(sender As Object, e As EventArgs) Handles closeApp.Click
        Close()
    End Sub

    Sub ChangeIcon()
        fichier = TextBox2.Text
        icone = TextBox4.Text
        IconInjector.InjectIcon(fichier, icone)
        MsgBox("Icon succesfully changed.", MsgBoxStyle.Information + MsgBoxStyle.OkOnly, "Information")
    End Sub

    Private Sub Panel1_MouseDown(sender As Object, e As MouseEventArgs) Handles Panel1.MouseDown
        draggable = True
        mouseX = Cursor.Position.X - Me.Left
        mouseY = Cursor.Position.Y - Me.Top
    End Sub

    Private Sub Panel1_MouseMove(sender As Object, e As MouseEventArgs) Handles Panel1.MouseMove
        If draggable Then
            Me.Top = Cursor.Position.Y - mouseY
            Me.Left = Cursor.Position.X - mouseX
        End If
    End Sub

    Private Sub Panel1_MouseUp(sender As Object, e As MouseEventArgs) Handles Panel1.MouseUp
        draggable = False
    End Sub

    Private Sub Label6_MouseDown(sender As Object, e As MouseEventArgs) Handles Label6.MouseDown
        draggable = True
        mouseX = Cursor.Position.X - Me.Left
        mouseY = Cursor.Position.Y - Me.Top
    End Sub

    Private Sub Label6_MouseMove(sender As Object, e As MouseEventArgs) Handles Label6.MouseMove
        If draggable Then
            Me.Top = Cursor.Position.Y - mouseY
            Me.Left = Cursor.Position.X - mouseX
        End If
    End Sub

    Private Sub Label6_MouseUp(sender As Object, e As MouseEventArgs) Handles Label6.MouseUp
        draggable = False
    End Sub

    Private Sub PictureBox2_MouseDown(sender As Object, e As MouseEventArgs) Handles PictureBox2.MouseDown
        draggable = True
        mouseX = Cursor.Position.X - Me.Left
        mouseY = Cursor.Position.Y - Me.Top
    End Sub

    Private Sub PictureBox2_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox2.MouseMove
        If draggable Then
            Me.Top = Cursor.Position.Y - mouseY
            Me.Left = Cursor.Position.X - mouseX
        End If
    End Sub

    Private Sub KryptonLinkLabel1_LinkClicked(sender As Object, e As EventArgs) Handles KryptonLinkLabel1.LinkClicked
        Readme.Show()
    End Sub

    Private Sub PictureBox2_MouseUp(sender As Object, e As MouseEventArgs) Handles PictureBox2.MouseUp
        draggable = False
    End Sub
End Class

