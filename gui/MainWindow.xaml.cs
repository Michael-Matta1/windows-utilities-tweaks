using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using WindowsTweakLauncher.Models;
using WindowsTweakLauncher.Services;

namespace WindowsTweakLauncher;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    public AppSettings Settings => _settings;
    private AppSettings _settings = AppSettings.CreateDefault();
    public ObservableCollection<TweakItem> Tweaks { get; } = new();

    private TweakItem? _selectedTweak;
    private bool _loaded;
    private System.ComponentModel.CancelEventHandler? _closingHandler;
    public TweakItem? SelectedTweak
    {
        get => _selectedTweak;
        set
        {
            _selectedTweak = value;
            OnPropertyChanged(nameof(SelectedTweak));
            if (Dispatcher.CheckAccess())
                UpdateDetailPanel();
            else if (!Dispatcher.HasShutdownStarted)
                Dispatcher.BeginInvoke(UpdateDetailPanel);
        }
    }

    private bool _isApplying;
    private bool _isProcessingBatch;

    public MainWindow()
    {
        try { SettingsService.Load(); }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load settings: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        _settings = SettingsService.Current ?? AppSettings.CreateDefault();
        DataContext = this;
        InitializeComponent();
        _closingHandler = (sender, e) => { try { SettingsService.Save(); } catch (Exception ex) { ProcessRunner.AppendLog($"Closing save failed: {ex.Message}"); } };
        Closing += _closingHandler;
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (_loaded) return;
        _loaded = true;
        try
        {
            var items = TweakCatalog.Load() ?? [];
            Tweaks.Clear();
            foreach (var item in items)
                Tweaks.Add(item);

            TweaksList.ItemsSource = Tweaks;

            foreach (var t in Tweaks)
                t.PropertyChanged += TweakPropertyChanged;

            AutoDetectRepoPath();

            CtxDotMap["CtxHideCastToDevice"] = CtxHideCastToDeviceDot;
            CtxDotMap["CtxHideShare"] = CtxHideShareDot;
            CtxDotMap["CtxHideGiveAccessTo"] = CtxHideGiveAccessToDot;
            CtxDotMap["CtxHidePaint3D"] = CtxHidePaint3DDot;
            CtxDotMap["CtxHideEditPhotos"] = CtxHideEditPhotosDot;
            CtxDotMap["CtxHideDefenderScan"] = CtxHideDefenderScanDot;

            if (!string.IsNullOrWhiteSpace(Settings.RepoPath) && Directory.Exists(Settings.RepoPath))
            {
                RegistryService.RefreshAllStatus(Tweaks.ToList());
                PlaceholderText.Text = "Select a tweak to get started";
            }
            else
            {
                PlaceholderText.Text = "Set your repo path above first";
            }

            UpdateGrouping();
            UpdateSelectionCount();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error during startup: {ex.Message}", "Startup Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static readonly HashSet<string> NeedsRestartIds = new(StringComparer.OrdinalIgnoreCase)
    {
        "classic_ctx",
        "explorer_location",
        "hide_context_menu_items",
        "pin_taskbar",
        "ocr_shortcut",
        "voice_typing_shortcut",
        "custom_shortcut",
        "explorer_cmd",
    };

    private static readonly Dictionary<string, string> CtxClsidMapping = new(System.StringComparer.OrdinalIgnoreCase)
    {
        ["CtxHideCastToDevice"] = Paths.Clsids.CtxHideCastToDevice,
        ["CtxHideShare"] = Paths.Clsids.CtxHideShare,
        ["CtxHideGiveAccessTo"] = Paths.Clsids.CtxHideGiveAccessTo,
        ["CtxHidePaint3D"] = Paths.Clsids.CtxHidePaint3D,
        ["CtxHideEditPhotos"] = Paths.Clsids.CtxHideEditPhotos,
        ["CtxHideDefenderScan"] = Paths.Clsids.CtxHideDefenderScan,
    };

    private static readonly Dictionary<string, System.Windows.Shapes.Ellipse> CtxDotMap = [];

    private static readonly string[] SignatureFolders =
    {
        "Fix Alt+Shift LanguageToggle",
        "Privacy & Telemetry Guide",
        "Add a Silent Markdown Creator to Context Menu",
        "Copy Path Without Quotes",
        "Set File Explorer to Open to a Custom Location"
    };

    private void AutoDetectRepoPath()
    {
        if (!string.IsNullOrWhiteSpace(Settings.RepoPath) && Directory.Exists(Settings.RepoPath))
            return;

        var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        for (int i = 0; i < 6 && dir != null; i++, dir = dir.Parent)
        {
            int matches = SignatureFolders.Count(f => Directory.Exists(Path.Combine(dir.FullName, f)));
            if (matches >= 3)
            {
                Settings.RepoPath = dir.FullName;
                SettingsService.SaveRepoPath(dir.FullName);
                RepoPathBox.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                return;
            }
        }
    }

    private void UpdateGrouping()
    {
        if (CollectionViewSource.GetDefaultView(Tweaks) is CollectionView view)
        {
            view.GroupDescriptions.Clear();
            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(TweakItem.Category)));
            view.Filter = o => o is TweakItem item && !item.HiddenFromList;
        }
    }

    private void UpdateDetailPanel()
    {
        try
        {
        if (SelectedTweak == null)
        {
            PlaceholderText.Visibility = Visibility.Visible;
            DetailPanel.Visibility = Visibility.Collapsed;
            BackupInfoPanel.Visibility = Visibility.Collapsed;
            ApplyButton.IsEnabled = false;
            RevertButton.IsEnabled = false;
            return;
        }
        var tweak = SelectedTweak;
        ApplyButton.IsEnabled = (tweak.Status != TweakStatus.Applied && tweak.Status != TweakStatus.PartialApplied)
                                   || tweak.Status == TweakStatus.Reapplicable;
        PlaceholderText.Visibility = Visibility.Collapsed;
        DetailPanel.Visibility = Visibility.Visible;

        DetailName.Text = tweak.Name;
        DetailDescription.Text = tweak.Description;

        StatusDot.Fill = tweak.Status switch
        {
            TweakStatus.Applied => TryFindResource("AppliedBrush") as Brush ?? Brushes.Green,
            TweakStatus.NotApplied => TryFindResource("NotAppliedBrush") as Brush ?? Brushes.Gray,
            TweakStatus.PartialApplied => TryFindResource("PartialBrush") as Brush ?? Brushes.Blue,
            TweakStatus.Reapplicable => TryFindResource("ReapplicableBrush") as Brush ?? Brushes.Purple,
            _ => TryFindResource("UnknownBrush") as Brush ?? Brushes.Goldenrod
        };
        StatusText.Text = tweak.Status switch
        {
            TweakStatus.Applied => "Applied",
            TweakStatus.NotApplied => "Not Applied",
            TweakStatus.PartialApplied => "Partial",
            TweakStatus.Reapplicable => "Re-applicable",
            _ => "Unknown"
        };

        if (tweak.BackupTargets?.Count > 0)
        {
            var lines = tweak.BackupTargets.Where(t => t != null).Select(t => $"Will back up: {t!.Hive}\\{t.RegPath}");
            BackupInfoText.Text = string.Join("\n", lines);
            BackupInfoText.Foreground = TryFindResource("MutedTextBrush") as Brush ?? Brushes.Gray;
        }
        else if (tweak.BackupNote != null)
        {
            BackupInfoText.Text = tweak.BackupNote;
            BackupInfoText.Foreground = TryFindResource("AmberBrush") as Brush ?? Brushes.Orange;
        }
        else
        {
            BackupInfoText.Text = "This tweak does not modify the registry — the backup setting has no effect here.";
            BackupInfoText.Foreground = TryFindResource("MutedTextBrush") as Brush ?? Brushes.Gray;
        }
        BackupInfoPanel.Visibility = Visibility.Visible;

        InputPanel.Visibility = Visibility.Collapsed;
        InputTextBox.Visibility = Visibility.Collapsed;
        InputComboBox.Visibility = Visibility.Collapsed;
        InputLabel.Visibility = Visibility.Collapsed;
        InputBrowseButton.Visibility = Visibility.Collapsed;
        HotkeyPanel.Visibility = Visibility.Collapsed;
        HwIdentityPanel.Visibility = Visibility.Collapsed;

        if (tweak.Strategy == TweakStrategy.NeedsInputThenReg)
        {
            InputPanel.Visibility = Visibility.Visible;
            InputTextBox.Visibility = Visibility.Visible;
            InputLabel.Text = "Target folder:";
            InputLabel.Visibility = Visibility.Visible;
            InputComboBox.Visibility = Visibility.Collapsed;
            if (tweak.UserInput == null)
                tweak.UserInput = Paths.Windows;
            InputTextBox.Text = tweak.UserInput;
            InputBrowseButton.Visibility = Visibility.Visible;
        }
        else if (tweak.Strategy == TweakStrategy.NeedsInputVariant && tweak.Variants?.Count > 0)
        {
            InputPanel.Visibility = Visibility.Visible;
            InputTextBox.Visibility = Visibility.Collapsed;
            InputComboBox.Visibility = Visibility.Visible;
            InputBrowseButton.Visibility = Visibility.Collapsed;
            InputLabel.Text = tweak.VariantLabel ?? "Choose script behavior:";
            InputLabel.Visibility = Visibility.Visible;
            InputComboBox.ItemsSource = tweak.Variants;
            InputComboBox.SelectedIndex = BoundVariantIndex(tweak);
            HotkeyPanel.Visibility = Visibility.Visible;
            HotkeyTextBox.Text = tweak.Hotkey;
        }
        else if (tweak.Strategy == TweakStrategy.HardwareIdentityTest)
        {
            InputPanel.Visibility = Visibility.Visible;
            InputTextBox.Visibility = Visibility.Collapsed;
            InputComboBox.Visibility = Visibility.Collapsed;
            InputBrowseButton.Visibility = Visibility.Collapsed;
            InputLabel.Visibility = Visibility.Collapsed;
            HotkeyPanel.Visibility = Visibility.Collapsed;
            HwIdentityPanel.Visibility = Visibility.Visible;
            if (tweak.ManufacturerInput == null && tweak.ProductNameInput == null)
            {
                tweak.ManufacturerInput = "Samsung";
                tweak.ProductNameInput = "950XDB";
            }
            ManufacturerTextBox.Text = tweak.ManufacturerInput ?? "";
            ProductNameTextBox.Text = tweak.ProductNameInput ?? "";
        }
        else if (tweak.Strategy == TweakStrategy.ShortcutWithManualPin && !string.IsNullOrWhiteSpace(tweak.VariantLabel))
        {
            InputPanel.Visibility = Visibility.Visible;
            InputTextBox.Visibility = Visibility.Visible;
            InputLabel.Text = tweak.VariantLabel;
            InputLabel.Visibility = Visibility.Visible;
            InputComboBox.Visibility = Visibility.Collapsed;
            InputTextBox.Text = tweak.UserInput ?? "";
            InputBrowseButton.Visibility = Visibility.Collapsed;
        }
        else if (tweak.Strategy == TweakStrategy.RunBat)
        {
            InputPanel.Visibility = Visibility.Visible;
            InputTextBox.Visibility = Visibility.Visible;
            InputLabel.Text = "Target folder to scan for .exe files:";
            InputLabel.Visibility = Visibility.Visible;
            InputComboBox.Visibility = Visibility.Collapsed;
            InputTextBox.Text = tweak.UserInput ?? "";
            InputBrowseButton.Visibility = Visibility.Visible;
        }
        else if (tweak.Strategy == TweakStrategy.ImportReg && tweak.Variants?.Count > 0)
        {
            InputPanel.Visibility = Visibility.Visible;
            InputTextBox.Visibility = Visibility.Collapsed;
            InputComboBox.Visibility = Visibility.Visible;
            InputBrowseButton.Visibility = Visibility.Collapsed;
            InputLabel.Text = tweak.VariantLabel ?? "Choose version:";
            InputLabel.Visibility = Visibility.Visible;
            InputComboBox.ItemsSource = tweak.Variants;
            InputComboBox.SelectedIndex = BoundVariantIndex(tweak);
        }
        else if (tweak.Strategy == TweakStrategy.NeedsInputVariant && tweak.Variants is not { Count: > 0 })
        {
            InputPanel.Visibility = Visibility.Visible;
            InputLabel.Visibility = Visibility.Visible;
            InputLabel.Text = "Error: variants list is empty";
            InputComboBox.Visibility = Visibility.Collapsed;
            InputTextBox.Visibility = Visibility.Collapsed;
            InputBrowseButton.Visibility = Visibility.Collapsed;
        }

        bool hideApplyRevert = tweak.Strategy == TweakStrategy.GuideOnly;
        ApplyButton.Visibility = hideApplyRevert ? Visibility.Collapsed : Visibility.Visible;
        RevertButton.Visibility = hideApplyRevert ? Visibility.Collapsed : Visibility.Visible;
        ManualGuideButton.Visibility = Visibility.Visible;
        OpenFolderButton.Visibility = Visibility.Visible;
        RestartExplorerButton.Visibility = NeedsRestartIds.Contains(tweak.Id) ? Visibility.Visible : Visibility.Collapsed;

        RevertButton.IsEnabled = tweak.CanRevert && (tweak.Status == TweakStatus.Applied || tweak.Status == TweakStatus.PartialApplied || tweak.Status == TweakStatus.Reapplicable);

        bool isCtxMenu = string.Equals(tweak.Id, "hide_context_menu_items", StringComparison.OrdinalIgnoreCase);
        bool isTermProfiles = string.Equals(tweak.Id, "terminal_profiles", StringComparison.OrdinalIgnoreCase);
        bool isExpCmd = string.Equals(tweak.Id, "explorer_cmd", StringComparison.OrdinalIgnoreCase);
        CtxMenuApplyBtn.Visibility = isCtxMenu ? Visibility.Visible : Visibility.Collapsed;
        CtxMenuRestoreBtn.Visibility = isCtxMenu ? Visibility.Visible : Visibility.Collapsed;
        TermProfilesApplyBtn.Visibility = isTermProfiles ? Visibility.Visible : Visibility.Collapsed;
        TermProfilesRestoreBtn.Visibility = isTermProfiles ? Visibility.Visible : Visibility.Collapsed;
        ExpCmdApplyBtn.Visibility = isExpCmd ? Visibility.Visible : Visibility.Collapsed;
        ExpCmdRestoreBtn.Visibility = isExpCmd ? Visibility.Visible : Visibility.Collapsed;

        SpecialFoldersPanel.Visibility = string.Equals(tweak.Id, "special_folders", StringComparison.OrdinalIgnoreCase) ? Visibility.Visible : Visibility.Collapsed;
        TerminalProfilesPanel.Visibility = string.Equals(tweak.Id, "terminal_profiles", StringComparison.OrdinalIgnoreCase) ? Visibility.Visible : Visibility.Collapsed;
        ContextMenuItemsPanel.Visibility = string.Equals(tweak.Id, "hide_context_menu_items", StringComparison.OrdinalIgnoreCase)
            ? Visibility.Visible : Visibility.Collapsed;
        ShortcutProfilesPanel.Visibility = string.Equals(tweak.Id, "pin_taskbar", StringComparison.OrdinalIgnoreCase)
            ? Visibility.Visible : Visibility.Collapsed;
        ExplorerCmdPanel.Visibility = string.Equals(tweak.Id, "explorer_cmd", StringComparison.OrdinalIgnoreCase)
            ? Visibility.Visible : Visibility.Collapsed;

        if (string.Equals(tweak.Id, "hide_context_menu_items", StringComparison.OrdinalIgnoreCase))
        {
            var blockedKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Blocked";
            CtxHideCastToDevice.IsChecked = RegistryService.CheckValueExists("HKLM", blockedKey, Paths.Clsids.CtxHideCastToDevice);
            CtxHideShare.IsChecked = RegistryService.CheckValueExists("HKLM", blockedKey, Paths.Clsids.CtxHideShare);
            CtxHideGiveAccessTo.IsChecked = RegistryService.CheckValueExists("HKLM", blockedKey, Paths.Clsids.CtxHideGiveAccessTo);
            CtxHidePaint3D.IsChecked = RegistryService.CheckValueExists("HKLM", blockedKey, Paths.Clsids.CtxHidePaint3D);
            CtxHideEditPhotos.IsChecked = RegistryService.CheckValueExists("HKLM", blockedKey, Paths.Clsids.CtxHideEditPhotos);
            CtxHideDefenderScan.IsChecked = RegistryService.CheckValueExists("HKLM", blockedKey, Paths.Clsids.CtxHideDefenderScan);
        }
        if (string.Equals(tweak.Id, "explorer_cmd", StringComparison.OrdinalIgnoreCase))
        {
            var winDir = Paths.Windows;
            ExpCmdAcmd.IsChecked = File.Exists(Path.Combine(winDir, "acmd.bat"));
            ExpCmdAps.IsChecked = File.Exists(Path.Combine(winDir, "aps.bat"));
            ExpCmdAtr.IsChecked = File.Exists(Path.Combine(winDir, "atr.bat"));
        }
        if (string.Equals(tweak.Id, "terminal_profiles", StringComparison.OrdinalIgnoreCase))
        {
            TermProfileIse.IsChecked = CheckTerminalProfileGuid(Paths.Clsids.TerminalIseProfile);
            TermProfileGitBash.IsChecked = CheckTerminalProfileGuid(Paths.Clsids.TerminalGitBashProfile);
        }

        if (string.Equals(tweak.Id, "markdown_ctx", StringComparison.OrdinalIgnoreCase))
        {
            MarkdownCtxPanel.Visibility = Visibility.Visible;
            if (!string.IsNullOrEmpty(tweak.MarkdownMenuName)) MarkdownCtxMenuName.Text = tweak.MarkdownMenuName;
            if (!string.IsNullOrEmpty(tweak.MarkdownExtension)) MarkdownCtxExtension.Text = tweak.MarkdownExtension;
        }
        else
        {
            MarkdownCtxPanel.Visibility = Visibility.Collapsed;
        }

        UpdateSubOptionDots(tweak);
        }
        catch (Exception ex)
        {
            ProcessRunner.AppendLog($"UpdateDetailPanel error: {ex}");
            StatusText.Text = "Error updating panel";
        }
    }

    private void UpdateSubOptionDots(TweakItem tweak)
    {
        if (string.Equals(tweak.Id, "explorer_cmd", StringComparison.OrdinalIgnoreCase))
        {
            var winDir = Paths.Windows;
            ExpCmdAcmdDot.Visibility = File.Exists(Path.Combine(winDir, "acmd.bat")) ? Visibility.Visible : Visibility.Collapsed;
            ExpCmdApsDot.Visibility = File.Exists(Path.Combine(winDir, "aps.bat")) ? Visibility.Visible : Visibility.Collapsed;
            ExpCmdAtrDot.Visibility = File.Exists(Path.Combine(winDir, "atr.bat")) ? Visibility.Visible : Visibility.Collapsed;
        }
        else if (string.Equals(tweak.Id, "terminal_profiles", StringComparison.OrdinalIgnoreCase))
        {
            TermProfileIseDot.Visibility = CheckTerminalProfileGuid(Paths.Clsids.TerminalIseProfile) ? Visibility.Visible : Visibility.Collapsed;
            TermProfileGitBashDot.Visibility = CheckTerminalProfileGuid(Paths.Clsids.TerminalGitBashProfile) ? Visibility.Visible : Visibility.Collapsed;
        }
        else if (string.Equals(tweak.Id, "hide_context_menu_items", StringComparison.OrdinalIgnoreCase))
        {
            var blockedKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Blocked";
            foreach (var kvp in CtxClsidMapping)
            {
                var applied = RegistryService.CheckValueExists("HKLM", blockedKey, kvp.Value);
                if (CtxDotMap.TryGetValue(kvp.Key, out var dot))
                    dot.Visibility = applied ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }

    private static bool CheckTerminalProfileGuid(string targetGuid)
    {
        var settingsPath = Paths.FindTerminalSettings();
        if (settingsPath == null) return false;

        try
        {
            var json = File.ReadAllText(settingsPath);
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("profiles", out var profilesEl) &&
                profilesEl.TryGetProperty("list", out var listEl) &&
                listEl.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var item in listEl.EnumerateArray())
                {
                    if (item.TryGetProperty("guid", out var guidEl) &&
                        string.Equals(guidEl.GetString(), targetGuid, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
        }
        catch (Exception ex) { ProcessRunner.AppendLog($"CheckTerminalProfileGuid error: {ex.Message}"); }

        return false;
    }

    private void SelectTweakName_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is TweakItem item)
            SelectedTweak = item;
    }

    private void RepoPath_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (PlaceholderText != null)
            PlaceholderText.Text = string.IsNullOrWhiteSpace(Settings.RepoPath) || !Directory.Exists(Settings.RepoPath)
                ? "Set your repo path above first"
                : "Select a tweak to get started";
    }

    private void RepoPath_LostFocus(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(Settings.RepoPath) && Directory.Exists(Settings.RepoPath))
                RegistryService.RefreshAllStatus(Tweaks.ToList());
        }
        catch (Exception ex)
        {
            ProcessRunner.AppendLog($"RepoPath_LostFocus error: {ex.Message}");
        }
    }

    private void BackupEnabled_Changed(object sender, RoutedEventArgs e)
    {
        try { SettingsService.Save(); }
        catch (Exception ex) { ProcessRunner.AppendLog($"BackupEnabled_Changed save failed: {ex.Message}"); }
    }

    private bool _updatingBackupDir;

    private void BackupDir_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_updatingBackupDir) return;
        _updatingBackupDir = true;
        try
        {
            var val = (BackupDirBox.Text ?? "").Trim();
            Settings.BackupDirectory = val;
            if (string.IsNullOrWhiteSpace(Settings.BackupDirectory))
                Settings.BackupDirectory = Paths.BackupDirDefault;
            else if (Settings.BackupDirectory.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0
                     || Settings.BackupDirectory.IndexOfAny(new[] { '*', '?', '\"', '|' }) >= 0)
            {
                ProcessRunner.AppendLog($"Backup directory contains invalid characters \u2014 resetting to default.");
                Settings.BackupDirectory = Paths.BackupDirDefault;
            }
            BackupDirBox.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
        }
        finally { _updatingBackupDir = false; }
    }

    private void BackupDir_LostFocus(object sender, RoutedEventArgs e)
    {
        try { SettingsService.Save(); }
        catch (Exception ex) { ProcessRunner.AppendLog($"BackupDir_LostFocus save failed: {ex.Message}"); }
    }

    private void BrowseRepoPath_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new OpenFolderDialog { Title = "Select Repo Root" };
            if (dialog.ShowDialog() == true)
            {
                Settings.RepoPath = dialog.FolderName;
                SettingsService.SaveRepoPath(dialog.FolderName);
                RepoPathBox.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                RefreshAllStatus();
                PlaceholderText.Text = "Select a tweak to get started";
            }
        }
        catch (Exception ex) { ProcessRunner.AppendLog($"BrowseRepoPath error: {ex.Message}"); }
    }

    private void BrowseBackupDir_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new OpenFolderDialog { Title = "Select Backup Directory" };
            if (dialog.ShowDialog() == true)
            {
                if (dialog.FolderName.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0
                    || dialog.FolderName.IndexOfAny(new[] { '*', '?', '\"', '|' }) >= 0)
                {
                    ProcessRunner.AppendLog($"BrowseBackupDir: invalid path characters in selected folder.");
                    return;
                }
                Settings.BackupDirectory = dialog.FolderName;
                BackupDirBox.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                SettingsService.Save();
            }
            else if (string.IsNullOrWhiteSpace(Settings.BackupDirectory))
            {
                Settings.BackupDirectory = Paths.BackupDirDefault;
                BackupDirBox.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                SettingsService.Save();
            }
        }
        catch (Exception ex) { ProcessRunner.AppendLog($"BrowseBackupDir error: {ex.Message}"); }
    }

    private void RefreshStatus_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Settings.RepoPath) || !Directory.Exists(Settings.RepoPath))
            {
                PlaceholderText.Text = "Set your repo path above first";
                return;
            }
            RefreshAllStatus();
        }
        catch (Exception ex)
        {
            ProcessRunner.AppendLog($"Status refresh failed: {ex.Message}");
            FooterText.Text = "Status refresh failed.";
        }
    }

    private void RefreshAllStatus()
    {
        RegistryService.RefreshAllStatus(Tweaks.ToList());
        if (SelectedTweak != null)
            UpdateDetailPanel();
        UpdateSelectionCount();
        FooterText.Text = "Status refreshed.";
    }

    private void SelectAllScriptable_Click(object sender, RoutedEventArgs e)
    {
        foreach (var t in Tweaks)
        {
            if (t.HiddenFromList)
            {
                t.IsSelected = false;
                continue;
            }

            if (t.Strategy == TweakStrategy.GuideOnly)
            {
                t.IsSelected = string.Equals(t.Id, "hide_context_menu_items", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(t.Id, "explorer_cmd", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(t.Id, "terminal_profiles", StringComparison.OrdinalIgnoreCase);
                continue;
            }

            t.IsSelected = t.Strategy switch
            {
                TweakStrategy.NeedsInputThenReg => false,
                TweakStrategy.NeedsInputVariant => false,
                TweakStrategy.HardwareIdentityTest => false,
                TweakStrategy.RunBat => false,
                _ => t.Strategy != TweakStrategy.CopyVbsThenReg && t.Strategy != TweakStrategy.ShortcutWithManualPin
                    && t.Strategy != TweakStrategy.TerminalProfileJson && t.Variants is null
            };
        }
        UpdateSelectionCount();
    }

    private void ClearAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var t in Tweaks)
            t.IsSelected = false;
        UpdateSelectionCount();
    }

    private void TweakPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TweakItem.IsSelected))
            UpdateSelectionCount();
    }

    private void UpdateSelectionCount()
    {
        var visible = Tweaks.Where(t => !t.HiddenFromList).ToList();
        var count = visible.Count(t => t.IsSelected);
        var total = visible.Count;
        SelectionCountText.Text = $"{count} of {total} selected";
        ApplySelectedButton.Content = $"\u25B6 Apply Selected ({count})";
        ApplySelectedButton.IsEnabled = count > 0;
        var revertibleCount = visible.Count(t => t.IsSelected && t.CanRevert && (t.Status == TweakStatus.Applied || t.Status == TweakStatus.PartialApplied || t.Status == TweakStatus.Reapplicable));
        RevertSelectedButton.Content = $"\u21a9 Revert Selected ({revertibleCount})";
        RevertSelectedButton.IsEnabled = revertibleCount > 0;
    }

    private void ApplySelected_Click(object sender, RoutedEventArgs e)
    {
        if (_isProcessingBatch) return;
        var selected = Tweaks.Where(t => t.IsSelected && !t.HiddenFromList).ToList();
        if (selected.Count == 0) return;

        _isProcessingBatch = true;
        try
        {
        // Handle GuideOnly tweaks first — their sub-handlers have their own guards
        foreach (var tweak in selected.Where(t => t.Strategy == TweakStrategy.GuideOnly))
        {
            try
            {
                if (string.Equals(tweak.Id, "hide_context_menu_items", StringComparison.OrdinalIgnoreCase))
                {
                    CtxHideCastToDevice.IsChecked = true;
                    CtxHideShare.IsChecked = true;
                    CtxHideGiveAccessTo.IsChecked = true;
                    CtxHidePaint3D.IsChecked = true;
                    CtxHideEditPhotos.IsChecked = true;
                    CtxHideDefenderScan.IsChecked = true;
                    ContextMenuHide_Click(sender, e);
                }
                else if (string.Equals(tweak.Id, "explorer_cmd", StringComparison.OrdinalIgnoreCase))
                {
                    ExpCmdAcmd.IsChecked = true;
                    ExpCmdAps.IsChecked = true;
                    ExpCmdAtr.IsChecked = true;
                    ExplorerCmdApply_Click(sender, e);
                }
                else if (string.Equals(tweak.Id, "terminal_profiles", StringComparison.OrdinalIgnoreCase))
                {
                    TermProfileIse.IsChecked = true;
                    TermProfileGitBash.IsChecked = true;
                    TerminalProfilesApply_Click(sender, e);
                }
                else
                {
                    AppendLog(tweak, $"{tweak.Name}: this guide has no automated batch apply.");
                }
            }
            catch (Exception ex)
            {
                ProcessRunner.AppendLog($"Error applying {tweak.Id}: {ex.Message}");
                AppendLog(tweak, $"Error: {ex.Message}");
            }
        }

        if (_isApplying) return;
        _isApplying = true;
        ApplySelectedButton.IsEnabled = false;
        ApplyButton.IsEnabled = false;
        RevertButton.IsEnabled = false;
        try
        {
            foreach (var tweak in selected.Where(t => t.Strategy != TweakStrategy.GuideOnly))
            {
                try
                {
            if (tweak.Strategy == TweakStrategy.NeedsInputThenReg && string.IsNullOrWhiteSpace(tweak.UserInput))
            {
                AppendLog(tweak, $"{tweak.Name}: skipped \u2014 needs configuration, open it individually.");
                continue;
            }

            if (tweak.Strategy == TweakStrategy.RunBat && string.IsNullOrWhiteSpace(tweak.UserInput))
            {
                AppendLog(tweak, $"{tweak.Name}: skipped \u2014 needs a target folder, open it individually.");
                continue;
            }

            ApplyTweakInternal(tweak);
                }
                catch (Exception ex)
                {
                    ProcessRunner.AppendLog($"Error applying {tweak.Id}: {ex.Message}");
                    AppendLog(tweak, $"Error: {ex.Message}");
                }
            }

            RefreshAllStatus();
            UpdateSelectionCount();
        }
        catch (Exception ex)
        {
            ProcessRunner.AppendLog($"Error during batch apply: {ex}");
            MessageBox.Show($"Error during batch apply: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            RefreshAllStatus();
            if (SelectedTweak != null) UpdateDetailPanel();
            _isApplying = false;
            ApplySelectedButton.IsEnabled = true;
            UpdateSelectionCount();
        }
        }
        finally { _isProcessingBatch = false; }
    }

    private void RevertSelected_Click(object sender, RoutedEventArgs e)
    {
        if (_isProcessingBatch) return;
        var selected = Tweaks.Where(t => t.IsSelected && !t.HiddenFromList && t.CanRevert
            && (t.Status == TweakStatus.Applied || t.Status == TweakStatus.PartialApplied || t.Status == TweakStatus.Reapplicable)).ToList();
        if (selected.Count == 0) return;

        _isProcessingBatch = true;
        try
        {
        // Handle GuideOnly tweaks first — their sub-handlers have their own guards
        foreach (var tweak in selected.Where(t => t.Strategy == TweakStrategy.GuideOnly))
        {
            try
            {
                if (string.Equals(tweak.Id, "hide_context_menu_items", StringComparison.OrdinalIgnoreCase))
                {
                    CtxHideCastToDevice.IsChecked = true;
                    CtxHideShare.IsChecked = true;
                    CtxHideGiveAccessTo.IsChecked = true;
                    CtxHidePaint3D.IsChecked = true;
                    CtxHideEditPhotos.IsChecked = true;
                    CtxHideDefenderScan.IsChecked = true;
                    ContextMenuRestore_Click(sender, e);
                }
                else if (string.Equals(tweak.Id, "explorer_cmd", StringComparison.OrdinalIgnoreCase))
                {
                    ExpCmdAcmd.IsChecked = true;
                    ExpCmdAps.IsChecked = true;
                    ExpCmdAtr.IsChecked = true;
                    ExplorerCmdRestore_Click(sender, e);
                }
                else if (string.Equals(tweak.Id, "terminal_profiles", StringComparison.OrdinalIgnoreCase))
                {
                    TermProfileIse.IsChecked = true;
                    TermProfileGitBash.IsChecked = true;
                    TerminalProfilesRestore_Click(sender, e);
                }
                else
                {
                    AppendLog(tweak, $"{tweak.Name}: this guide has no automated batch revert.");
                }
            }
            catch (Exception ex)
            {
                ProcessRunner.AppendLog($"Error reverting {tweak.Id}: {ex.Message}");
                AppendLog(tweak, $"Error: {ex.Message}");
            }
        }

        if (_isApplying) return;
        _isApplying = true;
        RevertSelectedButton.IsEnabled = false;
        ApplyButton.IsEnabled = false;
        RevertButton.IsEnabled = false;
        try
        {
            foreach (var tweak in selected.Where(t => t.Strategy != TweakStrategy.GuideOnly))
            {
                try
                {
            RevertTweakInternal(tweak);
                }
                catch (Exception ex)
                {
                    ProcessRunner.AppendLog($"Error reverting {tweak.Id}: {ex.Message}");
                    AppendLog(tweak, $"Error: {ex.Message}");
                }
            }

            RefreshAllStatus();
            UpdateSelectionCount();
        }
        catch (Exception ex)
        {
            ProcessRunner.AppendLog($"Error during batch revert: {ex}");
            MessageBox.Show($"Error during batch revert: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            RefreshAllStatus();
            if (SelectedTweak != null) UpdateDetailPanel();
            _isApplying = false;
            RevertSelectedButton.IsEnabled = true;
            UpdateSelectionCount();
        }
        }
        finally { _isProcessingBatch = false; }
    }

    private void ApplyTweak_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedTweak == null) return;
        if (SelectedTweak.Strategy == TweakStrategy.GuideOnly && !string.Equals(SelectedTweak.Id, "hide_context_menu_items", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(SelectedTweak.Id, "explorer_cmd", StringComparison.OrdinalIgnoreCase) && !string.Equals(SelectedTweak.Id, "terminal_profiles", StringComparison.OrdinalIgnoreCase)) return;

        // Handle GuideOnly tweaks outside _isApplying guard so sub-handlers don't no-op
        if (string.Equals(SelectedTweak.Id, "hide_context_menu_items", StringComparison.OrdinalIgnoreCase))
        {
            ContextMenuHide_Click(sender, e);
            UpdateDetailPanel();
            UpdateSelectionCount();
            return;
        }
        if (string.Equals(SelectedTweak.Id, "explorer_cmd", StringComparison.OrdinalIgnoreCase))
        {
            ExplorerCmdApply_Click(sender, e);
            UpdateDetailPanel();
            UpdateSelectionCount();
            return;
        }
        if (string.Equals(SelectedTweak.Id, "terminal_profiles", StringComparison.OrdinalIgnoreCase))
        {
            TerminalProfilesApply_Click(sender, e);
            UpdateDetailPanel();
            UpdateSelectionCount();
            return;
        }

        if (_isApplying) return;
        _isApplying = true;
        try
        {
            ApplyTweakInternal(SelectedTweak);
            RefreshAllStatus();
            UpdateDetailPanel();
            UpdateSelectionCount();
        }
        catch (Exception ex) { AppendLog(SelectedTweak, $"Error: {ex}"); UpdateDetailPanel(); UpdateSelectionCount(); }
        finally { _isApplying = false; }
    }

    private void ApplyTweakInternal(TweakItem tweak)
    {
        if (tweak == null) { AppendLogStatic("ApplyTweakInternal: tweak is null"); return; }
        if (tweak.RequiresConfirmation)
        {
            var result = MessageBox.Show(
                $"This will modify {tweak.Name}, which affects the whole system. Continue?",
                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
            {
                AppendLog(tweak, "Apply cancelled by user.");
                return;
            }
        }

        if (Settings.BackupEnabled && Settings.BackupPreApply)
        {
            if (tweak.BackupTargets != null)
                foreach (var target in tweak.BackupTargets)
                {
                    var (success, message) = RegistryService.BackupRegistryKey(target, tweak.Id);
                    AppendLog(tweak, message);
                    if (!success)
                    {
                    AppendLog(tweak, $"ABORTED: backup failed for {target.Hive}\\{target.RegPath}");
                    return;
                }
            }
        }
        else
        {
            AppendLog(tweak, "Pre-apply backup disabled \u2014 skipped");
        }

        try
        {
            if (string.IsNullOrWhiteSpace(Settings.RepoPath))
            {
                AppendLog(tweak, "ABORTED: RepoPath is not set.");
                return;
            }
            var repoPath = Settings.RepoPath;
            if (string.IsNullOrWhiteSpace(tweak.FolderName))
            {
                AppendLog(tweak, "ABORTED: tweak has no folder defined.");
                return;
            }
            if (tweak.FolderName.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                AppendLog(tweak, $"ABORTED: folder name contains invalid characters: {tweak.FolderName}");
                return;
            }
            if (Path.IsPathRooted(tweak.FolderName))
            {
                AppendLog(tweak, $"ABORTED: folder name is an absolute path: {tweak.FolderName}");
                return;
            }
            var folder = Path.Combine(repoPath, tweak.FolderName);

            switch (tweak.Strategy)
            {
                case TweakStrategy.ImportReg:
                {
                    var importEc = -1;
                    if (tweak.Variants != null && tweak.Variants.Count > 0)
                    {
                        var idx = tweak.SelectedVariantIndex;
                        if (idx < 0 || idx >= tweak.Variants.Count) { idx = 0; AppendLog(tweak, "Variant index out of range \u2014 defaulting to first variant."); }
                        var variantLabel = tweak.Variants[idx] ?? "";
                        var parenIdx = variantLabel.LastIndexOf(" (", StringComparison.Ordinal);
                        var actualFile = parenIdx >= 0 ? variantLabel.Substring(0, parenIdx) : variantLabel;
                        var regFile = Path.Combine(folder, actualFile);
                        if (File.Exists(regFile))
                        {
                            var (ec, outp) = ProcessRunner.RunRegImport(regFile);
                            importEc = ec;
                            AppendLog(tweak, $"reg import {actualFile} \u2192 exit code {ec}");
                            if (!string.IsNullOrWhiteSpace(outp)) AppendLog(tweak, outp);
                        }
                        else
                        {
                            AppendLog(tweak, $"Reg file not found: {regFile}");
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(tweak.ApplyFile))
                    {
                        var regFile = Path.Combine(folder, tweak.ApplyFile);
                        if (File.Exists(regFile))
                        {
                            var (ec, outp) = ProcessRunner.RunRegImport(regFile);
                            importEc = ec;
                            AppendLog(tweak, $"reg import {tweak.ApplyFile} \u2192 exit code {ec}");
                            if (!string.IsNullOrWhiteSpace(outp)) AppendLog(tweak, outp);
                        }
                        else
                        {
                            AppendLog(tweak, $"Reg file not found: {regFile}");
                        }
                    }
                    else
                    {
                        AppendLog(tweak, "No reg file specified for this tweak \u2014 nothing imported.");
                    }
                    if (importEc == 0)
                    {
                        if (string.Equals(tweak.Id, "classic_ctx", StringComparison.OrdinalIgnoreCase))
                        {
                            AppendLog(tweak, "\u26A0 Restart Windows Explorer for this change to take effect.");
                            ShowRestartExplorerPrompt();
                        }
                        else if (string.Equals(tweak.Id, "fix_alt_shift", StringComparison.OrdinalIgnoreCase))
                        {
                            AppendLog(tweak, "\u26A0 A full system restart is required for this change to take effect.");
                        }
                    }
                    else if (importEc != -1)
                    {
                        AppendLog(tweak, "Reg import returned a non-zero exit code \u2014 changes may not have been applied.");
                    }
                    break;
                }

                case TweakStrategy.CopyVbsThenReg:
                {
                    if (string.Equals(tweak.Id, "markdown_ctx", StringComparison.OrdinalIgnoreCase))
                    {
                        var rawExt = tweak.MarkdownExtension?.Trim();
                        if (string.IsNullOrWhiteSpace(rawExt))
                        {
                            AppendLog(tweak, "ABORTED: File extension cannot be empty.");
                            return;
                        }
                        var ext = rawExt;
                        if (!ext.StartsWith('.'))
                            ext = "." + ext;
                        var safeExt = string.Concat(ext.Where(c => char.IsLetterOrDigit(c) || c == '.' || c == '_'));
                        if (safeExt != ext)
                        {
                            AppendLog(tweak, $"Sanitized extension '{ext}' to '{safeExt}'");
                            ext = safeExt;
                        }
                        if (string.IsNullOrWhiteSpace(ext) || ext == ".")
                        {
                            AppendLog(tweak, "ABORTED: File extension is invalid after sanitization.");
                            return;
                        }

                        var menuName = tweak.MarkdownMenuName?.Trim();
                        if (string.IsNullOrWhiteSpace(menuName))
                        {
                            AppendLog(tweak, "ABORTED: Context menu name cannot be empty.");
                            return;
                        }
                        var safeMenuName = new string(menuName.Where(c => char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '_').ToArray()).Trim();
                        if (string.IsNullOrWhiteSpace(safeMenuName))
                        {
                            AppendLog(tweak, "ABORTED: Context menu name contains only invalid characters.");
                            return;
                        }

                        var cleanName = safeMenuName;
                        var prefixes = new[] { "Create ", "Add ", "New " };
                        bool stripped;
                        do
                        {
                            stripped = false;
                            foreach (var prefix in prefixes)
                            {
                                if (cleanName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                                {
                                    cleanName = cleanName.Substring(prefix.Length);
                                    stripped = true;
                                    break;
                                }
                            }
                        } while (stripped);
                        var firstWord = cleanName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                        if (string.IsNullOrWhiteSpace(firstWord))
                            firstWord = "Document";
                        var invalidChars = System.IO.Path.GetInvalidFileNameChars();
                        var safeWord = string.Concat(firstWord.Where(c => !invalidChars.Contains(c)));
                        if (string.IsNullOrWhiteSpace(safeWord))
                            safeWord = "Document";
                        var baseName = "New" + safeWord;

                        var vbsContent = $@"' CreateMarkdownSilent.vbs - Auto-generated
' windows-utilities-tweaks - {safeExt}
' Menu name: {safeMenuName}

Dim objShell, objFSO, targetDir, i, fileName, objFile

If WScript.Arguments.Count > 0 Then
    targetDir = WScript.Arguments(0)
Else
    targetDir = CreateObject(""WScript.Shell"").CurrentDirectory
End If

Set objFSO = CreateObject(""Scripting.FileSystemObject"")

If Not objFSO.FolderExists(targetDir) Then
    WScript.Quit
End If

For i = 0 To 100
    If i = 0 Then
        fileName = targetDir & ""\{baseName}"" & ""{safeExt}""
    Else
        fileName = targetDir & ""\{baseName}("" & i & "")"" & ""{safeExt}""
    End If
    
    If Not objFSO.FileExists(fileName) Then
        Set objFile = objFSO.CreateTextFile(fileName, True)
        objFile.Close
        Set objFile = Nothing
        Exit For
    End If
Next

Set objFSO = Nothing";

                        var vbsDest = Path.Combine(Paths.System32, "CreateMarkdownSilent.vbs");
                        var vbsDir = Path.GetDirectoryName(vbsDest);
                        if (!string.IsNullOrEmpty(vbsDir)) Directory.CreateDirectory(vbsDir);
                        File.WriteAllText(vbsDest, vbsContent);
                        AppendLog(tweak, $"Generated CreateMarkdownSilent.vbs with extension '{safeExt}' \u2192 {vbsDest}");

                        var vbsRegPath = Path.Combine(Paths.System32, "CreateMarkdownSilent.vbs").Replace("\\", "\\\\");
                        var regContent = $"[HKEY_CLASSES_ROOT\\Directory\\Background\\shell\\CreateMarkdownFile]\r\n" +
                            $"@=\"{safeMenuName}\"\r\n" +
                            $"\"Position\"=\"Top\"\r\n\r\n" +
                            $"[HKEY_CLASSES_ROOT\\Directory\\Background\\shell\\CreateMarkdownFile\\command]\r\n" +
                            $"@=\"wscript.exe \\\"{vbsRegPath}\\\" \\\"%V\\\"\"";

                        var (ec, outp) = ProcessRunner.ImportRegContent(regContent);
                        AppendLog(tweak, $"Generated reg import for '{menuName}' \u2192 exit code {ec}");
                        if (!string.IsNullOrWhiteSpace(outp)) AppendLog(tweak, outp);
                    }
                    else
                    {
                    if (!Directory.Exists(folder))
                    {
                        AppendLog(tweak, $"Source folder not found in repo: {folder}");
                        return;
                    }
                    foreach (var vbs in Directory.GetFiles(folder, "*.vbs"))
                    {
                        var dest = Path.Combine(Paths.System32, Path.GetFileName(vbs));
                        ProcessRunner.CopyFileWithDirCreate(vbs, dest);
                        AppendLog(tweak, $"Copied {Path.GetFileName(vbs)} \u2192 {dest}");
                    }
                    if (!string.IsNullOrWhiteSpace(tweak.ApplyFile))
                        {
                            var regFile = Path.Combine(folder, tweak.ApplyFile);
                            if (File.Exists(regFile))
                            {
                                var (ec, outp) = ProcessRunner.RunRegImport(regFile);
                                AppendLog(tweak, $"reg import {tweak.ApplyFile} \u2192 exit code {ec}");
                                if (!string.IsNullOrWhiteSpace(outp)) AppendLog(tweak, outp);
                            }
                        }
                    }
                    break;
                }

                case TweakStrategy.RunPS1:
                {
                    if (!Directory.Exists(folder))
                    {
                        AppendLog(tweak, "ABORTED: tweak folder does not exist.");
                        return;
                    }
                    var ps1Files = Directory.GetFiles(folder, "*.ps1");
                    if (ps1Files.Length > 0)
                    {
                        var args = $"-NoProfile -ExecutionPolicy Bypass -File \"{ps1Files[0]}\"";
                        try
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "powershell.exe",
                                Arguments = args,
                                UseShellExecute = false,
                                CreateNoWindow = false,
                                WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal
                            })?.Dispose();
                            AppendLog(tweak, $"Launched PowerShell script in a separate window.");
                        }
                        catch (Exception ex)
                        {
                            AppendLog(tweak, $"Failed to launch PowerShell: {ex.Message}");
                        }
                    }
                    else
                    {
                        AppendLog(tweak, "No .ps1 script found in folder.");
                    }
                    break;
                }

                case TweakStrategy.RunBat:
                {
                    if (string.IsNullOrWhiteSpace(tweak.UserInput))
                    {
                        AppendLog(tweak, "ABORTED: target folder path is required for this tweak.");
                        return;
                    }
                    if (!Directory.Exists(tweak.UserInput))
                    {
                        AppendLog(tweak, $"ABORTED: target folder does not exist: {tweak.UserInput}");
                        return;
                    }
                    var safeInput = tweak.UserInput;
                    if (safeInput.Contains("\"") || safeInput.Contains("&") || safeInput.Contains("|") || safeInput.Contains("%")
                        || safeInput.Contains(">") || safeInput.Contains("<") || safeInput.Contains("^")
                        || safeInput.Contains("`") || safeInput.Contains(";"))
                    {
                        AppendLog(tweak, "ABORTED: target folder path contains invalid characters.");
                        return;
                    }
                    safeInput = safeInput.TrimEnd('\\');
                    if (!Directory.Exists(folder))
                    {
                        AppendLog(tweak, "ABORTED: tweak folder does not exist.");
                        return;
                    }
                    var batFiles = Directory.GetFiles(folder, "*.bat");
                    if (batFiles.Length == 0)
                    {
                        AppendLog(tweak, "ABORTED: No .bat file found in tweak folder.");
                        return;
                    }
                    else
                    {
                        var args = $"/S /C \"\"{batFiles[0]}\"\" \"{safeInput}\"";
                        var (ec, outp) = ProcessRunner.RunProcess("cmd.exe", args, ProcessRunner.LongTimeoutMs);
                        AppendLog(tweak, $"Ran batch file {Path.GetFileName(batFiles[0])} \u2192 exit code {ec}");
                        if (!string.IsNullOrWhiteSpace(outp)) AppendLog(tweak, outp);
                    }
                    break;
                }

                case TweakStrategy.RegistryAPI:
                {
                    using var baseKey = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(@"*\shell\RunWithPowerShellAdmin") 
                        ?? throw new System.UnauthorizedAccessException("Failed to create registry key — run as Administrator.");
                    baseKey.SetValue("", "Run with PowerShell (Admin)");
                    baseKey.SetValue("HasLUAShield", "");
                    baseKey.SetValue("AppliesTo", "System.FileExtension:=\".ps1\"");
                    using var cmd = baseKey.CreateSubKey("command") 
                        ?? throw new System.UnauthorizedAccessException("Failed to create 'command' subkey.");
                    cmd.SetValue("", "powershell.exe -NoProfile -ExecutionPolicy Bypass -Command " +
                        "\"Start-Process -Verb RunAs -FilePath 'powershell.exe' -ArgumentList " +
                        "'-NoProfile -ExecutionPolicy Bypass -File \\\"%1\\\"'\"");
                    AppendLog(tweak, "Registry keys created via Registry API.");
                    break;
                }

                case TweakStrategy.GenerateReg:
                {
                    var srcVbs = Path.Combine(folder, "CopyPathWithoutQuotes.vbs");
                    var destVbs = Paths.CopyPathVbsDest;
                    if (!File.Exists(srcVbs))
                    {
                        AppendLog(tweak, $"ABORTED: VBS script not found at {srcVbs} \u2014 cannot create Copy Path context menu.");
                        break;
                    }
                    ProcessRunner.CopyFileWithDirCreate(srcVbs, destVbs);
                    AppendLog(tweak, $"Copied {Path.GetFileName(srcVbs)} \u2192 {destVbs}");

                    var regContent = $$"""
    [HKEY_CLASSES_ROOT\AllFilesystemObjects\shell\CopyPathWithoutQuotes]
    @="Copy Path Without Quotes"
    "Icon"="shell32.dll,46"
    "Position"="Bottom"

    [HKEY_CLASSES_ROOT\AllFilesystemObjects\shell\CopyPathWithoutQuotes\command]
    @="wscript.exe \"{{destVbs.Replace("\\", "\\\\")}}\" \"%1\""
    """;

                    var (ec, outp) = ProcessRunner.ImportRegContent(regContent);
                    AppendLog(tweak, $"Generated reg import \u2192 exit code {ec}");
                    if (!string.IsNullOrWhiteSpace(outp)) AppendLog(tweak, outp);
                    break;
                }

                case TweakStrategy.NeedsInputThenReg:
                {
                    var chosenPath = tweak.UserInput;
                    if (string.IsNullOrWhiteSpace(chosenPath) || !Directory.Exists(chosenPath))
                    {
                        AppendLog(tweak, "ABORTED: target folder does not exist or is empty.");
                        return;
                    }
                    var escapedPath = chosenPath.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "");
                    var regContent = $$"""
[HKEY_CURRENT_USER\SOFTWARE\Classes\CLSID\{{Paths.Clsids.ExplorerLocation}}\shell\OpenNewWindow\command]
@="Explorer \"{{escapedPath}}\""
"DelegateExecute"=-

[HKEY_CURRENT_USER\SOFTWARE\Classes\CLSID\{{Paths.Clsids.ExplorerLocation}}\shell\open\command]
@="Explorer \"{{escapedPath}}\""
"DelegateExecute"=-
""";

                    var (ec, outp) = ProcessRunner.ImportRegContent(regContent);
                    AppendLog(tweak, $"Custom location reg import \u2192 exit code {ec}");
                    if (!string.IsNullOrWhiteSpace(outp)) AppendLog(tweak, outp);
                    AppendLog(tweak, "\u26A0 Restart Windows Explorer for this change to take effect.");
                    ShowRestartExplorerPrompt();
                    break;
                }

                case TweakStrategy.NeedsInputVariant:
                {
                    var idx = tweak.SelectedVariantIndex;
                    if (idx < 0 || tweak.Variants == null || idx >= tweak.Variants.Count)
                    {
                        AppendLog(tweak, "ABORTED: No variants available for this tweak.");
                        return;
                    }
                    var parenIdx = (tweak.Variants[idx] ?? "").LastIndexOf(" (", StringComparison.Ordinal);
                    var chosenVbs = parenIdx >= 0 ? (tweak.Variants[idx] ?? "").Substring(0, parenIdx) : (tweak.Variants[idx] ?? "");

                    var srcDir = Path.Combine(repoPath, tweak.FolderName);
                    var destDir = Paths.NewTextFileDir;
                    Directory.CreateDirectory(destDir);

                    var srcVbsPath = Path.Combine(srcDir, chosenVbs);
                    if (!File.Exists(srcVbsPath))
                    {
                        AppendLog(tweak, $"ABORTED: Source file {chosenVbs} not found in repo.");
                        return;
                    }
                    File.Copy(srcVbsPath, Path.Combine(destDir, chosenVbs), true);
                    AppendLog(tweak, $"Copied {chosenVbs} \u2192 {destDir}");

                    var ps1Src = Path.Combine(srcDir, "GetActiveExplorerPath.ps1");
                    if (File.Exists(ps1Src))
                    {
                        File.Copy(ps1Src, Path.Combine(destDir, "GetActiveExplorerPath.ps1"), true);
                        AppendLog(tweak, "Copied GetActiveExplorerPath.ps1");
                    }

                    var lnkPath = Path.Combine(Paths.StartMenuPrograms, "NewTextFile.lnk");
                    var lnkDir = Path.GetDirectoryName(lnkPath);
                    if (!string.IsNullOrEmpty(lnkDir)) Directory.CreateDirectory(lnkDir);
                    var hotkey = !string.IsNullOrWhiteSpace(tweak.Hotkey) ? tweak.Hotkey : "CTRL+ALT+N";
                    if (!IsValidLnkHotkey(hotkey))
                    {
                        AppendLog(tweak, $"ABORTED: Invalid hotkey format: \"{hotkey}\". Use one or more modifiers (CTRL/ALT/SHIFT) joined by '+', ending in a letter, digit, or special key (F1-F12, SPACE, ENTER, etc.). The Windows key is not supported for shortcut hotkeys.");
                        return;
                    }
                    var (scec, scout) = ShortcutService.CreateShortcut(
                        "wscript.exe",
                        $"\"{destDir}\\{chosenVbs}\"",
                        lnkPath,
                        hotkey);
                    if (scec == 0)
                        AppendLog(tweak, $"Created shortcut with {hotkey} hotkey.");
                    else
                        AppendLog(tweak, $"Shortcut creation may have failed (exit code {scec}): {scout}");
                    break;
                }

                case TweakStrategy.ShortcutWithManualPin:
                {
                    string destDir, iconSearchPattern;
                    string? lnkName, vbsFileName;

                    if (string.Equals(tweak.Id, "voice_typing_shortcut", StringComparison.OrdinalIgnoreCase))
                    {
                        var srcDir = Path.Combine(repoPath, tweak.FolderName, "Enable Voice Typing");
                        destDir = Paths.VoiceTypingShortcutDir;
                        Directory.CreateDirectory(destDir);

                        if (!Directory.Exists(srcDir))
                        { AppendLog(tweak, $"Source folder not found: {srcDir}"); return; }
                        foreach (var file in Directory.GetFiles(srcDir))
                        {
                            var dest = Path.Combine(destDir, Path.GetFileName(file));
                            File.Copy(file, dest, true);
                            AppendLog(tweak, $"Copied {Path.GetFileName(file)} \u2192 {destDir}");
                        }

                        lnkName = "Voice Typing Shortcut.lnk";
                        vbsFileName = "voice_typing_vbscript.vbs";
                        iconSearchPattern = "*.ico";
                    }
                    else if (string.Equals(tweak.Id, "custom_shortcut", StringComparison.OrdinalIgnoreCase))
                    {
                        var keys = tweak.UserInput?.Trim();
                        if (string.IsNullOrWhiteSpace(keys))
                        {
                            AppendLog(tweak, "ABORTED: Please enter a key combination (e.g., Win+H, Ctrl+Shift+Esc).");
                            return;
                        }

                        if (!IsValidKeyCombination(keys))
                        {
                            AppendLog(tweak, "ABORTED: Invalid key combination format. Use format like 'Win+H', 'Ctrl+Shift+Esc', 'Win+Ctrl+D'.");
                            return;
                        }

                        destDir = Paths.CustomShortcutDir;
                        Directory.CreateDirectory(destDir);

                        var (psScript, vbsScript, psFileName, vbsFileNameGen, lnkNameGenerated) = GenerateCustomShortcutScripts(keys);
                        File.WriteAllText(Path.Combine(destDir, psFileName), psScript);
                        File.WriteAllText(Path.Combine(destDir, vbsFileNameGen), vbsScript);
                        AppendLog(tweak, $"Generated custom shortcut scripts for: {keys}");

                        lnkName = lnkNameGenerated;
                        vbsFileName = vbsFileNameGen;
                        iconSearchPattern = "*.ico";
                    }
                    else
                    {
                        var srcDir = Path.Combine(repoPath, tweak.FolderName, "OCR");
                        destDir = Paths.OcrShortcutDir;
                        Directory.CreateDirectory(destDir);

                        if (!Directory.Exists(srcDir))
                        { AppendLog(tweak, $"Source folder not found: {srcDir}"); return; }
                        foreach (var file in Directory.GetFiles(srcDir))
                        {
                            var dest = Path.Combine(destDir, Path.GetFileName(file));
                            File.Copy(file, dest, true);
                            AppendLog(tweak, $"Copied {Path.GetFileName(file)} \u2192 {destDir}");
                        }

                        lnkName = "OCR Shortcut.lnk";
                        vbsFileName = "Enable OCR.vbs";
                        iconSearchPattern = "*.ico";
                    }

                    var lnkPath = Path.Combine(destDir, lnkName);
                    var vbsPath = Path.Combine(destDir, vbsFileName);

                    string? iconPath = tweak.SelectedIconPath;
                    if (iconPath == null || !File.Exists(iconPath))
                    {
                        var icoFiles = Directory.GetFiles(destDir, iconSearchPattern);
                        if (icoFiles.Length > 0)
                            iconPath = icoFiles[0];
                    }

                    var (scec2, scout2) = ShortcutService.CreateShortcut(
                        "wscript.exe",
                        $"\"{vbsPath}\"",
                        lnkPath,
                        "",
                        iconPath);

                    if (scec2 == 0)
                    {
                        AppendLog(tweak, $"Created shortcut at {lnkPath}");
                        PinShortcutToTaskbar(lnkPath, tweak);
                    }
                    else
                        AppendLog(tweak, $"Shortcut creation may have failed (exit code {scec2}): {scout2}");
                    break;
                }

                case TweakStrategy.TerminalProfileJson:
                {
                    var settingsPath = Paths.FindTerminalSettings();
                    if (settingsPath == null)
                    {
                        AppendLog(tweak, "ABORTED: Windows Terminal settings.json not found.");
                        return;
                    }

                    if (Settings.BackupEnabled && Settings.BackupPreApply)
                    {
                        try
                        {
                            var backupDir = Settings.BackupDirectory;
                            Directory.CreateDirectory(backupDir);
                            var backupFile = Path.Combine(backupDir, $"{tweak.Id}_terminal_{DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture)}.json");
                            File.Copy(settingsPath, backupFile, true);
                            AppendLog(tweak, $"Backed up Terminal settings to {backupFile}");
                        }
                        catch (Exception ex)
                        {
                            AppendLog(tweak, $"ABORTED: Terminal settings backup failed: {ex.Message}");
                            return;
                        }
                    }
                    else
                    {
                        AppendLog(tweak, "Pre-apply backup disabled \u2014 skipped Terminal settings backup.");
                    }

                    string json;
                    try { json = File.ReadAllText(settingsPath); }
                    catch (Exception ex) { AppendLog(tweak, $"ABORTED: could not read Terminal settings: {ex.Message}"); break; }
                    var node = System.Text.Json.Nodes.JsonNode.Parse(json, default, new System.Text.Json.JsonDocumentOptions
                    {
                        CommentHandling = System.Text.Json.JsonCommentHandling.Skip
                    });
                    if (node == null)
                    {
                        AppendLog(tweak, "ABORTED: could not parse Terminal settings.");
                        return;
                    }
                    var profiles = node["profiles"]?["list"] as System.Text.Json.Nodes.JsonArray;
                    if (profiles == null)
                    {
                        AppendLog(tweak, "ABORTED: could not find profiles.list in Terminal settings.");
                        return;
                    }

                    bool ProfileExists(System.Text.Json.Nodes.JsonNode? node, string guid)
                    {
                        try
                        {
                            if (node?["guid"] is System.Text.Json.Nodes.JsonValue jv)
                                return string.Equals(jv.GetValue<string>(), guid, StringComparison.OrdinalIgnoreCase);
                        }
                        catch (Exception ex) { AppendLog(tweak, $"ProfileExists error: {ex.Message}"); }
                        return false;
                    }

                    if (string.Equals(tweak.Id, "terminal_gitbash_profile", StringComparison.OrdinalIgnoreCase))
                    {
                        var gitBashCandidates = new[]
                        {
                            @"C:\Program Files\Git\bin\bash.exe",
                            @"C:\Program Files (x86)\Git\bin\bash.exe",
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Git", "bin", "bash.exe"),
                        };
                        var gitBashPath = gitBashCandidates.FirstOrDefault(File.Exists);
                        if (gitBashPath == null)
                        {
                            AppendLog(tweak, "ABORTED: Git Bash not found (checked standard install paths).");
                            return;
                        }
                        var fixedGuid = Paths.Clsids.TerminalGitBashProfile;
                        var exists = profiles.Any(p => ProfileExists(p, fixedGuid));
                        if (!exists)
                        {
                            var iconFile = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(gitBashPath) ?? "", @"..\..\mingw64\share\git\git-for-windows.ico"));
                            var iconPath = File.Exists(iconFile) ? iconFile : null;
                            var profile = new System.Text.Json.Nodes.JsonObject
                            {
                                ["guid"] = fixedGuid,
                                ["name"] = "Git Bash",
                                ["commandline"] = $"\"{gitBashPath}\" --login -i",
                                ["hidden"] = false
                            };
                            if (iconPath != null && File.Exists(iconPath))
                                profile["icon"] = iconPath;
                            profiles.Add(profile);
                            AppendLog(tweak, "Added Git Bash profile to Terminal settings.");
                        }
                        else
                        {
                            AppendLog(tweak, "Git Bash profile already exists \u2014 skipping.");
                        }
                    }
                    else
                    {
                        var fixedGuid = Paths.Clsids.TerminalIseProfile;
                        var exists = profiles.Any(p => ProfileExists(p, fixedGuid));
                        if (!exists)
                        {
                            profiles.Add(new System.Text.Json.Nodes.JsonObject
                            {
                                ["guid"] = fixedGuid,
                                ["name"] = "PowerShell ISE",
                                ["commandline"] = "powershell_ise.exe",
                                ["hidden"] = false
                            });
                            AppendLog(tweak, "Added PowerShell ISE profile to Terminal settings.");
                        }
                        else
                        {
                            AppendLog(tweak, "PowerShell ISE profile already exists \u2014 skipping.");
                        }
                    }

                    var jsonOptions = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                    var tmp = settingsPath + "." + Guid.NewGuid() + ".tmp";
                    try
                    {
                        try { File.Delete(tmp); } catch { }
                        File.WriteAllText(tmp, node.ToJsonString(jsonOptions));
                        File.Move(tmp, settingsPath, overwrite: true);
                    }
                    catch (Exception ex) { AppendLog(tweak, $"Failed to write Terminal settings: {ex.Message}"); }
                    finally
                    {
                        try { if (File.Exists(tmp)) File.Delete(tmp); } catch (Exception ex) { AppendLog(tweak, $"Terminal tmp cleanup failed: {ex.Message}"); }
                    }
                    break;
                }

                case TweakStrategy.HardwareIdentityTest:
                {
                    var manufacturer = tweak.ManufacturerInput?.Replace("\"", "");
                    var productName = tweak.ProductNameInput?.Replace("\"", "");

                    if (string.IsNullOrWhiteSpace(manufacturer))
                    {
                        manufacturer = "Samsung";
                        AppendLog(tweak, "Manufacturer empty — using default: Samsung");
                    }
                    if (string.IsNullOrWhiteSpace(productName))
                    {
                        productName = "950XDB";
                        AppendLog(tweak, "ProductName empty — using default: 950XDB");
                    }

                    var allowed = new HashSet<char>(@"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 _-.,()");
                    manufacturer = new string((manufacturer ?? "").Where(c => allowed.Contains(c)).ToArray()).Trim();
                    productName = new string((productName ?? "").Where(c => allowed.Contains(c)).ToArray()).Trim();
                    if (string.IsNullOrWhiteSpace(manufacturer)) manufacturer = "Samsung";
                    if (string.IsNullOrWhiteSpace(productName)) productName = "950XDB";

                    var (ecM, _) = ProcessRunner.RunProcess("reg.exe",
                        $"add \"HKLM\\HARDWARE\\DESCRIPTION\\System\\BIOS\" /v \"SystemManufacturer\" /t REG_SZ /d \"{manufacturer}\" /f");
                    var (ecP, _) = ProcessRunner.RunProcess("reg.exe",
                        $"add \"HKLM\\HARDWARE\\DESCRIPTION\\System\\BIOS\" /v \"SystemProductName\" /t REG_SZ /d \"{productName}\" /f");
                    if (ecM != 0 || ecP != 0)
                        AppendLog(tweak, $"Hardware identity write may have failed \u2014 SystemManufacturer exit code {ecM}, SystemProductName exit code {ecP}. Run as Administrator and retry.");
                    else
                        AppendLog(tweak, "Hardware identity applied successfully.");
                    break;
                }
                default:
                    AppendLog(tweak, $"Unknown strategy \u2014 nothing applied.");
                    break;
            }

            if (Settings.BackupEnabled && Settings.BackupPostApply)
            {
                if (tweak.BackupTargets != null)
                    foreach (var target in tweak.BackupTargets)
                    {
                        var (success, message) = RegistryService.BackupRegistryKey(target, tweak.Id, "_post");
                        AppendLog(tweak, message);
                    }
            }
            else
            {
                AppendLog(tweak, "Post-apply backup disabled \u2014 skipped");
            }

            RegistryService.RefreshStatus(tweak);
            var newStatus = string.Equals(tweak.Id, "privacy", StringComparison.OrdinalIgnoreCase)
                ? "Script launched \u2014 check the PowerShell window for its own completion status."
                : tweak.Status is TweakStatus.Applied or TweakStatus.Reapplicable
                    ? "Applied successfully."
                    : "Command succeeded but verification failed \u2014 check manually.";
            AppendLog(tweak, newStatus);
        }
        catch (Exception ex)
        {
            AppendLog(tweak, $"Error: {ex}");
        }
    }

    private void RevertTweak_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedTweak == null || !SelectedTweak.CanRevert) return;
        if (SelectedTweak.Status != TweakStatus.Applied && SelectedTweak.Status != TweakStatus.PartialApplied && SelectedTweak.Status != TweakStatus.Reapplicable)
        { AppendLog(SelectedTweak, "Cannot revert \u2014 tweak is not in an applied state."); return; }

        if (SelectedTweak.RequiresConfirmation)
        {
            var result = MessageBox.Show(
                $"This will revert {SelectedTweak.Name}, which affects the whole system. Continue?",
                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
            {
                AppendLog(SelectedTweak, "Revert cancelled by user.");
                return;
            }
        }

        // Handle GuideOnly tweaks outside _isApplying guard
        if (string.Equals(SelectedTweak.Id, "hide_context_menu_items", StringComparison.OrdinalIgnoreCase))
        {
            ContextMenuRestore_Click(sender, e);
            RefreshAllStatus();
            UpdateDetailPanel();
            UpdateSelectionCount();
            return;
        }
        if (string.Equals(SelectedTweak.Id, "explorer_cmd", StringComparison.OrdinalIgnoreCase))
        {
            ExplorerCmdRestore_Click(sender, e);
            RefreshAllStatus();
            UpdateDetailPanel();
            UpdateSelectionCount();
            return;
        }
        if (string.Equals(SelectedTweak.Id, "terminal_profiles", StringComparison.OrdinalIgnoreCase))
        {
            TerminalProfilesRestore_Click(sender, e);
            RefreshAllStatus();
            UpdateDetailPanel();
            UpdateSelectionCount();
            return;
        }

        if (_isApplying) return;
        _isApplying = true;
        try
        {
            RevertTweakInternal(SelectedTweak);
            RefreshAllStatus();
        }
        catch (Exception ex) { AppendLog(SelectedTweak, $"Error during revert: {ex}"); }
        finally { _isApplying = false; }
        UpdateDetailPanel();
        UpdateSelectionCount();
    }

    private void RevertTweakInternal(TweakItem tweak)
    {
        if (tweak == null) { AppendLogStatic("RevertTweakInternal: tweak is null"); return; }
        try
        {
        if (string.IsNullOrWhiteSpace(Settings.RepoPath))
        {
            AppendLog(tweak, "ABORTED: RepoPath is not set.");
            return;
        }
        var repoPath = Settings.RepoPath ?? "";
        if (string.IsNullOrWhiteSpace(tweak.FolderName))
        {
            AppendLog(tweak, "ABORTED: tweak has no folder defined.");
            return;
        }
        if (tweak.FolderName.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            AppendLog(tweak, $"ABORTED: folder name contains invalid characters: {tweak.FolderName}");
            return;
        }
        if (Path.IsPathRooted(tweak.FolderName))
        {
            AppendLog(tweak, $"ABORTED: folder name is an absolute path: {tweak.FolderName}");
            return;
        }
        var folder = Path.Combine(repoPath, tweak.FolderName);

        if (Settings.BackupEnabled && Settings.BackupPreRevert)
            {
                if (tweak.BackupTargets != null)
                    foreach (var target in tweak.BackupTargets)
                    {
                        var (success, message) = RegistryService.BackupRegistryKey(target, tweak.Id, "_before_revert");
                        AppendLog(tweak, message);
                        if (!success)
                        {
                        AppendLog(tweak, $"ABORTED: pre-revert backup failed for {target.Hive}\\{target.RegPath}");
                        return;
                    }
                }
            }

            switch (tweak.Strategy)
            {
                case TweakStrategy.ImportReg:
                {
                    if (string.Equals(tweak.Id, "run_py", StringComparison.OrdinalIgnoreCase))
                    {
                        // remove_run_with_python.reg uses [-HKEY] (regedit-only syntax)
                        var (ec, _) = ProcessRunner.RunProcess("reg.exe",
                            "delete \"HKCR\\SystemFileAssociations\\.py\\shell\\runwithpython\" /f");
                        AppendLog(tweak, $"revert reg delete \u2192 exit code {ec}");
                    }
                    else if (string.Equals(tweak.Id, "fix_alt_shift", StringComparison.OrdinalIgnoreCase))
                    {
                        // revert-right-alt-scancode.reg uses "Scancode Map"=- (regedit-only syntax)
                        var (ec, _) = ProcessRunner.RunProcess("reg.exe",
                            "delete \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Keyboard Layout\" /v \"Scancode Map\" /f");
                        AppendLog(tweak, $"revert reg delete \u2192 exit code {ec}");
                    }
                    else if (string.Equals(tweak.Id, "classic_ctx", StringComparison.OrdinalIgnoreCase))
                    {
                        // disable_classic_context_menu.reg uses [-HKEY] (regedit-only syntax)
                        var (ec, _) = ProcessRunner.RunProcess("reg.exe",
                            $"delete \"HKCU\\Software\\Classes\\CLSID\\{Paths.Clsids.ClassicContextMenu}\" /f");
                        AppendLog(tweak, $"revert reg delete classic_ctx \u2192 exit code {ec}");
                        ShowRestartExplorerPrompt();
                    }
                    else if (!string.IsNullOrWhiteSpace(tweak.RevertFile))
                    {
                        var regFile = Path.Combine(folder, tweak.RevertFile);
                        if (File.Exists(regFile))
                        {
                            var (ec, outp) = ProcessRunner.RunRegImport(regFile);
                            AppendLog(tweak, $"revert reg import {tweak.RevertFile} \u2192 exit code {ec}");
                            if (!string.IsNullOrWhiteSpace(outp)) AppendLog(tweak, outp);
                        }
                        else
                        {
                            AppendLog(tweak, $"Revert file {tweak.RevertFile} not found \u2014 no action taken.");
                        }
                    }
                    else
                    {
                        AppendLog(tweak, "No revert file specified for this tweak \u2014 no action taken.");
                    }
                    break;
                }

                case TweakStrategy.CopyVbsThenReg:
                {
                    var vbsDest = Path.Combine(Paths.System32, "CreateMarkdownSilent.vbs");
                    try
                    {
                        if (File.Exists(vbsDest))
                        {
                            var header = File.ReadAllText(vbsDest);
                            if (header.Contains("windows-utilities-tweaks", StringComparison.OrdinalIgnoreCase))
                                File.Delete(vbsDest);
                            else
                                AppendLog(tweak, "VBS file does not appear to be ours \u2014 skipping delete.");
                        }
                    }
                    catch (Exception ex) { AppendLog(tweak, $"VBS cleanup failed: {ex.Message}"); }
                    var (ec, _) = ProcessRunner.RunProcess("reg.exe",
                        "delete \"HKCR\\Directory\\Background\\shell\\CreateMarkdownFile\" /f");
                    AppendLog(tweak, $"Revert \u2192 exit code {ec}");
                    break;
                }

                case TweakStrategy.RegistryAPI:
                {
                    using var key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(@"*\shell\RunWithPowerShellAdmin");
                    if (key != null)
                    {
                        Microsoft.Win32.Registry.ClassesRoot.DeleteSubKeyTree(
                            @"*\shell\RunWithPowerShellAdmin", false);
                        AppendLog(tweak, "Deleted registry key tree.");
                    }
                    else
                    {
                        AppendLog(tweak, "Registry key not found \u2014 nothing to revert.");
                    }
                    break;
                }

                case TweakStrategy.GenerateReg:
                {
                    var vbsDest = Paths.CopyPathVbsDest;
                    try
                    {
                        if (File.Exists(vbsDest))
                        {
                            var header = File.ReadAllText(vbsDest);
                            if (header.Contains("windows-utilities-tweaks", StringComparison.OrdinalIgnoreCase))
                                File.Delete(vbsDest);
                            else
                                AppendLog(tweak, "VBS file does not appear to be ours \u2014 skipping delete.");
                        }
                    }
                    catch (Exception ex) { AppendLog(tweak, $"VBS cleanup failed: {ex.Message}"); }
                    var (ecGenReg, _) = ProcessRunner.RunProcess("reg.exe",
                        "delete \"HKCR\\AllFilesystemObjects\\shell\\CopyPathWithoutQuotes\" /f");
                    AppendLog(tweak, $"Reverted Copy Path Without Quotes (exit code {ecGenReg}).");
                    break;
                }

                case TweakStrategy.NeedsInputThenReg:
                {
                    var (ec, _) = ProcessRunner.RunProcess("reg.exe",
                        $"delete \"HKCU\\SOFTWARE\\Classes\\CLSID\\{Paths.Clsids.ExplorerLocation}\" /f");
                    AppendLog(tweak, $"Revert \u2192 exit code {ec}");
                    ShowRestartExplorerPrompt();
                    break;
                }

                case TweakStrategy.NeedsInputVariant:
                {
                    try
                    {
                        var lnkPath = Path.Combine(Paths.StartMenuPrograms, "NewTextFile.lnk");
                        if (File.Exists(lnkPath))
                            File.Delete(lnkPath);
                    }
                    catch (Exception ex) { AppendLog(tweak, $"Failed to delete shortcut: {ex.Message}"); }
                    try
                    {
                        var destDir = Paths.NewTextFileDir;
                        if (Directory.Exists(destDir))
                            Directory.Delete(destDir, true);
                    }
                    catch (Exception ex) { AppendLog(tweak, $"Failed to delete directory: {ex.Message}"); }
                    AppendLog(tweak, "Removed shortcut and files.");
                    break;
                }

                case TweakStrategy.RunPS1:
                    AppendLog(tweak, "Revert not available for this tweak.");
                    break;

                case TweakStrategy.RunBat:
                    AppendLog(tweak, "Revert not available for this tweak — remove firewall rules manually if needed.");
                    break;

                case TweakStrategy.ShortcutWithManualPin:
                {
                    string destDir;
                    var lnkPattern = "";

                    if (string.Equals(tweak.Id, "voice_typing_shortcut", StringComparison.OrdinalIgnoreCase))
                    {
                        destDir = Paths.VoiceTypingShortcutDir;
                        lnkPattern = "Voice Typing Shortcut.lnk";
                    }
                    else if (string.Equals(tweak.Id, "custom_shortcut", StringComparison.OrdinalIgnoreCase))
                    {
                        destDir = Paths.CustomShortcutDir;
                    }
                    else
                    {
                        destDir = Paths.OcrShortcutDir;
                        lnkPattern = "OCR Shortcut.lnk";
                    }

                    if (Directory.Exists(destDir))
                    {
                        if (string.Equals(destDir, Paths.OcrShortcutDir, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(destDir, Paths.VoiceTypingShortcutDir, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(destDir, Paths.CustomShortcutDir, StringComparison.OrdinalIgnoreCase))
                        {
                            Directory.Delete(destDir, true);
                            AppendLog(tweak, $"Removed shortcut and files from {destDir}");
                        }
                        else
                        {
                            AppendLog(tweak, $"Directory {destDir} does not appear to be ours \u2014 skipping delete.");
                        }
                    }

                    if (Directory.Exists(Paths.StartMenuPrograms))
                    {
                        try
                        {
                            if (string.Equals(tweak.Id, "custom_shortcut", StringComparison.OrdinalIgnoreCase))
                            {
                                foreach (var lnk in Directory.GetFiles(Paths.StartMenuPrograms, "Custom Shortcut (*).lnk"))
                                {
                                    try { File.Delete(lnk); }
                                    catch (Exception ex) { AppendLog(tweak, $"Failed to remove from Start Menu: {ex.Message}"); }
                                }
                            }
                            else
                            {
                                var startMenuLnk = Path.Combine(Paths.StartMenuPrograms, lnkPattern);
                                if (File.Exists(startMenuLnk))
                                {
                                    try { File.Delete(startMenuLnk); }
                                    catch (Exception ex) { AppendLog(tweak, $"Failed to remove from Start Menu: {ex.Message}"); }
                                    AppendLog(tweak, "Removed from Start Menu.");
                                }
                            }
                        }
                        catch (Exception ex) { AppendLog(tweak, $"Failed to remove from Start Menu: {ex.Message}"); }
                    }

                    if (Directory.Exists(Paths.TaskbarPinnedFolder))
                    {
                        if (string.Equals(tweak.Id, "custom_shortcut", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var lnk in Directory.GetFiles(Paths.TaskbarPinnedFolder, "Custom Shortcut (*).lnk"))
                            {
                                try { File.Delete(lnk); } catch (Exception ex) { AppendLog(tweak, $"Failed to delete pinned shortcut: {ex.Message}"); }
                            }
                        }
                        else
                        {
                            var taskbarLnk = Path.Combine(Paths.TaskbarPinnedFolder, lnkPattern);
                            if (File.Exists(taskbarLnk))
                            {
                                try { File.Delete(taskbarLnk); }
                                catch (Exception ex) { AppendLog(tweak, $"Failed to remove from TaskBar pinned folder: {ex.Message}"); }
                                AppendLog(tweak, "Removed from TaskBar pinned folder.");
                            }
                        }
                    }

                    RebuildTaskbarLayoutXml(tweak);
                    break;
                }

                case TweakStrategy.TerminalProfileJson:
                {
                    var settingsPath = Paths.FindTerminalSettings();
                    if (settingsPath == null)
                    {
                        AppendLog(tweak, "Terminal settings.json not found \u2014 nothing to revert.");
                        break;
                    }

                    if (Settings.BackupEnabled && Settings.BackupPreRevert)
                    {
                        try
                        {
                            var backupDir = Settings.BackupDirectory;
                            Directory.CreateDirectory(backupDir);
                            var backupFile = Path.Combine(backupDir, $"{tweak.Id}_terminal_{DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture)}.json");
                            File.Copy(settingsPath, backupFile, true);
                            AppendLog(tweak, $"Backed up Terminal settings before revert to {backupFile}");
                        }
                        catch (Exception ex)
                        {
                            AppendLog(tweak, $"ABORTED: Terminal settings backup before revert failed: {ex.Message}");
                            break;
                        }
                    }
                    else
                    {
                        AppendLog(tweak, "Pre-revert backup disabled \u2014 skipped Terminal settings backup.");
                    }

                    string json;
                    try { json = File.ReadAllText(settingsPath); }
                    catch (Exception ex) { AppendLog(tweak, $"Could not read Terminal settings: {ex.Message}"); break; }
                    var node = System.Text.Json.Nodes.JsonNode.Parse(json, default, new System.Text.Json.JsonDocumentOptions
                    {
                        CommentHandling = System.Text.Json.JsonCommentHandling.Skip
                    });
                    if (node == null)
                    {
                        AppendLog(tweak, "Could not parse Terminal settings \u2014 nothing to revert.");
                        break;
                    }
                    var profiles = node["profiles"]?["list"] as System.Text.Json.Nodes.JsonArray;
                    if (profiles == null)
                    {
                        AppendLog(tweak, "Could not find profiles.list \u2014 nothing to revert.");
                        break;
                    }

                    string profileGuid = string.Equals(tweak.Id, "terminal_gitbash_profile", StringComparison.OrdinalIgnoreCase)
                        ? Paths.Clsids.TerminalGitBashProfile
                        : Paths.Clsids.TerminalIseProfile;
                    string profileName = string.Equals(tweak.Id, "terminal_gitbash_profile", StringComparison.OrdinalIgnoreCase) ? "Git Bash" : "PowerShell ISE";
                    var toRemove = profiles.FirstOrDefault(p =>
                    {
                        if (p?["guid"] is System.Text.Json.Nodes.JsonValue jv)
                        {
                            try { return string.Equals(jv.GetValue<string>(), profileGuid, StringComparison.OrdinalIgnoreCase); }
                            catch (InvalidOperationException) { return false; }
                        }
                        return false;
                    });
                    if (toRemove != null)
                    {
                        profiles.Remove(toRemove);
                        var jsonOptions = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                        var tmp = settingsPath + "." + Guid.NewGuid() + ".tmp";
                        try
                        {
                            try { File.Delete(tmp); } catch { }
                            File.WriteAllText(tmp, node.ToJsonString(jsonOptions));
                            File.Move(tmp, settingsPath, overwrite: true);
                            AppendLog(tweak, $"Removed {profileName} profile from Terminal settings.");
                        }
                        catch (Exception ex) { AppendLog(tweak, $"Failed to write Terminal settings during revert: {ex.Message}"); }
                        finally
                        {
                            try { if (File.Exists(tmp)) File.Delete(tmp); } catch (Exception ex) { AppendLog(tweak, $"Terminal tmp cleanup failed: {ex.Message}"); }
                        }
                    }
                    else
                    {
                        AppendLog(tweak, "Profile not found \u2014 nothing to revert.");
                    }
                    break;
                }

                case TweakStrategy.HardwareIdentityTest:
                {
                        var backupDir = Settings.BackupDirectory;
                    if (!Directory.Exists(backupDir))
                    {
                        AppendLog(tweak, "No backup directory found \u2014 cannot revert.");
                        RegistryService.RefreshStatus(tweak);
                        return;
                    }

                    var backupFiles = Directory.GetFiles(backupDir, "hw_identity_*.reg")
                        .Where(f =>
                        {
                            var name = Path.GetFileName(f);
                            return name.Contains("_default_", StringComparison.OrdinalIgnoreCase)
                                || name.Contains("_pre_", StringComparison.OrdinalIgnoreCase);
                        })
                        .OrderByDescending(f => File.GetLastWriteTimeUtc(f))
                        .ToArray();

                    if (backupFiles.Length == 0)
                    {
                        AppendLog(tweak, "No valid backup found \u2014 cannot revert. Enable 'Back up registry' and 'Pre-apply backup' before applying.");
                        RegistryService.RefreshStatus(tweak);
                        return;
                    }

                    var newestBackup = backupFiles[0];
                    var (ec, _) = ProcessRunner.RunRegImport(newestBackup);
                    AppendLog(tweak, $"Restored backup {Path.GetFileName(newestBackup)} \u2192 exit code {ec}");
                    break;
                }
                default:
                    AppendLog(tweak, $"Unknown strategy \u2014 nothing reverted.");
                    break;
            }

            if (Settings.BackupEnabled && Settings.BackupPostRevert)
            {
                if (tweak.BackupTargets != null)
                    foreach (var target in tweak.BackupTargets)
                    {
                        var (success, message) = RegistryService.BackupRegistryKey(target, tweak.Id, "_post_revert");
                        AppendLog(tweak, message);
                    }
            }
            else
            {
                AppendLog(tweak, "Post-revert backup disabled \u2014 skipped");
            }

            RegistryService.RefreshStatus(tweak);
            AppendLog(tweak, "Revert completed.");
        }
        catch (Exception ex)
        {
            AppendLog(tweak, $"Error during revert: {ex}");
        }
    }

    private void ViewManualGuide_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedTweak == null) return;
        var url = GetGuideUrl(SelectedTweak);
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            })?.Dispose();
        }
        catch (Exception ex) { AppendLog(SelectedTweak, $"Failed to open guide: {ex.Message}"); }
    }

    private static string GetGuideUrl(TweakItem tweak)
    {
        var id = !string.IsNullOrEmpty(tweak.FolderName) ? tweak.FolderName : tweak.Id;
        var encoded = Uri.EscapeDataString(id);
        return $"https://github.com/Michael-Matta1/windows-utilities-tweaks/tree/main/{encoded}";
    }

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        var tweak = SelectedTweak;
        if (tweak == null) return;
        try
        {
            var folder = Path.Combine(Settings.RepoPath ?? "", tweak.FolderName);
            if (Directory.Exists(folder))
                ProcessRunner.OpenFolder(folder);
            else
                AppendLog(tweak, $"Folder does not exist: {folder}");
        }
        catch (Exception ex) { AppendLog(tweak, $"Could not open folder: {ex.Message}"); }
    }

    private void OpenShellFolder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string shellCmd)
        {
            try
            {
                    System.Diagnostics.Process.Start("explorer.exe", $"\"{shellCmd}\"")?.Dispose();
            }
            catch (Exception ex) { AppendLog(SelectedTweak ?? Tweaks.FirstOrDefault(), $"Failed to open shell folder: {ex.Message}"); }
        }
    }

    private void OpenTaskbarPins_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Directory.Exists(Paths.TaskbarPinnedFolder))
                ProcessRunner.OpenFolder(Paths.TaskbarPinnedFolder);
            else
                AppendLog(SelectedTweak ?? Tweaks.FirstOrDefault(), "Taskbar pinned folder does not exist.");
        }
        catch (Exception ex) { AppendLog(SelectedTweak ?? Tweaks.FirstOrDefault(), $"Failed to open taskbar pins: {ex.Message}"); }
    }

    private void CreateGodMode_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var folderName = "GodMode.{ED7BA470-8E54-465E-825C-99712043E01C}";
            var godModePath = Path.Combine(desktop, folderName);
            var alreadyExists = Directory.Exists(godModePath);
            Directory.CreateDirectory(godModePath);
            if (SelectedTweak != null)
                AppendLog(SelectedTweak, alreadyExists ? $"GodMode folder already exists at {godModePath}" : $"Created GodMode folder at {godModePath}");
        }
        catch (Exception ex)
        {
            var log = SelectedTweak ?? Tweaks.FirstOrDefault();
            if (log != null)
                AppendLog(log, $"Error creating GodMode folder: {ex.Message}");
        }
    }

    private void TerminalProfilesApply_Click(object sender, RoutedEventArgs e)
    {
        if (_isApplying) return;
        try
        {
            if (TermProfileIse.IsChecked == true)
            {
                var tweak = Tweaks.FirstOrDefault(t => string.Equals(t.Id, "terminal_ise_profile", StringComparison.OrdinalIgnoreCase));
                if (tweak != null) ApplyTweakInternal(tweak);
                else AppendLog(SelectedTweak ?? Tweaks.FirstOrDefault(), "TerminalProfilesApply: terminal_ise_profile tweak not found");
            }
            if (TermProfileGitBash.IsChecked == true)
            {
                var tweak = Tweaks.FirstOrDefault(t => string.Equals(t.Id, "terminal_gitbash_profile", StringComparison.OrdinalIgnoreCase));
                if (tweak != null) ApplyTweakInternal(tweak);
                else AppendLog(SelectedTweak ?? Tweaks.FirstOrDefault(), "TerminalProfilesApply: terminal_gitbash_profile tweak not found");
            }
            RefreshAllStatus();
        }
        catch (Exception ex) { AppendLog(SelectedTweak ?? Tweaks.FirstOrDefault(), $"TerminalProfilesApply failed: {ex.Message}"); }
    }

    private void TerminalProfilesRestore_Click(object sender, RoutedEventArgs e)
    {
        if (_isApplying) return;
        try
        {
            if (TermProfileIse.IsChecked == true)
            {
                var tweak = Tweaks.FirstOrDefault(t => string.Equals(t.Id, "terminal_ise_profile", StringComparison.OrdinalIgnoreCase));
                if (tweak != null) RevertTweakInternal(tweak);
                else AppendLog(SelectedTweak ?? Tweaks.FirstOrDefault(), "TerminalProfilesRestore: terminal_ise_profile tweak not found");
            }
            if (TermProfileGitBash.IsChecked == true)
            {
                var tweak = Tweaks.FirstOrDefault(t => string.Equals(t.Id, "terminal_gitbash_profile", StringComparison.OrdinalIgnoreCase));
                if (tweak != null) RevertTweakInternal(tweak);
                else AppendLog(SelectedTweak ?? Tweaks.FirstOrDefault(), "TerminalProfilesRestore: terminal_gitbash_profile tweak not found");
            }
            RefreshAllStatus();
        }
        catch (Exception ex) { AppendLog(SelectedTweak ?? Tweaks.FirstOrDefault(), $"TerminalProfilesRestore failed: {ex.Message}"); }
    }

    private void BrowseShortcutIcon_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select an icon file",
                Filter = "Icon files (*.ico)|*.ico|Executables and libraries (*.exe;*.dll)|*.exe;*.dll|All files (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false
            };
            if (dialog.ShowDialog() == true)
                ShortcutIconPath.Text = dialog.FileName;
        }
        catch (Exception ex) { ProcessRunner.AppendLog($"BrowseShortcutIcon_Click failed: {ex.Message}"); }
    }

    private void ShortcutAddOcr_Click(object sender, RoutedEventArgs e)
    {
        if (_isApplying) return;
        var tweak = Tweaks.FirstOrDefault(t => string.Equals(t.Id, "ocr_shortcut", StringComparison.OrdinalIgnoreCase));
        if (tweak != null)
        {
            _isApplying = true;
            try
            {
                tweak.SelectedIconPath = !string.IsNullOrWhiteSpace(ShortcutIconPath.Text) ? ShortcutIconPath.Text : null;
                ApplyTweakInternal(tweak);
            }
            catch (Exception ex) { AppendLog(SelectedTweak ?? tweak, $"Error: {ex.Message}"); }
            finally { _isApplying = false; }
            UpdateDetailPanel();
        }
        else if (SelectedTweak != null)
            AppendLog(SelectedTweak, "OCR shortcut tweak not found in manifest.");
    }

    private void ShortcutAddVoiceTyping_Click(object sender, RoutedEventArgs e)
    {
        if (_isApplying) return;
        var tweak = Tweaks.FirstOrDefault(t => string.Equals(t.Id, "voice_typing_shortcut", StringComparison.OrdinalIgnoreCase));
        if (tweak != null)
        {
            _isApplying = true;
            try
            {
                tweak.SelectedIconPath = !string.IsNullOrWhiteSpace(ShortcutIconPath.Text) ? ShortcutIconPath.Text : null;
                ApplyTweakInternal(tweak);
            }
            catch (Exception ex) { AppendLog(SelectedTweak ?? tweak, $"Error: {ex.Message}"); }
            finally { _isApplying = false; }
            UpdateDetailPanel();
        }
        else if (SelectedTweak != null)
            AppendLog(SelectedTweak, "Voice Typing shortcut tweak not found in manifest.");
    }

    private void ShortcutAddCustom_Click(object sender, RoutedEventArgs e)
    {
        if (_isApplying) return;
        var keys = CustomShortcutKeys.Text?.Trim();
        if (string.IsNullOrWhiteSpace(keys))
        {
            if (SelectedTweak != null)
                AppendLog(SelectedTweak, "Please enter a key combination before creating the shortcut.");
            return;
        }
        if (!IsValidKeyCombination(keys))
        {
            if (SelectedTweak != null)
                AppendLog(SelectedTweak, "Invalid key combination format. Use format like 'Win+H', 'Ctrl+Shift+Esc', 'Win+Ctrl+D'.");
            return;
        }
        var iconPath = !string.IsNullOrWhiteSpace(ShortcutIconPath.Text) ? ShortcutIconPath.Text : null;
        if (iconPath != null && !File.Exists(iconPath))
        {
            if (SelectedTweak != null)
                AppendLog(SelectedTweak, $"Icon file not found: {iconPath} \u2014 proceeding without custom icon.");
            iconPath = null;
        }
        var tweak = Tweaks.FirstOrDefault(t => string.Equals(t.Id, "custom_shortcut", StringComparison.OrdinalIgnoreCase));
        if (tweak != null)
        {
            _isApplying = true;
            try
            {
                tweak.UserInput = keys;
                tweak.SelectedIconPath = iconPath;
                ApplyTweakInternal(tweak);
            }
            catch (Exception ex) { AppendLog(SelectedTweak ?? tweak, $"Error: {ex.Message}"); }
            finally { _isApplying = false; }
            UpdateDetailPanel();
        }
        else if (SelectedTweak != null)
            AppendLog(SelectedTweak, "Custom shortcut tweak not found in manifest.");
    }

    private void ExplorerCmdApply_Click(object sender, RoutedEventArgs e)
    {
        if (_isApplying) return;
        try
        {
            var repoPath = Settings.RepoPath ?? "";
            if (string.IsNullOrWhiteSpace(repoPath))
            {
                AppendLog(SelectedTweak ?? Tweaks.FirstOrDefault(), "Repo path not set \u2014 configure it in Settings first.");
                return;
            }
            var tweakItem = SelectedTweak ?? Tweaks.FirstOrDefault(t => string.Equals(t.Id, "explorer_cmd", StringComparison.OrdinalIgnoreCase));
            var srcFolder = tweakItem != null
                ? Path.Combine(repoPath, tweakItem.FolderName)
                : Path.Combine(repoPath, "Explorer Address Bar Admin CMD shortcut");
            var logTweak = SelectedTweak ?? Tweaks.FirstOrDefault();
            var checkboxes = new[] { ExpCmdAcmd, ExpCmdAps, ExpCmdAtr };
            var filenames = new[] { "acmd.bat", "aps.bat", "atr.bat" };

            for (int i = 0; i < checkboxes.Length; i++)
            {
                if (checkboxes[i] == null) continue;
                if (checkboxes[i].IsChecked == true)
                {
                    var src = Path.Combine(srcFolder, filenames[i]);
                    var dest = Path.Combine(Paths.Windows, filenames[i]);
                    if (File.Exists(src))
                    {
                        var backupDir = string.IsNullOrWhiteSpace(Settings.BackupDirectory) ? Path.Combine(Path.GetTempPath(), "WindowsTweakLauncher_backup") : Path.Combine(Settings.BackupDirectory, "explorer_cmd");
                        Directory.CreateDirectory(backupDir);
                        var backupDest = Path.Combine(backupDir, filenames[i] + "." + DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + ".backup");
                        if (File.Exists(dest))
                        {
                            try { File.Copy(dest, backupDest, true); } catch (Exception ex) { if (logTweak != null) AppendLog(logTweak, $"Backup failed for {dest}: {ex}"); }
                        }
                        try { File.Copy(src, dest, true); } catch (Exception ex) { if (logTweak != null) AppendLog(logTweak, $"Failed to copy {filenames[i]} to {dest}: {ex}"); continue; }
                        if (logTweak != null)
                            AppendLog(logTweak, $"Copied {filenames[i]} \u2192 {dest}");
                    }
                    else if (logTweak != null)
                        AppendLog(logTweak, $"Source {filenames[i]} not found in repo.");
                }
            }
            RefreshAllStatus();
        }
        catch (Exception ex)
        {
            var log = SelectedTweak ?? Tweaks.FirstOrDefault();
            if (log != null)
                AppendLog(log, $"Error applying explorer cmd: {ex.Message}");
        }
    }

    private void ExplorerCmdRestore_Click(object sender, RoutedEventArgs e)
    {
        if (_isApplying) return;
        try
        {
            var logTweak = SelectedTweak ?? Tweaks.FirstOrDefault();
            var checkboxes = new[] { ExpCmdAcmd, ExpCmdAps, ExpCmdAtr };
            var filenames = new[] { "acmd.bat", "aps.bat", "atr.bat" };

            for (int i = 0; i < checkboxes.Length; i++)
            {
                if (checkboxes[i] == null) continue;
                if (checkboxes[i].IsChecked == true)
                {
                    var backupDir = string.IsNullOrWhiteSpace(Settings.BackupDirectory) ? Path.Combine(Path.GetTempPath(), "WindowsTweakLauncher_backup") : Path.Combine(Settings.BackupDirectory, "explorer_cmd");
                    Directory.CreateDirectory(backupDir);
                    var backupPattern = filenames[i] + ".*.backup";
                    string[] matchingBackups;
                    try { matchingBackups = Directory.GetFiles(backupDir, backupPattern).OrderByDescending(f => File.GetLastWriteTime(f)).ToArray(); } catch { matchingBackups = []; }
                    var backupFile = matchingBackups.Length > 0 ? matchingBackups[0] : "";
                    var dest = Path.Combine(Paths.Windows, filenames[i]);
                    if (File.Exists(backupFile))
                    {
                        try { File.Copy(backupFile, dest, true); } catch (Exception ex) { if (logTweak != null) AppendLog(logTweak, $"Failed to restore {filenames[i]} from backup: {ex}"); continue; }
                        if (logTweak != null)
                            AppendLog(logTweak, $"Restored {filenames[i]} from backup.");
                    }
                    else if (logTweak != null)
                        AppendLog(logTweak, $"No backup found for {filenames[i]} \u2014 skipping (will not delete from C:\\Windows without a backup).");
                }
            }
            RefreshAllStatus();
            ShowRestartExplorerPrompt();
        }
        catch (Exception ex)
        {
            var log = SelectedTweak ?? Tweaks.FirstOrDefault();
            if (log != null)
                AppendLog(log, $"Error restoring explorer cmd: {ex.Message}");
        }
    }

    private void ContextMenuHide_Click(object sender, RoutedEventArgs e)
    {
        if (_isApplying) return;
        try
        {
        var checkboxes = new CheckBox[] { CtxHideCastToDevice, CtxHideShare, CtxHideGiveAccessTo, CtxHidePaint3D, CtxHideEditPhotos, CtxHideDefenderScan };
        var blockedKey = @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Blocked";
        var logTweak = SelectedTweak ?? Tweaks.FirstOrDefault();

        var (addEc, addOut) = ProcessRunner.RunProcess("reg.exe", $"add \"{blockedKey}\" /f", 5000);
        if (addEc != 0)
        {
            if (logTweak != null)
                AppendLog(logTweak, $"ABORTED: Cannot create the Blocked key \u2014 access denied. Run as Administrator and retry.");
            return;
        }

        foreach (var cb in checkboxes)
        {
            if (cb == null || string.IsNullOrEmpty(cb.Name)) continue;
            if (cb.IsChecked == true && CtxClsidMapping.TryGetValue(cb.Name, out var clsid))
            {
                var (ec, _) = ProcessRunner.RunProcess("reg.exe",
                    $"add \"{blockedKey}\" /v \"{clsid}\" /t REG_SZ /d \"\" /f");
                if (logTweak != null)
                    AppendLog(logTweak, $"Blocked {cb.Content}: {clsid} \u2192 exit code {ec}");
            }
        }

        RefreshAllStatus();
        ShowRestartExplorerPrompt();
        }
        catch (Exception ex)
        {
            var log = SelectedTweak ?? Tweaks.FirstOrDefault();
            if (log != null)
                AppendLog(log, $"Error hiding context menu items: {ex.Message}");
        }
    }

    private void ContextMenuRestore_Click(object sender, RoutedEventArgs e)
    {
        if (_isApplying) return;
        try
        {
        var checkboxes = new CheckBox[] { CtxHideCastToDevice, CtxHideShare, CtxHideGiveAccessTo, CtxHidePaint3D, CtxHideEditPhotos, CtxHideDefenderScan };
        var blockedKey = @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Blocked";
        var logTweak = SelectedTweak ?? Tweaks.FirstOrDefault();

        var (keyExistsEc, _) = ProcessRunner.RunProcess("reg.exe", $"query \"{blockedKey}\"", 5000);
        if (keyExistsEc != 0)
        {
            if (logTweak != null)
                AppendLog(logTweak, "Blocked key not found \u2014 nothing to restore for CLSID items.");
        }
        else
        {
            foreach (var cb in checkboxes)
            {
                if (cb == null || string.IsNullOrEmpty(cb.Name)) continue;
                if (cb.IsChecked == true && CtxClsidMapping.TryGetValue(cb.Name, out var clsid))
                {
                    var (ec, _) = ProcessRunner.RunProcess("reg.exe",
                        $"delete \"{blockedKey}\" /v \"{clsid}\" /f");
                    if (logTweak != null)
                        AppendLog(logTweak, $"Restored {cb.Content}: {clsid} \u2192 exit code {ec}");
                }
            }
        }
        RefreshAllStatus();
        ShowRestartExplorerPrompt();
        }
        catch (Exception ex)
        {
            var log = SelectedTweak ?? Tweaks.FirstOrDefault();
            if (log != null)
                AppendLog(log, $"Error restoring context menu items: {ex.Message}");
        }
    }

    private void ContextMenuSelectAll_Click(object sender, RoutedEventArgs e)
    {
        CtxHideCastToDevice.IsChecked = true;
        CtxHideShare.IsChecked = true;
        CtxHideGiveAccessTo.IsChecked = true;
        CtxHidePaint3D.IsChecked = true;
        CtxHideEditPhotos.IsChecked = true;
        CtxHideDefenderScan.IsChecked = true;
    }

    private void ContextMenuClearAll_Click(object sender, RoutedEventArgs e)
    {
        CtxHideCastToDevice.IsChecked = false;
        CtxHideShare.IsChecked = false;
        CtxHideGiveAccessTo.IsChecked = false;
        CtxHidePaint3D.IsChecked = false;
        CtxHideEditPhotos.IsChecked = false;
        CtxHideDefenderScan.IsChecked = false;
    }

    private bool _restartingExplorer;

    private async void RestartExplorer_Click(object sender, RoutedEventArgs e)
    {
        if (_restartingExplorer) return;
        _restartingExplorer = true;
        try
        {
            bool alive() { try { return this.IsLoaded; } catch { return false; } }
            if (!alive()) { _restartingExplorer = false; return; }
            await Task.Run(() => ProcessRunner.RunProcess("taskkill.exe", "/f /im explorer.exe"));
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(200);
                var procs = await Task.Run(() => System.Diagnostics.Process.GetProcessesByName("explorer"));
                foreach (var p in procs) p.Dispose();
                if (procs.Length == 0)
                    break;
            }
            using var explorer = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("explorer.exe") { UseShellExecute = true });
            if (explorer == null)
            {
                ProcessRunner.AppendLog("RestartExplorer: explorer.exe failed to start");
                if (alive())
                    System.Windows.MessageBox.Show("Windows Explorer failed to restart. Your desktop may be missing. Press Ctrl+Alt+Del and run 'explorer.exe' from Task Manager.",
                        "Explorer Restart Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                await Task.Delay(500);
                var runningProcs = await Task.Run(() => System.Diagnostics.Process.GetProcessesByName("explorer"));
                foreach (var p in runningProcs) p.Dispose();
                if (runningProcs.Length == 0)
                {
                    ProcessRunner.AppendLog("RestartExplorer: explorer.exe exited prematurely");
                    if (alive())
                        System.Windows.MessageBox.Show("Windows Explorer failed to restart. Your desktop may be missing. Press Ctrl+Alt+Del and run 'explorer.exe' from Task Manager.",
                            "Explorer Restart Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    if (alive())
                    {
                        var target = SelectedTweak ?? Tweaks.FirstOrDefault();
                        if (target != null)
                            AppendLog(target, "Windows Explorer restarted.");
                    }
                }
            }
        }
        catch (Exception ex) { ProcessRunner.AppendLog($"RestartExplorer error: {ex.Message}"); }
        finally { _restartingExplorer = false; }
    }

    private void PinShortcutToTaskbar(string shortcutPath, TweakItem tweak)
    {
        if (string.IsNullOrEmpty(shortcutPath))
        {
            AppendLog(tweak, "ABORTED: shortcutPath is null or empty");
            return;
        }
        var name = Path.GetFileName(shortcutPath);
        if (!File.Exists(shortcutPath))
        {
            AppendLog(tweak, $"ABORTED: shortcut file not found at {shortcutPath}");
            return;
        }

        // Step 1: Copy the .lnk to the Start Menu (needed for LayoutModification.xml to reference it)
        var startMenuPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"Microsoft\Windows\Start Menu\Programs",
            name);
        try { File.Copy(shortcutPath, startMenuPath, true); }
        catch { /* Start Menu copy is best-effort */ }

        // Step 2: Copy to TaskBar pinned folder (legacy, may help on older Windows)
        var taskbarLnk = Path.Combine(Paths.TaskbarPinnedFolder, name);
        try
        {
            Directory.CreateDirectory(Paths.TaskbarPinnedFolder);
            File.Copy(shortcutPath, taskbarLnk, true);
            AppendLog(tweak, $"Copied shortcut to TaskBar pinned folder.");
        }
        catch (Exception ex)
        {
            AppendLog(tweak, $"Error copying shortcut to TaskBar: {ex.Message}");
        }

        // Step 3: Windows 11 — rebuild LayoutModification.xml from ALL app-pinned shortcuts (persistent, keeps prior pins)
        RebuildTaskbarLayoutXml(tweak);

        AppendLog(tweak, "Restart Explorer to apply the taskbar pin (use the Restart Explorer button below).");
        ShowRestartExplorerPrompt();
    }

    private void RebuildTaskbarLayoutXml(TweakItem tweak)
    {
        try
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var shellDir = Path.Combine(localAppData, @"Microsoft\Windows\Shell");
            Directory.CreateDirectory(shellDir);
            var xmlPath = Path.Combine(shellDir, "LayoutModification.xml");

            // Collect every app-owned pinned shortcut currently present in the TaskBar locked folder.
            var entries = new List<string>();
            if (Directory.Exists(Paths.TaskbarPinnedFolder))
            {
                foreach (var file in Directory.GetFiles(Paths.TaskbarPinnedFolder, "*.lnk"))
                {
                    var name = Path.GetFileName(file);
                    bool ours = string.Equals(name, "OCR Shortcut.lnk", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(name, "Voice Typing Shortcut.lnk", StringComparison.OrdinalIgnoreCase)
                        || name.StartsWith("Custom Shortcut (", StringComparison.OrdinalIgnoreCase);
                    if (!ours) continue;
                    var escaped = file.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
                    entries.Add($"        <taskbar:DesktopApp DesktopApplicationLinkPath=\"{escaped}\" PinGeneration=\"1\" />");
                }
            }

            if (entries.Count == 0)
            {
                if (File.Exists(xmlPath))
                {
                    File.Delete(xmlPath);
                    AppendLog(tweak, "LayoutModification.xml removed (no app-pinned shortcuts remain).");
                }
                ClearDefaultLayoutsCache(shellDir, tweak);
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine(@"<LayoutModificationTemplate xmlns:defaultlayout=""http://schemas.microsoft.com/Start/2014/FullDefaultLayout"" xmlns:taskbar=""http://schemas.microsoft.com/Start/2014/TaskbarLayout"" xmlns=""http://schemas.microsoft.com/Start/2014/LayoutModification"" Version=""1"">");
            sb.AppendLine("  <CustomTaskbarLayoutCollection>");
            sb.AppendLine("    <defaultlayout:TaskbarLayout>");
            sb.AppendLine("      <taskbar:TaskbarPinList>");
            foreach (var e in entries) sb.AppendLine(e);
            sb.AppendLine("      </taskbar:TaskbarPinList>");
            sb.AppendLine("    </defaultlayout:TaskbarLayout>");
            sb.AppendLine("  </CustomTaskbarLayoutCollection>");
            sb.Append("</LayoutModificationTemplate>");
            File.WriteAllText(xmlPath, sb.ToString(), Encoding.UTF8);

            ClearDefaultLayoutsCache(shellDir, tweak);
            AppendLog(tweak, $"LayoutModification.xml now lists {entries.Count} pinned app shortcut(s) — all are retained together.");
        }
        catch (Exception ex)
        {
            AppendLog(tweak, $"Windows 11 pinning prep failed: {ex.Message}");
        }
    }

    private void ClearDefaultLayoutsCache(string shellDir, TweakItem tweak)
    {
        try
        {
            var defaultLayoutsDir = Path.Combine(shellDir, "DefaultLayouts");
            if (Directory.Exists(defaultLayoutsDir))
            {
                Directory.Delete(defaultLayoutsDir, true);
                AppendLog(tweak, "Cleared DefaultLayouts cache.");
            }
        }
        catch (Exception ex)
        {
            AppendLog(tweak, $"Could not clear DefaultLayouts cache: {ex.Message}");
        }
    }

    private void ShowRestartExplorerPrompt()
    {
        if (!Dispatcher.HasShutdownStarted && Dispatcher.Thread == System.Threading.Thread.CurrentThread)
        {
            MessageBox.Show(
                "This change requires restarting Windows Explorer to take effect.\n\n" +
                "Use the \"Restart Explorer\" button in the detail panel to do this quickly.",
                "Restart Required", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else if (!Dispatcher.HasShutdownStarted)
        {
            Dispatcher.Invoke(() => MessageBox.Show(
                "This change requires restarting Windows Explorer to take effect.\n\n" +
                "Use the \"Restart Explorer\" button in the detail panel to do this quickly.",
                "Restart Required", MessageBoxButton.OK, MessageBoxImage.Information));
        }
    }

    private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (SelectedTweak != null)
            SelectedTweak.UserInput = (InputTextBox.Text ?? "").Trim();
    }

    private void InputComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SelectedTweak != null && InputComboBox.SelectedIndex >= 0)
            SelectedTweak.SelectedVariantIndex = InputComboBox.SelectedIndex;
    }

    private void HotkeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (SelectedTweak != null)
            SelectedTweak.Hotkey = (HotkeyTextBox.Text ?? "").Trim();
    }

    private void CustomShortcutKeys_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (SelectedTweak != null)
            SelectedTweak.UserInput = (CustomShortcutKeys.Text ?? "").Trim();
    }

    private void HwPreset_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string tag)
        {
            var parts = tag.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length >= 2)
            {
                ManufacturerTextBox.Text = parts[0];
                ProductNameTextBox.Text = parts[1];
            }
        }
    }

    private void ManufacturerTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (SelectedTweak != null)
            SelectedTweak.ManufacturerInput = (ManufacturerTextBox.Text ?? "").Trim();
    }

    private void ProductNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (SelectedTweak != null)
            SelectedTweak.ProductNameInput = (ProductNameTextBox.Text ?? "").Trim();
    }

    private void InputBrowse_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new OpenFolderDialog { Title = "Select Target Folder" };
            if (dialog.ShowDialog() == true)
            {
                InputTextBox.Text = dialog.FolderName.Trim();
                if (SelectedTweak != null)
                    SelectedTweak.UserInput = dialog.FolderName.Trim();
            }
        }
        catch (Exception ex) { if (SelectedTweak != null) AppendLog(SelectedTweak, $"InputBrowse error: {ex.Message}"); else ProcessRunner.AppendLog($"InputBrowse error: {ex.Message}"); }
    }

    private void MarkdownCtxMenuName_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (SelectedTweak != null)
            SelectedTweak.MarkdownMenuName = (MarkdownCtxMenuName.Text ?? "").Trim();
    }

    private void MarkdownCtxExtension_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (SelectedTweak != null)
            SelectedTweak.MarkdownExtension = (MarkdownCtxExtension.Text ?? "").Trim();
    }

    private void VisitSource_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/Michael-Matta1/windows-utilities-tweaks",
                UseShellExecute = true
            })?.Dispose();
        }
        catch (Exception ex) { AppendLog(SelectedTweak ?? Tweaks.FirstOrDefault(), $"VisitSource error: {ex.Message}"); }
    }

    internal static readonly string LogPlaceholder = "Operation log \u2014 output from Apply/Revert will appear here.";

    private void AppendLog(TweakItem? tweak, string line)
    {
        if (tweak == null) { AppendLogStatic(line); return; }
        var entry = $"[{DateTime.Now.ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)}] [{tweak.Id}] {line}";
        Action updateLog = () =>
        {
            try
            {
                if (!LogBox.IsLoaded) return;
                if (LogBox.Text.Length == 0 || LogBox.Text == LogPlaceholder)
                    LogBox.Clear();
                LogBox.AppendText(entry + Environment.NewLine);
                LogBox.ScrollToEnd();
            }
            catch (Exception) { ProcessRunner.AppendLog("AppendLog: failed to update log UI"); }
        };
        if (Dispatcher.CheckAccess())
            updateLog();
        else if (!Dispatcher.HasShutdownStarted)
            Dispatcher.BeginInvoke(updateLog);
        ProcessRunner.AppendLog($"[{tweak.Id}] {line}");
    }

    private static void AppendLogStatic(string line)
    {
        ProcessRunner.AppendLog(line);
    }

    private static int BoundVariantIndex(TweakItem tweak)
    {
        if (tweak?.Variants == null || tweak.Variants.Count == 0) return 0;
        var idx = tweak.SelectedVariantIndex;
        if (idx < 0 || idx >= tweak.Variants.Count) idx = 0;
        return idx;
    }

    private void ClearLog_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            LogBox.Clear();
            LogBox.Text = LogPlaceholder;
        }
        catch (Exception ex) { ProcessRunner.AppendLog($"ClearLog_Click failed: {ex.Message}"); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    protected override void OnClosed(EventArgs e)
    {
        Loaded -= MainWindow_Loaded;
        if (_closingHandler != null)
            Closing -= _closingHandler;
        foreach (var t in Tweaks)
            if (t != null) t.PropertyChanged -= TweakPropertyChanged;
        base.OnClosed(e);
    }

    private static (string ps1Content, string vbsContent, string psFileName, string vbsFileName, string shortcutName) GenerateCustomShortcutScripts(string keys)
    {
        if (keys == null) throw new System.ArgumentNullException(nameof(keys));
        var safeKeys = string.Join("_", keys.Split(System.IO.Path.GetInvalidFileNameChars(), System.StringSplitOptions.RemoveEmptyEntries));
        var parts = keys.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var modifiers = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string mainKey = "";

        foreach (var part in parts)
        {
            var p = part.ToUpperInvariant();
            if (p == "WIN" || p == "WINDOWS" || p == "LWIN")
                modifiers.Add("LWIN");
            else if (p == "RWIN")
                modifiers.Add("RWIN");
            else if (p == "CTRL" || p == "CONTROL")
                modifiers.Add("CONTROL");
            else if (p == "ALT" || p == "MENU")
                modifiers.Add("MENU");
            else if (p == "SHIFT")
                modifiers.Add("SHIFT");
            else if (string.IsNullOrEmpty(mainKey))
                mainKey = p;
            else
                throw new System.ArgumentException($"Multiple non-modifier keys in \"{keys}\": \"{mainKey}\" and \"{p}\"");
        }

        if (string.IsNullOrEmpty(mainKey))
            mainKey = "H"; // default fallback

        var vkMain = GetVirtualKeyCode(mainKey);
        var winKeyUsed = modifiers.Contains("LWIN") || modifiers.Contains("RWIN");
        var useRightWin = modifiers.Contains("RWIN");

        var sbPs1 = new StringBuilder();
        sbPs1.AppendLine("# PowerShell script to activate custom shortcut - Silent version");
        sbPs1.AppendLine($"# Keys: {keys}");
        sbPs1.AppendLine();
        sbPs1.AppendLine("Add-Type -TypeDefinition @\"");
        sbPs1.AppendLine("using System;");
        sbPs1.AppendLine("using System.Runtime.InteropServices;");
        sbPs1.AppendLine();
        sbPs1.AppendLine("public class KeyboardSender {");
        sbPs1.AppendLine("    [DllImport(\"user32.dll\", SetLastError = true)]");
        sbPs1.AppendLine("    public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);");
        sbPs1.AppendLine();

        if (winKeyUsed)
        {
            sbPs1.AppendLine("    public const int VK_LWIN = 0x5B;");
            if (useRightWin)
                sbPs1.AppendLine("    public const int VK_RWIN = 0x5C;");
        }

        if (modifiers.Contains("CONTROL"))
            sbPs1.AppendLine("    public const int VK_CONTROL = 0x11;");

        if (modifiers.Contains("MENU"))
            sbPs1.AppendLine("    public const int VK_MENU = 0x12;");

        if (modifiers.Contains("SHIFT"))
            sbPs1.AppendLine("    public const int VK_SHIFT = 0x10;");

        sbPs1.AppendLine($"    public const int VK_MAIN = 0x{vkMain};");
        sbPs1.AppendLine("    public const uint KEYEVENTF_KEYUP = 0x0002;");
        sbPs1.AppendLine();
        sbPs1.AppendLine("    public static void SendCustomKeys() {");

        if (winKeyUsed)
        {
            var winKeyVar = useRightWin ? "VK_RWIN" : "VK_LWIN";
            sbPs1.AppendLine($"        keybd_event({winKeyVar}, 0, 0, UIntPtr.Zero);");
        }

        if (modifiers.Contains("CONTROL"))
            sbPs1.AppendLine("        keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);");

        if (modifiers.Contains("MENU"))
            sbPs1.AppendLine("        keybd_event(VK_MENU, 0, 0, UIntPtr.Zero);");

        if (modifiers.Contains("SHIFT"))
            sbPs1.AppendLine("        keybd_event(VK_SHIFT, 0, 0, UIntPtr.Zero);");

        sbPs1.AppendLine("        System.Threading.Thread.Sleep(50);");
        sbPs1.AppendLine("        keybd_event(VK_MAIN, 0, 0, UIntPtr.Zero);");
        sbPs1.AppendLine("        System.Threading.Thread.Sleep(50);");
        sbPs1.AppendLine("        keybd_event(VK_MAIN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);");

        if (modifiers.Contains("SHIFT"))
            sbPs1.AppendLine("        keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);");

        if (modifiers.Contains("MENU"))
            sbPs1.AppendLine("        keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);");

        if (modifiers.Contains("CONTROL"))
            sbPs1.AppendLine("        keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);");

        if (winKeyUsed)
        {
            var winKeyVar = useRightWin ? "VK_RWIN" : "VK_LWIN";
            sbPs1.AppendLine($"        keybd_event({winKeyVar}, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);");
        }

        sbPs1.AppendLine("    }");
        sbPs1.AppendLine("}");
        sbPs1.AppendLine("\"@ -ErrorAction SilentlyContinue | Out-Null");
        sbPs1.AppendLine();
        sbPs1.AppendLine("# Send custom key combination");
        sbPs1.AppendLine("[KeyboardSender]::SendCustomKeys()");

        var psFileName = $"CustomShortcut_{safeKeys}.ps1";
        var vbsFileName = $"EnableCustomShortcut_{safeKeys}.vbs";

        var sbVbs = new StringBuilder();
        sbVbs.AppendLine("' VBScript to silently run PowerShell custom shortcut script");
        sbVbs.AppendLine($"' Keys: {keys}");
        sbVbs.AppendLine("' This script runs the PowerShell script completely hidden");
        sbVbs.AppendLine();
        sbVbs.AppendLine("Dim objShell, strCommand, strPowerShellPath");
        sbVbs.AppendLine();
        sbVbs.AppendLine("' Create Shell object");
        sbVbs.AppendLine("Set objShell = CreateObject(\"WScript.Shell\")");
        sbVbs.AppendLine();
        sbVbs.AppendLine("' Get the directory where this VBS script is located");
        sbVbs.AppendLine("strScriptPath = Left(WScript.ScriptFullName, InStrRev(WScript.ScriptFullName, \"\\\"))");
        sbVbs.AppendLine();
        sbVbs.AppendLine($"' PowerShell script filename (assumes it's in the same directory)");
        sbVbs.AppendLine($"strPowerShellScript = strScriptPath & \"{psFileName}\"");
        sbVbs.AppendLine();
        sbVbs.AppendLine("' Build the command to run PowerShell silently");
        sbVbs.AppendLine("strCommand = \"powershell.exe -WindowStyle Hidden -ExecutionPolicy Bypass -File \"\"\" & strPowerShellScript & \"\"\"\"");
        sbVbs.AppendLine();
        sbVbs.AppendLine("' Run the command completely hidden (0 = hidden window, False = don't wait)");
        sbVbs.AppendLine("objShell.Run strCommand, 0, False");
        sbVbs.AppendLine();
        sbVbs.AppendLine("' Clean up");
        sbVbs.AppendLine("Set objShell = Nothing");

        var shortcutName = $"Custom Shortcut ({safeKeys}).lnk";

        return (sbPs1.ToString(), sbVbs.ToString(), psFileName, vbsFileName, shortcutName);
    }

    private static string GetVirtualKeyCode(string key)
    {
        if (key == null) throw new System.ArgumentNullException(nameof(key));
        return key.ToUpperInvariant() switch
        {
            "A" => "41", "B" => "42", "C" => "43", "D" => "44", "E" => "45", "F" => "46", "G" => "47",
            "H" => "48", "I" => "49", "J" => "4A", "K" => "4B", "L" => "4C", "M" => "4D", "N" => "4E",
            "O" => "4F", "P" => "50", "Q" => "51", "R" => "52", "S" => "53", "T" => "54", "U" => "55",
            "V" => "56", "W" => "57", "X" => "58", "Y" => "59", "Z" => "5A",
            "0" => "30", "1" => "31", "2" => "32", "3" => "33", "4" => "34", "5" => "35",
            "6" => "36", "7" => "37", "8" => "38", "9" => "39",
            "F1" => "70", "F2" => "71", "F3" => "72", "F4" => "73", "F5" => "74", "F6" => "75",
            "F7" => "76", "F8" => "77", "F9" => "78", "F10" => "79", "F11" => "7A", "F12" => "7B",
            "SPACE" => "20", "ENTER" => "0D", "TAB" => "09", "ESC" => "1B", "ESCAPE" => "1B",
            "UP" => "26", "DOWN" => "28", "LEFT" => "25", "RIGHT" => "27",
            "HOME" => "24", "END" => "23", "PGUP" => "21", "PGDN" => "22",
            "CAPSLOCK" => "14", "NUMLOCK" => "90", "SCROLLLOCK" => "91",
            "PRINTSCREEN" => "2C", "PAUSE" => "13", "BREAK" => "13",
            "INS" => "2D", "DEL" => "2E", "DELETE" => "2E", "BACK" => "08",
            _ => throw new System.ArgumentException($"Unknown key: {key}")
        };
    }

    private static readonly HashSet<string> ValidModifiers = new(StringComparer.OrdinalIgnoreCase)
        { "WIN", "WINDOWS", "LWIN", "RWIN", "CTRL", "CONTROL", "ALT", "MENU", "SHIFT" };
    private static readonly HashSet<string> ValidKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z",
        "0","1","2","3","4","5","6","7","8","9",
        "F1","F2","F3","F4","F5","F6","F7","F8","F9","F10","F11","F12",
        "SPACE","ENTER","TAB","ESC","ESCAPE",
        "UP","DOWN","LEFT","RIGHT",
        "HOME","END","PGUP","PGDN",
        "INS","DEL","DELETE","BACK",
        "CAPSLOCK","NUMLOCK","SCROLLLOCK",
        "PRINTSCREEN","PAUSE","BREAK",
    };

    private static bool IsValidKeyCombination(string keys)
    {
        if (string.IsNullOrWhiteSpace(keys))
            return false;

        var parts = keys.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var hasModifier = false;
        var hasMainKey = false;
        var usedModifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var part in parts)
        {
            if (ValidModifiers.Contains(part))
            {
                if (hasMainKey)
                    return false; // modifier after main key is invalid
                if (!usedModifiers.Add(part))
                    return false; // duplicate modifier
                hasModifier = true;
                continue;
            }
            if (!hasMainKey && ValidKeys.Contains(part))
            {
                hasMainKey = true;
                continue;
            }
            return false; // Invalid key or duplicate main key
        }

        return hasModifier && hasMainKey;
    }

    private static bool IsValidLnkHotkey(string keys)
    {
        if (!IsValidKeyCombination(keys))
            return false;
        var parts = keys.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var part in parts)
        {
            var p = part.ToUpperInvariant();
            if (p == "WIN" || p == "WINDOWS" || p == "LWIN" || p == "RWIN")
                return false;
        }
        return true;
    }
}

public class NotGuideOnlyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TweakStrategy strategy)
            return strategy != TweakStrategy.GuideOnly;
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return System.Windows.Data.Binding.DoNothing;
    }
}
