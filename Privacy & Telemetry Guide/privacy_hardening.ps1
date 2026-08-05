# source of this file is https://github.com/Michael-Matta1/windows-utilities-tweaks (c) Michael-Matta1

# ============================================================
# Windows Privacy & Telemetry Hardening Script
# Run as Administrator in PowerShell
# ============================================================

# Check for admin rights
if (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]"Administrator")) {
    Write-Host "ERROR: Please run this script as Administrator!" -ForegroundColor Red
    exit
}

Write-Host "`n================================================" -ForegroundColor Cyan
Write-Host "   Windows Privacy Hardening Script" -ForegroundColor Cyan
Write-Host "================================================`n" -ForegroundColor Cyan

# Helper functions
function Set-Reg($path, $name, $value, $type = "DWord") {
    if (-not (Test-Path $path)) {
        try {
            New-Item -Path $path -Force -ErrorAction Stop | Out-Null
        }
        catch {
            Write-Host "  [!] Failed to create registry key $path : $_" -ForegroundColor Yellow
        }
    }
    try {
        Set-ItemProperty -Path $path -Name $name -Value $value -Type $type -Force -ErrorAction Stop
    }
    catch {
        Write-Host "  [!] Failed to set $name under $path : $_" -ForegroundColor Yellow
        if ($path -like "*Explorer\Advanced*") {
            Write-Host "      Note: $name is protected on Windows 11 24H2+ (UserChoice Protection Driver)." -ForegroundColor DarkYellow
            Write-Host "      Use Settings > Personalization > Taskbar instead, or Group Policy (Windows Components > Widgets / Turn off Copilot)." -ForegroundColor DarkYellow
        }
    }
}

function Disable-Svc($name) {
    $svc = Get-Service -Name $name -ErrorAction SilentlyContinue
    if ($svc) {
        Stop-Service -Name $name -Force -ErrorAction SilentlyContinue
        Set-Service -Name $name -StartupType Disabled -ErrorAction SilentlyContinue
        Write-Host "  [OK] Disabled service: $name" -ForegroundColor Green
    }
    else {
        Write-Host "  [--] Service not found (skipping): $name" -ForegroundColor Yellow
    }
}

# ============================================================
# SECTION 1: REGISTRY - Telemetry & Data Collection
# ============================================================
Write-Host "[1/9] Applying Registry - Telemetry & Data Collection..." -ForegroundColor Yellow

$dc = "HKLM:\SOFTWARE\Policies\Microsoft\Windows\DataCollection"
Set-Reg $dc "AllowTelemetry"                  0
Set-Reg $dc "AllowCommercialDataPipeline"      0
Set-Reg $dc "AllowDesktopAnalyticsProcessing"  0
Set-Reg $dc "AllowDeviceNameInTelemetry"       0
Set-Reg $dc "LimitDiagnosticLogCollection"     0
Set-Reg $dc "LimitDumpCollection"              0

Write-Host "  [OK] Telemetry registry keys set" -ForegroundColor Green

# ============================================================
# SECTION 2: REGISTRY - Privacy Settings
# ============================================================
Write-Host "`n[2/9] Applying Registry - Privacy Settings..." -ForegroundColor Yellow

# Advertising ID
Set-Reg "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo" "Enabled" 0

# Activity History
$sys = "HKLM:\SOFTWARE\Policies\Microsoft\Windows\System"
Set-Reg $sys "PublishUserActivities" 0
Set-Reg $sys "UploadUserActivities"  0

# Input & Handwriting
$inp = "HKCU:\SOFTWARE\Microsoft\InputPersonalization"
Set-Reg $inp "RestrictImplicitInkCollection"  1
Set-Reg $inp "RestrictImplicitTextCollection" 1

# Tailored Experiences
Set-Reg "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Privacy" "TailoredExperiencesWithDiagnosticDataEnabled" 0

# Speech data collection
Set-Reg "HKCU:\SOFTWARE\Microsoft\Speech_OneCore\Settings\OnlineSpeechPrivacy" "HasAccepted" 0

# Windows Error Reporting
$wer = "HKLM:\SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting"
Set-Reg $wer "Disabled"               1
Set-Reg $wer "DontSendAdditionalData" 1
Set-Reg "$wer\Consent" "DefaultConsent" 1

# App Privacy - Background apps
$ap = "HKLM:\SOFTWARE\Policies\Microsoft\Windows\AppPrivacy"
Set-Reg $ap "LetAppsRunInBackground" 2

