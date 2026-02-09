# Windows 11 Right Alt + Shift Language Toggle Fix

A solution for the Windows 11 bug where Right Alt + Shift only switches languages in one direction.

## Table of Contents

- [Problem Description](#problem-description)
- [Solution Overview](#solution-overview)
- [Installation](#installation)
  - [Method 1: Manual Command (Recommended)](#method-1-manual-command-recommended)
  - [Method 2: Registry File](#method-2-registry-file)
  - [Method 3: PowerShell Script](#method-3-powershell-script)
- [Reverting the Fix](#reverting-the-fix)
- [Technical Details](#technical-details)
- [Troubleshooting](#troubleshooting)
- [Alternative Workarounds](#alternative-workarounds)
- [FAQ](#faq)

## Problem Description

After Windows 11 update KB5070311 and subsequent updates, users with multiple keyboard layouts experience a critical bug where the Right Alt + Shift keyboard shortcut only switches languages in one direction:

- ✓ Primary Language → Second Language (works)
- ✗ Second Language → Primary Language (fails)

**Root Cause:** Windows incorrectly interprets the Right Alt key as AltGr (which sends Ctrl+Alt signals), creating a conflict with non-English keyboard layouts when attempting to switch back to the primary language.

**Note:** Left Alt + Shift continues to function correctly in both directions.

## Solution Overview

This fix remaps the Right Alt key to behave identically to the Left Alt key at the driver level, eliminating the AltGr interference. The solution modifies the Windows Scancode Map registry value to remap keyboard scancodes before Windows processes them.

### Requirements

- Windows 11 (affected by KB5070311 or later)
- Administrator privileges
- System restart after applying the fix

### Files Included

| File | Description |
|------|-------------|
| `fix-right-alt-scancode.reg` | Registry file to apply the fix |
| `revert-right-alt-scancode.reg` | Registry file to remove the fix |
| `fix-right-alt-scancode.ps1` | PowerShell script with automatic backup |
| `revert-right-alt-scancode.ps1` | PowerShell script to revert changes |

## Installation

Choose one of the following methods. All methods require **Administrator privileges** and a **full system restart**.

### Method 1: Manual Command (Recommended)

1. Open Command Prompt or PowerShell as Administrator:
   - Press `Win + X`
   - Select "Terminal (Admin)" or "Command Prompt (Admin)"

2. Execute the following command:

```cmd
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Keyboard Layout" /v "Scancode Map" /t REG_BINARY /d 000000000000000002000000380038E000000000 /f
```

3. Restart your computer

4. Test the Right Alt + Shift shortcut in both directions

### Method 2: Registry File

1. Download `fix-right-alt-scancode.reg`

2. Right-click the file and select "Run as Administrator" or "Merge"

3. Click "Yes" when prompted to confirm the registry modification

4. Restart your computer

### Method 3: PowerShell Script

1. Download `fix-right-alt-scancode.ps1`

2. Right-click PowerShell and select "Run as Administrator"

3. Navigate to the download location:
   ```powershell
   cd "C:\Path\To\Downloaded\Files"
   ```

4. Execute the script:
   ```powershell
   .\fix-right-alt-scancode.ps1
   ```

5. Follow the on-screen prompts

6. Restart your computer when prompted

**Note:** The PowerShell script automatically creates a backup of any existing Scancode Map configuration in `%USERPROFILE%\Desktop\KeyboardToggle_Backup\`.

## Reverting the Fix

If you need to restore the original keyboard behavior:

### Using Command Line

```cmd
reg delete "HKLM\SYSTEM\CurrentControlSet\Control\Keyboard Layout" /v "Scancode Map" /f
```

Then restart your computer.

### Using Registry File

1. Right-click `revert-right-alt-scancode.reg`
2. Select "Run as Administrator"
3. Restart your computer

### Using PowerShell Script

```powershell
.\revert-right-alt-scancode.ps1
```

Then restart your computer.

## Technical Details

### Registry Modification

The fix modifies the following registry location:

```
HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Keyboard Layout
```

**Value Name:** `Scancode Map`
**Type:** `REG_BINARY`
**Data:** `00 00 00 00 00 00 00 00 02 00 00 00 38 00 38 E0 00 00 00 00`

### Scancode Map Structure

The binary data follows this format:

| Offset | Value | Description |
|--------|-------|-------------|
| 0x00-0x03 | `00 00 00 00` | Header: Version (always 0) |
| 0x04-0x07 | `00 00 00 00` | Header: Flags (always 0) |
| 0x08-0x0B | `02 00 00 00` | Number of mappings + 1 (null terminator) |
| 0x0C-0x0F | `38 00 38 E0` | Mapping: E0 38 (Right Alt) → 00 38 (Left Alt) |
| 0x10-0x13 | `00 00 00 00` | Null terminator |

### How It Works

The Scancode Map operates at the kernel driver level, intercepting keyboard scancodes before Windows processes them. This ensures that:

1. Right Alt scancode (E0 38) is remapped to Left Alt scancode (00 38)
2. Both Alt keys produce identical signals
3. The AltGr conflict is completely eliminated
4. Language switching works bidirectionally with both Alt keys

### Why Right Alt Acts as AltGr

Many international keyboard layouts (particularly European layouts) use AltGr (Alt Graph) to access special characters (€, @, #, etc.). Windows implements AltGr as Ctrl+Alt, which conflicts with Alt+Shift when combined with certain keyboard layouts, preventing proper language switching.

## Troubleshooting

### Access Denied Error

**Cause:** Insufficient privileges
**Solution:** Ensure you are running the command/script/registry file as Administrator

### Fix Applied But Not Working

1. **Verify system restart:** Log out is insufficient; a full restart is required
2. **Check registry modification:**
   ```powershell
   Get-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\Keyboard Layout" -Name "Scancode Map"
   ```
3. **Perform complete shutdown:** Use `shutdown /s /t 0` instead of restart if issues persist

### Right Alt Key Not Working At All

**Solution:** Revert the fix using one of the methods described in [Reverting the Fix](#reverting-the-fix)

### PowerShell Script Execution Error

**Error:** "Execution of scripts is disabled"

**Solution:**
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

## Alternative Workarounds

If you prefer not to modify the registry:

1. **Use Left Alt + Shift:** Functions correctly without modifications
2. **Use Windows + Space:** Cycles through all installed keyboard layouts
3. **Use taskbar language selector:** Click the language indicator in the system tray
4. **Await official fix:** Microsoft may address this in future updates

## FAQ

### Will this affect other keyboard functionality?

No. The modification only affects the Right Alt key, making it behave identically to the Left Alt key. All other keyboard functions remain unchanged.

### Is this safe to use?

Yes. The Scancode Map is a documented Windows feature used by numerous keyboard remapping utilities (e.g., SharpKeys, AutoHotkey). The modification:
- Only affects keyboard input mapping
- Does not modify system files
- Is fully reversible
- Operates in user-mode registry space

### Will this work on work/enterprise computers?

Check with your IT department first. Some organizations have policies restricting registry modifications, particularly in `HKEY_LOCAL_MACHINE`.

### Does this affect gaming?

Generally no. However, if a specific game explicitly checks for Right Alt input, it will now receive Left Alt signals instead. This is rarely an issue in practice.

### Why hasn't Microsoft fixed this?

The bug originated in Windows Insider builds and was inadvertently released to stable channels. Microsoft is aware of the issue, but no official timeline for a fix has been announced.

---

## Acknowledgments

This solution is based on community-reported fixes from:
- Microsoft Q&A forums
- Windows 11 community feedback
- User reports on HP and Dell support forums

**Reported Working:** Windows 11 22H2, 23H2, 24H2 (builds affected by KB5070311 and later)

---

**⚠️ Important:** This fix requires a complete system restart to take effect. Logging out or restarting Windows Explorer is insufficient.
