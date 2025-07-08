Set shell = CreateObject("Shell.Application")
Set folder = shell.Windows.Item(shell.Windows.Count - 1)

If Not folder Is Nothing Then
    path = folder.Document.Folder.Self.Path
    Set fso = CreateObject("Scripting.FileSystemObject")

    filename = "NewTextFile.txt"
    i = 1
    Do While fso.FileExists(path & "\" & filename)
        filename = "NewTextFile(" & i & ").txt"
        i = i + 1
    Loop

    On Error Resume Next
    fso.CreateTextFile path & "\" & filename, True
    On Error GoTo 0
End If