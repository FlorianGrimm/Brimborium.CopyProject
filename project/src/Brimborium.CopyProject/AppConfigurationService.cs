using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Brimborium.CopyProject;

public sealed partial class AppConfigurationService {
    private readonly AppConfiguration _Configuration;
    private readonly ILogger<AppConfigurationService> _Logger;
    private string? _RootFolder;
    private string? _SettingsFolder;
    private string? _SettingsFile;

    public AppConfigurationService(
        Microsoft.Extensions.Options.IOptions<AppConfiguration> configuration,
        ILogger<AppConfigurationService> logger
    ) {
        this._Configuration = configuration.Value;
        this._Logger = logger;
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
            if (this._SettingsFile is { Length: > 0 } settingsFile) {
                if (System.IO.Path.GetDirectoryName(settingsFile) is { Length: > 0 } settingsFolder) {
                    return this._SettingsFolder = settingsFolder;
                }
            }
            this._Logger.ConfigurationNameMustBeSet(nameof(AppConfiguration.SettingsFolder));
            throw new BuissnessException("SettingsFolder must be set.");
        }
        {
            var rootFolder = this.GetRootFolder();
            var fullPath = System.IO.Path.GetFullPath(
                System.IO.Path.Combine(
                    rootFolder,
                    configurationSettingsFolder));
            if (!System.IO.Directory.Exists(fullPath)) {
                this._Logger.ConfigurationFolderDoesNotExist(nameof(AppConfiguration.SettingsFolder), fullPath);
                throw new BuissnessException($"SettingsFolder '{fullPath}' does not exist.");
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
        } else {
            if (System.IO.Path.IsPathFullyQualified(configurationSettingsFile)) {
                if (!System.IO.File.Exists(configurationSettingsFile)) {
                    throw new InvalidOperationException($"SettingsFile '{configurationSettingsFile}' does not exist.");
                }
                if (this._SettingsFolder is null && string.IsNullOrEmpty(this._Configuration.SettingsFolder)) {
                    this._SettingsFolder = System.IO.Path.GetDirectoryName(configurationSettingsFile);
                }
                return this._SettingsFile = configurationSettingsFile;
            }
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
        try {
            using (var json = System.IO.File.OpenRead(settingsFile)) {
                settings = System.Text.Json.JsonSerializer.Deserialize<CopyProjectSettings>(json, options) ?? new();
            }
        } catch (System.Exception error) {
            this._Logger.LoadCopyProjectSettingsFailed(error, settingsFile);
            throw new BuissnessException($"LoadCopyProjectSettings failed {settingsFile}.", error);
        }

        return new CopyProjectSettings() {
            ExcludeFolderNames = settings.ExcludeFolderNames,
            Projects = settings.Projects.Select(project => project.ConvertToUse()).ToList(),
        };
    }

    public void SaveCopyProjectSettings(
        CopyProjectSettings settings) {
        var settingsFile = this.GetSettingsFile();
        var options = GetJsonSerializerOptions();

        var settingsToSave = new CopyProjectSettings() {
            ExcludeFolderNames = settings.ExcludeFolderNames,
            Projects = settings.Projects.Select(project => project.ConvertPathForSettings()).ToList()
        };
        using (var jsonStream = System.IO.File.Create(settingsFile)) {
            System.Text.Json.JsonSerializer.Serialize(
                jsonStream,
                settingsToSave,
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
        try {
            using (var json = System.IO.File.Open(fullPath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read)) {
                var settings = System.Text.Json.JsonSerializer.Deserialize<List<CopyFileSettings>>(json, options);
                return (settings ?? [])
                    .Select(item=>new CopyFileSettings() { 
                        Path = AppConfigurationUtility.ConvertPathToUse(item.Path),
                        ActionE = item.ActionE
                    })
                    .ToList();
            }
        } catch (System.Exception error) {
            this._Logger.LoadCopyFileSettingsFailed(error, fullPath);
            throw new BuissnessException($"Cannot LoadCopyFileSettings from ${fullPath}", error);
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
        var listCopyFileSettingsToSave = listCopyFileSettings
            .Select(item => new CopyFileSettings() {
                Path = AppConfigurationUtility.ConvertPathForSettings(item.Path),
                ActionE = item.ActionE
            }).ToList();
        using (var jsonStream = System.IO.File.Create(fullPath)) {
            System.Text.Json.JsonSerializer.Serialize(
                jsonStream,
                listCopyFileSettingsToSave,
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
