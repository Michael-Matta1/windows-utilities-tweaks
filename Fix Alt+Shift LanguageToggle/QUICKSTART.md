# Quick Start Guide

**Problem:** Right Alt + Shift only switches one way (English → Second Language ✓, but Second Language → English ✗)

**Solution:** One command + restart

## TL;DR - Fast Fix

### Step 1: Run This Command (as Administrator)

Press `Win + X`, select "Terminal (Admin)", then paste:

```cmd
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Keyboard Layout" /v "Scancode Map" /t REG_BINARY /d 000000000000000002000000380038E000000000 /f
```

### Step 2: Restart Your Computer

**That's it!** Right Alt + Shift now works in both directions.

---

## To Undo Later

```cmd
reg delete "HKLM\SYSTEM\CurrentControlSet\Control\Keyboard Layout" /v "Scancode Map" /f
```

Then restart.

---

## Need More Details?

See [README.md](README.md) for:
- Why this happens
- Alternative installation methods  
- Troubleshooting
- Technical explanation

## What Does This Do?

Remaps Right Alt to behave like Left Alt, eliminating the AltGr conflict that prevents language switching.

**Safe?** Yes. Fully reversible, no system files modified.

**Will it break anything?** No. Only affects Right Alt key behavior.