# Cloud Content
$cc = "HKLM:\SOFTWARE\Policies\Microsoft\Windows\CloudContent"
Set-Reg $cc "DisableWindowsConsumerFeatures"     1
Set-Reg $cc "DisableConsumerAccountStateContent" 1
Set-Reg $cc "DisableSoftLanding"                 1

# Explorer privacy
$adv = "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced"
Set-Reg $adv "ShowSyncProviderNotifications" 0
Set-Reg $adv "ShowCopilotButton"             0
Set-Reg $adv "TaskbarDa"                     0

# App Compatibility Telemetry
$ac = "HKLM:\SOFTWARE\Policies\Microsoft\Windows\AppCompat"
Set-Reg $ac "AITEnable"        0
Set-Reg $ac "DisableInventory" 1
Set-Reg $ac "DisableUAR"       1

# Search
$ws = "HKLM:\SOFTWARE\Policies\Microsoft\Windows\Windows Search"
Set-Reg $ws "AllowSearchToUseLocation" 0
Set-Reg $ws "DisableWebSearch"         1

Write-Host "  [OK] Privacy registry keys set" -ForegroundColor Green

# ============================================================
# SECTION 3: SERVICES - Disable Telemetry Services
# ============================================================
Write-Host "`n[3/9] Disabling Telemetry & Privacy Services..." -ForegroundColor Yellow

Disable-Svc "DiagTrack"               # Connected User Experiences and Telemetry
Disable-Svc "dmwappushservice"        # WAP Push Routing (telemetry helper)
Disable-Svc "WerSvc"                  # Windows Error Reporting
Disable-Svc "wercplsupport"           # Problem Reports Control Panel
Disable-Svc "PcaSvc"                  # Program Compatibility Assistant
Disable-Svc "wisvc"                   # Windows Insider Service
Disable-Svc "DoSvc"                   # Delivery Optimization (uses your bandwidth for others)
Disable-Svc "MapsBroker"              # Downloaded Maps Manager
Disable-Svc "RetailDemo"              # Retail Demo Service
Disable-Svc "wuqisvc"                 # Microsoft Usage and Quality Insights
Disable-Svc "Wecsvc"                  # Windows Event Collector
Disable-Svc "RemoteRegistry"          # Remote Registry
Disable-Svc "tzautoupdate"            # Auto Time Zone Updater
Disable-Svc "SQLTELEMETRY`$SQLEXPRESS" # SQL Server CEIP Telemetry (if SQL Server is installed)

# ============================================================
# SECTION 4: GROUP POLICY (via Registry.pol using PolicyFileEditor)
# ============================================================
Write-Host "`n[4/9] Applying Group Policy settings..." -ForegroundColor Yellow

$polModule = Get-Module -ListAvailable -Name PolicyFileEditor
if (-not $polModule) {
    Write-Host "  [!] PolicyFileEditor not installed. Installing..." -ForegroundColor Yellow
    Install-Module -Name PolicyFileEditor -Force -Scope CurrentUser
}

Import-Module PolicyFileEditor

$machinePol = "C:\Windows\System32\GroupPolicy\Machine\Registry.pol"

function Set-GPReg($key, $name, $value, $type = "DWord") {
    Set-PolicyFileEntry -Path $machinePol -Key $key -ValueName $name -Data $value -Type $type
}

# Data Collection
Set-GPReg "Software\Policies\Microsoft\Windows\DataCollection" "AllowTelemetry"                  0
Set-GPReg "Software\Policies\Microsoft\Windows\DataCollection" "AllowCommercialDataPipeline"      0
Set-GPReg "Software\Policies\Microsoft\Windows\DataCollection" "AllowDesktopAnalyticsProcessing"  0
Set-GPReg "Software\Policies\Microsoft\Windows\DataCollection" "AllowDeviceNameInTelemetry"       0
Set-GPReg "Software\Policies\Microsoft\Windows\DataCollection" "LimitDiagnosticLogCollection"     0
Set-GPReg "Software\Policies\Microsoft\Windows\DataCollection" "LimitDumpCollection"              0

# Cloud Content
Set-GPReg "Software\Policies\Microsoft\Windows\CloudContent" "DisableWindowsConsumerFeatures"     1
Set-GPReg "Software\Policies\Microsoft\Windows\CloudContent" "DisableConsumerAccountStateContent" 1
Set-GPReg "Software\Policies\Microsoft\Windows\CloudContent" "DisableSoftLanding"                 1

# Windows Error Reporting
Set-GPReg "Software\Policies\Microsoft\Windows\Windows Error Reporting"          "Disabled"               1
Set-GPReg "Software\Policies\Microsoft\Windows\Windows Error Reporting"          "DontSendAdditionalData" 1
Set-GPReg "Software\Policies\Microsoft\Windows\Windows Error Reporting\Consent"  "DefaultConsent"         1

