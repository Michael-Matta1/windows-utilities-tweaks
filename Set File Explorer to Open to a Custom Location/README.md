# Set File Explorer to Open to a Custom Location by Default (Updated for New Windows 11 Versions)

This tweak sets File Explorer to open a custom folder by default — not just when using **Win+E**, but also when **double-clicking the File Explorer icon**, which recent Windows 11 updates broke in the original method.

> ⚠️ **Based on Shawn Brink’s original tweak**, with edits to restore compatibility with the latest Windows 11 builds (as of June 2025).

---

## ✅ What’s New

Recent updates to Windows 11 broke the icon-based launch of File Explorer. The original tweak only applied to **Win+E** by modifying the `OpenNewWindow` verb.
This version adds support for the missing `open` verb as well — so both **Win+E** and the **Explorer icon** respect your custom folder.

---

## 📄 How to Use

1. **Download** the `Custom_File_Explorer_Location_Setter.reg` file.
2. **Save** it to your desktop.
3. **Double-click** the `.reg` file to merge it.
4. When prompted, click **Run**, **Yes (UAC)**, **Yes**, and then **OK**.
5. **Restart File Explorer** via Task Manager (or reboot) to apply changes.
6. Now test both **Win+E** and **double-clicking the File Explorer icon** — they should open your chosen folder.

---


## 📝 Customize the Folder Path

You can customize the folder that File Explorer opens to in **either of two ways**:

---

### 🔹 Option 1: Edit the `.reg` File

1. **Right-click** the `.reg` file and choose **Edit**.
2. Replace both instances of:

   ```reg
   "Explorer \"C:\\Windows\""
   ```

   with your desired folder path. For example:

   ```reg
   "Explorer \"C:\\Users\\userName\\My Data\""
   ```
3. **Save** the file.
4. **Double-click** the file to merge it into the registry.
5. **Restart File Explorer** (or reboot) to apply changes.

---

### 🔹 Option 2: Edit via Registry Editor 

1. Press **Win + R**, type `regedit`, and press **Enter**.

2. Navigate to the first registry key:

   ```
   HKEY_CURRENT_USER\SOFTWARE\Classes\CLSID\{52205fd8-5dfb-447d-801a-d0b52f2e83e1}\shell\OpenNewWindow\command
   ```

   * In the right pane, **double-click** the `(Default)` value.
   * Replace its contents with:

     ```
     Explorer "C:\Your\Custom\Path"
     ```

3. Now navigate to the second key:

   ```
   HKEY_CURRENT_USER\SOFTWARE\Classes\CLSID\{52205fd8-5dfb-447d-801a-d0b52f2e83e1}\shell\open\command
   ```

   * Again, **double-click** the `(Default)` value.
   * Replace it with:

     ```
     Explorer "C:\Your\Custom\Path"
     ```


4. Restart File Explorer or reboot your PC.



   Make sure the path is valid and quoted properly.


---

## 🔧 Registry Keys Modified

```reg
Windows Registry Editor Version 5.00

; Win+E shortcut
[HKEY_CURRENT_USER\Software\Classes\CLSID\{52205fd8-5dfb-447d-801a-d0b52f2e83e1}\shell\OpenNewWindow\command]
@="Explorer \"C:\\Windows\""
"DelegateExecute"=""

; File Explorer icon (double-click)
[HKEY_CURRENT_USER\Software\Classes\CLSID\{52205fd8-5dfb-447d-801a-d0b52f2e83e1}\shell\open\command]
@="Explorer \"C:\\Windows\""
"DelegateExecute"=""

; ref: https://github.com/Michael-Matta1/edit-windows-features/tree/main/Set%20File%20Explorer%20to%20Open%20to%20a%20Custom%20Location 
```

---

## 🔗 References

* This updated version hosted at: [Your GitHub Repo](https://github.com/Michael-Matta1/edit-windows-features/tree/main/Set%20File%20Explorer%20to%20Open%20to%20a%20Custom%20Location)
* Original tweak by Shawn Brink: [ElevenForum Guide](https://www.elevenforum.com/t/change-folder-to-open-file-explorer-to-by-default-in-windows-11.675/)


