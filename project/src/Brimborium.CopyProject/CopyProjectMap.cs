using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace Brimborium.CopyProject;

public sealed partial class CopyProjectMap {
    public string? SettingsName { get; set; }
    public string? SourcePath { get; set; }
    public string? TargetPath { get; set; }
    public string? DiffPath { get; set; }
    public List<string> ExcludeFolderNames { get; set; } = new();
}

partial class CopyProjectMap {
    [JsonIgnore]
    internal ExcludeSettings? ExcludeSettings { get; set; }
}

public static class CopyProjectMapExtension {
    public static CopyProjectMap ConvertToUse(this CopyProjectMap that) {
        return new CopyProjectMap() {
            SettingsName = AppConfigurationUtility.ConvertPathToUse(that.SettingsName),
            SourcePath = AppConfigurationUtility.ConvertPathToUse(that.SourcePath),
            TargetPath = AppConfigurationUtility.ConvertPathToUse(that.TargetPath),
            DiffPath = AppConfigurationUtility.ConvertPathToUse(that.DiffPath),
            ExcludeFolderNames = new List<string>(that.ExcludeFolderNames ?? []),
        };
    }

    public static CopyProjectMap ConvertPathForSettings(this CopyProjectMap that) {
        return new CopyProjectMap() {
            SettingsName = AppConfigurationUtility.ConvertPathForSettings(that.SettingsName),
            SourcePath = AppConfigurationUtility.ConvertPathForSettings(that.SourcePath),
            TargetPath = AppConfigurationUtility.ConvertPathForSettings(that.TargetPath),
            DiffPath = AppConfigurationUtility.ConvertPathForSettings(that.DiffPath),
            ExcludeFolderNames = that.ExcludeFolderNames,
        };
    }

    public static bool Normalize(
        this CopyProjectMap that,
        int idxProject,
        string rootDir) {
        if (!(that.SettingsName is { Length: > 0 } settingsName)) {
            throw new InvalidOperationException($"{idxProject} SettingsName is empty.");
        }
        if (string.IsNullOrEmpty(that.SourcePath)
            || string.IsNullOrEmpty(that.TargetPath)) {
            throw new InvalidOperationException($"{that.SettingsName} - SourcePath and TargetPath must be set.");
        }

        string newSrcPath; bool changed1 = false;
        string newDstPath; bool changed2 = false;

        if (that.SourcePath is { } srcPath) {
            (newSrcPath, changed1) = AppConfigurationUtility.GetRelativePath(rootDir, srcPath);
            if (changed1 || !string.Equals(that.SourcePath, newSrcPath, StringComparison.Ordinal)) {
                that.SourcePath = newSrcPath;
                changed1 = true;
            }
        }
        if (that.TargetPath is { } dstPath) {
            (newDstPath, changed2) = AppConfigurationUtility.GetRelativePath(rootDir, dstPath);
            if (changed2 || !string.Equals(that.TargetPath, newDstPath, StringComparison.Ordinal)) {
                that.TargetPath = newDstPath;
                changed2 = true;
            }
        }
        return changed1 || changed2;
    }

    public static ContentMapping? CreateContentMapping(
        this CopyProjectMap that,
        string rootDir,
        ExcludeSettings excludeSettings,
        ILogger logger) {
        if (that.ExcludeSettings is null) {
            that.ExcludeSettings = excludeSettings = new ExcludeSettings(excludeSettings.Excludes, that.ExcludeFolderNames);
        } else {
            excludeSettings = that.ExcludeSettings;
        }

        rootDir = AppConfigurationUtility.ConvertPathToUse(rootDir);
        if (that.SourcePath is { } srcPath
            && that.TargetPath is { } dstPath) {
            AppConfigurationUtility.GetRelativePath(rootDir, srcPath);

            var srcRepoContentList = RepoContentList.Create(
                rootDir: rootDir,
                repoDir: AppConfigurationUtility.ConvertPathToUse(srcPath),
                excludeSettings: excludeSettings);

            var dstRepoContentList = RepoContentList.Create(
                rootDir: rootDir,
                repoDir: AppConfigurationUtility.ConvertPathToUse(dstPath),
                excludeSettings: excludeSettings);

            RepoContentList? diffRepoContentList;
            if (that.DiffPath is { Length: > 0 } diffPath) {
                diffRepoContentList = RepoContentList.Create(
                    rootDir: rootDir,
                    repoDir: AppConfigurationUtility.ConvertPathToUse(diffPath),
                    excludeSettings: excludeSettings);

            } else {
                diffRepoContentList = null;
            }

            return new ContentMapping(
                that,
                srcRepoContentList,
                dstRepoContentList,
                diffRepoContentList,
                logger);

        } else {
            return null;
        }
    }

    public static ContentMapping? LoadProjectFormFile(
        this CopyProjectMap project,
        AppConfigurationService appConfigurationService,
        ExcludeSettings excludeSettings,
        ILogger logger) {
        if (project.SettingsName is { Length: > 0 } settingsName) {
            var rootFolder = appConfigurationService.GetRootFolder();
            var settingsNameJson = AppConfigurationUtility.EnsureJsonExtension(project.SettingsName);
            var listCopyFileSettings = appConfigurationService.LoadCopyFileSettings(settingsNameJson);

            if (project.CreateContentMapping(rootFolder, excludeSettings, logger) is { } contentMapping) {
                foreach (var copyFileSettings in listCopyFileSettings) {
                    if (copyFileSettings.Path is { Length: > 0 } copyFileSettingsPath) {
                        contentMapping.Src.AddFile(copyFileSettingsPath, copyFileSettings.ActionE);
                    }
                }
                return contentMapping;
            }
        }
        return null;
    }
}