# App Compatibility
Set-GPReg "Software\Policies\Microsoft\Windows\AppCompat" "AITEnable"        0
Set-GPReg "Software\Policies\Microsoft\Windows\AppCompat" "DisableInventory" 1
Set-GPReg "Software\Policies\Microsoft\Windows\AppCompat" "DisableUAR"       1

# Search
Set-GPReg "Software\Policies\Microsoft\Windows\Windows Search" "AllowSearchToUseLocation" 0
Set-GPReg "Software\Policies\Microsoft\Windows\Windows Search" "DisableWebSearch"         1

# App Privacy
Set-GPReg "Software\Policies\Microsoft\Windows\AppPrivacy" "LetAppsRunInBackground" 2

Write-Host "  [OK] Group Policy entries written" -ForegroundColor Green

# ============================================================
# SECTION 5: CLEAR TRACKING HISTORY
# ============================================================
Write-Host "`n[5/9] Clearing Explorer tracking history..." -ForegroundColor Yellow

$explorerBase = "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer"
Remove-Item "$explorerBase\RunMRU"        -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "$explorerBase\TypedPaths"    -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "$explorerBase\WordWheelQuery" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "  [OK] Cleared Run history, typed paths, search history" -ForegroundColor Green

# ============================================================
# SECTION 6: TASK SCHEDULER - Disable Telemetry Tasks
# ============================================================
Write-Host "`n[6/9] Disabling telemetry scheduled tasks..." -ForegroundColor Yellow

function Disable-Task($path, $name) {
    $task = Get-ScheduledTask -TaskPath $path -TaskName $name -ErrorAction SilentlyContinue
    if ($task) {
        Disable-ScheduledTask -TaskPath $path -TaskName $name -ErrorAction SilentlyContinue | Out-Null
        Write-Host "  [OK] Disabled task: $name" -ForegroundColor Green
    } else {
        Write-Host "  [--] Task not found (skipping): $name" -ForegroundColor Yellow
    }
}

# Application Experience
Disable-Task "\Microsoft\Windows\Application Experience\" "Microsoft Compatibility Appraiser"
Disable-Task "\Microsoft\Windows\Application Experience\" "Microsoft Compatibility Appraiser Exp"
Disable-Task "\Microsoft\Windows\Application Experience\" "StartupAppTask"
Disable-Task "\Microsoft\Windows\Application Experience\" "PcaPatchDbTask"
Disable-Task "\Microsoft\Windows\Application Experience\" "SdbinstMergeDbTask"
Disable-Task "\Microsoft\Windows\Application Experience\" "ProgramDataUpdater"  # absent on many installs, skipped gracefully
Disable-Task "\Microsoft\Windows\Application Experience\" "AitAgent"            # absent on many installs, skipped gracefully

# Autochk
Disable-Task "\Microsoft\Windows\Autochk\"                "Proxy"

# Cloud Experience Host
Disable-Task "\Microsoft\Windows\CloudExperienceHost\"    "CreateObjectTask"

# Customer Experience Improvement Program
Disable-Task "\Microsoft\Windows\Customer Experience Improvement Program\" "Consolidator"
Disable-Task "\Microsoft\Windows\Customer Experience Improvement Program\" "UsbCeip"
Disable-Task "\Microsoft\Windows\Customer Experience Improvement Program\" "KernelCeipTask"

# Disk Diagnostic
Disable-Task "\Microsoft\Windows\DiskDiagnostic\"         "Microsoft-Windows-DiskDiagnosticDataCollector"
Disable-Task "\Microsoft\Windows\DiskDiagnostic\"         "Microsoft-Windows-DiskDiagnosticResolver"

# Feedback / SIUF
Disable-Task "\Microsoft\Windows\Feedback\Siuf\"          "DmClient"
Disable-Task "\Microsoft\Windows\Feedback\Siuf\"          "DmClientOnScenarioDownload"

# Feature Flighting / A-B Testing
Disable-Task "\Microsoft\Windows\Flighting\FeatureConfig\" "BootstrapUsageDataReporting"

# Maps
Disable-Task "\Microsoft\Windows\Maps\"                   "MapsUpdateTask"

# NetTrace
Disable-Task "\Microsoft\Windows\NetTrace\"               "GatherNetworkInfo"  # absent on many installs, skipped gracefully

# PI / Software Quality Metrics
Disable-Task "\Microsoft\Windows\PI\"                     "Sqm-Tasks"

# User Profile Service
Disable-Task "\Microsoft\Windows\User Profile Service\"   "HiveUploadTask"

