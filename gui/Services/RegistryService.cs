using System.IO;
using System.Linq;
using WindowsTweakLauncher.Models;
using static WindowsTweakLauncher.Services.Paths;

namespace WindowsTweakLauncher.Services;

public static class RegistryService
{
    private const int MaxBackupsPerTweak = 10;
    public static (bool success, string message) BackupRegistryKey(
        RegistryTarget target, string tweakId, string suffix = "")
    {
        if (target == null)
            return (false, "Backup failed: target is null");

        var settings = SettingsService.Current;
        if (!settings.BackupEnabled)
            return (true, "Backup disabled \u2014 skipped");

        try { Directory.CreateDirectory(settings.BackupDirectory); }
        catch (Exception ex) { return (false, $"Backup failed: could not create directory \u2014 {ex.Message}"); }

        var (checkEc, checkOutput) = ProcessRunner.RunProcess("reg.exe",
            $"query \"{target.Hive}\\{target.RegPath}\"");
        if (checkEc == -1)
            return (false, $"Backup failed: {checkOutput}");
        if (checkEc == 5)
            return (false, $"Backup failed: access denied \u2014 run as Administrator to back up \"{target.Hive}\\{target.RegPath}\"");
        if (checkEc == 1)
            return (true, "No existing key \u2014 nothing to back up (expected the first time a tweak is applied)");
        if (checkEc != 0)
            return (false, $"Backup failed: unexpected exit code {checkEc} \u2014 {checkOutput}");

        var safeId = string.Join("_", (tweakId ?? "").Split(Path.GetInvalidFileNameChars()));
        if (string.IsNullOrEmpty(safeId))
            safeId = "_unknown_tweak";

        var safeSuffix = string.Join("_", (suffix ?? "").Split(Path.GetInvalidFileNameChars(), System.StringSplitOptions.RemoveEmptyEntries));
        var fileSuffix = string.IsNullOrEmpty(safeSuffix) ? "_default" : safeSuffix;
        var backupFile = Path.Combine(settings.BackupDirectory,
            $"{safeId}{fileSuffix}_{DateTime.Now:yyyyMMdd_HHmmss}.reg");

        var safeBackupPath = backupFile.Replace("\"", "");
        var (exitCode, output) = ProcessRunner.RunProcess("reg.exe",
            $"export \"{target.Hive}\\{target.RegPath}\" \"{safeBackupPath}\" /y");

        if (exitCode == 0)
        {
            try
            {
                var prefix = $"{safeId}{fileSuffix}_";
                var candidates = Directory.GetFiles(settings.BackupDirectory, $"{prefix}*.reg");
                var withTimestamps = new System.Collections.Generic.List<(string path, DateTime time)>();
                foreach (var f in candidates)
                {
                    try { withTimestamps.Add((f, File.GetLastWriteTimeUtc(f))); }
                    catch (Exception ex) { ProcessRunner.AppendLog($"Backup cleanup: skipping unreadable {f}: {ex.Message}"); }
                }
                var toDelete = withTimestamps.OrderByDescending(x => x.time).Skip(MaxBackupsPerTweak);
                foreach (var old in toDelete)
                    try { File.Delete(old.path); } catch (Exception ex) { ProcessRunner.AppendLog($"Backup cleanup: {ex.Message}"); }
            }
            catch (Exception ex) { ProcessRunner.AppendLog($"Backup cleanup failed: {ex.Message}"); }
            return (true, $"Backed up to {backupFile}");
        }

        return (false, $"Backup failed: {output}");
    }

    public static bool CheckKeyExists(string hive, string path)
    {
        if (hive == null || path == null) return false;
        var (exitCode, output) = ProcessRunner.RunProcess("reg.exe",
            $"query \"{hive}\\{path}\"");
        if (exitCode == 5)
            ProcessRunner.AppendLog($"CheckKeyExists: access denied reading \"{hive}\\{path}\" \u2014 run as Administrator");
        else if (exitCode != 0 && exitCode != 1)
            ProcessRunner.AppendLog($"CheckKeyExists: unexpected exit code {exitCode} for \"{hive}\\{path}\": {output}");
        return exitCode == 0;
    }

