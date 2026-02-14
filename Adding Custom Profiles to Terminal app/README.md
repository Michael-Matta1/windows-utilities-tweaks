# Adding Custom Terminal Profiles to Windows Terminal

A comprehensive guide for configuring custom terminal profiles in Windows Terminal on Windows 11.

## Table of Contents

- [Overview](#overview)
- [Accessing Terminal Settings](#accessing-terminal-settings)
- [Adding a New Profile](#adding-a-new-profile)
- [Example Configurations](#example-configurations)
  - [Python IDLE](#python-idle)
  - [Git Bash](#git-bash)
  - [PowerShell ISE](#powershell-ise)
- [Profile Configuration Options](#profile-configuration-options)
- [Additional Settings](#additional-settings)
- [Troubleshooting](#troubleshooting)


## Overview

Windows Terminal is a modern, feature-rich terminal application that supports multiple command-line tools and shells. This guide will walk you through the process of adding custom terminal profiles to streamline your development workflow.

## Accessing Terminal Settings

1. Open **Windows Terminal**
2. Click the **dropdown arrow** (˅) next to the tab bar
3. Select **Settings** (or press `Ctrl + ,`)

Alternatively, you can access the JSON configuration file directly by selecting **Open JSON file** at the bottom of the settings sidebar.

## Adding a New Profile

1. In the Settings menu, navigate to **Profiles** in the left sidebar
2. Click **+ Add a new profile** at the bottom of the profiles list
3. Choose **New empty profile**
4. Configure the profile settings as described below

## Example Configurations

### Python IDLE

Python IDLE can be integrated into Windows Terminal, though the command path may vary depending on your installation method and Python version.

#### Configuration Steps

1. **Name**: `Python IDLE`

2. **Command line**: The location varies based on installation. Common paths include:
   ```
   C:\Python313\python.exe
   ```
   Or for user-specific installations:
   ```
   C:\Users\<username>\AppData\Local\Programs\Python\Python313\python.exe
   ```

   **Note**: Replace `<username>` with your Windows username and `Python313` with your installed Python version.

3. **Starting directory**:
   - Select `Use parent process directory` for convenience
   - Or specify a custom directory like `C:\Users\<username>\Documents\Python`

4. **Icon**: Set to the same path as the command executable:
   ```
   C:\Python313\python.exe
   ```

5. **Run this profile as Administrator**:
   - Toggle **On** if you need elevated privileges
   - Usually not required for standard Python operations

#### Finding Your Python Installation

If you're unsure where Python is installed:

1. Open Command Prompt
2. Run: `where python`
3. Use the returned path in the Command line field

### Git Bash

Git Bash provides a Unix-like command-line experience on Windows.

#### Configuration Steps

1. **Name**: `Git Bash`

2. **Command line**:
   ```
   "C:\Program Files\Git\bin\bash.exe" --login -i
   ```

   **Note**: This assumes Git was installed with default settings. If you chose a custom installation directory, adjust the path accordingly.

3. **Starting directory**:
   - Check `Use parent process directory` for dynamic directory handling
   - Or specify your preferred working directory

4. **Icon**:
   ```
   C:\Program Files\Git\mingw64\share\git\git-for-windows.ico
   ```

5. **Tab title**: `Git Bash` (or leave as default)

6. **Run this profile as Administrator**:
   - Generally set to **Off** unless you specifically need elevated permissions

### PowerShell ISE

PowerShell ISE (Integrated Scripting Environment) is a specialized PowerShell host application.

#### Configuration Steps

1. **Name**: `Windows PowerShell ISE`

2. **Command line**:
   ```
   C:\Windows\System32\WindowsPowerShell\v1.0\powershell_ise.exe
   ```

3. **Starting directory**:
   - Check `Use parent process directory`

4. **Icon**:
   - The default icon is usually auto-detected
   - Or specify the PowerShell ISE executable path

5. **Run this profile as Administrator**:
   - Toggle **On** if you frequently need admin access

**Important**: When you select PowerShell ISE from the terminal dropdown, it will launch the PowerShell ISE application in a separate window rather than opening within the Windows Terminal tab. This is because PowerShell ISE is a graphical application, not a console-based shell.

## Profile Configuration Options

### Core Settings

| Setting | Description |
|---------|-------------|
| **Name** | Display name that appears in the profile dropdown |
| **Command line** | Full path to the executable with any required arguments |
| **Starting directory** | Default working directory when the profile launches |
| **Icon** | Path to .ico, .png, or executable file for tab/dropdown icon |
| **Tab title** | Custom title for the tab (defaults to profile name) |

### Advanced Settings

| Setting | Description |
|---------|-------------|
| **Run as Administrator** | Automatically launch with elevated privileges |
| **Hide profile from dropdown** | Keep the profile in settings but remove from UI |
| **Tab color** | Set a custom color for easy visual identification |
| **Background image** | Add a custom background image to the terminal |
| **Transparency** | Adjust opacity level of the terminal window |

## Additional Settings

After creating your basic profile, explore the **Additional settings** option at the bottom of the profile configuration page for more customization:

### Appearance
- Color schemes (choose from predefined themes or create custom ones)
- Font family, size, and weight
- Cursor shape and styling
- Background images and opacity

### Terminal Emulation
- Scrollback buffer size
- VT sequences handling
- Terminal emulation type

### Advanced
- Bell notification settings
- Anti-aliasing text rendering
- Experimental features
- Environment variables

## Troubleshooting

### Profile Doesn't Appear in Dropdown

- Ensure you clicked **Save** after configuring the profile
- Check that "Hide profile from dropdown" is set to **Off**
- Restart Windows Terminal

### Command Not Found Error

- Verify the command line path is correct
- Use absolute paths with proper escaping (use `\\` for backslashes in JSON)
- Check that the executable exists at the specified location

### Icon Not Displaying

- Ensure the icon file path is correct and the file exists
- Supported formats: `.ico`, `.png`, and executable files
- Try using an absolute path

### Permission Issues

- Enable "Run this profile as Administrator" if the tool requires elevated privileges
- Some system directories may require admin access

### Python Path Variations

If the provided Python paths don't work:

1. Check Start Menu for Python installation folder
2. Use Windows search for `python.exe`
3. Check these common locations:
   - `C:\Python<version>\`
   - `C:\Users\<username>\AppData\Local\Programs\Python\Python<version>\`
   - `C:\Program Files\Python<version>\`



## Resources

- [Windows Terminal Documentation](https://docs.microsoft.com/en-us/windows/terminal/)
- [Windows Terminal GitHub Repository](https://github.com/microsoft/terminal)
- [Custom Terminal Profiles Guide](https://docs.microsoft.com/en-us/windows/terminal/customize-settings/profile-general)
