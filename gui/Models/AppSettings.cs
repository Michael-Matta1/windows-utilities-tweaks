using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace WindowsTweakLauncher.Models;

public class AppSettings : INotifyPropertyChanged
{
    private string _repoPath = "";
    private bool _backupEnabled = true;
    private bool _backupPreApply = true;
    private bool _backupPostApply = true;
    private bool _backupPreRevert = true;
    private bool _backupPostRevert = true;
    private string _backupDirectory = WindowsTweakLauncher.Services.Paths.BackupDirDefault;

    [JsonPropertyName("repoPath")]
    public string RepoPath
    {
        get => _repoPath;
        set { _repoPath = value ?? ""; OnPropertyChanged(); }
    }

    [JsonPropertyName("backupEnabled")]
    public bool BackupEnabled
    {
        get => _backupEnabled;
        set { _backupEnabled = value; OnPropertyChanged(); }
    }

    [JsonPropertyName("backupPreApply")]
    public bool BackupPreApply
    {
        get => _backupPreApply;
        set { _backupPreApply = value; OnPropertyChanged(); }
    }

    [JsonPropertyName("backupPostApply")]
    public bool BackupPostApply
    {
        get => _backupPostApply;
        set { _backupPostApply = value; OnPropertyChanged(); }
    }

    [JsonPropertyName("backupPreRevert")]
    public bool BackupPreRevert
    {
        get => _backupPreRevert;
        set { _backupPreRevert = value; OnPropertyChanged(); }
    }

    [JsonPropertyName("backupPostRevert")]
    public bool BackupPostRevert
    {
        get => _backupPostRevert;
        set { _backupPostRevert = value; OnPropertyChanged(); }
    }

    [JsonPropertyName("backupDirectory")]
    public string BackupDirectory
    {
        get => _backupDirectory;
        set
        {
            var v = string.IsNullOrWhiteSpace(value) ? WindowsTweakLauncher.Services.Paths.BackupDirDefault : value;
            if (v.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0
                || v.IndexOfAny(new[] { '*', '?', '"', '|' }) >= 0
                || !System.IO.Path.IsPathRooted(v))
                v = WindowsTweakLauncher.Services.Paths.BackupDirDefault;
            try { v = System.IO.Path.GetFullPath(v); }
            catch { v = WindowsTweakLauncher.Services.Paths.BackupDirDefault; }
            _backupDirectory = v;
            OnPropertyChanged();
        }
    }

    public static AppSettings CreateDefault()
    {
        return new AppSettings
        {
            RepoPath = "",
            BackupEnabled = true,
            BackupPreApply = true,
            BackupPostApply = true,
            BackupPreRevert = true,
            BackupPostRevert = true,
            BackupDirectory = WindowsTweakLauncher.Services.Paths.BackupDirDefault
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
