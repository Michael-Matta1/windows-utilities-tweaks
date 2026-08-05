using System.IO;

namespace WindowsTweakLauncher.Services;

public static class Paths
{
    public static readonly string ProgramData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
    public static readonly string Windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
    public static readonly string System32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
    public static readonly string StartMenuPrograms = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs");

    public static readonly string TweaksBaseDir = Path.Combine(ProgramData, "WindowsTweaks");
    public static readonly string OcrShortcutDir = Path.Combine(TweaksBaseDir, "OcrShortcut");
    public static readonly string VoiceTypingShortcutDir = Path.Combine(TweaksBaseDir, "VoiceTypingShortcut");
    public static readonly string CustomShortcutDir = Path.Combine(TweaksBaseDir, "CustomShortcut");
    public static readonly string NewTextFileDir = Path.Combine(TweaksBaseDir, "NewTextFile");
    public static readonly string CopyPathVbsDest = Path.Combine(TweaksBaseDir, "CopyPathWithoutQuotes.vbs");
    public static readonly string BackupDirDefault = Path.Combine(TweaksBaseDir, "Backups");
    public static readonly string TaskbarPinnedFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        @"Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar");

    private static readonly string LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public static string[] TerminalSettingsCandidates =>
    [
        Path.Combine(LocalAppData, "Packages", "Microsoft.WindowsTerminal_8wekyb3d8bbwe", "LocalState", "settings.json"),
        Path.Combine(LocalAppData, "Packages", "Microsoft.WindowsTerminalPreview_8wekyb3d8bbwe", "LocalState", "settings.json"),
        Path.Combine(LocalAppData, "Packages", "Microsoft.WindowsTerminalDev_8wekyb3d8bbwe", "LocalState", "settings.json"),
        Path.Combine(LocalAppData, "Packages", "Microsoft.WindowsTerminalCanary_8wekyb3d8bbwe", "LocalState", "settings.json"),
        Path.Combine(LocalAppData, "Microsoft", "Windows Terminal", "settings.json"),
    ];

    public static string? FindTerminalSettings()
    {
        foreach (var cp in TerminalSettingsCandidates)
            if (File.Exists(cp)) return cp;
        return null;
    }

    // Registry CLSIDs / GUIDs shared across multiple files
    public static class Clsids
    {
        // Context menu items to hide (Shell Extension Blocked list)
        public const string CtxHideCastToDevice = "{7AD84985-87B4-4a16-BE58-8B72A5B390F7}";
        public const string CtxHideShare = "{e2bf9676-5f8f-435c-97eb-11607a5bedf7}";
        public const string CtxHideGiveAccessTo = "{f81e9010-6ea4-11ce-a7ff-00aa003ca9f6}";
        public const string CtxHidePaint3D = "{2484F1CA-9F8A-4E6A-9C73-BDC0E72BA6F8}";
        public const string CtxHideEditPhotos = "{FFE2A43C-56B9-4bf5-9A79-CC6D4285608A}";
        public const string CtxHideDefenderScan = "{09A47860-11B0-4DA5-AFA5-26D86198A780}";

        // Classic context menu
        public const string ClassicContextMenu = "{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}";

        // Explorer custom location
        public const string ExplorerLocation = "{52205fd8-5dfb-447d-801a-d0b52f2e83e1}";

        // Terminal profiles
        public const string TerminalIseProfile = "{C9D1D9A1-5E8E-4B7A-8F5C-2E5A5D5E5F6E}";
        public const string TerminalGitBashProfile = "{2C4A2E1A-5E8E-4B7A-8F5C-6E7A8B9C0D1E}";
    }
}