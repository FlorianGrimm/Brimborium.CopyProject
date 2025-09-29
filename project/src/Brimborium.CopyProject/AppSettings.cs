namespace Brimborium.CopyProject;

public sealed partial class AppConfiguration {
    /// <summary>
    /// The relative path to a file that marks the root folder 
    /// or if RootFolder is relative this will be added.
    /// </summary>
    public string? RootMarkerFile { get; set; }

    /// <summary>
    /// If RootFolder is an absolute path, it is used as RootFolder.
    /// </summary>
    public string? RootFolder { get; set; }
    public string? SettingsFile { get; set; }
    public string? SettingsFolder { get; set; }
}
