# 📄 Create New Text File with `Ctrl + Alt + N`

This guide lets you create a new `.txt` file in the **currently open File Explorer folder** by pressing `Ctrl + Alt + N` (or any other keyboard shortcut you assign), just like `Ctrl + Shift + N` creates a new folder.

---

## 📁 Files Included

This repository contains several versions of the script to suit different needs:

### **CreateTextFile.vbs Scripts** (Now with Windows 11 Tabs Support!)

This collection contains several versions of a script to create a new text file in the currently active File Explorer folder via a keyboard shortcut (`Ctrl + Alt + N` recommended).

**New Feature**: All scripts now reliably support **Windows 11 Tabs** and multiple Explorer windows using a robust PowerShell helper.

### ⚠️ Important Requirement
All `.vbs` scripts **require** `GetActiveExplorerPath.ps1` to be in the same folder.
If you move a script, you must move the `.ps1` file with it.

---
## Features

- Creates `.txt` files in the active File Explorer window
- Automatically avoids overwriting by appending numbers: `NewTextFile(1).txt`, `NewTextFile(2).txt`, etc.
- Uses **VBScript**, no extra software required
- Multiple versions available for different user preferences
- Lightweight and fast execution


## 🧠 Notes

* All versions handle duplicate filenames automatically
* You can modify the default filename or location in any script
* The hotkey can be changed in the shortcut properties
* Scripts work with any Windows version that supports VBScript



---

### 📂 Included Versions

### **CreateTextFileNeverOpen.vbs** (Basic Version)
- Creates `NewTextFile.txt` without any user prompts
- Never opens the file automatically
- Simplest version - just creates the file and exits
- Good for users who prefer minimal interaction

### **CreateTextFileAlwaysOpen.vbs** (Auto-Open Version)
- Creates `NewTextFile.txt` without prompting for filename
- Always opens the file in Notepad automatically
- Perfect for users who always want to edit the file immediately

### **CreateTextFile2DialogsVersion.vbs** (Two-Dialog Version)
- First dialog: asks for filename
- Second dialog: asks if you want to open the file
- Shows success message with filename
- Most user-friendly with clear feedback

### **CreateTextFileSpaceVersion.vbs** (Single Dialog Version)
- Single dialog asks for filename
- Add a space at the end of filename to auto-open the file
- Example: `MyFile ` (with space) opens the file, `MyFile` doesn't
- The name of the created file won't have space at the end.
- A workaround to combine both options in one dialog

#### **MultiExtension-CreateTextFile.vbs** (Recommended)
- Supports custom extensions (e.g., `script.js`, `page.html`).
- Smart handling: `name` -> `.txt`, `name.md` -> `.md`.
- Add a space at the end to auto-open the file.


---

### 🛠 Installation

#### Setp 1: Create the shortcut
1.  **Download** all files in this folder (or at least the Script you want + `GetActiveExplorerPath.ps1`).
2.  **Move** them to a permanent location (e.g., `C:\Scripts\`).
3.  **Right-click** your chosen `.vbs` file -> **Create shortcut**.
4.  **Right-click** the shortcut -> **Properties**.
5.  **Shortcut key**: Press `Ctrl + Alt + N` (or your preferred key).
6.  **Click OK**.

### Setp 2: Move the Shortcut to a Valid Location

To ensure the hotkey works, move the shortcut to:

* Your **Desktop**, or
* Your **Start Menu** folder **(Recommended)**:
  * Press `Win + R`, type: `shell:Start Menu\Programs`
  * Move the shortcut into this folder.

### Setp 3: Restart

**Restart Windows Explorer process via task manager**: or restart your PC .

If the shortcut is not working:
1.  **Right-click** on the shortcut -> **properties**
2.  In the target field, type:
    ```
    wscript.exe "C:\Path\To\Your\Script.vbs"
    ```
    *(Replace `C:\Path\To\Your\Script.vbs` with the actual path to your chosen `.vbs` file)*
3.  Click **OK**


### Important note:

- The helper runs via `powershell -ExecutionPolicy Bypass`, so it should work on most systems.

- if it fails to work in your system run these two commands in PowerShell as Administrator:
```powershell
Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope CurrentUser
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope LocalMachine
```

---

## 🔧 Version Comparison

| Version | User Input | Auto-Open | Best For |
|---------|------------|-----------|----------|
| **NeverOpen** | None | Never | Quick file creation |
| **AlwaysOpen** | None | Always | Immediate editing |
| **2Dialogs** | Filename + Yes/No | User choice | Full control |
| **SpaceVersion** | Filename + space trick | User choice | One-dialog convenience |


---



### Multi-Extension CreateTextFile.vbs (Final Version - Most Powerful)

- **Custom file extensions**: `.md`, `.js`, `.py`, `.html`, `.css`, `.json`, etc.
- **Smart extension detection**
  - `filename` → `filename.txt`
  - `filename.md` → `filename.md`
  - `filename.md ` → `filename.md` and opens it
- **Improved duplicate handling**: `filename(1).md`, `filename(2).md`, ...
- Updated dialog titled **"Create New File"**

### 📋 Examples
| Input | Result | Opens? |
|-------|--------|--------|
| `test` | `test.txt` | No |
| `test ` | `test.txt` | Yes |
| `notes.md` | `notes.md` | No |
| `notes.md ` | `notes.md` | Yes |
| `script.js ` | `script.js` | Yes |

### ✅ Key Features Preserved
- **Space functionality** – Add a space to auto‑open
- **Duplicate handling** – Numbered duplicates
- **Error handling** – Same validations
- **Dialogs** – Same user experience with clearer wording
