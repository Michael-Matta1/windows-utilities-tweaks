# 📄 Create New Text File with `Ctrl + Alt + N` 

This project lets you create a new `.txt` file in the **currently open File Explorer folder** by pressing `Ctrl + Alt + N`, just like `Ctrl + Shift + N` creates a new folder.

---

## 📁 Files Included

This repository contains several versions of the script to suit different needs:

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


### Multi-Extension CreateTextFile.vbs (Final Version – Most Powerful)
- Supports multiple custom file extensions (e.g., `.md`, `.js`, `.py`)  
- [Jump to full details ⬇️](#multi-extension-createtextfilevbs-final-version--most-powerful)
---

## Features

- Creates `.txt` files in the active File Explorer window
- Automatically avoids overwriting by appending numbers: `NewTextFile(1).txt`, `NewTextFile(2).txt`, etc.
- Uses **VBScript**, no extra software required
- Multiple versions available for different user preferences
- Lightweight and fast execution

---

## 🛠 How to Set Up

### 1. Choose Your Version

Pick the `.vbs` file that matches your preferred workflow:
- **Basic users**: `CreateTextFileNeverOpen.vbs`
- **Always edit immediately**: `CreateTextFileAlwaysOpen.vbs`
- **Want naming control**: `CreateTextFile2DialogsVersion.vbs`
- **Power users**: `CreateTextFileSpaceVersion.vbs`

### 2. Create a Shortcut

1. Right-click your chosen `.vbs` file → **Create shortcut**.
2. Rename the shortcut if desired (e.g. `New Text File Shortcut`).
3. Right-click the shortcut → **Properties**.
4. Go to the **Shortcut** tab.
5. In the **Shortcut key** field, press `Ctrl + Alt + N`.
6. Click **OK**.

### 3. Move the Shortcut to a Valid Location

To ensure the hotkey works, move the shortcut to:

* Your **Desktop**, or
* Your **Start Menu** folder:
  * Press `Win + R`, type: `shell:Start Menu\Programs`
  * Move the shortcut into this folder.

---

### ✅ You're Done!

Now, press `Ctrl + Alt + N` while a File Explorer window is open.
A new text file will appear in the current folder.

**Note: Restarting Windows Explorer is required (If it doesn't work, restart your PC)**

---

## 🔧 Version Comparison

| Version | User Input | Auto-Open | Best For |
|---------|------------|-----------|----------|
| **NeverOpen** | None | Never | Quick file creation |
| **AlwaysOpen** | None | Always | Immediate editing |
| **2Dialogs** | Filename + Yes/No | User choice | Full control |
| **SpaceVersion** | Filename + space trick | User choice | One-dialog convenience |


---

## 🧠 Notes

* All versions handle duplicate filenames automatically
* You can modify the default filename or location in any script
* The hotkey can be changed in the shortcut properties
* Scripts work with any Windows version that supports VBScript


---

## 🆕 Multi-Extension CreateTextFile.vbs (Final Version – Most Powerful)

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
