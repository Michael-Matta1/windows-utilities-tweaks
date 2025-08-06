# 🧭 Windows 11: Remove or Disable Context Menu Items via Registry

This comprehensive guide provides multiple approaches to customize Windows 11 right-click context menus, covering both the new modern interface and the legacy menu system. Choose the method that best fits your specific needs and technical comfort level.

## 📋 Table of Contents

- [⚠️ Before You Begin](#️-before-you-begin)
- [📁 Complete Registry Key Reference](#-complete-registry-key-reference)
- [🛠️ Three Safe Methods to Disable Items](#️-three-safe-methods-to-disable-items)
  - [1. Add LegacyDisable or Extended String](#1-add-legacydisable-or-extended-string)
  - [2. Use the Blocked CLSID Key](#2-use-the-blocked-clsid-key)
  - [3. Rename Registry Keys](#3-rename-registry-keys)
- [🎯 Windows 11 Specific: Modern vs Legacy Context Menu](#-windows-11-specific-modern-vs-legacy-context-menu)
- [📡 When to Use Which Method](#-when-to-use-which-method)
- [🔧 Advanced Registry Locations](#-advanced-registry-locations)
- [🧼 Restoration Methods](#-restoration-methods)
- [🧠 Technical Background](#-technical-background)
- [🎯 Troubleshooting Tips](#-troubleshooting-tips)
- [📝 Final Notes](#-final-notes)

---

## ⚠️ Before You Begin

- Always **create a System Restore Point** or back up your registry (export keys) before making changes.
- To **Restart Windows Explorer** after changes: `Ctrl+Shift+Esc` → Find "Windows Explorer" → Right-click → Restart
- **Administrator privileges** are required for most registry modifications
- Test changes on a non-critical system first

---

## 📁 Complete Registry Key Reference

| Scenario | Registry Path | Description |
|----------|---------------|-------------|
| All file types (verbs) | `HKEY_CLASSES_ROOT\*\shell\` | Commands that appear for any file type |
| Shell extensions (COM handlers) for files | `HKEY_CLASSES_ROOT\*\shellex\ContextMenuHandlers\` | Dynamic menu items for all files |
| Any file system object (files or folders) | `HKEY_CLASSES_ROOT\AllFileSystemObjects\ShellEx\ContextMenuHandlers\` | Extensions for both files and folders |
| Folder right‑click menu | `HKEY_CLASSES_ROOT\Directory\shell\` | Static commands when right-clicking folders |
| Shell extensions on folders | `HKEY_CLASSES_ROOT\Directory\shellex\ContextMenuHandlers\` | Dynamic menu items for folders |
| Drive (e.g. C:\) context menu | `HKEY_CLASSES_ROOT\Drive\shell\` | Commands for drive letters |
| Shell extensions on drives | `HKEY_CLASSES_ROOT\Drive\shellex\ContextMenuHandlers\` | Dynamic items for drives |
| Background of folder (empty space) | `HKEY_CLASSES_ROOT\Directory\Background\shell\`<br>`HKEY_CLASSES_ROOT\Directory\Background\shellex\ContextMenuHandlers\` | Items when right-clicking empty folder space |
| Desktop background context menu | `HKEY_CLASSES_ROOT\DesktopBackground\Shell\`<br>`HKEY_CLASSES_ROOT\DesktopBackground\shellex\ContextMenuHandlers\` | Desktop right-click menu items |
| "New" submenu items | `HKEY_CLASSES_ROOT\.ext\ShellNew\` | File creation templates in "New" submenu |
| "Send To" menu | `HKEY_CLASSES_ROOT\AllFilesystemObjects\shellex\ContextMenuHandlers\Send To` | Send To folder shortcuts |
| Per‑user override versions | `HKEY_CURRENT_USER\Software\Classes\...` | User-specific overrides (same structure as HKCR) |
| System-wide override versions | `HKEY_LOCAL_MACHINE\SOFTWARE\Classes\...` | Machine-wide overrides (same structure as HKCR) |

---

## 🛠️ Three Safe Methods to Disable Items

### 1. Add `LegacyDisable` or `Extended` String  
**Where it applies:** Static registry keys under `…\shell\…`  
**What it does:** 
- `LegacyDisable`: Completely hides the menu entry
- `Extended`: Shows the entry only when Shift+Right-clicking

**Steps:**
1. Navigate to the relevant `…\shell\YourItem` key
2. Right‑click in the right pane → **New → String Value**
3. Name it `LegacyDisable` (leave value empty) or `Extended` (leave value empty)
4. Restart Windows Explorer
  
**Note:** This does **not work** under `shellex\ContextMenuHandlers`.

---

### 2. Use the **Blocked CLSID** Key  
**Where it applies:** Shell extension handlers under `shellex\ContextMenuHandlers`  
**What it does:** Adds the handler's GUID to a "blocked" list so Windows does not load it.

**Steps:**
1. Find the CLSID of the handler (usually in the default value of the handler key)
2. Navigate to: `HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Blocked`
3. Add a **String Value** named with the CLSID (including braces) and leave the value empty
4. Restart Explorer or reboot

#### Common CLSIDs to Block:
- **Cast to Device**: `{7AD84985-87B4-4a16-BE58-8B72A5B390F7}`
- **Share**: `{e2bf9676-5f8f-435c-97eb-11607a5bedf7}`
- **Send To**: `{7BA4C740-9E81-11CF-99D3-00AA004AE837}`
- **Give access to**: `{f81e9010-6ea4-11ce-a7ff-00aa003ca9f6}`
- **Edit with Paint 3D**: `{2484F1CA-9F8A-4E6A-9C73-BDC0E72BA6F8}`
- **Edit with Photos**: `{FFE2A43C-56B9-4bf5-9A79-CC6D4285608A}`
- **Scan with Windows Defender**: `{09A47860-11B0-4DA5-AFA5-26D86198A780}`
---

### 3. Rename Registry Keys
**Where it applies:** Most context menu items, but not all (some are hardcoded)

**What it does:** Temporarily disables by making the key name unrecognizable to Windows

**Steps:**
1. Right-click the key → **Rename**
2. Add prefix like `_DISABLED_` or suffix like `_OLD`
3. Example: `MBAMShlExt` → `_DISABLED_MBAMShlExt`
4. Restart Windows Explorer

**Limitations:** Does not work for all types of context menu items, especially those hardcoded into Windows or managed by system policies.

---

## 🎯 Windows 11 Specific: Modern vs Legacy Context Menu

Windows 11 introduced a new simplified context menu. To restore the classic Windows 10-style context menu permanently, add this registry key:

```reg
Windows Registry Editor Version 5.00

[HKEY_CURRENT_USER\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32]
@=""
```

**Command line version:**
```cmd
reg.exe add "HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32" /f /ve
```

**To revert back to Windows 11 modern menu:**
```cmd
reg.exe delete "HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}" /f
```

---

## 📡 When to Use Which Method

| Context Menu Type | Registry Location | Best Method | Notes |
|-------------------|--------------------|-------------|-------|
| Static commands (`open`, `print`, `edit`) | `…\shell\…` | Add `LegacyDisable` or `Extended` | Works for built-in commands |
| COM-based handlers (antivirus, cloud storage) | `…\shellex\ContextMenuHandlers\…` | Use `Shell Extensions\Blocked` or rename key | System-wide blocking preferred |
| File type "New" items | `…\.ext\ShellNew` | Rename `ShellNew` key to `_ShellNew` | Simple and reversible |
| Modern Windows 11 menu items | N/A | Switch to legacy context menu first | Required before other methods |
| Third-party software items | Various | Try renaming first, then blocking | Less risky approach |

---

## 🔧 Advanced Registry Locations

### Core Context Menu Locations:
```
HKEY_CLASSES_ROOT\*\shell\                    # Commands for all files
HKEY_CLASSES_ROOT\*\shellex\ContextMenuHandlers\    # Extensions for all files
HKEY_CLASSES_ROOT\AllFileSystemObjects\ShellEx\ContextMenuHandlers\  # Files & folders
HKEY_CLASSES_ROOT\Directory\shell\            # Folder commands
HKEY_CLASSES_ROOT\Directory\shellex\ContextMenuHandlers\  # Folder extensions
HKEY_CLASSES_ROOT\Directory\Background\shell\ # Empty folder space commands
HKEY_CLASSES_ROOT\DesktopBackground\Shell\    # Desktop background commands
HKEY_CLASSES_ROOT\Drive\shell\                # Drive letter commands
```

### File Type Specific Menus:
- **Images:** `HKEY_CLASSES_ROOT\SystemFileAssociations\image\shell\`
- **Videos:** `HKEY_CLASSES_ROOT\SystemFileAssociations\video\shell\`
- **Text files:** `HKEY_CLASSES_ROOT\txtfile\shell\`
- **Executables:** `HKEY_CLASSES_ROOT\exefile\shell\`
- **Audio files:**  `HKEY_CLASSES_ROOT\SystemFileAssociations\audio\shell\`
- **Documents:**   `HKEY_CLASSES_ROOT\SystemFileAssociations\text\shell\`    
- **Archives:**   `HKEY_CLASSES_ROOT\CompressedFolder\shell\`  
- **Shortcuts:**   `HKEY_CLASSES_ROOT\lnkfile\shell\` 

### Specialized Context Menu Locations:

- **Open With Menu**   `HKEY_CLASSES_ROOT\*\OpenWithList\`  
- **Library Folders**   `HKEY_CLASSES_ROOT\LibraryFolder\background\shell\` : Special folders like Documents, Pictures libraries
- **Network Locations**   `HKEY_CLASSES_ROOT\Network\shell\` :  Network drive and UNC path context menus  
- **Recycle Bin**   `HKEY_CLASSES_ROOT\CLSID\{645FF040-5081-101B-9F08-00AA002F954E}\shell\` :  Recycle Bin specific options 
- **Control Panel Items**   `HKEY_CLASSES_ROOT\CLSID\{21EC2020-3AEA-1069-A2DD-08002B30309D}\shell\` : Control Panel context menu items 

### User vs System Override Hierarchy:
```
HKEY_CURRENT_USER\Software\Classes\...        # User-specific (highest priority)
├── *\shell\                                  # User file commands
├── Directory\shell\                          # User folder commands
└── DesktopBackground\Shell\                  # User desktop commands

HKEY_LOCAL_MACHINE\SOFTWARE\Classes\...       # System-wide (medium priority)
├── *\shell\                                  # System file commands
├── Directory\shell\                          # System folder commands
└── DesktopBackground\Shell\                  # System desktop commands

HKEY_CLASSES_ROOT\...                         # Default (lowest priority)
├── *\shell\                                  # Default file commands
├── Directory\shell\                          # Default folder commands
└── DesktopBackground\Shell\                  # Default desktop commands
```

### Application-Specific Locations:
| Application | Registry Path | Common Items |
|-------------|---------------|--------------|
| **7-Zip** | `HKEY_CLASSES_ROOT\*\shellex\ContextMenuHandlers\7-Zip` | Extract, Add to archive |
| **WinRAR** | `HKEY_CLASSES_ROOT\*\shellex\ContextMenuHandlers\WinRAR` | Extract to folder, Add to RAR |
| **Git** | `HKEY_CLASSES_ROOT\Directory\Background\shell\git_*` | Git Bash, Git GUI |
| **VSCode** | `HKEY_CLASSES_ROOT\*\shell\VSCode` | Open with Code |


---

## 🧼 Restoration Methods

### To re-enable a context menu item:

1. **For `LegacyDisable`/`Extended`:** Delete the string value from the registry key
2. **For Blocked CLSIDs:** Delete the CLSID entry from the `Blocked` key at:
   ```
   HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Blocked
   ```
3. **For renamed keys:** Rename back to original name (remove `_DISABLED_` prefix)
4. **For classic context menu:** Delete the CLSID key: 
   ```cmd
   reg.exe delete "HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}" /f
   ```
5. **Restart Windows Explorer** after making changes

---

## 🧠 Technical Background

### How Context Menus Work:
- **Shell Verbs** (`shell\...`): Static commands defined in registry with fixed text and actions
- **Shell Extensions** (`shellex\...`): Dynamic COM objects that can modify menus programmatically
- **Handler Priority:** HKCU → HKLM → HKCR (Current User overrides Machine overrides Classes Root)
- **Menu Loading:** Windows checks `Blocked` list before loading shell extensions
- **Context Types:** File, folder, drive, background, and desktop contexts are handled separately

### Windows 11 Context Menu Architecture:
- **Modern Menu:** Shows simplified, curated items with Windows 11 design language
- **Legacy Menu:** Full traditional context menu (accessed via "Show more options" or Shift+Right-click)
- **Registry Override:** Forces legacy menu as default by disabling modern context menu handler
- **Compatibility:** Some modern apps may only add items to the new context menu

### Registry Key Types:
- **String Values (REG_SZ):** Simple text values for commands and paths
- **DWORD Values (REG_DWORD):** Numeric flags and options
- **Default Values:** The main command or description for a menu item
- **Command Subkeys:** Contain the actual executable path and parameters

---

## 🎯 Troubleshooting Tips

### Changes Not Taking Effect:
1. **Restart Explorer:** `taskkill /f /im explorer.exe && start explorer.exe`
2. **Clear icon cache:** Delete `%localappdata%\IconCache.db` and restart
3. **Check permissions:** Ensure you have admin rights for HKLM changes
4. **Verify paths:** Double-check registry key paths and spelling
5. **Full restart:** Some changes require a complete system reboot

### Common Issues:
- **Modern menu still showing:** Apply the Windows 11 classic context menu registry fix first
- **Items reappear after updates:** Windows updates may restore default registry entries
- **Permissions denied:** Run Registry Editor as Administrator
- **System instability:** Use System Restore to revert problematic changes
- **Multiple locations:** Check for entries in HKCU, HKLM, and HKCR that might conflict

### Debugging Context Menu Issues:
1. **Check multiple registry locations** for the same item
2. **Verify CLSID values** match exactly (including braces)
3. **Test with different file types** to isolate the issue
4. **Use Process Monitor** to see which registry keys are being accessed
5. **Check Windows Event Viewer** for shell extension errors

---   

## 📝 Final Notes

- **ALWAYS** back up before editing the registry - export the entire key you're modifying
- Test changes by restarting Explorer: `Ctrl+Shift+Esc` → Restart Windows Explorer
- Items disabled via `Extended` appear only with Shift+Right-click
- `Blocked` CLSID entries apply system-wide and require administrator rights
- Some changes may require a full system restart to take effect
- Modern Windows 11 apps may not respect all registry changes
- **Windows Updates** may sometimes restore disabled context menu items
- Consider using **Group Policy** for enterprise environments instead of direct registry edits
- Keep a record of changes made for easier troubleshooting and restoration


