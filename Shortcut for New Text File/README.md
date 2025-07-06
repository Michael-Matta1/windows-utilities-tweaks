# 📄 Create New Text File with `Ctrl + Alt + N` 

This project lets you create a new `.txt` file in the **currently open File Explorer folder** by pressing `Ctrl + Alt + N`, just like `Ctrl + Shift + N` creates a new folder.


---

## Features

- Creates `NewTextFile.txt` in the active File Explorer window
- Automatically avoids overwriting by appending numbers: `NewTextFile(1).txt`, `NewTextFile(2).txt`, etc.
- Uses **VBScript**, no extra software required
- Can optionally open the file in Notepad automatically (see below)

---

## 🛠 How to Set Up

### 1. Create the Script

1. Open Notepad.
2. Paste the code below:

```vbscript
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
````

3. Save it as:

   ```
   CreateTextFile.vbs
   ```

---

### 2. Create a Shortcut

1. Right-click the `.vbs` file → **Create shortcut**.
2. Rename the shortcut if desired (e.g. `New Text File Shortcut`).
3. Right-click the shortcut → **Properties**.
4. Go to the **Shortcut** tab.
5. In the **Shortcut key** field, press `Ctrl + Alt + N`.
6. Click **OK**.

---

### 3. Move the Shortcut to a Valid Location

To ensure the hotkey works, move the shortcut to:

* Your **Desktop**, or
* Your **Start Menu** folder:

  * Press `Win + R`, type:

    ```
    shell:Start Menu\Programs
    ```
  * Move the shortcut into this folder.

---

### ✅ You're Done!

Now, press `Ctrl + Alt + N` while a File Explorer window is open.
A new text file will appear in the current folder.

**Note: restarting windows explorer is required (If didn't work restart your PC)**

---

## 🔧 Optional: Auto-Open the File in Notepad

To open the file automatically after creation, change this line:

```vbscript
fso.CreateTextFile path & "\" & filename, True
```

to:

```vbscript
fso.CreateTextFile path & "\" & filename, True
CreateObject("WScript.Shell").Run "notepad.exe """ & path & "\" & filename & """"
```

---

## 🧠 Note
* You can modify the filename or location easily in the script.

