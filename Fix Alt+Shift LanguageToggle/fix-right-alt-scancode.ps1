# Fix Right Alt + Shift Language Toggle Bug in Windows 11
# This script fixes the specific bug where Right Alt is treated as AltGr
# Requires Administrator privileges

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Right Alt + Shift Fix for Windows 11" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "ERROR: This script requires Administrator privileges!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please:" -ForegroundColor Yellow
    Write-Host "1. Right-click PowerShell" -ForegroundColor White
    Write-Host "2. Select 'Run as Administrator'" -ForegroundColor White
    Write-Host "3. Run this script again" -ForegroundColor White
    Write-Host ""
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "Running as Administrator: OK" -ForegroundColor Green
Write-Host ""

# Registry path
$registryPath = "HKLM:\SYSTEM\CurrentControlSet\Control\Keyboard Layout"
$valueName = "Scancode Map"

# Backup current value if it exists
Write-Host "Step 1: Checking for existing Scancode Map..." -ForegroundColor Green
$backupDir = "$env:USERPROFILE\Desktop\KeyboardToggle_Backup"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

if (-not (Test-Path $backupDir)) {
    New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
}

try {
    $existingValue = Get-ItemProperty -Path $registryPath -Name $valueName -ErrorAction SilentlyContinue

    if ($existingValue) {
        $backupFile = "$backupDir\scancode_backup_$timestamp.reg"

        # Export the registry key
        $exportPath = $registryPath -replace "HKLM:\\", "HKEY_LOCAL_MACHINE\"
        reg export "$exportPath" "$backupFile" /y | Out-Null

        Write-Host "Existing Scancode Map backed up to:" -ForegroundColor Green
        Write-Host "$backupFile" -ForegroundColor Cyan
    }
    else {
        Write-Host "No existing Scancode Map found (first time setup)" -ForegroundColor Green
    }
}
catch {
    Write-Host "Note: Could not check for existing value" -ForegroundColor Yellow
}

Write-Host ""

# Apply the fix
Write-Host "Step 2: Applying Right Alt fix..." -ForegroundColor Green

try {
    # The scancode map that fixes Right Alt behavior
    # Maps Right Alt (E038) to Left Alt (0038)
    [byte[]]$scancodeMap = @(
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x02, 0x00, 0x00, 0x00,
        0x38, 0x00, 0x38, 0xE0,
        0x00, 0x00, 0x00, 0x00
    )

    Set-ItemProperty -Path $registryPath -Name $valueName -Value $scancodeMap -Type Binary

    Write-Host "Scancode Map applied successfully" -ForegroundColor Green
    Write-Host ""

}
catch {
    Write-Host "Error applying fix: $_" -ForegroundColor Red
    Write-Host ""
    Read-Host "Press Enter to exit"
    exit 1
}

# Verify
Write-Host "Step 3: Verifying changes..." -ForegroundColor Green
$newValue = Get-ItemProperty -Path $registryPath -Name $valueName -ErrorAction SilentlyContinue

if ($newValue) {
    Write-Host "Scancode Map is now set" -ForegroundColor Green
}
else {
    Write-Host "Warning: Could not verify the change" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "FIX APPLIED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "IMPORTANT:" -ForegroundColor Yellow
Write-Host "You MUST restart your computer for this to take effect!" -ForegroundColor White
Write-Host ""
Write-Host "After restart:" -ForegroundColor Cyan
Write-Host "- Right Alt + Shift will work in BOTH directions" -ForegroundColor White
Write-Host "- SecondLanguage to English switching will be bidirectional" -ForegroundColor White
Write-Host ""
Write-Host "To revert this change later:" -ForegroundColor Yellow
Write-Host "- Run revert-right-alt-scancode.ps1 as Administrator" -ForegroundColor White
Write-Host ""

$restart = Read-Host "Do you want to restart now? (Y/N)"
if ($restart -eq "Y" -or $restart -eq "y") {
    Write-Host ""
    Write-Host "Restarting in 10 seconds..." -ForegroundColor Yellow
    Write-Host "Press Ctrl+C to cancel" -ForegroundColor Yellow
    Start-Sleep -Seconds 10
    Restart-Computer -Force
}
else {
    Write-Host ""
    Write-Host "Please restart manually when ready" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
}
