' CreateMarkdownSilent.vbs
' Silent markdown file creation script
' Usage: wscript.exe CreateMarkdownSilent.vbs "C:\target\directory"

Dim objShell, objFSO, targetDir, i, fileName

' Get the target directory from command line argument
If WScript.Arguments.Count > 0 Then
    targetDir = WScript.Arguments(0)
Else
    targetDir = CreateObject("WScript.Shell").CurrentDirectory
End If

Set objFSO = CreateObject("Scripting.FileSystemObject")

' Ensure target directory exists
If Not objFSO.FolderExists(targetDir) Then
    WScript.Quit
End If

' Try to create NewMarkdown.md first, then numbered versions
For i = 0 To 100
    If i = 0 Then
        fileName = targetDir & "\NewMarkdown.md"
    Else
        fileName = targetDir & "\NewMarkdown(" & i & ").md"
    End If
    
    ' Check if file doesn't exist, then create it
    If Not objFSO.FileExists(fileName) Then
        Dim objFile
        Set objFile = objFSO.CreateTextFile(fileName, True)
        objFile.Close
        Set objFile = Nothing
        Exit For
    End If
Next

Set objFSO = Nothing