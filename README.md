# Windows Utilities & Tweaks

A comprehensive collection of Windows registry tweaks, scripts, and utilities to enhance productivity and customize your Windows experience. These tools help you automate tasks, customize context menus, and streamline your workflow.

> ⚠️ **Important:** Always backup your registry before making modifications. Most tweaks require administrator privileges.

---

## 📋 Table of Contents

-   [Quick Start](#-quick-start)
-   [Features Overview](#-features-overview)
-   [Available Tweaks](#-available-tweaks)
    -   [File Creation & Management](#file-creation--management)
    -   [Context Menu Customization](#context-menu-customization)
    -   [Command Line Enhancements](#command-line-enhancements)
    -   [System Shortcuts & Navigation](#system-shortcuts--navigation)
    -   [Windows Explorer Customization](#windows-explorer-customization)
    -   [Testing & System Configuration](#testing--system-configuration)
-   [Installation Guide](#-installation-guide)
-   [Troubleshooting](#-troubleshooting)

---

## 🚀 Quick Start

1. **Browse the tweaks** in the sections below
2. **Navigate to the specific folder** for detailed instructions
3. **Read the README** in each folder before applying changes
4. **Create a system restore point** before making registry modifications
5. **Follow the installation steps** carefully

### Prerequisites

-   Windows 10 or Windows 11
-   Administrator privileges (for most tweaks)
-   Basic understanding of Windows Registry (recommended)

---

## ✨ Features Overview

This repository provides solutions for:

-   **Productivity Enhancement** - Keyboard shortcuts, quick file creation, taskbar shortcuts
-   **Context Menu Customization** - Add, remove, or modify right-click menu options
-   **Command Line Tools** - Quick access to elevated command prompts and PowerShell
-   **File Management** - Streamlined file creation and path handling
-   **System Navigation** - Direct access to special Windows folders and settings

---

## 📂 Available Tweaks

### File Creation & Management

#### [🆕 Create New Text File with Keyboard Shortcut](https://github.com/Michael-Matta1/windows-utilities-tweaks/tree/main/Add%20a%20Keyboard%20Shortcut%20to%20create%20New%20Text%20File)

Create new text files instantly with `Ctrl + Alt + N` in any File Explorer window.

**Features:**

-   Multiple versions (basic, auto-open, with dialogs)
-   Support for custom file extensions (`.md`, `.js`, `.py`, etc.)
-   Automatic duplicate handling
-   No software installation required

**Best for:** Developers, writers, and power users who frequently create new files

---

#### [📝 Silent Markdown Creator](https://github.com/Michael-Matta1/windows-utilities-tweaks/tree/main/Add%20a%20Silent%20Markdown%20Creator%20to%20Context%20Menu)

Add "Create Markdown File" to your right-click context menu.

**Features:**

-   One-click `.md` file creation
-   No popup dialogs
-   Automatic duplicate numbering
-   Appears at the top of context menu

**Best for:** Documentation writers, developers using Markdown

---

#### [📋 Copy Path Without Quotes](https://github.com/Michael-Matta1/windows-utilities-tweaks/tree/main/Copy%20Path%20Without%20Quotes)

Adds a context menu option to copy file paths without quotes.

**Features:**

-   Complements Windows' built-in "Copy as path"
-   Saves time by eliminating manual quote removal
-   Customizable menu position

**Best for:** Command line users, developers, system administrators

---

### Context Menu Customization

#### [🐍 Run Python Files from Context Menu](https://github.com/Michael-Matta1/windows-utilities-tweaks/tree/main/add%20run_with_python%20option%20to%20.py)

Execute Python scripts directly from File Explorer.

**Features:**

-   One-click script execution
-   Command window stays open to view output
-   Works with Python Launcher (`py.exe`)
-   Doesn't change default file associations

**Best for:** Python developers, data scientists

---

#### [⚡ Run PowerShell Scripts as Admin](https://github.com/Michael-Matta1/windows-utilities-tweaks/tree/main/Add%20Context%20Menu%20option%20to%20run%20.ps1%20files%20as%20Admin)

Add "Run with PowerShell (Admin)" to `.ps1` file context menu.

**Features:**

-   Two installation options (machine-wide or per-user)
-   UAC shield icon for visual clarity
-   Uses .NET Registry API for reliable installation
-   Bypasses execution policy restrictions

**Best for:** System administrators, DevOps engineers, PowerShell users

---

#### [🗑️ Remove or Disable Context Menu Items](https://github.com/Michael-Matta1/windows-utilities-tweaks/tree/main/Remove%20or%20Disable%20Context%20Menu%20Items)

Comprehensive guide to customizing Windows 11 context menus.

**Features:**

-   Multiple methods (LegacyDisable, Blocked CLSIDs, rename keys)
-   Complete registry location reference
-   Windows 11 modern vs legacy menu handling
-   Common CLSID list for blocking unwanted items

**Best for:** Users wanting a cleaner, faster context menu

---

### Command Line Enhancements

#### [💻 Admin CMD from Explorer Address Bar](https://github.com/Michael-Matta1/windows-utilities-tweaks/tree/main/Explorer%20Address%20Bar%20Admin%20CMD%20shortcut)

Open elevated Command Prompt directly from File Explorer's address bar.

**Features:**

-   Type `acmd` in any folder's address bar
-   Opens CMD as Administrator in current directory
-   Also includes PowerShell (`aps`) and Windows Terminal (`atr`) variants
-   No more navigating from System32

**Best for:** Developers, system administrators, power users

---

### System Shortcuts & Navigation

#### [📌 Pin Any Keyboard Shortcut to Taskbar](https://github.com/Michael-Matta1/windows-utilities-tweaks/tree/main/Pin%20Any%20Keyboard%20Shortcut%20to%20Taskbar%20-%20OCR%20Example)

Convert keyboard shortcuts into clickable taskbar icons (PowerToys OCR example).

**Features:**

-   Works with any keyboard shortcut
-   Silent VBScript + PowerShell execution
-   Includes virtual key code reference
-   Example: PowerToys OCR (`Win + Shift + T`)

**Best for:** Users who prefer mouse clicks over keyboard shortcuts

---

#### [⌨️ Fix Right Alt + Shift Language Toggle](https://github.com/Michael-Matta1/windows-utilities-tweaks/tree/main/Fix%20Alt%2BShift%20LanguageToggle)

A solution for the Windows 11 bug where Right Alt + Shift only switches languages in one direction.

**Features:**

-   Restores bidirectional language switching
-   Registry-based fix (reversible)
-   PowerShell script for auto-application
-   Solves conflict with AltGr on some layouts

**Best for:** Windows 11 users with multiple keyboard layouts

---

#### [📁 Windows Special System Folders](https://github.com/Michael-Matta1/windows-utilities-tweaks/tree/main/Windows%20Special%20System%20Folders%20and%20Their%20Usages)

Quick access guide to special Windows folders using `shell:` commands.

**Features:**

-   Access hidden system folders instantly
-   Manage startup applications
-   Customize Start Menu and Send To menu
-   Enable "GodMode" for advanced settings

**Best for:** System customization, troubleshooting, backup tasks

---

### Windows Explorer Customization

#### [🏠 Set Custom File Explorer Start Location](https://github.com/Michael-Matta1/windows-utilities-tweaks/tree/main/Set%20File%20Explorer%20to%20Open%20to%20a%20Custom%20Location)

Make File Explorer open to your preferred folder by default.

**Features:**

-   Works with both `Win + E` and taskbar icon
-   Updated for latest Windows 11 builds
-   Easy customization via `.reg` file or Registry Editor
-   Based on Shawn Brink's original tweak

**Best for:** Users who work primarily in specific folders

---

### Testing & System Configuration

#### [🧪 Testing Brand-Exclusive Software - Hardware Identity Modification](https://github.com/Michael-Matta1/windows-utilities-tweaks/tree/main/Testing%20Brand-Exclusive%20Software%20-%20Hardware%20Identity%20Modification)

Temporarily modify Windows registry values to test brand-exclusive software before purchasing hardware.

**Features:**

-   Test manufacturer-restricted software (Samsung, ASUS, Lenovo, Dell, HP, etc.)
-   Temporary registry modifications that reset after restart
-   Step-by-step tutorial with brand-specific examples
-   Includes revert instructions and troubleshooting guide

**Best for:** Users evaluating brand-exclusive software before making hardware purchases

---

## 🛠️ Installation Guide

### General Installation Steps

1. **Create a System Restore Point**

    - Press `Win + R`, type `sysdm.cpl`, press Enter
    - Go to "System Protection" tab → Click "Create"

2. **Choose Your Tweak**

    - Navigate to the specific folder in this repository
    - Read the README file thoroughly

3. **Follow Specific Instructions**

    - Each tweak has detailed installation steps
    - Some require `.reg` files, others use scripts or manual registry edits

4. **Restart Windows Explorer** (if needed)

    - Press `Ctrl + Shift + Esc` (Task Manager)
    - Find "Windows Explorer" → Right-click → Restart

5. **Test the Changes**
    - Verify the tweak works as expected
    - Check for any conflicts with existing software

---

## 🔧 Troubleshooting

### Changes Not Taking Effect

1. **Restart Windows Explorer**

    ```cmd
    taskkill /f /im explorer.exe && start explorer.exe
    ```

2. **Clear Icon Cache**

    - Delete `%localappdata%\IconCache.db`
    - Restart Windows Explorer

3. **Full System Restart**
    - Some changes require a complete reboot

### Common Issues

| Problem                       | Solution                                  |
| ----------------------------- | ----------------------------------------- |
| Registry permission denied    | Run Registry Editor as Administrator      |
| Script doesn't execute        | Check file paths and permissions          |
| Context menu item not showing | Verify registry entries, restart Explorer |
| Changes revert after update   | Reapply the tweak after Windows updates   |
| UAC prompts not appearing     | Check User Account Control settings       |
