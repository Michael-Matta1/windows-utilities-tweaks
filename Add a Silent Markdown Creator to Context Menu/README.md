# Silent CreateMarkdown

Add a "Create Markdown File" option to the top of the Windows right-click context menu. Creates new `.md` files instantly without any popup windows.

## Installation

1. **Copy** `CreateMarkdownSilent.vbs` to `C:\Windows\System32\`
2. **Double-click** `CreateMarkdown.reg` to add the context menu entry
3. **Right-click** in any folder and select "Create Markdown File"

## Files

- `CreateMarkdown.reg` - Adds the context menu entry
- `CreateMarkdownSilent.vbs` - Creates the markdown file silently
- `README.md` - Installation guide

## How it works

Right-clicking in a folder creates `NewMarkdown.md`. If that exists, it creates `NewMarkdown(1).md`, `NewMarkdown(2).md`, etc.

## Uninstall

Run this registry file to remove:
```registry
Windows Registry Editor Version 5.00

[-HKEY_CLASSES_ROOT\Directory\Background\shell\CreateMarkdownFile]
```

## Requirements

- Administrator access for installation
- If you are using Windows 11, you can [enable classic context menu](https://github.com/Michael-Matta1/windows-utilities-tweaks/blob/de4376356469cf77d5c0a3c59560804b47b33245/Remove%20or%20Disable%20Context%20Menu%20Items/enable_classic_context_menu.reg) if you want to see it without clicking "Show more options"