using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace WindowsTweakLauncher.Services;

public static class ProcessRunner
{
    public const int DefaultTimeoutMs = 60000;
    public const int LongTimeoutMs = 300000; // 5 minutes for batch files that process many files
    public const int KillTimeoutMs = 5000;
    public const int MaxLogFiles = 90;

    public static (int exitCode, string output) RunProcess(string exe, string args, int timeoutMs = DefaultTimeoutMs)
    {
        if (exe == null) return (-1, "Process error: exe is null");
        if (args == null) args = "";
        if (timeoutMs < 0) timeoutMs = DefaultTimeoutMs;

        try
        {
            using var process = new Process();
            process.StartInfo.FileName = exe;
            process.StartInfo.Arguments = args;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
            process.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            var outputBuilder = new System.Text.StringBuilder();
            var lockObj = new object();
            process.OutputDataReceived += (_, e) => { if (e.Data != null) lock (lockObj) outputBuilder.AppendLine(e.Data); };
            process.ErrorDataReceived += (_, e) => { if (e.Data != null) lock (lockObj) outputBuilder.AppendLine(e.Data); };

            if (!process.Start())
                return (-1, "Process failed to start.");
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (process.WaitForExit(timeoutMs))
            {
                process.WaitForExit();
                lock (lockObj) return (process.ExitCode, outputBuilder.ToString().TrimEnd());
            }

            try
            {
                if (process.HasExited)
                {
                    process.WaitForExit();
                    lock (lockObj) return (process.ExitCode, outputBuilder.ToString().TrimEnd());
                }
                process.Kill(entireProcessTree: true);
                process.WaitForExit(KillTimeoutMs);
            }
            catch (System.Exception killEx) { AppendLog($"Failed to kill timed-out process: {killEx.Message}"); }
            return (-1, $"Process timed out after {timeoutMs}ms.");
        }
        catch (System.Exception ex)
        {
            return (-1, $"Process error: {ex.Message}");
        }
    }

    public static (int exitCode, string output) RunRegImport(string regFilePath)
    {
        if (regFilePath == null)
            return (-1, "RegImport error: file path is null");
        return RunProcess("reg.exe", $"import \"{regFilePath}\"");
    }

    public static (int exitCode, string output) ImportRegContent(string regContent)
    {
        if (regContent == null)
            return (-1, "ImportRegContent error: content is null");

        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".reg");
        try
        {
            if (!regContent.TrimStart().StartsWith("Windows Registry Editor", StringComparison.OrdinalIgnoreCase))
                regContent = "Windows Registry Editor Version 5.00\r\n\r\n" + regContent;
            File.WriteAllText(tempFile, regContent, System.Text.Encoding.Unicode);
            return RunRegImport(tempFile);
        }
        finally
        {
            try { if (File.Exists(tempFile)) File.Delete(tempFile); } catch (Exception cleanupEx) { AppendLog($"Failed to delete temp reg file: {cleanupEx.Message}"); }
        }
    }

    public static void CopyFileWithDirCreate(string src, string dest)
    {
        if (src == null) { AppendLog("CopyFileWithDirCreate: src is null"); return; }
        if (dest == null) { AppendLog("CopyFileWithDirCreate: dest is null"); return; }

        try
        {
            if (!File.Exists(src)) { AppendLog($"CopyFileWithDirCreate: source not found: {src}"); return; }
            var destDir = System.IO.Path.GetDirectoryName(dest);
            if (string.IsNullOrEmpty(destDir)) { AppendLog($"CopyFileWithDirCreate: cannot determine directory from dest: {dest}"); return; }
            System.IO.Directory.CreateDirectory(destDir);
            File.Copy(src, dest, overwrite: true);
        }
        catch (System.Exception ex) { AppendLog($"CopyFileWithDirCreate failed: {ex.Message}"); }
    }

    public static void OpenFolder(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath)) { AppendLog("OpenFolder: folderPath is null or empty"); return; }

        var safePath = folderPath.Replace("\"", "");
        try
        {
            Process.Start("explorer.exe", $"\"{safePath}\"")?.Dispose();
        }
        catch (System.Exception ex) { AppendLog($"Failed to open folder: {ex.Message}"); }
    }

    [ThreadStatic]
    private static bool _loggingRecursive;

    private static readonly object _appendLogLock = new();
    private static int _logCleanupCounter;

    public static void AppendLog(string line)
    {
        if (_loggingRecursive)
        {
            System.Diagnostics.Debug.WriteLine($"DROPPED: {line}");
            return;
        }
        _loggingRecursive = true;
        try
        {
            var now = DateTime.Now;
            var timestamp = now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            var entry = $"[{timestamp}] {line}";

            var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(logDir);
            var logFile = Path.Combine(logDir, now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".log");
            lock (_appendLogLock)
            {
                File.AppendAllText(logFile, entry + Environment.NewLine);

                try
                {
                    if (Interlocked.Increment(ref _logCleanupCounter) % 10 == 0)
                    {
                        var allLogs = Directory.GetFiles(logDir, "*.log").OrderByDescending(f => File.GetLastWriteTimeUtc(f)).ToArray();
                        if (allLogs.Length > MaxLogFiles)
                            foreach (var old in allLogs.Skip(MaxLogFiles))
                                try { File.Delete(old); } catch (Exception cleanEx) { System.Diagnostics.Debug.WriteLine($"Log cleanup: {cleanEx.Message}"); }
                    }
                }
                catch (Exception cleanEx) { System.Diagnostics.Debug.WriteLine($"Log cleanup error: {cleanEx.Message}"); }
            }
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"AppendLog failed: {ex.Message}"); }
        finally { _loggingRecursive = false; }
    }
}
