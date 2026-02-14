# Source: https://github.com/Michael-Matta1/windows-utilities-tweaks
# Silent helper: detects the active File Explorer tab's folder path
# Called by CreateTextFile.vbs — runs completely hidden
#
# Strategy:
#   1. Enumerate Shell.Windows to find all Explorer entries
#   2. Group by HWND (Windows)
#   3. For each Window, separate logic to find the Active Tab (uiAutomation)
#   4. Identify the Foreground Window by walking Z-Order (handles PS/focus stealing)

Add-Type -AssemblyName UIAutomationClient -ErrorAction SilentlyContinue | Out-Null
Add-Type -AssemblyName UIAutomationTypes -ErrorAction SilentlyContinue | Out-Null

$outputFile = $args[0]

# --- Win32 API for Z-Order Walking ---
$win32 = @"
using System;
using System.Runtime.InteropServices;

public class Win32 {
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

    public const uint GW_HWNDNEXT = 2;
}
"@
Add-Type -TypeDefinition $win32 -ErrorAction SilentlyContinue | Out-Null

# Step 1: Collect all Explorer entries
$shell = New-Object -ComObject Shell.Application
$entries = @()

foreach ($w in $shell.Windows()) {
    try {
        $p = $w.Document.Folder.Self.Path
        if ($p -and $w.Visible) {
            $entries += [PSCustomObject]@{
                HWND         = [long]$w.HWND
                Path         = $p
                LocationName = $w.LocationName
                MatchScore   = 0
            }
        }
    }
    catch {}
}

if ($entries.Count -eq 0) {
    "" | Out-File -FilePath $outputFile -Encoding ascii -NoNewline
    exit
}

# Step 2: Group by HWND
$grouped = $entries | Group-Object -Property HWND
$activeTabs = @()

# Step 3: Find Active Tab for EACH Window
foreach ($group in $grouped) {
    $bestTab = $null
    
    if ($group.Count -gt 1) {
        # Multiple tabs in this window - use UIAutomation
        try {
            $hwndPtr = [IntPtr]::new([long]$group.Name)
            $element = [System.Windows.Automation.AutomationElement]::FromHandle($hwndPtr)

            $tabCondition = New-Object System.Windows.Automation.PropertyCondition(
                [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
                [System.Windows.Automation.ControlType]::TabItem
            )
            $tabs = $element.FindAll([System.Windows.Automation.TreeScope]::Descendants, $tabCondition)

            $activeTabName = ""
            foreach ($tab in $tabs) {
                try {
                    $pattern = $tab.GetCurrentPattern(
                        [System.Windows.Automation.SelectionItemPattern]::Pattern
                    )
                    if ($pattern.Current.IsSelected) {
                        $activeTabName = $tab.Current.Name
                        break
                    }
                }
                catch {}
            }

            if ($activeTabName) {
                # Score entries in this window
                foreach ($entry in $group.Group) {
                    if ($entry.Path -eq $activeTabName) { $entry.MatchScore = 3 }
                    elseif ($entry.LocationName -eq $activeTabName) { $entry.MatchScore = 2 }
                    elseif ($activeTabName.Contains($entry.LocationName) -or $entry.Path.EndsWith($activeTabName)) { $entry.MatchScore = 1 }
                }
                # Pick best scorer
                $bestTab = $group.Group | Sort-Object MatchScore -Descending | Select-Object -First 1
            }
        }
        catch {}
    } 
    
    # Fallback / Single Tab
    if (-not $bestTab) {
        $bestTab = $group.Group[0]
    }
    
    if ($bestTab) {
        $activeTabs += $bestTab
    }
}

# Step 4: Pick the Correct Window (Z-Order Walk)
# We have one active tab per window. Now we need to know WHICH window is foreground.
# GetForegroundWindow might return the VBS/PS host, so we walk down the Z-Order.

$currentHwnd = [Win32]::GetForegroundWindow()
$maxDepth = 50 # Avoid infinite loops
$found = $null

for ($i = 0; $i -lt $maxDepth; $i++) {
    if ($currentHwnd -eq [IntPtr]::Zero) { break }
    
    $hwndLong = $currentHwnd.ToInt64()
    
    # Check if this HWND matches any of our Explorer windows
    $match = $activeTabs | Where-Object { $_.HWND -eq $hwndLong }
    if ($match) {
        $found = $match
        break
    }
    
    # Move to next window in Z-Order
    $currentHwnd = [Win32]::GetWindow($currentHwnd, [Win32]::GW_HWNDNEXT)
}

# Result
if ($found) {
    $found.Path | Out-File -FilePath $outputFile -Encoding ascii -NoNewline
}
elseif ($activeTabs.Count -gt 0) {
    # Fallback to first if Z-order walk failed
    $activeTabs[0].Path | Out-File -FilePath $outputFile -Encoding ascii -NoNewline
}
