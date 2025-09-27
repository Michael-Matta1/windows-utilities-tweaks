# PowerToys OCR Taskbar Shortcut

A simple solution to add Microsoft PowerToys OCR functionality directly to your Windows taskbar as a clickable icon, eliminating the need to press the `Windows + Shift + T` keyboard shortcut.

## 🎯 Purpose

This project allows you to create a taskbar icon that, when clicked, instantly activates PowerToys OCR (Optical Character Recognition) feature. Instead of remembering and pressing the keyboard combination `Windows + Shift + T`, you can simply click an icon on your taskbar.

## 📁 Files Included

- `Enable OCR.vbs` - VBScript that silently executes the PowerShell script
- `WinShiftT.ps1` - PowerShell script that simulates the `Windows + Shift + T` key combination

## 🔧 Setup Instructions

### Step 1: Download Files
1. Download both `Enable OCR.vbs` and `WinShiftT.ps1` files
2. Place them in the same folder (e.g., `C:\Users\[YourUsername]\Scripts\OCR\`)

### Step 2: Create Windows Shortcut
1. Right-click on your desktop or in a folder where you want the shortcut
2. Select "New" → "Shortcut"
3. In the target field, enter:
`C:\Windows\System32\wscript.exe` followed by the `path of the Enable OCR.vbs file` between quotation marks
   
   **As Follows:**
    ```
   C:\Windows\System32\wscript.exe "C:\Path\To\Your\Enable OCR.vbs"
   ```

4. Click "Next" and give your shortcut a name (e.g., "PowerToys OCR")
5. Click "Finish"

### Step 3: Customize Icon (Optional)
1. Right-click on the shortcut you just created
2. Select "Properties"
3. Click "Change Icon..."
4. Browse and select an icon that represents OCR or text recognition
5. Click "OK" to apply

### Step 4: Pin to Taskbar
1. Right-click on your shortcut
2. Select "Pin to taskbar"

**Alternative:** You can also pin it to the Start Menu by selecting "Pin to Start"

## 🚀 Usage

Once set up, simply click the icon on your taskbar to activate PowerToys OCR. The script runs silently in the background and triggers the OCR functionality immediately.

## ⚙️ How It Works

1. **VBScript Layer (`Enable OCR.vbs`)**: Provides a silent execution environment that hides any console windows
2. **PowerShell Layer (`WinShiftT.ps1`)**: Uses Windows API calls to simulate the exact key combination `Windows + Shift + T`
3. **Key Simulation**: The script properly presses and releases the keys in the correct sequence with appropriate timing delays

## 🔄 Customization for Other Shortcuts

This technique can be adapted for any keyboard shortcut! To create similar taskbar shortcuts for other key combinations:

### For Different Key Combinations:
1. **Modify the PowerShell script** (`WinShiftT.ps1`):
   - Change the virtual key codes (e.g., `VK_T = 0x54` for different keys)
   - Adjust the key combination logic in the `SendWinShiftT()` method
   - Rename the method to reflect the new shortcut

2. **Update the VBScript** if needed:
   - Change the PowerShell script filename reference
   - Modify the script name for clarity

### Common Virtual Key Codes:
- `A-Z`: 0x41-0x5A
- `0-9`: 0x30-0x39
- `F1-F12`: 0x70-0x7B
- `Ctrl`: 0x11
- `Alt`: 0x12
- `Shift`: 0x10
- `Windows`: 0x5B

### Example Applications:
- `Windows + I`: Settings
- `Ctrl + Shift + Esc`: Task Manager
- `Windows + L`: Lock screen
- `Win + Ctrl + D` : New desktop
- `Win + Ctrl + Left/Right` : Switch desktops
- `Win + V` : Clipboard history
- `Win + H` : Voice typing
- `Win + Shift + S` : Screenshot
- **Microsoft PowerToys Utilities Shortcuts**
- **Any custom application shortcuts**

## ⚠️ Prerequisites

- **Microsoft PowerToys** must be installed and the OCR feature enabled
- **Windows 10/11** (PowerShell 5.0+ and VBScript support)
- **PowerToys OCR shortcut** (`Windows + Shift + T`) must be working normally

## 🛠️ Troubleshooting

**Script doesn't work:**
- Ensure both files are in the same directory
- Check that PowerToys is installed and OCR is enabled
- Verify the file path in your shortcut target is correct
- Try running PowerShell as administrator if needed

**Icon doesn't appear:**
- Make sure you're using the correct wscript.exe path
- Check file permissions for the script files

**OCR doesn't activate:**
- Test the manual `Windows + Shift + T` shortcut first
- Ensure PowerToys is running in the background

---

**Note:** This solution works by simulating keyboard input, so make sure your target application (PowerToys) is properly configured and running for the best results.