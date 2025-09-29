using System.Text.Json;

namespace Brimborium.CopyProject;

public sealed partial class AppConfigurationService {
    private readonly AppConfiguration _Configuration;
    private string? _RootFolder;
    private string? _SettingsFolder;
    private string? _SettingsFile;

    public AppConfigurationService(
        Microsoft.Extensions.Options.IOptions<AppConfiguration> configuration
    ) {
        this._Configuration = configuration.Value;
    }


    public string GetRootFolder() {
        if (this._RootFolder is { Length: > 0 } rootFolder) {
            return rootFolder;

        } else if (this._Configuration.RootFolder is { Length: > 0 } configurationRootFolder
            && System.IO.Path.IsPathFullyQualified(configurationRootFolder)) {
            return this._RootFolder = AppConfigurationUtility.GetRootFolder(
                configurationRootFolder);

        } else if (this._Configuration.RootMarkerFile is { Length: > 0 } rootMarkerFile) {
            return this._RootFolder = AppConfigurationUtility.GetRootFolderFromMarkerFile(
                rootMarkerFile,
                this._Configuration.RootFolder);

        } else {
            throw new InvalidOperationException("RootFolder or RootMarkerFile must be set.");
        }
    }

    public string GetSettingsFolder() {
        if (this._SettingsFolder is { Length: > 0 } cacheSettingsFolder) {
            return cacheSettingsFolder;
        }

        if (!(this._Configuration.SettingsFolder is { Length: > 0 } configurationSettingsFolder)) {
            throw new InvalidOperationException("SettingsFolder must be set.");
        } 
        {
            var rootFolder = this.GetRootFolder();
            var fullPath = System.IO.Path.GetFullPath(
                System.IO.Path.Combine(
                    rootFolder, 
                    configurationSettingsFolder));
            if (!System.IO.Directory.Exists(fullPath)) {
                throw new InvalidOperationException($"SettingsFolder '{fullPath}' does not exist.");
            }
            return this._SettingsFolder = fullPath;
        }
    }
    public string GetSettingsFile() {
        if (this._SettingsFile is { Length: > 0 } cacheSettingsFile) {
            return cacheSettingsFile;
        }
        if (!(this._Configuration.SettingsFile is { Length: > 0 } configurationSettingsFile)) {
            throw new InvalidOperationException("SettingsFile must be set.");
        }
        var settingsFolder = this.GetSettingsFolder();
        var fullPath = System.IO.Path.GetFullPath(
                System.IO.Path.Combine(
                    settingsFolder, 
                    configurationSettingsFile));
        if (!System.IO.File.Exists(fullPath)) {
            throw new InvalidOperationException($"SettingsFile '{fullPath}' does not exist.");
        }
        return this._SettingsFile = fullPath;
    }

    public CopyProjectSettings LoadCopyProjectSettings() {
        var settingsFile = this.GetSettingsFile();

        var options = GetJsonSerializerOptions();
        CopyProjectSettings? settings;
        using (var json = System.IO.File.OpenRead(settingsFile)) {
            settings = System.Text.Json.JsonSerializer.Deserialize<CopyProjectSettings>(json, options);
        }
        if (settings is null) {
            throw new InvalidOperationException($"Cannot deserialize '{settingsFile}'.");
        }
        return settings;
    }

    public void SaveCopyProjectSettings(
        CopyProjectSettings value) {
        var settingsFile = this.GetSettingsFile();
        var options = GetJsonSerializerOptions();
            
        using (var jsonStream = System.IO.File.Create(settingsFile)) {
            System.Text.Json.JsonSerializer.Serialize(
                jsonStream, 
                value, 
                options);
        }
    }

    public List<CopyFileSettings> LoadCopyFileSettings(string filename) {
        var settingsFolder = this.GetSettingsFolder();
        var fullPath = System.IO.Path.GetFullPath(
                System.IO.Path.Combine(
                    settingsFolder, 
                    filename));
        if (!System.IO.File.Exists(fullPath)) {
            return [];
        }
        var options = GetJsonSerializerOptions();
        using (var json = System.IO.File.Open(fullPath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read)) {
            var settings = System.Text.Json.JsonSerializer.Deserialize<List<CopyFileSettings>>(json, options);
            //if (settings is null) {
            //    throw new InvalidOperationException($"Cannot deserialize '{fullPath}'.");
            //}
            return settings ?? [];
        }
    }

    public void SaveCopyFileSettings(
        string filename,
        List<CopyFileSettings> listCopyFileSettings) {
        var settingsFolder = this.GetSettingsFolder();
        var fullPath = System.IO.Path.GetFullPath(
                System.IO.Path.Combine(
                    settingsFolder, 
                    filename));
        var options = GetJsonSerializerOptions();
            
        using (var jsonStream = System.IO.File.Create(fullPath)) {
            System.Text.Json.JsonSerializer.Serialize(
                jsonStream, 
                listCopyFileSettings, 
                options);
        }
    }

    private static JsonSerializerOptions? _JsonSerializerOptions;
    public static JsonSerializerOptions? GetJsonSerializerOptions() {
        return _JsonSerializerOptions ??= new System.Text.Json.JsonSerializerOptions() {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            WriteIndented = true
        };
    }
}
