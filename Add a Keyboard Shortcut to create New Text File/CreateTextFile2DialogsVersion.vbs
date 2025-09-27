Set shell = CreateObject("Shell.Application")
Set folder = shell.Windows.Item(shell.Windows.Count - 1)

If Not folder Is Nothing Then
    path = folder.Document.Folder.Self.Path
    Set fso = CreateObject("Scripting.FileSystemObject")
    Set wshShell = CreateObject("WScript.Shell")

    ' Ask the user for a filename
    userInput = InputBox("Enter file name (without extension):", "Create New Text File", "NewTextFile")

    If Trim(userInput) = "" Then
        filename = "NewTextFile.txt"
    Else
        If LCase(Right(userInput, 4)) <> ".txt" Then
            filename = userInput & ".txt"
        Else
            filename = userInput
        End If
    End If

    ' Avoid overwriting existing files
    i = 1
    baseName = filename
    Do While fso.FileExists(path & "\" & filename)
        nameOnly = Left(baseName, Len(baseName) - 4)
        filename = nameOnly & "(" & i & ").txt"
        i = i + 1
    Loop

    ' Create the new file
    On Error Resume Next
    fso.CreateTextFile path & "\" & filename, True

    If Err.Number <> 0 Then
        MsgBox "Error creating file: " & Err.Description, vbCritical, "Error"
    Else
        ' Ask the user if they want to open the file
        answer = MsgBox("File '" & filename & "' created successfully." & vbCrLf & vbCrLf & _
                        "Do you want to open and edit it now?", vbYesNo + vbQuestion, "Open File?")

        If answer = vbYes Then
            wshShell.Run "notepad.exe """ & path & "\" & filename & """"
        End If
    End If

    On Error GoTo 0
End If

' Source : https://github.com/Michael-Matta1