Set shell = CreateObject("Shell.Application")
Set folder = shell.Windows.Item(shell.Windows.Count - 1)

If Not folder Is Nothing Then
    path = folder.Document.Folder.Self.Path
    Set fso = CreateObject("Scripting.FileSystemObject")
    Set wshShell = CreateObject("WScript.Shell")
    
    ' Ask the user for filename and open option in one dialog
    userInput = InputBox("Enter file name (with or without extension):" & vbCrLf & vbCrLf & _
                        "Add a space at the end if you want to open the file after creation" & vbCrLf & _
                        "Examples: MyFile, test.md, script.js ", "Create New File", "NewTextFile")
    
    If userInput = "" Then
        WScript.Quit
    End If
    
    ' Check if user wants to open the file (trailing space)
    openFile = False
    If Right(userInput, 1) = " " Then
        openFile = True
        userInput = RTrim(userInput)
    End If
    
    ' Process filename - check if user provided extension
    If Trim(userInput) = "" Then
        filename = "NewTextFile.txt"
        fileExtension = ".txt"
    Else
        ' Check if filename contains a dot (indicating extension)
        If InStr(userInput, ".") > 0 Then
            ' User provided extension - use as is
            filename = userInput
            ' Extract extension for duplicate handling
            dotPos = InStrRev(filename, ".")
            fileExtension = Mid(filename, dotPos)
        Else
            ' No extension provided - add .txt
            filename = userInput & ".txt"
            fileExtension = ".txt"
        End If
    End If
    
    ' Avoid overwriting existing files
    i = 1
    baseName = filename
    Do While fso.FileExists(path & "\" & filename)
        ' Extract name without extension for numbering
        dotPos = InStrRev(baseName, ".")
        If dotPos > 0 Then
            nameOnly = Left(baseName, dotPos - 1)
            filename = nameOnly & "(" & i & ")" & fileExtension
        Else
            filename = baseName & "(" & i & ")"
        End If
        i = i + 1
    Loop
    
    ' Create the file
    On Error Resume Next
    fso.CreateTextFile path & "\" & filename, True
    
    If Err.Number = 0 Then
        ' Only open if user added space
        If openFile Then
            wshShell.Run "notepad.exe """ & path & "\" & filename & """"
        End If
    Else
        MsgBox "Error creating file: " & Err.Description, vbCritical, "Error"
    End If
    On Error GoTo 0
End If

' Source : https://github.com/Michael-Matta1