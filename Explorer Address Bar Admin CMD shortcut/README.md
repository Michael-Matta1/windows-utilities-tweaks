# Open Admin Command Prompt from File Explorer Address Bar

A simple method to open an **elevated Command Prompt as Administrator** directly from File Explorer's address bar, starting in the current directory.

## 🎯 What This Does

- Type `acmd` in any File Explorer address bar
- Opens Command Prompt **as Administrator** in that exact folder
- No more navigating from `C:\Windows\System32` to your target directory
- Works from any location in File Explorer

## 📋 Background

Normally, when you type `cmd` in the File Explorer address bar, it opens a **normal Command Prompt** in the current directory (not as Administrator).  
This guide shows you how to create a custom address bar shortcut (`acmd`) that opens **Command Prompt as Administrator** in the current directory.

## 🚀 Quick Setup

### Step 1: Create the Batch File

1. **Open Notepad as Administrator**
   - Right-click Notepad → "Run as administrator"

2. **Copy and paste this code:**
   ```bat
   @echo off
   powershell -Command "Start-Process cmd -ArgumentList '/k','cd /d %CD%' -Verb RunAs"
   ```

3. **Save the file:**
   - File → Save As
   - **Location:** `C:\Windows\`
   - **Filename:** `acmd.bat`
   - **Save as type:** All Files

### Step 2: Test It

1. Open **File Explorer**
2. Navigate to any folder (e.g., your Documents, Desktop, or project folder)
3. Click in the **address bar** and type: `acmd`
4. Press **Enter**
5. Click **Yes** when UAC prompt appears

✅ **Result:** Command Prompt opens as Administrator in your current directory!

## 🔧 How It Works

| Component | Purpose |
|-----------|---------|
| `@echo off` | Hides batch file commands from displaying |
| `%CD%` | Expands to current directory when command is run |
| `cd /d %CD%` | Changes directory in the elevated Command Prompt |
| `-Verb RunAs` | Tells PowerShell to run CMD as Administrator |
| `/k` | Keeps Command Prompt window open after running command |

The batch file is placed in `C:\Windows\` because this directory is in your system's PATH, making the command available from anywhere.

## 🔄 Additional Variations

### Admin PowerShell
Create `aps.bat` with this content:
```bat
@echo off
powershell -Command "Start-Process powershell -ArgumentList '-NoExit','-Command','Set-Location ''%CD%''' -Verb RunAs"
```

**Usage:** Type `aps` in Explorer address bar

### Admin Windows Terminal
Create `atr.bat` with this content:
```bat
@echo off
powershell -Command "Start-Process wt -ArgumentList '-d','\"%CD%\"' -Verb RunAs"
```

**Usage:** Type `atr` in Explorer address bar

## 📝 Command Reference

| Command | Opens |
|---------|-------|
| `cmd` | Normal Command Prompt (built-in Windows) |
| `acmd` | **Admin Command Prompt in current folder** |
| `aps` | Admin PowerShell in current folder |
| `atr` | Admin Windows Terminal in current folder |

## ⚠️ Important Notes

1. **PATH Requirement:** The batch file must be in a directory that's in your system PATH (like `C:\Windows\`)
2. **UAC Prompt:** You'll see a User Account Control prompt each time - click "Yes" to proceed
3. **Administrator Rights:** You need admin privileges to create files in `C:\Windows\`
4. **Antivirus:** Some antivirus software might flag batch files in system directories

## 🛠️ Troubleshooting

### "Command not found" Error
- Ensure the `.bat` file is saved in `C:\Windows\` or another PATH directory
- Restart File Explorer or reboot if needed

### UAC Prompt Doesn't Appear
- Check if your account has administrator privileges
- Try running the batch file directly from `C:\Windows\` first

### Wrong Directory Opens
- The batch file uses `%CD%` which captures the current directory when typed
- Make sure you're typing the command in the address bar