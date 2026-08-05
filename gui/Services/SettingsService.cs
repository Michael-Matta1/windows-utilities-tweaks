using System.Text.Json;
using WindowsTweakLauncher.Models;

namespace WindowsTweakLauncher.Services;

public static class SettingsService
{
    private static readonly string SettingsDir = GetSettingsDir();

    private static string GetSettingsDir()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (string.IsNullOrEmpty(appData))
            appData = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
        return System.IO.Path.Combine(appData, "WindowsTweakLauncher");
    }

    private static readonly string SettingsFile = System.IO.Path.Combine(SettingsDir, "settings.json");

    private static readonly object _saveLock = new();
    internal static bool TrySaveWithLock()
    {
        if (!System.Threading.Monitor.TryEnter(_saveLock, TimeSpan.FromMilliseconds(100)))
            return false;
        try { return WriteSettings(); }
        finally { System.Threading.Monitor.Exit(_saveLock); }
    }

    private static volatile AppSettings _current = AppSettings.CreateDefault();
    public static AppSettings Current => _current;
    private static void SetCurrent(AppSettings value) => _current = value;

    public static void Load()
    {
        lock (_saveLock)
        {
            if (!System.IO.File.Exists(SettingsFile))
            {
                SetCurrent(AppSettings.CreateDefault());
                try { WriteSettings(); }
                catch (Exception ex) { ProcessRunner.AppendLog($"Failed to write initial settings: {ex.Message}"); }
                return;
            }

            try
            {
                var json = System.IO.File.ReadAllText(SettingsFile);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                SetCurrent(JsonSerializer.Deserialize<AppSettings>(json, options) ?? AppSettings.CreateDefault());
            }
            catch (System.Text.Json.JsonException ex)
            {
                ProcessRunner.AppendLog($"Failed to parse settings: {ex.Message}. Using defaults.");
                SetCurrent(AppSettings.CreateDefault());
            }
            catch (System.UnauthorizedAccessException ex)
            {
                ProcessRunner.AppendLog($"Access denied reading settings: {ex.Message}. Using defaults.");
                SetCurrent(AppSettings.CreateDefault());
            }
            catch (System.IO.IOException ex)
            {
                ProcessRunner.AppendLog($"Failed to read settings file: {ex.Message}. Using defaults.");
                SetCurrent(AppSettings.CreateDefault());
            }
            catch (SystemException ex) when (ex is not System.AccessViolationException
                and not System.StackOverflowException and not System.OutOfMemoryException)
            {
                ProcessRunner.AppendLog($"Unexpected error reading settings: {ex.Message}. Using defaults.");
                SetCurrent(AppSettings.CreateDefault());
            }
            catch (Exception ex) when (ex is not System.AccessViolationException
                and not System.StackOverflowException and not System.OutOfMemoryException)
            {
                ProcessRunner.AppendLog($"Unexpected error reading settings: {ex.Message}. Using defaults.");
                SetCurrent(AppSettings.CreateDefault());
            }
        }
    }

    private static bool WriteSettings()
    {
        try
        {
            System.IO.Directory.CreateDirectory(SettingsDir);
            var json = JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true });
            var tempFile = SettingsFile + "." + Guid.NewGuid() + ".tmp";
            try
            {
                System.IO.File.WriteAllText(tempFile, json);
                if (System.IO.File.Exists(SettingsFile))
                    System.IO.File.Replace(tempFile, SettingsFile, SettingsFile + ".bak");
                else
                    System.IO.File.Move(tempFile, SettingsFile);
                return true;
            }
            finally
            {
                try { if (System.IO.File.Exists(tempFile)) System.IO.File.Delete(tempFile); } catch (Exception ex) { ProcessRunner.AppendLog($"Failed to clean up temp settings file: {ex.Message}"); }
            }
        }
        catch (Exception ex)
        {
            ProcessRunner.AppendLog($"Failed to save settings: {ex.Message}");
            return false;
        }
    }

    public static void Save()
    {
        lock (_saveLock)
        {
            WriteSettings();
        }
    }

    public static void SaveRepoPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            ProcessRunner.AppendLog("SaveRepoPath: path is null or empty \u2014 ignored");
            return;
        }
        lock (_saveLock)
        {
            var oldPath = Current.RepoPath;
            Current.RepoPath = path;
            if (!WriteSettings())
            {
                ProcessRunner.AppendLog("SaveRepoPath: failed to persist settings, reverting in-memory RepoPath");
                Current.RepoPath = oldPath;
            }
        }
    }
}