    public static bool CheckValueExists(string hive, string path, string valueName)
    {
        if (hive == null || path == null || valueName == null) return false;
        var (exitCode, output) = ProcessRunner.RunProcess("reg.exe",
            $"query \"{hive}\\{path}\" /v \"{valueName}\"");
        if (exitCode == 5)
            ProcessRunner.AppendLog($"CheckValueExists: access denied reading \"{hive}\\{path}\" \u2014 run as Administrator");
        else if (exitCode != 0 && exitCode != 1)
            ProcessRunner.AppendLog($"CheckValueExists: unexpected exit code {exitCode} for \"{hive}\\{path}\" /v \"{valueName}\": {output}");
        return exitCode == 0;
    }

    public static bool CheckValueEquals(string hive, string path, string valueName, byte[] expectedBytes)
    {
        if (expectedBytes == null) return false;
        if (hive == null || path == null || valueName == null) return false;

        try
        {
            var hiveKey = hive.ToUpperInvariant() switch
            {
                "HKCR" => Microsoft.Win32.Registry.ClassesRoot,
                "HKCU" => Microsoft.Win32.Registry.CurrentUser,
                "HKLM" => Microsoft.Win32.Registry.LocalMachine,
                "HKU" => Microsoft.Win32.Registry.Users,
                _ => null
            };
            if (hiveKey == null) return false;

            using var key = hiveKey.OpenSubKey(path);
            if (key == null) return false;

            var value = key.GetValue(valueName);
            return value is byte[] bytes && bytes.SequenceEqual(expectedBytes);
        }
        catch (Exception ex)
        {
            ProcessRunner.AppendLog($"CheckValueEquals: error reading \"{hive}\\{path}\" /v \"{valueName}\": {ex.Message}");
            return false;
        }
    }

