Set shell = CreateObject("Shell.Application")
Set folder = shell.Windows.Item(shell.Windows.Count - 1)

If Not folder Is Nothing Then
    path = folder.Document.Folder.Self.Path
    Set fso = CreateObject("Scripting.FileSystemObject")
    Set wshShell = CreateObject("WScript.Shell")

    ' Ask the user for filename and open option in one dialog
    userInput = InputBox("Enter file name (without extension):" & vbCrLf & vbCrLf & _
                        "Add a space at the end if you want to open the file after creation" & vbCrLf & _
                        "Example: MyFile ", "Create New Text File", "NewTextFile")

    If userInput = "" Then
        WScript.Quit
    End If

    ' Check if user wants to open the file (trailing space)
    openFile = False
    If Right(userInput, 1) = " " Then
        openFile = True
        userInput = RTrim(userInput)
    End If

    ' Process filename
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

    ' Create the file
    On Error Resume Next
    fso.CreateTextFile path & "\" & filename, True
    
    If Err.Number = 0 Then
        ' Only open if user added /open
        If openFile Then
            wshShell.Run "notepad.exe """ & path & "\" & filename & """"
        End If
    Else
        MsgBox "Error creating file: " & Err.Description, vbCritical, "Error"
    End If
    On Error GoTo 0
End If

' Source : https://github.com/Michael-Matta1