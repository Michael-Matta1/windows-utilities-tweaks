# Pin Any Keyboard Shortcut to Taskbar — OCR & Voice Typing

A simple solution to convert keyboard shortcuts into clickable taskbar icons. Includes examples for **PowerToys OCR** (`Win + Shift + T`) and **Windows Voice Typing** (`Win + H`).

## 🎯 Purpose

Turn any keyboard shortcut into a taskbar icon you can click. Instead of remembering key combinations like `Win + Shift + T` or `Win + H`, you just click an icon on your taskbar.

### Included Examples:
- **OCR (Optical Character Recognition)** — triggers PowerToys Text Extractor
- **Voice Typing** — activates Windows built-in dictation
- **Any Custom Shortcut** — enter your own key combination and get the same treatment

## 📁 Files Included

### OCR (`OCR\` subfolder)
- `Enable OCR.vbs` — VBScript that silently executes the PowerShell script
- `WinShiftT.ps1` — PowerShell script that simulates `Win + Shift + T`
- `ocr.ico` — custom icon for the shortcut

### Voice Typing (`Enable Voice Typing\` subfolder)
- `voice_typing_vbscript.vbs` — VBScript that silently executes the PowerShell script
- `VoiceTyping.ps1` — PowerShell script that simulates `Win + H`
- `mic_microphone_14162.ico` — custom icon for the shortcut
- `Enable Voice Typing.lnk` — pre-made shortcut reference (optional)

## 🔧 Setup Instructions

### Step 1: Download Files
1. Choose which shortcut you want (OCR, Voice Typing, or custom)
2. Download the corresponding `.vbs` and `.ps1` files from the subfolder above
3. Place them in the same folder (e.g., `C:\Users\[YourUsername]\Scripts\`)

### Step 2: Create Windows Shortcut
1. Right-click on your desktop or in a folder where you want the shortcut
2. Select "New" → "Shortcut"
3. In the target field, enter:
`C:\Windows\System32\wscript.exe` followed by the path to the `.vbs` file between quotation marks

   For **OCR**:
   ```
   C:\Windows\System32\wscript.exe "C:\Path\To\Your\Enable OCR.vbs"
   ```

   For **Voice Typing**:
   ```
   C:\Windows\System32\wscript.exe "C:\Path\To\Your\voice_typing_vbscript.vbs"
   ```

4. Click "Next" and give your shortcut a name (e.g., "PowerToys OCR" or "Voice Typing")
5. Click "Finish"

### Step 3: Customize Icon (Optional)
1. Right-click on the shortcut you just created
2. Select "Properties"
3. Click "Change Icon..."
4. Browse to the `.ico` file included in the subfolder (or choose your own)
5. Click "OK" to apply

### Step 4: Pin to Taskbar
1. Right-click on your shortcut
2. Select "Pin to taskbar"

**Alternative:** You can also pin it to the Start Menu by selecting "Pin to Start"

## 🚀 Usage

Once set up, simply click the icon on your taskbar to activate the shortcut. The script runs silently in the background and triggers the action immediately.

## ⚙️ How It Works

1. **VBScript Layer**: Provides a silent execution environment that hides any console windows
2. **PowerShell Layer**: Uses Windows API calls to simulate the exact key combination
3. **Key Simulation**: The script properly presses and releases the keys in the correct sequence with appropriate timing delays

## 🎤 Bonus: Voice Typing (Win+H) Shortcut

The `Enable Voice Typing` subfolder contains everything you need to activate Windows Voice Typing via `Win + H`.

### Prerequisites:
- **Windows 11** (Voice Typing is built into Windows 11)
- No additional software required

### Setup:
Same process as OCR — copy the files, create a shortcut pointing to the `.vbs` file via `wscript.exe`, customize the icon, and pin to taskbar.

## ✨ Custom Keybinding Shortcut

You can create a taskbar shortcut for **any** keyboard combination using the GUI app or by manually adapting the scripts.

### Via the GUI (Windows Tweaks Launcher)
Select **"Custom Keybinding Shortcut (Pin to Taskbar)"**, enter your key combination (e.g., `Win+I`, `Ctrl+Shift+Esc`, `Win+Ctrl+D`), and click Apply. The app generates the scripts and creates a desktop shortcut automatically.

### Manually
1. **Modify the PowerShell script**: Change the virtual key codes and key combination logic.
   - See the [Virtual Key Codes reference](#common-virtual-key-codes) below
   - Rename the method to reflect the new shortcut
2. **Update the VBScript**: Change the PowerShell script filename reference

### Common Virtual Key Codes:
| Key      | Code    |
|----------|---------|
| `A`-`Z`  | 0x41-0x5A |
| `0`-`9`  | 0x30-0x39 |
| `F1`-`F12` | 0x70-0x7B |
| `Ctrl`   | 0x11    |
| `Alt`    | 0x12    |
| `Shift`  | 0x10    |
| `Windows`| 0x5B    |

### Example Applications:
- `Win + I`: Settings
- `Ctrl + Shift + Esc`: Task Manager
- `Win + L`: Lock screen
- `Win + Ctrl + D`: New desktop
- `Win + V`: Clipboard history
- `Win + H`: Voice typing
- `Win + Shift + S`: Screenshot
- **Microsoft PowerToys Utilities Shortcuts**
- **Any custom application shortcuts**

## ⚠️ Prerequisites

- **For OCR**: [Microsoft PowerToys](https://learn.microsoft.com/en-us/windows/powertoys/) must be installed and the **Text Extractor** feature enabled (`Win + Shift + T` must work)
- **For Voice Typing**: Windows 11 (built-in, no additional software needed)
- **For Custom shortcuts**: None — works on any Windows 10/11 system
- **Windows 10/11** (PowerShell 5.0+ and VBScript support)

## 🛠️ Troubleshooting

**Script doesn't work:**
- Ensure both `.vbs` and `.ps1` files are in the same directory
- For OCR: verify PowerToys is installed and Text Extractor is enabled
- Verify the file path in your shortcut target is correct
- Try running PowerShell as administrator if needed

**Icon doesn't appear:**
- Make sure you're using the correct `wscript.exe` path
- Check file permissions for the script files

**Shortcut doesn't activate:**
- Test the manual keyboard shortcut first to confirm it works
- For OCR: ensure PowerToys is running in the background

> **Note:** This solution works by simulating keyboard input, so make sure your target application is properly configured and running for the best results.