    public static void RefreshStatus(TweakItem tweak)
    {
        if (tweak == null)
        {
            ProcessRunner.AppendLog("RefreshStatus: tweak is null");
            return;
        }

        try
        {
        if (string.IsNullOrEmpty(tweak.Id)) { tweak.Status = TweakStatus.Unknown; return; }
        switch (tweak.Id.ToLowerInvariant())
        {
            case "markdown_ctx":
                tweak.Status = CheckKeyExists("HKCR", "Directory\\Background\\shell\\CreateMarkdownFile") && File.Exists(Path.Combine(System32, "CreateMarkdownSilent.vbs"))
                    ? TweakStatus.Applied : TweakStatus.NotApplied;
                break;

            case "run_py":
                tweak.Status = CheckKeyExists("HKCR", "SystemFileAssociations\\.py\\shell\\runwithpython")
                    ? TweakStatus.Applied : TweakStatus.NotApplied;
                break;

            case "ps1_admin_ctx":
                tweak.Status = CheckKeyExists("HKCR", "*\\shell\\RunWithPowerShellAdmin")
                    ? TweakStatus.Applied : TweakStatus.NotApplied;
                break;

            case "copy_path":
                tweak.Status = CheckKeyExists("HKCR", "AllFilesystemObjects\\shell\\CopyPathWithoutQuotes")
                    && File.Exists(Paths.CopyPathVbsDest)
                    ? TweakStatus.Applied : TweakStatus.NotApplied;
                break;

            case "classic_ctx":
                // InprocServer32 EXISTS = classic menu is enabled = tweak IS applied
                tweak.Status = CheckKeyExists("HKCU", $"Software\\Classes\\CLSID\\{Clsids.ClassicContextMenu}\\InprocServer32")
                    ? TweakStatus.Applied : TweakStatus.NotApplied;
                break;

            case "fix_alt_shift":
                var expected = new byte[] { 0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x02,0x00,0x00,0x00,0x38,0x00,0x38,0xe0,0x00,0x00,0x00,0x00 };
                tweak.Status = CheckValueEquals("HKLM", "SYSTEM\\CurrentControlSet\\Control\\Keyboard Layout",
                    "Scancode Map", expected)
                    ? TweakStatus.Applied : TweakStatus.NotApplied;
                break;

            case "explorer_cmd":
            {
                var allBats = new[] { "acmd.bat", "aps.bat", "atr.bat" };
                var exists = allBats.Select(f => File.Exists(Path.Combine(Windows, f))).ToList();
                var count = exists.Count(e => e);
                tweak.Status = count == allBats.Length
                    ? TweakStatus.Applied
                    : count > 0
                        ? TweakStatus.PartialApplied
                        : TweakStatus.NotApplied;
                break;
            }

            case "new_txt_file":
                tweak.Status = File.Exists(Path.Combine(StartMenuPrograms, "NewTextFile.lnk"))
                    ? TweakStatus.Applied : TweakStatus.NotApplied;
                break;

            case "explorer_location":
            {
                var basePath = $"SOFTWARE\\Classes\\CLSID\\{Clsids.ExplorerLocation}";
                var cmd1 = CheckKeyExists("HKCU", basePath + "\\shell\\open\\command");
                var cmd2 = CheckKeyExists("HKCU", basePath + "\\shell\\OpenNewWindow\\command");
                tweak.Status = cmd1 && cmd2
                    ? TweakStatus.Applied
                    : cmd1 || cmd2
                        ? TweakStatus.PartialApplied
                        : TweakStatus.NotApplied;
                break;
            }

            case "privacy":
                tweak.Status = TweakStatus.Unknown;
                break;

            case "ocr_shortcut":
            {
                var ocrLnk = Path.Combine(Paths.OcrShortcutDir, "OCR Shortcut.lnk");
                tweak.Status = File.Exists(ocrLnk) ? TweakStatus.Applied : TweakStatus.NotApplied;
                break;
            }

            case "voice_typing_shortcut":
            {
                var vtLnk = Path.Combine(Paths.VoiceTypingShortcutDir, "Voice Typing Shortcut.lnk");
                tweak.Status = File.Exists(vtLnk) ? TweakStatus.Applied : TweakStatus.NotApplied;
                break;
            }

            case "custom_shortcut":
            {
                var dir = Paths.CustomShortcutDir;
                var hasLnk = Directory.Exists(dir) && Directory.GetFiles(dir, "*.lnk").Length > 0;
                tweak.Status = hasLnk
                    ? TweakStatus.Applied : TweakStatus.NotApplied;
                break;
            }

            case "firewall_folder_blocker":
            {
                tweak.Status = TweakStatus.Reapplicable;
                break;
            }

            case "hide_context_menu_items":
            {
                var blockedKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Blocked";
                var clsidList = new[]
                {
                    Clsids.CtxHideCastToDevice,
                    Clsids.CtxHideShare,
                    Clsids.CtxHideGiveAccessTo,
                    Clsids.CtxHidePaint3D,
                    Clsids.CtxHideEditPhotos,
                    Clsids.CtxHideDefenderScan,
                };
                var blockedCount = clsidList.Count(c => CheckValueExists("HKLM", blockedKey, c));
                var totalApplied = blockedCount;
                var totalItems = clsidList.Length;
                tweak.Status = totalApplied == totalItems
                    ? TweakStatus.Applied
                    : totalApplied > 0
                        ? TweakStatus.PartialApplied
                        : TweakStatus.NotApplied;
                break;
            }

            case "terminal_ise_profile":
            case "terminal_gitbash_profile":
            {
                string? settingsPath = Paths.FindTerminalSettings();

                if (settingsPath == null)
                {
                    tweak.Status = TweakStatus.NotApplied;
                    break;
                }

                string json;
                try { json = File.ReadAllText(settingsPath); }
                catch (Exception ex) { ProcessRunner.AppendLog($"RefreshStatus: failed to read Terminal settings: {ex.Message}"); tweak.Status = TweakStatus.NotApplied; break; }
                using var doc = System.Text.Json.JsonDocument.Parse(json, new System.Text.Json.JsonDocumentOptions
                {
                    CommentHandling = System.Text.Json.JsonCommentHandling.Skip
                });
                if (doc.RootElement.TryGetProperty("profiles", out var profilesEl) &&
                    profilesEl.TryGetProperty("list", out var listEl) &&
                    listEl.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    var fixedGuid = string.Equals(tweak.Id, "terminal_gitbash_profile", StringComparison.OrdinalIgnoreCase)
                        ? Clsids.TerminalGitBashProfile
                        : Clsids.TerminalIseProfile;
                    var exists = false;
                    foreach (var item in listEl.EnumerateArray())
                    {
                        if (item.TryGetProperty("guid", out var guidEl) &&
                            string.Equals(guidEl.GetString(), fixedGuid, StringComparison.OrdinalIgnoreCase))
                        { exists = true; break; }
                    }
                    tweak.Status = exists ? TweakStatus.Applied : TweakStatus.NotApplied;
                }
                else
                {
                    tweak.Status = TweakStatus.NotApplied;
                }
                break;
            }

            case "hw_identity":
                tweak.Status = TweakStatus.Reapplicable;
                break;

            case "pin_taskbar":
            case "special_folders":
                tweak.Status = TweakStatus.Reapplicable;
                break;

            case "terminal_profiles":
            {
                string? settingsPath = Paths.FindTerminalSettings();

                if (settingsPath == null)
                {
                    tweak.Status = TweakStatus.NotApplied;
                    break;
                }

                string json;
                try { json = File.ReadAllText(settingsPath); }
                catch (Exception ex) { ProcessRunner.AppendLog($"RefreshStatus: failed to read Terminal settings: {ex.Message}"); tweak.Status = TweakStatus.NotApplied; break; }
                using var doc = System.Text.Json.JsonDocument.Parse(json, new System.Text.Json.JsonDocumentOptions
                {
                    CommentHandling = System.Text.Json.JsonCommentHandling.Skip
                });
                if (doc.RootElement.TryGetProperty("profiles", out var profilesEl) &&
                    profilesEl.TryGetProperty("list", out var listEl) &&
                    listEl.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    var guids = new[]
                    {
                        Clsids.TerminalIseProfile,
                        Clsids.TerminalGitBashProfile
                    };
                    var found = 0;
                    foreach (var item in listEl.EnumerateArray())
                    {
                        if (item.TryGetProperty("guid", out var guidEl))
                        {
                            var g = guidEl.GetString();
                            if (Array.Exists(guids, x => string.Equals(x, g, StringComparison.OrdinalIgnoreCase))) found++;
                        }
                    }
                    tweak.Status = found == guids.Length
                        ? TweakStatus.Applied
                        : found > 0
                            ? TweakStatus.PartialApplied
                            : TweakStatus.NotApplied;
                }
                else
                {
                    tweak.Status = TweakStatus.NotApplied;
                }
                break;
            }

            default:
                tweak.Status = TweakStatus.Unknown;
                break;
        }
        }
        catch (System.Exception ex)
        {
            ProcessRunner.AppendLog($"RefreshStatus ({tweak.Id}): {ex.Message}");
            tweak.Status = TweakStatus.Unknown;
        }
    }

    public static void RefreshAllStatus(System.Collections.Generic.List<TweakItem> tweaks)
    {
        if (tweaks == null) { ProcessRunner.AppendLog("RefreshAllStatus: tweaks list is null"); return; }
        foreach (var t in tweaks)
        {
            try { RefreshStatus(t); }
            catch (System.Exception ex) { ProcessRunner.AppendLog($"RefreshAllStatus ({t.Id}): {ex.Message}"); }
        }
    }
}
