# Add "Run with PowerShell (Admin)" to .ps1 Right-Click Context Menu

A clear, step-by-step guide to add a right-click context menu entry that runs `.ps1` PowerShell scripts elevated (as Administrator). This README explains what was done, why it works, exact commands you can copy-paste, how to test, and how to remove the change.

## 🚀 Summary

This guide creates a context menu item that appears when you right-click a `.ps1` file: **Run with PowerShell (Admin)**. The method attaches a menu verb under the wildcard `*` file class and uses the `AppliesTo` registry value so the item only shows for `.ps1` files. To avoid slow or hanging behavior caused by PowerShell's registry provider interpreting `*` as a wildcard, the method writes registry keys using the .NET `Microsoft.Win32.Registry` API.

Two installation options are provided:
- **Machine-wide (recommended)** — writes to `HKEY_CLASSES_ROOT` (requires Administrator). Applies to all users.
- **Per-user (safer)** — writes to `HKEY_CURRENT_USER\Software\Classes` (no Administrator required). Applies to the current user only.


## 📋 Requirements

- Windows (tested on Windows 10 / Windows 11)
- PowerShell (built-in Windows PowerShell or PowerShell Core)
- For machine-wide changes: run PowerShell **as Administrator**

## 🔍 What the Registry Entries Do

- `HKCR:\*\shell\RunWithPowerShellAdmin` — creates a menu entry under the generic file class `*`. This attaches the menu item to all files, but we filter it.
- `AppliesTo` — a shell property query telling Explorer to show the item only when the file extension matches `.ps1`. We set `AppliesTo = System.FileExtension:=".ps1"`.
- `HasLUAShield` — tells Explorer to show the UAC (User Account Control) shield icon next to the menu item.
- `command` subkey default value — the exact command that runs when the menu item is clicked. It uses `Start-Process -Verb RunAs` to launch a new elevated PowerShell process that runs the selected script.

## 🖥️ Machine-wide Installation (Recommended)

**⚡ Fast, reliable — run PowerShell as Administrator**

1. Open PowerShell **as Administrator** (right-click → Run as administrator).
2. Paste and run the following block exactly (it uses the .NET Registry API and will complete quickly):

```powershell
# === Create machine-wide context menu ===
$root = [Microsoft.Win32.Registry]::ClassesRoot

# Create base key *\shell\RunWithPowerShellAdmin
$base = $root.CreateSubKey('*\shell\RunWithPowerShellAdmin')
$base.SetValue('', 'Run with PowerShell (Admin)')     # default value (menu text)
$base.SetValue('HasLUAShield', '')                   # show UAC shield
$base.SetValue('AppliesTo', 'System.FileExtension:=".ps1"')  # only for .ps1 files

# Create command subkey and set command
$cmd = $base.CreateSubKey('command')
$cmd.SetValue('', 'powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "Start-Process -Verb RunAs -FilePath ''powershell.exe'' -ArgumentList ''-NoProfile -ExecutionPolicy Bypass -File \"%1\"''"')

# Close handles
$cmd.Close()
$base.Close()

'Done. Created HKCR:\*\shell\RunWithPowerShellAdmin (AppliesTo .ps1). Restart Explorer if needed.'
```

3. Restart Explorer so the change takes effect immediately (optional but recommended):

```powershell
Stop-Process -Name explorer -Force
# Explorer should restart automatically. If it does not, run: Start-Process explorer.exe
```

4. Right-click any `.ps1` file and choose **Run with PowerShell (Admin)**. If your system uses the compact menu, the entry will appear directly; if Windows is using the compact menu it may appear under **Show more options** unless you previously configured Explorer to always show the full classic menu.

## 👤 Per-user Installation (No Admin Required)

Run these commands in a normal PowerShell window (no elevation required):

```powershell
$root = [Microsoft.Win32.Registry]::CurrentUser
$base = $root.CreateSubKey('Software\Classes\*\shell\RunWithPowerShellAdmin')
$base.SetValue('', 'Run with PowerShell (Admin)')
$base.SetValue('HasLUAShield', '')
$base.SetValue('AppliesTo', 'System.FileExtension:=".ps1"')
$cmd = $base.CreateSubKey('command')
$cmd.SetValue('', 'powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "Start-Process -Verb RunAs -FilePath ''powershell.exe'' -ArgumentList ''-NoProfile -ExecutionPolicy Bypass -File \"%1\"''"')
$cmd.Close(); $base.Close()
'Done. Created per-user HKCU entry.'
```

Log off and back on (or restart Explorer) if the new item does not show immediately.

## ✅ How to Verify the Registry Entries

Run these checks in PowerShell (elevated for HKCR checks):

- Check the base key and default menu text:

```powershell
$key = [Microsoft.Win32.Registry]::ClassesRoot.OpenSubKey('*\shell\RunWithPowerShellAdmin')
if ($null -eq $key) { 'No key found' } else { "Found key. Default value: $($key.GetValue(''))" }
```

- Check whether the `command` subkey exists and read the command string:

```powershell
$cmd = [Microsoft.Win32.Registry]::ClassesRoot.OpenSubKey('*\shell\RunWithPowerShellAdmin\command')
if ($null -eq $cmd) { 'No command subkey found' } else { "Command value: $($cmd.GetValue(''))" }
```

For per-user keys use `CurrentUser` instead of `ClassesRoot` and the path `Software\Classes\*\shell\RunWithPowerShellAdmin`.

## 🧪 Manual Command Test

Confirm the Start-Process call works:

1. Create a test script `C:\Temp\test.ps1` with the single line:

```powershell
Start-Process notepad
```

2. From any PowerShell prompt run the exact command the context menu will invoke (this forces a UAC prompt and runs the script elevated):

```powershell
Start-Process -Verb RunAs -FilePath 'powershell.exe' -ArgumentList '-NoProfile -ExecutionPolicy Bypass -File "C:\Temp\test.ps1"'
```

After approving the UAC prompt, Notepad should open. If that works but the context menu still doesn't show, proceed to Troubleshooting below.

## 🗑️ How to Remove the Menu (Rollback)

Run this command block to remove both machine-wide and per-user entries:

```powershell
# Remove from HKEY_CLASSES_ROOT (machine-wide)
$root = [Microsoft.Win32.Registry]::ClassesRoot
try {
    $root.DeleteSubKeyTree('*\shell\RunWithPowerShellAdmin', $false)
    'Key removed from HKCR.'
} catch {
    "Nothing to delete in HKCR: $_"
}

# Remove from HKEY_CURRENT_USER (per-user)
$root = [Microsoft.Win32.Registry]::CurrentUser
try {
    $root.DeleteSubKeyTree('Software\Classes\*\shell\RunWithPowerShellAdmin', $false)
    'Key removed from HKCU.'
} catch {
    "Nothing to delete in HKCU: $_"
}
```

Restart Explorer or log off/on after removal.

## 📝 Note

### Windows 11 compact vs classic context menu

If Explorer is using the compact context menu, classic items may be under **Show more options**. If already configured Explorer to show the full classic menu, this method will appear directly in that case.
