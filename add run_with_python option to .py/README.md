# Python Context Menu - Run with Python

A Windows Registry file that adds a "Run with Python" option to the right-click context menu for `.py` files.

## Purpose

This registry modification allows you to:
- Set your preferred code editor as the default program for opening `.py` files
- Keep a quick "Run with Python" option in the context menu for immediate script execution
- Execute Python scripts directly from File Explorer without changing your default file association or opening CLI

## What it does

The registry file adds a context menu entry that appears when you right-click on any `.py` file in Windows Explorer. When selected, it opens a Command Prompt window and runs the Python script using the `py.exe` launcher.

## Installation

1. **Download** the `run_with_python.reg` file
2. **Double-click** the file to run it
3. **Click "Yes"** when Windows asks for permission to modify the registry
4. **Click "OK"** to confirm the changes

## Usage

1. Right-click on any `.py` file in Windows Explorer
2. Select **"Run with Python"** from the context menu
3. A Command Prompt window will open and execute your script
4. The window will remain open (`/k` flag) so you can see the output

## Technical Details

The registry modification:
- Creates a new shell command under `HKEY_CLASSES_ROOT\SystemFileAssociations\.py\shell\`
- Uses `cmd.exe /k` to keep the command window open after execution
- Utilizes `py.exe` (Python Launcher) to automatically detect and use the appropriate Python version

## Command Breakdown

```
cmd.exe /k "py.exe \"%1\""
```

- `cmd.exe` - Opens Command Prompt
- `/k` - Keeps the window open after command execution
- `py.exe` - Python Launcher (automatically selects Python version)
- `\"%1\"` - The selected file path (quoted to handle spaces in filenames)

## Uninstalling

To remove this context menu option:

1. Open Registry Editor (`regedit.exe`)
2. Navigate to `HKEY_CLASSES_ROOT\SystemFileAssociations\.py\shell\`
3. Delete the `runwithpython` folder
4. Close Registry Editor

## Safety Note

Always backup your registry before making modifications. This script only adds entries and doesn't modify existing associations, making it relatively safe.
