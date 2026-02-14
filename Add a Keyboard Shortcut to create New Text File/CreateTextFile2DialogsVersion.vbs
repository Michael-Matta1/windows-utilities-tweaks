' Source: https://github.com/Michael-Matta1/windows-utilities-tweaks

Set shell = CreateObject("Shell.Application")
Set fso = CreateObject("Scripting.FileSystemObject")
Set wshShell = CreateObject("WScript.Shell")

' ---- Step 1: Get the active Explorer tab's path via silent PS1 helper ----
' The PS1 uses UIAutomation to query which tab is selected in Explorer,
' then matches it against Shell.Application.Windows entries.
' It runs completely hidden — no visible UI changes.
strScriptPath = Left(WScript.ScriptFullName, InStrRev(WScript.ScriptFullName, "\"))
strPs1 = strScriptPath & "GetActiveExplorerPath.ps1"
strTempFile = fso.GetSpecialFolder(2).Path & "\~explorer_path.tmp"

' Clean up any leftover temp file
If fso.FileExists(strTempFile) Then fso.DeleteFile strTempFile

path = ""

' Only use PS1 if the helper script exists
If fso.FileExists(strPs1) Then
    ' Run PowerShell silently (hidden window, wait for completion)
    strCommand = "powershell.exe -WindowStyle Hidden -ExecutionPolicy Bypass -File """ & strPs1 & """ """ & strTempFile & """"
    wshShell.Run strCommand, 0, True

    ' Read the detected path from temp file
    If fso.FileExists(strTempFile) Then
        Set tf = fso.OpenTextFile(strTempFile, 1)
        On Error Resume Next
        path = Trim(tf.ReadAll())
        If Err.Number <> 0 Then path = ""
        Err.Clear
        On Error GoTo 0
        tf.Close
        fso.DeleteFile strTempFile
    End If
End If

' Initialize activeFolder variable
Set activeFolder = Nothing

' ---- Step 2: Use the detected path to find the matching Shell window object ----
If path <> "" And fso.FolderExists(path) Then
    For Each objWindow In shell.Windows
        On Error Resume Next
        windowType = TypeName(objWindow.Document)
        If windowType = "IShellFolderViewDual" Or windowType = "IShellFolderViewDual3" Then
            testPath = objWindow.Document.Folder.Self.Path
            ' Find any window that is showing this path (since we know it is the active one from PS1)
            If Err.Number = 0 And LCase(testPath) = LCase(path) And objWindow.Visible Then
                Set activeFolder = objWindow
                Exit For
            End If
        End If
        Err.Clear
        On Error GoTo 0
    Next
End If

' ---- Step 3: Fallback if PS1 failed or didn't return a valid path ----
If activeFolder Is Nothing Then
    ' Fallback: Get the topmost Explorer window (original logic)
    maxRetries = 3
    retryCount = 0

    Do While activeFolder Is Nothing And retryCount < maxRetries
        For Each objWindow In shell.Windows
            On Error Resume Next
            windowType = TypeName(objWindow.Document)

            If windowType = "IShellFolderViewDual" Or windowType = "IShellFolderViewDual3" Then
                testPath = objWindow.Document.Folder.Self.Path
                If Err.Number = 0 And testPath <> "" And objWindow.Visible Then
                    Set activeFolder = objWindow
                End If
            End If
            Err.Clear
            On Error GoTo 0
        Next

        If activeFolder Is Nothing Then
            retryCount = retryCount + 1
            If retryCount < maxRetries Then WScript.Sleep 100
        End If
    Loop
End If

If activeFolder Is Nothing Then
    MsgBox "No active File Explorer window found!" & vbCrLf & vbCrLf & "Please open a File Explorer window and try again.", vbExclamation, "Error"
    WScript.Quit
End If

' Update path from activeFolder if it wasn't set by PS1
If path = "" Then
    On Error Resume Next
    path = activeFolder.Document.Folder.Self.Path
    If Err.Number <> 0 Or path = "" Then
        MsgBox "Unable to determine the current folder path." & vbCrLf & vbCrLf & "Please make sure you're in a valid folder location.", vbCritical, "Error"
        WScript.Quit
    End If
    On Error GoTo 0
End If

' ---- Step 4: File Creation Logic (2 Dialogs Variant) ----

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
    If Not activeFolder Is Nothing Then activeFolder.Refresh

    ' Ask the user if they want to open the file
    answer = MsgBox("File '" & filename & "' created successfully." & vbCrLf & vbCrLf & _
                    "Do you want to open and edit it now?", vbYesNo + vbQuestion, "Open File?")

    If answer = vbYes Then
        wshShell.Run "notepad.exe """ & path & "\" & filename & """"
    End If
End If

On Error GoTo 0
