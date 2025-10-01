namespace Brimborium.CopyProject;

public sealed class CopyFileSettings {
    private string? _Action;
    private CopyFileSettingsAction _CopyFileSettingsAction;

    public CopyFileSettings() {
    }

    public CopyFileSettings(
        string path,
        string? action = null) {
        this.Path = path;
        this.Action = action;
    }

    public string? Path { get; set; }

    public string? Action {
        get => this._Action;
        set {
            this._Action = value;
            this._CopyFileSettingsAction = (value?.ToLowerInvariant()??string.Empty) switch {
                "" => CopyFileSettingsAction.Default,
                "copy" => CopyFileSettingsAction.Copy,
                "delete" => CopyFileSettingsAction.Delete,
                "ignore" => CopyFileSettingsAction.Ignore,
                "diff" => CopyFileSettingsAction.Diff,
                _ => CopyFileSettingsAction.Invalid
            };
        }
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public CopyFileSettingsAction ActionE {
        get => this._CopyFileSettingsAction;
        set {
            this._CopyFileSettingsAction = value;
            this._Action = value switch {
                CopyFileSettingsAction.Default => string.Empty,
                CopyFileSettingsAction.Copy => "copy",
                CopyFileSettingsAction.Delete => "delete",
                CopyFileSettingsAction.Ignore => "ignore",
                CopyFileSettingsAction.Diff => "diff",
                _ => string.Empty
            };
        }
    }
}
