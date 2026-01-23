# Testing Brand-Exclusive Software: Registry Modification Guide

## Overview

This guide explains how to temporarily modify Windows registry values to test brand-exclusive software before purchasing hardware. Some manufacturers restrict their software to only run on their own devices, even when your current hardware is fully capable of running it.

> **Note:** This guide uses Samsung as an example, which offers exclusive software like Samsung Notes, Samsung Gallery, and other applications that are typically restricted to Samsung devices. However, the same method works for any brand's exclusive software.

**Important Notes:**
- This is for **testing purposes only** to evaluate software before making a purchase decision
- Registry changes reset automatically after restarting your PC
- Always note down original values before making changes
- Use at your own risk and ensure you have system backups

## What This Does

Many brands (Samsung, ASUS, Lenovo, Dell, HP, etc.) lock their software to only work on their hardware by checking the system manufacturer information in the Windows registry. By temporarily modifying these values, you can test their exclusive software to make an informed purchasing decision.

## Prerequisites

- Windows 10 or Windows 11
- Administrator privileges
- A text file or notepad to save your original system information

## Complete Tutorial

### Step 1: Access the Windows Registry

First, you need to open the Registry Editor:

1. Press `Windows + R` on your keyboard to launch the Run dialog
2. Type `regedit` into the text box
3. Press Enter or click OK
4. If prompted by User Account Control, click Yes to grant permission

### Step 2: Navigate to the BIOS Information

Once Registry Editor opens:

1. In the left sidebar, expand the folders to navigate to:
   ```
   HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\BIOS
   ```
2. Click on the `BIOS` folder to view its contents in the right pane

### Step 3: Record Your Original System Information

**This is crucial** - you must save your current values:

1. In the right pane, locate these two entries:
   - `SystemManufacturer`
   - `SystemProductName`
2. Double-click each one to see its current value
3. **Copy or write down both values exactly as they appear**
4. Save them in a text file or take a screenshot for safekeeping

### Step 4: Modify the Registry Values

Now you'll change the system identity to match your target brand:

1. Double-click on `SystemManufacturer` in the right pane
2. In the "Value data" field, delete the existing text
3. Enter the manufacturer name for your target brand (e.g., `Samsung` for Samsung devices)
4. Click OK to confirm
5. Double-click on `SystemProductName`
6. Delete the existing value and enter a valid model number (e.g., `NP960XFG-KC4UK` for Samsung Galaxy Book)
7. Click OK to save

### Step 5: Apply the Changes

For the modifications to take effect:

1. Close the Registry Editor
2. Restart your computer completely
3. Wait for Windows to boot back up

### Step 6: Confirm the Modification Worked

You can verify the changes in two ways:

**Option A: Test with the Software (Recommended)**
- Download and install the brand-exclusive software you want to test
- Launch the application
- If it opens and runs without error messages, your modification was successful

**Option B: Check the Registry**
- Open Registry Editor again (`Windows + R`, type `regedit`, press Enter)
- Navigate back to `HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\BIOS`
- Check that `SystemManufacturer` and `SystemProductName` display your modified values

### Step 7: Reverting to Original Settings

When you finish testing and want to restore your actual system information:

1. Launch Registry Editor (`Windows + R`, then `regedit`)
2. Go to `HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\BIOS`
3. Double-click `SystemManufacturer`
4. Paste or type your **original saved value**
5. Click OK
6. Double-click `SystemProductName`
7. Enter your **original saved value**
8. Click OK
9. Restart your computer

**Note:** On many systems, these values automatically reset to their genuine information upon restart, but it's best practice to manually restore them.

## Brand-Specific Configuration Examples

### Samsung (e.g., for Samsung Notes, Samsung Gallery)
```
SystemManufacturer: Samsung
SystemProductName: NP960XFG-KC4UK
```

### ASUS (e.g., for Armoury Crate, MyASUS)
```
SystemManufacturer: ASUSTeK COMPUTER INC.
SystemProductName: ZenBook UX425EA
```

### Lenovo (e.g., for Lenovo Vantage)
```
SystemManufacturer: LENOVO
SystemProductName: 20U9CTO1WW
```

### Dell (e.g., for Dell SupportAssist, Dell Mobile Connect)
```
SystemManufacturer: Dell Inc.
SystemProductName: XPS 13 9310
```

### HP (e.g., for HP Command Center)
```
SystemManufacturer: HP
SystemProductName: HP ENVY Laptop 13-ba1xxx
```

## Finding Model Numbers for Other Brands

To find valid model information for other manufacturers:

1. Visit the brand's official website
2. Browse their product pages or support section
3. Look for model numbers in product specifications
4. Check technical documentation or user manuals
5. Search manufacturer forums for model identifiers

## Troubleshooting Common Issues

### The Software Still Won't Install or Run
- Double-check that you entered the values with correct capitalization and spacing
- Ensure you restarted your PC after making the registry changes
- Some applications perform additional hardware verification beyond manufacturer checks
- Try using a different model number from the same brand

### Unable to Open Registry Editor
- Verify you have administrator rights on your Windows account
- Try right-clicking the Start button and selecting "Windows Terminal (Admin)" or "Command Prompt (Admin)"
- Some corporate or school computers may restrict registry access

### Forgot to Save Original Values
- Check your computer's physical case for manufacturer stickers
- Look in your computer's original documentation or box
- Open Command Prompt and type `systeminfo` to see system details
- Visit your PC manufacturer's support site and enter your serial number

### Changes Don't Persist After Restart
- This is normal behavior on some systems - the BIOS information resets automatically
- You may need to make the changes each time before launching the software
- Consider this a security feature that protects your system identity

## Important Warnings

⚠️ **Read Before Proceeding:**

- **Purpose:** This method is intended solely for evaluating software before purchasing compatible hardware
- **Licensing:** Some software terms of service may prohibit registry modification
- **System Stability:** Incorrect registry edits can cause system issues - always backup important data
- **Limitations:** Not all brand-exclusive features will work, especially those requiring specific hardware components
- **Temporary Use:** This should be a temporary testing solution, not a permanent workaround
- **Legal Compliance:** Respect software licensing agreements and purchase legitimate hardware if you decide to use the software long-term

## Legal and Ethical Considerations

This guide is designed to help consumers make informed purchasing decisions by allowing them to test software functionality before investing in new hardware. It should not be used to:

- Permanently bypass software licensing restrictions
- Violate manufacturer terms of service
- Pirate or illegally distribute software
- Circumvent paid software protections

Always purchase appropriate licenses and hardware for long-term software use.

---

**Disclaimer:** This guide is provided for educational purposes. The author assumes no responsibility for any system issues, data loss, or terms of service violations that may occur from following these instructions. Proceed at your own risk and always maintain proper backups.