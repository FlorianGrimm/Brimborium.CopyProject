using Microsoft.Extensions.Logging;

namespace Brimborium.CopyProject;

public static partial class LogExtensions {
    [LoggerMessage(Level = LogLevel.Error, Message = "Configuration '{configurationName}': must be set.")]
    public static partial void ConfigurationNameMustBeSet(this ILogger logger, string configurationName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Configuration '{configurationName}': '{fullPath}' does not exist.")]
    public static partial void ConfigurationFolderDoesNotExist(this ILogger logger, string configurationName, string fullPath);

    [LoggerMessage(Level = LogLevel.Error, Message = "LoadCopyProjectSettings '{settingsFile}' failed.")]
    public static partial void LoadCopyProjectSettingsFailed(this ILogger logger, Exception error, string settingsFile);

    [LoggerMessage(Level = LogLevel.Error, Message = "LoadCopyFileSettings '{settingsFile}' failed.")]
    public static partial void LoadCopyFileSettingsFailed(this ILogger logger, Exception error, string settingsFile);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{idxProject}: Cannot create ContentMapping for project with SettingsName '{settingsName}'.")]
    public static partial void LoadProjectFormFileFailed(this ILogger logger, int idxProject, string? settingsName);

    //
    [LoggerMessage(Level = LogLevel.Information, Message = "UpdateAction '{relativePath}' '{actionE}'")]
    public static partial void UpdateAction(this ILogger logger, string relativePath, CopyFileSettingsAction actionE);

    [LoggerMessage(Level = LogLevel.Information, Message = "ExecuteAction '{relativePath}' '{actionE}'")]
    public static partial void ExecuteAction(this ILogger logger, string relativePath, CopyFileSettingsAction actionE);

    [LoggerMessage(Level = LogLevel.Information, Message = "Diff '{relativePath}'")]
    public static partial void DiffAction(this ILogger logger, string relativePath);

    [LoggerMessage(Level = LogLevel.Information, Message = "Copy")]
    public static partial void StartCopy(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Diff")]
    public static partial void StartDiff(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "ScanFolder")]
    public static partial void StartScanFolder(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "ShowConfig")]
    public static partial void StartShowConfig(this ILogger logger);
    
    [LoggerMessage(Level = LogLevel.Information, Message = "Update")]
    public static partial void StartUpdate(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "CopyProjectSettings changed - save to file.")]
    public static partial void CopyProjectSettingsChanged(this ILogger logger);

    /*
     * 
    [LoggerMessage(Level =LogLevel.Information, Message = "")]
    public static partial void X(this ILogger logger);
    */
}