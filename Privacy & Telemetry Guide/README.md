# 🛡️ Windows Privacy & Telemetry Hardening Guide

A comprehensive guide to hardening Windows 11 privacy by disabling telemetry, data collection, and unnecessary Microsoft tracking — through Group Policy, Registry, and Services.

> **📌 Note:** This guide covers the deeper, less obvious privacy settings that are **not** easily accessible through `Settings → Privacy & Security` in Windows. Before going through this guide, make sure you've already gone through the standard Windows privacy settings (`Settings → Privacy & Security`) and turned off everything you don't need there — this guide picks up where that leaves off.

> **🎛️ Manual configuration is recommended.** While an automation script is provided, going through the steps manually gives you full visibility and control over exactly what is being changed on your system. You may not want or need every single setting — the manual approach lets you pick and choose. The script is best suited for re-applying known settings on a fresh install.

> **⚡ Want to skip the manual steps?**
> A PowerShell script (`privacy_hardening.ps1`) is included in this repo that automates everything below. See [Running the Script](#-running-the-script) at the bottom.
> Otherwise, follow this guide for full manual control over what you apply.

---

## 📋 Table of Contents

- [Prerequisites](#-prerequisites)
- [Important Notes](#-important-notes)
- [Step 1 — Group Policy (gpedit.msc)](#step-1--group-policy-gpeditmsc)
- [Step 2 — Registry Editor](#step-2--registry-editor)
- [Step 3 — Services](#step-3--services)
- [Step 4 — Task Scheduler](#step-4--task-scheduler)
- [Step 5 — Hosts File](#step-5--hosts-file)
- [Step 6 — Firewall Rules](#step-6--firewall-rules)
- [Step 7 — Clear Tracking History](#step-7--clear-tracking-history)
- [Step 8 — Apply Changes](#step-8--apply-changes)
- [Verifying Your Changes](#-verifying-your-changes)
- [Running the Script](#-running-the-script)
- [Bonus — Extra Tweaks (Not Privacy/Telemetry Related)](#-bonus--extra-tweaks-not-privacytelemetry-related)

---

## 🔧 Prerequisites

- **Windows 11 Pro or Enterprise** — Group Policy Editor (`gpedit.msc`) is not available on Windows Home
- **Administrator account** — Required for all steps
- **PowerShell** — Run as Administrator for registry and service changes

> ⚠️ **Windows Home users**: Group Policy steps won't work. You can still apply all Registry and Services changes manually.

---

## ⚠️ Important Notes

- **Always create a restore point** before making system changes:
  `Control Panel → System → System Protection → Create`
- **Export a registry backup** before editing:
  ```powershell
  reg export HKLM C:\backup_HKLM.reg
  reg export HKCU C:\backup_HKCU.reg
  ```
- **Some telemetry cannot be fully eliminated** on Windows 11 Home — Microsoft enforces a baseline level regardless of policy.
- After all changes, always run `gpupdate /force` in an Administrator CMD/PowerShell to apply Group Policy changes immediately.
- A **restart is recommended** after completing all steps.

---

## Step 1 — Group Policy (gpedit.msc)

**What this does:** Group Policy lets you enforce system-wide rules that override default Windows behavior. The settings here tell Windows to stop collecting diagnostic data, sending telemetry, showing ads, and using cloud-based features that phone home to Microsoft.

Open `gpedit.msc` (press `Win + R`, type `gpedit.msc`, press Enter).

---

### 1.1 — Data Collection & Telemetry

**Path:** `Computer Configuration → Administrative Templates → Windows Components → Data Collection and Preview Builds`

| Setting | Value | Purpose |
|---|---|---|
| `Allow Diagnostic Data` | **Disabled** | Stops Windows from sending diagnostic/telemetry data to Microsoft |
| `Allow commercial data pipeline` | **Disabled** | Prevents data from being used for Microsoft 365 analytics |
| `Allow Desktop Analytics Processing` | **Disabled** | Disables Desktop Analytics data reporting |
| `Allow device name to be sent in Windows diagnostics` | **Disabled** | Stops your PC name from being included in telemetry reports |
| `Limit Diagnostic Log Collection` | **Disabled** | Restricts how many diagnostic logs Windows collects |
| `Limit Dump Collection` | **Disabled** | Stops crash dump files from being collected and sent |
| `Configure Connected User Experiences and Telemetry` | **Disabled** | Disables the core telemetry service via policy |
| `Toggle user control over Insider builds` | **Disabled** | Prevents enrolling the PC into Microsoft's beta testing program |
| `Do not show feedback notifications` | **Enabled** | Removes "How are you enjoying Windows?" popups |
| `Disable OneSettings Downloads` | **Enabled** | Stops Windows from downloading remote configuration changes |
| `Enable OneSettings Auditing` | **Disabled** | Disables logging of OneSettings activity |
| `Configure Authenticated Proxy usage` | **Disabled** | Prevents telemetry from routing through a proxy |
| `Configure collection of browsing data for Desktop Analytics` | **Disabled** | Stops browser data being included in analytics |

---

### 1.2 — Application Compatibility (Telemetry)

**Path:** `Computer Configuration → Administrative Templates → Windows Components → Application Compatibility`

| Setting | Value | Purpose |
|---|---|---|
| `Turn off Application Telemetry` | **Enabled** | Stops Windows from tracking which apps you use and how |
| `Turn off Inventory Collector` | **Enabled** | Disables collection of installed app/driver inventory sent to Microsoft |
| `Turn off Steps Recorder` | **Enabled** | Disables the tool that records your screen steps for "problem reporting" |

---

### 1.3 — Cloud Content

**Path:** `Computer Configuration → Administrative Templates → Windows Components → Cloud Content`

| Setting | Value | Purpose |
|---|---|---|
| `Turn off Microsoft consumer experiences` | **Enabled** | Removes app suggestions and ads in the Start Menu |
| `Turn off cloud consumer account state content` | **Enabled** | Stops Windows fetching personalized content from Microsoft's servers |
| `Do not show Windows tips` | **Enabled** | Disables Windows "did you know?" tip notifications |

---

### 1.4 — Search & Cortana

**Path:** `Computer Configuration → Administrative Templates → Windows Components → Search`

| Setting | Value | Purpose |
|---|---|---|
| `Allow Cortana` | **Disabled** | Fully removes Cortana from the system |
| `Allow search and Cortana to use location` | **Disabled** | Stops search from accessing your physical location |
| `Do not allow web search` | **Enabled** | Keeps Start Menu search local only — no Bing results |

---

### 1.5 — Windows Error Reporting

**Path:** `Computer Configuration → Administrative Templates → Windows Components → Windows Error Reporting`

| Setting | Value | Purpose |
|---|---|---|
| `Disable Windows Error Reporting` | **Enabled** | Stops crash reports from being sent to Microsoft |
| `Do not send additional data` | **Enabled** | Ensures no extra diagnostic data is attached to reports |

**Path:** `Windows Error Reporting → Consent`

| Setting | Value | Purpose |
|---|---|---|
| `Configure Default consent` | **Enabled → Always ask** | Requires explicit consent before any data is sent |

---

### 1.6 — App Privacy

**Path:** `Computer Configuration → Administrative Templates → Windows Components → App Privacy`

| Setting | Value | Purpose |
|---|---|---|
| `Let Windows apps run in the background` | **Enabled → User is in control (2)** | Gives you control over which apps run in the background instead of allowing all by default |

---

### 1.7 — Start Menu & Taskbar (User Configuration)

**Path:** `User Configuration → Administrative Templates → Start Menu and Taskbar`

| Setting | Value | Purpose |
|---|---|---|
| `Turn off user tracking` | **Enabled** | Stops Windows tracking which apps you use most frequently |
| `Do not keep history of recently opened documents` | **Enabled** | Windows won't log your recently opened files |
| `Remove balloon tips on Start Menu items` | **Enabled** | Removes notification bubbles on Start Menu |

---

### 1.8 — Remote Desktop (Security)

**Path:** `Computer Configuration → Administrative Templates → Windows Components → Remote Desktop Services → Remote Desktop Session Host → Connections`

| Setting | Value | Purpose |
|---|---|---|
| `Allow users to connect remotely using Remote Desktop Services` | **Disabled** | Closes a major remote attack vector if you don't need Remote Desktop |

---

### 1.9 — Security Options

**Path:** `Computer Configuration → Windows Settings → Security Settings → Local Policies → Security Options`

| Setting | Value | Purpose |
|---|---|---|
| `Accounts: Guest account status` | **Disabled** | Closes an open account anyone could access |
| `Interactive logon: Do not display last user name` | **Enabled** | Hides your username on the login screen |
| `User Account Control: Behavior of elevation prompt` | **Prompt for credentials** | UAC asks for password instead of just a click |

---

## Step 2 — Registry Editor

**What this does:** The Windows Registry stores low-level system and user settings. Some privacy settings can only be controlled here — either because they don't have a Group Policy equivalent, or because the policy writes to the registry anyway. These changes directly tell Windows components to stop collecting or sending certain types of data.

Open `regedit` as Administrator (`Win + R` → type `regedit` → right-click → Run as administrator).

> ⚠️ Navigate to each path exactly as written. If a key doesn't exist, right-click the parent folder → `New → Key` to create it.

---

### 2.1 — Disable Telemetry (reinforces Group Policy)

**Path:** `HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection`

| Value Name | Type | Data | Purpose |
|---|---|---|---|
| `AllowTelemetry` | DWORD | `0` | Master telemetry switch — fully disabled |
| `AllowCommercialDataPipeline` | DWORD | `0` | No data sent to Microsoft 365 analytics |
| `AllowDesktopAnalyticsProcessing` | DWORD | `0` | No Desktop Analytics data |
| `AllowDeviceNameInTelemetry` | DWORD | `0` | PC name excluded from telemetry |
| `LimitDiagnosticLogCollection` | DWORD | `0` | Diagnostic logs restricted |
| `LimitDumpCollection` | DWORD | `0` | Crash dumps not collected |

---

### 2.2 — Disable Advertising ID

**Path:** `HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo`

| Value Name | Type | Data | Purpose |
|---|---|---|---|
| `Enabled` | DWORD | `0` | Disables the unique advertising ID Windows assigns to you for targeted ads |

---

### 2.3 — Disable Activity History / Timeline

**Path:** `HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System`

| Value Name | Type | Data | Purpose |
|---|---|---|---|
| `PublishUserActivities` | DWORD | `0` | Stops Windows logging your app activity for the Timeline feature |
| `UploadUserActivities` | DWORD | `0` | Stops activity history being synced to Microsoft's cloud |
| `EnableSmartScreen` | DWORD | `0` | Disables SmartScreen cloud lookups |

---

### 2.4 — Disable Input & Handwriting Data Collection

**Path:** `HKEY_CURRENT_USER\SOFTWARE\Microsoft\InputPersonalization`

| Value Name | Type | Data | Purpose |
|---|---|---|---|
| `RestrictImplicitInkCollection` | DWORD | `1` | Stops Windows collecting handwriting/inking data to "learn your handwriting" |
| `RestrictImplicitTextCollection` | DWORD | `1` | Stops Windows collecting typing data to build a personal dictionary for Microsoft |

---

### 2.5 — Disable Tailored Experiences

**Path:** `HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Privacy`

| Value Name | Type | Data | Purpose |
|---|---|---|---|
| `TailoredExperiencesWithDiagnosticDataEnabled` | DWORD | `0` | Stops Microsoft using your diagnostic data to show you personalized tips, ads and recommendations |

---

### 2.6 — Disable Online Speech Recognition

**Path:** `HKEY_CURRENT_USER\SOFTWARE\Microsoft\Speech_OneCore\Settings\OnlineSpeechPrivacy`

> Create this key if it doesn't exist.

| Value Name | Type | Data | Purpose |
|---|---|---|---|
| `HasAccepted` | DWORD | `0` | Disables sending your voice data to Microsoft for speech recognition improvement |

---

### 2.7 — Windows Error Reporting

**Path:** `HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting`

| Value Name | Type | Data | Purpose |
|---|---|---|---|
| `Disabled` | DWORD | `1` | Disables error reporting entirely |
| `DontSendAdditionalData` | DWORD | `1` | No extra data attached to any report |

**Path:** `HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting\Consent`

| Value Name | Type | Data | Purpose |
|---|---|---|---|
| `DefaultConsent` | DWORD | `1` | Sets consent level to "Always ask" before sending |

---

### 2.8 — Explorer Privacy Tweaks

**Path:** `HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced`

| Value Name | Type | Data | Purpose |
|---|---|---|---|
| `ShowSyncProviderNotifications` | DWORD | `0` | Removes OneDrive and cloud storage ads in File Explorer |
| `ShowCopilotButton` | DWORD | `0` | Removes the Copilot button from the taskbar |
| `TaskbarDa` | DWORD | `0` | Removes the Widgets button from the taskbar |
| `TaskbarBadges` | DWORD | `0` | Removes notification count badges from taskbar icons |
| `TaskbarFlashing` | DWORD | `0` | Stops taskbar buttons flashing to get your attention |

---

### 2.9 — App Compatibility Telemetry

**Path:** `HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppCompat`

| Value Name | Type | Data | Purpose |
|---|---|---|---|
| `AITEnable` | DWORD | `0` | Disables Application Impact Telemetry |
| `DisableInventory` | DWORD | `1` | Stops Windows inventorying your installed apps and sending the list to Microsoft |
| `DisableUAR` | DWORD | `1` | Disables User Activity Reporting |

---

## Step 3 — Services

**What this does:** Windows runs background services that constantly collect data, report errors, and send telemetry even when you're not actively using your PC. Disabling these stops them from running entirely — not just pausing them.

Open `services.msc` (`Win + R` → type `services.msc` → Enter).

For each service: **double-click** → set `Startup type` to **Disabled** → if `Status` shows Running, click **Stop** first → click **OK**.

| Display Name | Service Name | Purpose of disabling |
|---|---|---|
| Connected User Experiences and Telemetry | `DiagTrack` | The main telemetry service — sends usage data to Microsoft continuously |
| Device Management WAP Push message Routing | `dmwappushservice` | Helper service for telemetry routing — no purpose for regular users |
| Windows Error Reporting Service | `WerSvc` | Sends crash reports and error data to Microsoft |
| Problem Reports Control Panel Support | `wercplsupport` | UI support for error reporting — unnecessary if WerSvc is disabled |
| Program Compatibility Assistant Service | `PcaSvc` | Reports app compatibility data back to Microsoft |
| Windows Insider Service | `wisvc` | Enrolls/manages beta Windows builds — unnecessary for stable installs |
| Delivery Optimization | `DoSvc` | Uses your internet bandwidth to upload Windows updates to other PCs |
| Downloaded Maps Manager | `MapsBroker` | Manages offline maps — rarely needed, syncs map data automatically |
| Retail Demo Service | `RetailDemo` | Only used in store demo PCs — no purpose on personal machines |
| Microsoft Usage and Quality Insights | `wuqisvc` | Collects and uploads usage and quality data to Microsoft |
| Windows Event Collector | `Wecsvc` | Collects event logs from remote machines — no purpose on a personal PC and can be a data leakage point |
| Remote Registry | `RemoteRegistry` | Allows remote editing of your registry over the network — a security risk |
| Auto Time Zone Updater | `tzautoupdate` | Automatically updates timezone — disable if you set it manually |
| SQL Server CEIP Service | `SQLTELEMETRY$SQLEXPRESS` | Sends SQL Server usage/telemetry data to Microsoft — only present if SQL Server is installed |

---

## Step 4 — Task Scheduler

**What this does:** Windows has scheduled tasks that run telemetry and data collection on a timer, completely independently of services and policies. Even if you disable all the telemetry services, these tasks can still wake up and collect/send data on a schedule. Disabling them closes this gap.

Open `taskschd.msc` (`Win + R` → type `taskschd.msc` → Enter).

Navigate to each path in the left panel under `Task Scheduler Library → Microsoft → Windows` and **right-click → Disable** each task listed:

| Folder | Task Name | What it does |
|---|---|---|
| `Application Experience` | `Microsoft Compatibility Appraiser` | Sends app compatibility and usage data to Microsoft on a schedule |
| `Application Experience` | `Microsoft Compatibility Appraiser Exp` | Variant of the above — may appear instead of or alongside the base task |
| `Application Experience` | `StartupAppTask` | Scans startup apps and reports compatibility telemetry |
| `Application Experience` | `PcaPatchDbTask` | Patches the app compatibility database used by the PCA telemetry chain |
| `Application Experience` | `SdbinstMergeDbTask` | Merges shim databases for app compatibility reporting |
| `Application Experience` | `ProgramDataUpdater` | Updates compatibility data for the Program Compatibility Assistant (absent on many installs) |
| `Application Experience` | `AitAgent` | Application Impact Telemetry agent (absent on many installs) |
| `Autochk` | `Proxy` | Telemetry proxy task — routes diagnostic data collection |
| `CloudExperienceHost` | `CreateObjectTask` | Connected to cloud-based personalization and account state |
| `Customer Experience Improvement Program` | `Consolidator` | Collects and uploads CEIP telemetry data |
| `Customer Experience Improvement Program` | `UsbCeip` | Collects USB device telemetry |
| `Customer Experience Improvement Program` | `KernelCeipTask` | Collects kernel-level diagnostic data |
| `DiskDiagnostic` | `Microsoft-Windows-DiskDiagnosticDataCollector` | Sends disk diagnostic data to Microsoft |
| `DiskDiagnostic` | `Microsoft-Windows-DiskDiagnosticResolver` | Companion task — resolves and acts on collected disk diagnostic data |
| `Feedback\Siuf` | `DmClient` | Feedback hub telemetry — sends usage signals to Microsoft |
| `Feedback\Siuf` | `DmClientOnScenarioDownload` | Downloads and triggers feedback scenarios on demand |
| `Flighting\FeatureConfig` | `BootstrapUsageDataReporting` | Reports usage data to Microsoft for Windows feature A/B testing |
| `Maps` | `MapsUpdateTask` | Downloads and syncs offline map data — pair with disabling the `MapsBroker` service |
| `NetTrace` | `GatherNetworkInfo` | Collects and logs network configuration data (absent on many installs) |
| `PI` | `Sqm-Tasks` | Software Quality Metrics — sends telemetry data to Microsoft |
| `User Profile Service` | `HiveUploadTask` | Can upload user profile hive data to Microsoft |
| `Windows Error Reporting` | `QueueReporting` | Queues collected error reports for sending to Microsoft |

> ⚠️ **Windows Recall** — see the dedicated note below before proceeding.

> 💡 Some of these tasks may not exist on your system depending on your Windows edition and install history — that's fine, just skip them.

---

### 4.1 — Windows Recall (AI Screenshot Logging)

> ⚠️ **This is one of the most significant privacy items in this entire guide.**

**Windows Recall** is an AI feature that continuously takes screenshots of everything you do on your PC and makes it searchable. Even if you never consciously enabled it, the underlying scheduled task may still be present.

**Path:** `Task Scheduler Library → Microsoft → Windows → WindowsAI → Recall`

| Task Name | What it does |
|---|---|
| `InitialConfiguration` | Sets up and initialises the Windows Recall feature |

Right-click → **Disable**.

You can also permanently disable Recall via Group Policy:

**Path:** `Computer Configuration → Administrative Templates → Windows Components → Windows AI`

| Setting | Value |
|---|---|
| `Turn off Saving Snapshots for Windows` | **Enabled** |

### How to check what you've already changed in Task Scheduler:
```powershell
Get-ScheduledTask | Where-Object {$_.State -eq 'Disabled'} | Select-Object TaskName, TaskPath | Sort-Object TaskPath
```
Any task you manually disabled will appear here.

---

## Step 5 — Hosts File

**What this does:** Blocks Microsoft's telemetry server addresses at the network level by redirecting them to `0.0.0.0` (nowhere). This is the most resilient layer — even if a Windows update resets your registry or policy settings, the domains stay blocked. It works as a safety net on top of everything else.

Open **Notepad as Administrator**, then open the file:
```
C:\Windows\System32\drivers\etc\hosts
```

Add these lines at the very bottom:
```
0.0.0.0 telemetry.microsoft.com
0.0.0.0 vortex.data.microsoft.com
0.0.0.0 vortex-win.data.microsoft.com
0.0.0.0 telecommand.telemetry.microsoft.com
0.0.0.0 oca.telemetry.microsoft.com
0.0.0.0 sqm.telemetry.microsoft.com
0.0.0.0 watson.telemetry.microsoft.com
0.0.0.0 redir.metaservices.microsoft.com
0.0.0.0 choice.microsoft.com
0.0.0.0 df.telemetry.microsoft.com
0.0.0.0 reports.wes.df.telemetry.microsoft.com
0.0.0.0 wes.df.telemetry.microsoft.com
0.0.0.0 statsfe2.ws.microsoft.com
0.0.0.0 feedback.microsoft-hohm.com
0.0.0.0 feedback.search.microsoft.com
0.0.0.0 feedback.windows.com
```

Save the file. No restart needed — takes effect immediately.

> ⚠️ Do **not** block `microsoft.com`, `update.microsoft.com`, or `windowsupdate.microsoft.com` — that will break Windows Update.

---

## Step 6 — Firewall Rules

**What this does:** Adds outbound firewall rules to block the specific Windows executables responsible for sending telemetry and error reports. Even if the services are running, they won't be able to reach Microsoft's servers.

Run in **PowerShell as Administrator**:

```powershell
# Block CompatTelRunner (compatibility & app telemetry sender)
New-NetFirewallRule -DisplayName "Block-DiagTrack-Telemetry" -Direction Outbound -Program "$env:SystemRoot\System32\CompatTelRunner.exe" -Action Block

# Block WerFault (Windows Error Reporting sender)
New-NetFirewallRule -DisplayName "Block-WerFault-Reporting" -Direction Outbound -Program "$env:SystemRoot\System32\WerFault.exe" -Action Block

# Block WerFaultSecure (elevated error reporting)
New-NetFirewallRule -DisplayName "Block-WerFaultSecure" -Direction Outbound -Program "$env:SystemRoot\System32\WerFaultSecure.exe" -Action Block
```

To verify the rules were created:
```powershell
Get-NetFirewallRule | Where-Object {$_.DisplayName -like "Block-*"} | Select-Object DisplayName, Enabled, Action
```

---

## Step 7 — Clear Tracking History

**What this does:** Windows automatically logs things like what you've typed in the Run dialog, what folders you've navigated to, and what you've searched in File Explorer. These are stored in the registry and are worth clearing periodically.

Run these commands in **PowerShell as Administrator**:

```powershell
# Clear Run dialog history (Win+R history)
Remove-Item "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\RunMRU" -Recurse -Force -ErrorAction SilentlyContinue

# Clear File Explorer address bar history
Remove-Item "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\TypedPaths" -Recurse -Force -ErrorAction SilentlyContinue

# Clear File Explorer search history
Remove-Item "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\WordWheelQuery" -Recurse -Force -ErrorAction SilentlyContinue
```

> 💡 These keys repopulate over time as you use Windows. You can re-run these commands periodically or schedule them as a task.

---

## Step 8 — Apply Changes

After completing all steps, force Group Policy to apply immediately:

```cmd
gpupdate /force
```

Then **restart your PC** for all service, task scheduler, and registry changes to fully take effect.

---

## ✅ Verifying Your Changes

### Check Group Policy settings:
```powershell
# Install the module if you haven't already
Install-Module -Name PolicyFileEditor

# View all your configured GPO settings
Get-PolicyFileEntry -Path "C:\Windows\System32\GroupPolicy\Machine\Registry.pol" -All
```

### Check registry privacy settings:
```powershell
Get-ItemProperty "HKLM:\SOFTWARE\Policies\Microsoft\Windows\DataCollection"
Get-ItemProperty "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo"
Get-ItemProperty "HKCU:\SOFTWARE\Microsoft\InputPersonalization"
Get-ItemProperty "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Privacy"
Get-ItemProperty "HKLM:\SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting"
```

### Check disabled services:
```powershell
Get-Service | Where-Object {$_.StartType -eq 'Disabled'} | Select-Object DisplayName, Name | Sort-Object DisplayName
```

### Check disabled scheduled tasks:
```powershell
Get-ScheduledTask | Where-Object {$_.State -eq 'Disabled'} | Select-Object TaskName, TaskPath | Sort-Object TaskPath
```

### Check hosts file blocks:
```powershell
Get-Content "$env:SystemRoot\System32\drivers\etc\hosts" | Where-Object {$_ -match "0\.0\.0\.0"}
```

### Check firewall rules:
```powershell
Get-NetFirewallRule | Where-Object {$_.DisplayName -like "Block-*"} | Select-Object DisplayName, Enabled, Action
```

---

## ⚡ Running the Script

If you'd prefer to apply everything automatically instead of following the manual steps above, use the included `privacy_hardening.ps1` script.

### Setup (one time only):

By default, Windows blocks running PowerShell scripts as a security measure. To allow it, run this once in PowerShell as Administrator:

```powershell
Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned
```

> This only allows scripts that are either locally created or digitally signed — it does **not** open your system to running any arbitrary script from the internet.

### Running the script:

1. Right-click `privacy_hardening.ps1` → **Run with PowerShell** (as Administrator)
2. Wait for all 6 steps to complete
3. Restart your PC

### What the script does:
- ✅ Applies all registry privacy keys (Sections 2.1 – 2.9 above)
- ✅ Disables all telemetry-related services including SQL Server CEIP (Step 3)
- ✅ Writes Group Policy entries via PolicyFileEditor (Step 1)
- ✅ Disables telemetry scheduled tasks (Step 4)
- ✅ Blocks telemetry domains in the hosts file (Step 5)
- ✅ Adds outbound firewall rules to block telemetry executables (Step 6)
- ✅ Clears Explorer tracking history (Step 7)
- ✅ Runs `gpupdate /force` automatically

---

## 📌 Summary of What Each Area Controls

| Area | What it controls |
|---|---|
| **Group Policy** | System-wide enforced rules — override defaults and user settings |
| **Registry** | Low-level settings for individual Windows components and features |
| **Services** | Background processes — disabling stops them from running at all |
| **Task Scheduler** | Scheduled tasks — stops telemetry from running on a timer independently of services |
| **Hosts File** | Network-level domain blocking — stops telemetry even if other settings get reset |
| **Firewall Rules** | Blocks specific executables from reaching Microsoft's servers outbound |
| **History cleanup** | Removes logs Windows has already collected about your behavior |

---

## 🔁 Keeping It Clean

- Re-run the script or re-check settings after **major Windows updates** — Microsoft occasionally resets policy and registry values during feature updates
- Periodically re-run the history cleanup commands from Step 4
- Export your services list as a baseline to detect future changes:
  ```powershell
  Get-Service | Select-Object Name, DisplayName, Status, StartType | Export-Csv -Path "C:\services_baseline.csv" -NoTypeInformation
  ```

---

## 🎁 Bonus — Extra Tweaks (Not Privacy/Telemetry Related)

These are additional quality-of-life and UI tweaks that are not related to privacy or telemetry, but you may find useful. They are **not included in the automation script** — apply them manually if you want them.

---

### Disable the Lock Screen

**What it does:** Removes the lock screen that appears before the login screen — you go straight to the password prompt instead.

**Via Group Policy:**
> `Computer Configuration → Administrative Templates → Control Panel → Personalization`

| Setting | Value |
|---|---|
| `Do not display the lock screen` | **Enabled** |

**Via Registry:**
> `HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Personalization`

| Value Name | Type | Data |
|---|---|---|
| `NoLockScreen` | DWORD | `1` |

---

### Disable SmartScreen

**What it does:** SmartScreen is a security feature that checks apps and websites against Microsoft's cloud database of known threats. Disabling it means Windows won't make these cloud lookups — useful if you want to avoid the network calls, but it does reduce protection against unknown malware.

> ⚠️ Only disable this if you're confident in your own judgment about what you download and run.

**Via Registry:**
> `HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System`

| Value Name | Type | Data |
|---|---|---|
| `EnableSmartScreen` | DWORD | `0` |

---

### Taskbar Cosmetic Tweaks

**What these do:** Pure UI preferences — no privacy or data implications.

**Via Registry:**
> `HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced`

| Value Name | Type | Data | Effect |
|---|---|---|---|
| `TaskbarBadges` | DWORD | `0` | Removes notification count badges from taskbar icons |
| `TaskbarFlashing` | DWORD | `0` | Stops taskbar buttons flashing to grab your attention |

---

### Firewall Folder Blocker (Bonus Script)

A companion script `firewall folder blocker.bat` is included in this folder. It uses `netsh advfirewall` to block **all `.exe` files in the current folder (and subfolders)** from both inbound and outbound network connections.

**Use case:** Drop this script into a folder containing portable apps or tools you don't want phoning home, run it as Administrator, and every `.exe` inside gets an outbound+inbound firewall rule named `Blocked: <filename>`.

**How it works:**
```bat
for /R %%f in (*.exe) do (
  netsh advfirewall firewall add rule name="Blocked: %%f" dir=out program="%%f" action=block
  netsh advfirewall firewall add rule name="Blocked: %%f" dir=in program="%%f" action=block
)
```

**To remove the rules later:**
```powershell
Get-NetFirewallRule -DisplayName "Blocked:*" | Remove-NetFirewallRule
```

> ⚠️ **Run as Administrator.** The script creates firewall rules which require elevation.
> ⚠️ This blocks **all** executables in the folder tree indiscriminately — use with caution on folders containing apps you actually want online.

---

## Disclaimer

These changes are intended for personal privacy hardening on your own machine. Some settings — particularly disabling Delivery Optimization and Windows Error Reporting — may slightly affect Windows Update behavior or Microsoft's ability to diagnose system issues remotely. Apply only what you're comfortable with.
