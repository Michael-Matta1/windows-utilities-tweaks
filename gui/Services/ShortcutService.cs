using System.IO;

namespace WindowsTweakLauncher.Services;

public static class ShortcutService
{
    private static string EscapeForPsString(string s)
    {
        if (s == null) return "";
        return s.Replace("`", "``").Replace("\n", "`n").Replace("\r", "`r").Replace("\t", "`t")
                .Replace("\"", "`\"").Replace("$", "`$");
    }

    public static (int exitCode, string output) CreateShortcut(
        string targetPath, string arguments, string shortcutPath, string hotkey, string? iconPath = null)
    {
        if (string.IsNullOrEmpty(targetPath) || arguments == null || string.IsNullOrEmpty(shortcutPath))
            return (-1, "CreateShortcut: required parameters cannot be null or empty");

        var sp = EscapeForPsString(shortcutPath);
        var tp = EscapeForPsString(targetPath);
        var a  = string.IsNullOrEmpty(arguments) ? "" : EscapeForPsString(arguments);
        var hk = string.IsNullOrEmpty(hotkey) ? null : EscapeForPsString(hotkey);
        var ip = !string.IsNullOrEmpty(iconPath) ? EscapeForPsString(iconPath) : null;

        var psScript = $@"
$ws = New-Object -ComObject WScript.Shell
try {{
    $sc = $ws.CreateShortcut(""{sp}"")
    try {{
        $sc.TargetPath = ""{tp}""
        $sc.Arguments = ""{a}""
        {(hk != null ? $"$sc.Hotkey = \"{hk}\"" : "")}
        {(ip != null ? $"$sc.IconLocation = \"{ip}\"" : "")}
        $sc.Save()
    }} finally {{
        if ($sc -ne $null) {{ [System.Runtime.InteropServices.Marshal]::ReleaseComObject($sc) | Out-Null }}
    }}
}} finally {{
    [System.Runtime.InteropServices.Marshal]::ReleaseComObject($ws) | Out-Null
}}
";
        var tempPs1 = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ps1");
        try
        {
            try
            {
                File.WriteAllText(tempPs1, psScript);
            }
            catch (System.Exception ex)
            {
                return (-1, $"Failed to write temp script: {ex.Message}");
            }
            return ProcessRunner.RunProcess("powershell.exe",
                $"-NoProfile -ExecutionPolicy Bypass -File \"{tempPs1}\"");
        }
        finally
        {
            try { if (File.Exists(tempPs1)) File.Delete(tempPs1); } catch (Exception ex) { ProcessRunner.AppendLog($"Failed to delete temp ps1: {ex.Message}"); }
        }
    }
}
