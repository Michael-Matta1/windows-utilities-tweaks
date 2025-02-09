# Set File Explorer to Open to a Custom Location by Default

This guide provides step-by-step instructions to set File Explorer to open to a custom location by default on Windows 11.

## Steps

1. **Download** the `Set_File_Explorer_to_open_to_custom_location.reg` file.
2. **Save** the `.reg` file to your desktop.
3. **Merge** the file by double-clicking/tapping on the downloaded `.reg` file.
4. When prompted, **click/tap** on `Run`, `Yes (UAC)`, `Yes`, and `OK` to approve the merge.
5. You can now **delete** the downloaded `.reg` file if you like.
6. **Open Registry Editor** (`regedit.exe`).
7. **Navigate** to the following registry key in the left pane of Registry Editor:
   ```
   HKEY_CURRENT_USER\SOFTWARE\Classes\CLSID\{52205fd8-5dfb-447d-801a-d0b52f2e83e1}\shell\opennewwindow\command
   ```
8. In the right pane of the `command` key, **double-click/tap** on the `(Default)` string value (`REG_SZ`) to modify it.
9. **Modify the path**: Change the red part of the Explorer `"C:\Windows"` path to the folder or drive path you want File Explorer to open to by default, and then **click/tap on OK**.
10. You can now **close Registry Editor** if you like.

## Reference
This method is sourced from [ElevenForum](https://www.elevenforum.com/t/change-folder-to-open-file-explorer-to-by-default-in-windows-11.675/).

