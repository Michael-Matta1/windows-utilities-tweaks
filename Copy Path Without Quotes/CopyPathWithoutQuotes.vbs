Set objShell = CreateObject("WScript.Shell")
Set objFSO = CreateObject("Scripting.FileSystemObject")

Dim strPath
strPath = ""

' Iterate over all arguments and get the absolute path
For i = 0 To WScript.Arguments.Count - 1
    strPath = strPath & objFSO.GetAbsolutePathName(WScript.Arguments(i)) & vbCrLf
Next

' Remove the trailing newline character
If Right(strPath, 2) = vbCrLf Then
    strPath = Left(strPath, Len(strPath) - 2)
End If

' Save the path to a temporary file
tempFile = objFSO.GetSpecialFolder(2) & "\temp_clipboard.txt"
Set tempFileStream = objFSO.CreateTextFile(tempFile, True)
tempFileStream.Write strPath
tempFileStream.Close

' Use PowerShell to copy the content of the temporary file to the clipboard
objShell.Run "powershell.exe -Command ""Get-Content -Path '" & tempFile & "' | Set-Clipboard""", 0, True

' Delete the temporary file
objFSO.DeleteFile(tempFile)
