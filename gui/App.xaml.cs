using System.Windows;
using System.Windows.Threading;
using WindowsTweakLauncher.Services;

namespace WindowsTweakLauncher;

public partial class App : Application
{
    private static Mutex? _instanceMutex;

    public App()
    {
        DispatcherUnhandledException += (sender, e) =>
        {
            try
            {
                SettingsService.TrySaveWithLock();
                var ex = e.Exception;
                if (ex is OutOfMemoryException)
                {
                    e.Handled = true;
                    try { ProcessRunner.AppendLog($"Critical exception (dispatcher): {ex.GetType().Name}: {ex.Message}"); } catch { }
                    Current?.Shutdown();
                    return;
                }
                // Write full crash details to temp file
                var crashPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsTweakLauncher_crash.log");
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"=== CRASH at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)} ===");
                for (var cur = ex; cur != null; cur = cur.InnerException)
                    sb.AppendLine($"{cur.GetType().Name}: {cur.Message}");
                sb.AppendLine($"Stack: {ex.StackTrace}");
                try { System.IO.File.AppendAllText(crashPath, sb.ToString()); } catch { }

                var msg = ex.Message;
                if (ex.InnerException != null)
                    msg += $"\n\u2192 {ex.InnerException.GetType().Name}: {ex.InnerException.Message}";
                MessageBox.Show($"Unhandled error: {msg}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
                // Shut down to prevent zombie processes holding the mutex
                Current?.Shutdown();
            }
            catch { }
        };
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            if (e.ExceptionObject is Exception ex && ex is not AccessViolationException
                and not StackOverflowException and not OutOfMemoryException)
            {
                SettingsService.TrySaveWithLock();
                ProcessRunner.AppendLog($"Fatal error: {ex.GetType().Name}: {ex.Message}");
                // Dispatch to UI thread for MessageBox and Shutdown
                try
                {
                    Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            MessageBox.Show($"Fatal error: {ex.Message}", "Fatal Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            Current?.Shutdown();
                        }
                        catch { }
                    });
                }
                catch { }
            }
        };
        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            ProcessRunner.AppendLog($"Unobserved task exception: {e.Exception}");
            e.SetObserved();
        };
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            _instanceMutex = new Mutex(false, @"Global\WindowsTweakLauncher-InstanceMutex", out bool _);
            try
            {
                if (!_instanceMutex.WaitOne(TimeSpan.Zero, false))
                {
                    // Brief retry to avoid race with exiting instance (MINOR-4)
                    System.Threading.Thread.Sleep(500);
                    if (!_instanceMutex.WaitOne(TimeSpan.Zero, false))
                    {
                        MessageBox.Show("Application is already running.", "Already Running",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        try { _instanceMutex.Dispose(); } catch { }
                        _instanceMutex = null;
                        Shutdown();
                        return;
                    }
                }
            }
            catch (AbandonedMutexException ame)
            {
                ProcessRunner.AppendLog($"Instance mutex was abandoned: {ame.Message}");
            }
        }
        catch (UnauthorizedAccessException)
        {
            try { _instanceMutex?.Dispose(); } catch { }
            _instanceMutex = null;
            MessageBox.Show("Application is already running at a different privilege level.", "Already Running",
                MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }
        catch (Exception ex)
        {
            try { _instanceMutex?.Dispose(); } catch { }
            _instanceMutex = null;
            ProcessRunner.AppendLog($"Instance mutex error: {ex.Message}");
            Shutdown();
            return;
        }
        if (_instanceMutex != null)
            base.OnStartup(e);
        else
            Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        var m = _instanceMutex;
        if (m != null)
        {
            try { m.ReleaseMutex(); } catch (Exception ex) { ProcessRunner.AppendLog($"Failed to release instance mutex: {ex.Message}"); }
            try { m.Dispose(); } catch (Exception ex) { ProcessRunner.AppendLog($"Failed to dispose instance mutex: {ex.Message}"); }
        }
        _instanceMutex = null;
        base.OnExit(e);
    }
}
