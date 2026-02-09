# Revert Right Alt + Shift Fix
# This script removes the Scancode Map and restores default behavior
# Requires Administrator privileges

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Revert Right Alt Fix" -ForegroundColor Cyan
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

# Check if there are backups
$backupDir = "$env:USERPROFILE\Desktop\KeyboardToggle_Backup"

if (Test-Path $backupDir) {
    $backupFiles = Get-ChildItem -Path $backupDir -Filter "scancode_backup_*.reg" | Sort-Object LastWriteTime -Descending
    
    if ($backupFiles.Count -gt 0) {
        Write-Host "Found backup files:" -ForegroundColor Green
        for ($i = 0; $i -lt $backupFiles.Count; $i++) {
            Write-Host "  [$i] $($backupFiles[$i].Name) - $($backupFiles[$i].LastWriteTime)" -ForegroundColor Cyan
        }
        Write-Host "  [D] Just delete Scancode Map (restore Windows default)" -ForegroundColor Cyan
        Write-Host ""
        
        $choice = Read-Host "Enter choice (0-$($backupFiles.Count - 1), D, or Q to quit)"
        
        if ($choice -eq "Q" -or $choice -eq "q") {
            exit
        } elseif ($choice -eq "D" -or $choice -eq "d") {
            # Will delete below
        } else {
            # Restore from backup
            try {
                $selectedBackup = $backupFiles[[int]$choice]
                Write-Host ""
                Write-Host "Restoring from: $($selectedBackup.Name)" -ForegroundColor Yellow
                
                reg import "$($selectedBackup.FullName)" /y
                
                Write-Host "✓ Backup restored successfully" -ForegroundColor Green
                Write-Host ""
                Write-Host "Please restart your computer for changes to take effect" -ForegroundColor Yellow
                Read-Host "Press Enter to exit"
                exit
            } catch {
                Write-Host "Error restoring backup: $_" -ForegroundColor Red
                Write-Host "Will delete Scancode Map instead" -ForegroundColor Yellow
                Start-Sleep -Seconds 2
            }
        }
    }
}

# Delete the Scancode Map
Write-Host "Removing Scancode Map..." -ForegroundColor Yellow
Write-Host ""

try {
    $exists = Get-ItemProperty -Path $registryPath -Name $valueName -ErrorAction SilentlyContinue
    
    if ($exists) {
        Remove-ItemProperty -Path $registryPath -Name $valueName -Force
        Write-Host "✓ Scancode Map removed successfully" -ForegroundColor Green
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "REVERT SUCCESSFUL!" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "Windows will now use default keyboard behavior" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "IMPORTANT:" -ForegroundColor Yellow
        Write-Host "You MUST restart your computer for this to take effect!" -ForegroundColor White
        
    } else {
        Write-Host "ℹ Scancode Map was not set (nothing to remove)" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "Your keyboard is already using Windows defaults" -ForegroundColor Green
    }
    
} catch {
    Write-Host "✗ Error removing Scancode Map: $_" -ForegroundColor Red
}

Write-Host ""
$restart = Read-Host "Do you want to restart now? (Y/N)"
if ($restart -eq "Y" -or $restart -eq "y") {
    Write-Host ""
    Write-Host "Restarting in 10 seconds..." -ForegroundColor Yellow
    Write-Host "Press Ctrl+C to cancel" -ForegroundColor Yellow
    Start-Sleep -Seconds 10
    Restart-Computer -Force
} else {
    Write-Host ""
    Write-Host "Please restart manually when ready" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
}
