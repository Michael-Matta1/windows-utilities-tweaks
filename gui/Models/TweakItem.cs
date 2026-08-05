using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace WindowsTweakLauncher.Models;

public enum TweakStatus { Unknown, Applied, NotApplied, PartialApplied, Reapplicable }

public enum TweakStrategy
{
    None,
    ImportReg,
    CopyVbsThenReg,
    RunPS1,
    RunBat,
    RegistryAPI,
    GenerateReg,
    NeedsInputThenReg,
    NeedsInputVariant,
    GuideOnly,
    ShortcutWithManualPin,
    TerminalProfileJson,
    HardwareIdentityTest
}

public class RegistryTarget
{
    private string _hive = "";
    private string _regPath = "";

    [JsonPropertyName("hive")]
    public string Hive
    {
        get => _hive;
        set { _hive = value ?? ""; }
    }

    [JsonPropertyName("path")]
    public string RegPath
    {
        get => _regPath;
        set { _regPath = value ?? ""; }
    }
}

public class TweakItem : INotifyPropertyChanged
{
    private bool _isSelected;
    private TweakStatus _status = TweakStatus.Unknown;
    private int _selectedVariantIndex;
    private string? _userInput;
    private string _hotkey = "CTRL+ALT+N";
    private string? _selectedIconPath;
    private List<RegistryTarget> _backupTargets = new();
    private string _category = "";
    private string _id = "";
    private string _name = "";
    private string _description = "";
    private string _folderName = "";
    private TweakStrategy _strategy;
    private bool _canRevert = true;
    private bool _requiresConfirmation;
    private string? _backupNote;
    private string? _variantLabel;
    private List<string>? _variants;
    private string? _applyFile;
    private string? _revertFile;
    private bool _hiddenFromList;

    [JsonPropertyName("id")]
    public string Id
    {
        get => _id;
        set { _id = value ?? ""; OnPropertyChanged(); }
    }

    [JsonPropertyName("name")]
    public string Name
    {
        get => _name;
        set { _name = value ?? ""; OnPropertyChanged(); }
    }

    [JsonPropertyName("category")]
    public string Category
    {
        get => _category;
        set { _category = value ?? ""; OnPropertyChanged(); }
    }

    [JsonPropertyName("description")]
    public string Description
    {
        get => _description;
        set { _description = value ?? ""; OnPropertyChanged(); }
    }

    [JsonPropertyName("folder")]
    public string FolderName
    {
        get => _folderName;
        set { _folderName = value ?? ""; OnPropertyChanged(); }
    }

    [JsonPropertyName("strategy")]
    public TweakStrategy Strategy
    {
        get => _strategy;
        set { _strategy = value; OnPropertyChanged(); }
    }

    [JsonIgnore]
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); }
    }

    [JsonIgnore]
    public TweakStatus Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }

    [JsonPropertyName("canRevert")]
    public bool CanRevert
    {
        get => _canRevert;
        set { _canRevert = value; OnPropertyChanged(); }
    }

    [JsonPropertyName("requiresConfirmation")]
    public bool RequiresConfirmation
    {
        get => _requiresConfirmation;
        set { _requiresConfirmation = value; OnPropertyChanged(); }
    }

    [JsonPropertyName("backupTargets")]
    public List<RegistryTarget> BackupTargets
    {
        get => _backupTargets;
        set { _backupTargets = value ?? new(); OnPropertyChanged(); }
    }

    [JsonPropertyName("backupNote")]
    public string? BackupNote
    {
        get => _backupNote;
        set { _backupNote = value; OnPropertyChanged(); }
    }

    [JsonPropertyName("variantLabel")]
    public string? VariantLabel
    {
        get => _variantLabel;
        set { _variantLabel = value; OnPropertyChanged(); }
    }

    [JsonPropertyName("variants")]
    public List<string>? Variants
    {
        get => _variants;
        set
        {
            _variants = value;
            if (_variants is { Count: > 0 })
                _selectedVariantIndex = Math.Clamp(_selectedVariantIndex, 0, _variants.Count - 1);
            else
                _selectedVariantIndex = 0;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedVariantIndex));
        }
    }

    [JsonPropertyName("applyFile")]
    public string? ApplyFile
    {
        get => _applyFile;
        set { _applyFile = value; OnPropertyChanged(); }
    }

    [JsonPropertyName("revertFile")]
    public string? RevertFile
    {
        get => _revertFile;
        set { _revertFile = value; OnPropertyChanged(); }
    }

    [JsonPropertyName("hiddenFromList")]
    public bool HiddenFromList
    {
        get => _hiddenFromList;
        set { _hiddenFromList = value; OnPropertyChanged(); }
    }

    [JsonIgnore]
    public int SelectedVariantIndex
    {
        get => _selectedVariantIndex;
        set
        {
            if (Variants is not { Count: > 0 }) { _selectedVariantIndex = 0; OnPropertyChanged(); return; }
            var max = Variants.Count - 1;
            _selectedVariantIndex = Math.Clamp(value, 0, max);
            OnPropertyChanged();
        }
    }

    [JsonIgnore]
    public string? UserInput
    {
        get => _userInput;
        set { _userInput = value; OnPropertyChanged(); }
    }

    [JsonIgnore]
    public string? SelectedIconPath
    {
        get => _selectedIconPath;
        set { _selectedIconPath = value; OnPropertyChanged(); }
    }

    [JsonIgnore]
    public string Hotkey
    {
        get => _hotkey;
        set { _hotkey = string.IsNullOrWhiteSpace(value) ? "CTRL+ALT+N" : value; OnPropertyChanged(); }
    }

    private string? _manufacturerInput;
    [JsonIgnore]
    public string? ManufacturerInput
    {
        get => _manufacturerInput;
        set { _manufacturerInput = value; OnPropertyChanged(); }
    }

    private string? _productNameInput;
    [JsonIgnore]
    public string? ProductNameInput
    {
        get => _productNameInput;
        set { _productNameInput = value; OnPropertyChanged(); }
    }

    private string _markdownMenuName = "Create Markdown File";
    [JsonIgnore]
    public string MarkdownMenuName
    {
        get => _markdownMenuName;
        set { _markdownMenuName = string.IsNullOrWhiteSpace(value) ? "Create Markdown File" : value; OnPropertyChanged(); }
    }

    private string _markdownExtension = ".md";
    [JsonIgnore]
    public string MarkdownExtension
    {
        get => _markdownExtension;
        set { _markdownExtension = string.IsNullOrWhiteSpace(value) ? ".md" : value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
