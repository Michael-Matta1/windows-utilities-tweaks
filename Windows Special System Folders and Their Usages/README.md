# Windows System Folders and Their Usages

This repository contains information about various Windows system folders and their possible usages. These folders can be accessed through one of the following:

1. **Using the Run Dialog:**
   - Press `Windows + R` on your keyboard.
   - Type `shell:<folder_name>` (e.g., `shell:startup`).
   - Press `Enter`, and the folder will open.

2. **Using File Explorer:**
   - Open File Explorer (`Windows + E`).
   - In the address bar, type `shell:<folder_name>` (e.g., `shell:AppsFolder`).
   - Press `Enter` to open the folder.

3. **Using Command Prompt or PowerShell:**
   - Open Command Prompt (`cmd`) or PowerShell.
   - Type `explorer shell:<folder_name>` and press `Enter`.

## 📌 Navigation
- [📂 All Installed Applications](#all-installed-applications)
- [🚀 Applications That Automatically Run at Startup](#applications-that-automatically-run-at-startup)
- [📜 Start Menu Applications](#start-menu-applications)
- [✉️ "Send To" Right-Click Menu Options](#send-to-right-click-menu-options)
- [📌 Taskbar Pinned Applications](#taskbar-pinned-applications)
- [🛠️ Enable "GodMode"](#enable-godmode)

## 📂 All Installed Applications

- Command: `shell:AppsFolder`
- Description: Opens a folder that contains all installed applications, including Windows Store apps and traditional desktop applications.
- **Possible Usages:**
  - Quickly access and launch any installed application.
  - Uninstall applications directly from the folder.

## 🚀 Applications That Automatically Run at Startup

- **For the Current User:** `shell:startup`
- **For All Users:** `shell:common startup`
- Description: These folders contain shortcuts to programs that automatically run when Windows starts.
- **Possible Usages:**
  - Add or remove programs that should launch at startup.
  - Troubleshoot slow startup issues by managing unnecessary startup apps.

## 📜 Start Menu Applications

- **For the Current User:** `shell:programs`
- **For All Users:** `shell:common programs`
- Description: These folders store shortcuts for applications that appear in the Windows Start Menu.
- **Possible Usages:**
  - Customize the Start Menu by adding or removing application shortcuts.
  - Organize programs for easier access.

## ✉️ "Send To" Right-Click Menu Options

- Command: `shell:sendto`
- Description: This folder contains shortcuts for apps that appear in the "Send To" menu when right-clicking files.
- **Possible Usages:**
  - Add frequently used folders or apps for quick file transfers.
  - Remove unwanted shortcuts to declutter the menu.

## 📌 Taskbar Pinned Applications

- Path: `C:\Users\[Username]\AppData\Roaming\Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar`

- Description: This folder stores shortcuts for applications pinned to the taskbar.

- **Possible Usages:**

  - Backup and restore pinned taskbar items.
  - Quickly modify pinned applications without using the UI.

## 🛠️ Enable "GodMode"

- To enable "GodMode," create a new folder and rename it to:
  ```
  GodMode.{ED7BA470-8E54-465E-825C-99712043E01C}
  ```
- Description: This folder provides access to a wide range of system settings and administrative tools in one place.
- **Possible Usages:**
  - Quickly access advanced system settings.
  - Troubleshoot and optimize system performance with ease.

---

### 📢 Notes:

- Some folders require administrator privileges to access or modify.
- Custom paths may vary based on Windows versions and user configurations.