# Windows Error Reporting
Disable-Task "\Microsoft\Windows\Windows Error Reporting\" "QueueReporting"

# Windows Recall (AI screenshot logging)
Disable-Task "\Microsoft\Windows\WindowsAI\Recall\"       "InitialConfiguration"

# ============================================================
# SECTION 7: HOSTS FILE - Block Telemetry Domains
# ============================================================
Write-Host "`n[7/9] Blocking telemetry domains in hosts file..." -ForegroundColor Yellow

$hostsPath = "$env:SystemRoot\System32\drivers\etc\hosts"
$telemetryDomains = @(
    "0.0.0.0 telemetry.microsoft.com",
    "0.0.0.0 vortex.data.microsoft.com",
    "0.0.0.0 vortex-win.data.microsoft.com",
    "0.0.0.0 telecommand.telemetry.microsoft.com",
    "0.0.0.0 oca.telemetry.microsoft.com",
    "0.0.0.0 sqm.telemetry.microsoft.com",
    "0.0.0.0 watson.telemetry.microsoft.com",
    "0.0.0.0 redir.metaservices.microsoft.com",
    "0.0.0.0 choice.microsoft.com",
    "0.0.0.0 df.telemetry.microsoft.com",
    "0.0.0.0 reports.wes.df.telemetry.microsoft.com",
    "0.0.0.0 wes.df.telemetry.microsoft.com",
    "0.0.0.0 statsfe2.ws.microsoft.com",
    "0.0.0.0 feedback.microsoft-hohm.com",
    "0.0.0.0 feedback.search.microsoft.com",
    "0.0.0.0 feedback.windows.com"
)

$hostsContent = Get-Content $hostsPath -ErrorAction SilentlyContinue
$newBlocked = @()
foreach ($entry in $telemetryDomains) {
    $domain = $entry.Split(" ")[1]
    if ($hostsContent -notmatch [regex]::Escape($domain)) {
        $newBlocked += $entry
        Write-Host "  [OK] Blocked: $domain" -ForegroundColor Green
    } else {
        Write-Host "  [--] Already blocked (skipping): $domain" -ForegroundColor Yellow
    }
}
if ($newBlocked.Count -gt 0) {
    $retries = 5; $delayMs = 500
    for ($i = 0; $i -lt $retries; $i++) {
        try {
            Add-Content -Path $hostsPath -Value $newBlocked -ErrorAction Stop
            break
        } catch {
            if ($i -eq $retries - 1) {
                Write-Host "  [FAIL] Could not write to hosts file ($env:SystemRoot\System32\drivers\etc\hosts): $_" -ForegroundColor Red
            } else {
                Start-Sleep -Milliseconds $delayMs
            }
        }
    }
}

# ============================================================
# SECTION 8: FIREWALL - Block Telemetry Executables
# ============================================================
Write-Host "`n[8/9] Adding firewall rules to block telemetry..." -ForegroundColor Yellow

$fwRules = @(
    @{ Name = "Block-DiagTrack-Telemetry";   Program = "$env:SystemRoot\System32\CompatTelRunner.exe" },
    @{ Name = "Block-WerFault-Reporting";    Program = "$env:SystemRoot\System32\WerFault.exe" },
    @{ Name = "Block-WerFaultSecure";        Program = "$env:SystemRoot\System32\WerFaultSecure.exe" }
)

foreach ($rule in $fwRules) {
    $existing = Get-NetFirewallRule -DisplayName $rule.Name -ErrorAction SilentlyContinue
    if (-not $existing) {
        New-NetFirewallRule -DisplayName $rule.Name -Direction Outbound -Program $rule.Program -Action Block -ErrorAction SilentlyContinue | Out-Null
        Write-Host "  [OK] Firewall rule added: $($rule.Name)" -ForegroundColor Green
    } else {
        Write-Host "  [--] Rule already exists (skipping): $($rule.Name)" -ForegroundColor Yellow
    }
}

# ============================================================
# SECTION 9: APPLY GROUP POLICY
# ============================================================
Write-Host "`n[9/9] Forcing Group Policy update..." -ForegroundColor Yellow
gpupdate /force | Out-Null
Write-Host "  [OK] Group Policy updated" -ForegroundColor Green

# ============================================================
# DONE
# ============================================================
Write-Host "`n================================================" -ForegroundColor Cyan
Write-Host "   All privacy settings applied successfully!" -ForegroundColor Cyan
Write-Host "   Please RESTART your PC for full effect." -ForegroundColor Cyan
Write-Host "================================================`n" -ForegroundColor Cyan
