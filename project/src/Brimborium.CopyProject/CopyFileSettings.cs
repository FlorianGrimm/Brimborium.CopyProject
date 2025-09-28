namespace Brimborium.CopyProject;

public class CopyFileSettings {
    private string? _Action;
    private CopyFileSettingsAction _CopyFileSettingsAction;

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
    public string? TargetPath { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public CopyFileSettingsAction CopyFileSettingsAction {
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

public enum CopyFileSettingsAction {
    Default,
    Copy,
    Delete,
    Ignore,
    Diff,
    Invalid
}