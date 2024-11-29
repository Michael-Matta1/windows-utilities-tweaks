# Copy Path Without Quotes for Windows 11

This project adds a new context menu option, **"Copy Path Without Quotes"**, to Windows 11. It is a convenient addition to the existing **"Copy as path"** option, which copies file or folder paths enclosed in quotes. This new feature directly copies paths without quotes, saving time in scenarios where quoted paths are not needed.

## Why I Made This

While working with file paths, I found it frustrating to delete the quotes every time I used the **"Copy as path"** feature in Windows 11. This inspired me to create this simple and efficient solution. I hope it helps others as much as it helps me.

**Important Note:** Ensure that the directories in the file path are in English to avoid errors.

---

## Features

- Adds a **"Copy Path Without Quotes"** option to the right-click context menu.
- Works seamlessly alongside the original **"Copy as path"** option.
- Simple to install and configure.

---

## Installation

### Prerequisites

- Windows 11
- Administrator privileges

### Steps

1. **Download the Files**
   - Copy the provided `CopyPathWithoutQuotes.vbs` and `CopyPathWithoutQuotes.bat` files to a directory of your choice. For example:
     ```
     C:\Users\YourName\Scripts\CopyPathWithoutQuotes\
     ```

2. **Registry Changes**
   - Open the Registry Editor by pressing `Win + R`, typing `regedit`, and pressing `Enter`.
   - Navigate to:
     ```
     HKEY_CLASSES_ROOT\AllFilesystemObjects\shell
     ```
   - Create a new key named `CopyPathWithoutQuotes`.
   - Within the `CopyPathWithoutQuotes` key, create a subkey named `command`.
   - Set the default value of the `command` key to:
     ```
     wscript.exe "C:\Path\To\Script\CopyPathWithoutQuotes.vbs" "%1"
     ```
     `Note: You can use the .bat file but it will open cmd and we don't want that`

     Replace `C:\Path\To\Script\` with the actual path where you placed the `CopyPathWithoutQuotes.vbs` file. For example:
     ```
     wscript.exe "C:\Users\YourName\Scripts\CopyPathWithoutQuotes\CopyPathWithoutQuotes.vbs" "%1"
     ```

### Optional: Change Context Menu Position

To adjust the placement of the **"Copy Path Without Quotes"** option in the context menu, you can add a `Position` value to its registry entry. While Windows does not allow precise ordering of context menu items, the `Position` value lets you influence whether the item appears at the top or bottom of the menu.

#### Changing Position in the Context Menu

1. **Open Registry Editor**:
   - Press `Win + R`, type `regedit`, and press `Enter`.

2. **Navigate to the following registry key**:
   ```
   HKEY_CLASSES_ROOT\AllFilesystemObjects\shell\CopyPathWithoutQuotes
   ```

3. **Add a `Position` value**:
   - With the `CopyPathWithoutQuotes` key selected, right-click in the right pane, choose `New -> String Value`, and name it `Position`.
   - Double-click the `Position` value and set its data to one of the following:
     - `Top`: Places the menu item at the top of the context menu.
     - `Bottom`: Places the menu item at the bottom of the context menu.

#### Example Configurations

- **Setting the item at the top of the menu**:
    ```plaintext
    HKEY_CLASSES_ROOT\AllFilesystemObjects\shell\CopyPathWithoutQuotes
        (Default)    REG_SZ    Copy Path Without Quotes
        Position     REG_SZ    Top
    ```

- **Setting the item at the bottom of the menu**:
    ```plaintext
    HKEY_CLASSES_ROOT\AllFilesystemObjects\shell\CopyPathWithoutQuotes
        (Default)    REG_SZ    Copy Path Without Quotes
        Position     REG_SZ    Bottom
    ```

---

## Usage

1. Right-click on any file or folder in Windows Explorer.
2. Select **"Copy Path Without Quotes"** from the context menu.
3. Paste the path where needed—no quotes included!

---

## Troubleshooting

- Ensure that the directories in the file path are in English to avoid errors.
- If changes do not appear immediately, restart Windows Explorer:
  1. Press `Ctrl + Shift + Esc` to open Task Manager.
  2. Find and select **Windows Explorer**.
  3. Click **Restart**.

---

## Contributing

Feel free to fork the repository and submit pull requests with improvements or bug fixes.

---

## License

This project is licensed under the MIT License.

