namespace Brimborium.CopyProject;

public sealed class CopyProjectMap {
    public string? SettingsName { get; set; }
    public string? SourcePath { get; set; }
    public string? TargetPath { get; set; }
    public string? DiffPath { get; set; }
}


public static class CopyProjectMapExtension {
    public static bool Normalize(
        this CopyProjectMap that,
        string rootDir) {
        string newSrcPath; bool changed1 = false;
        string newDstPath; bool changed2 = false;

        if (that.SourcePath is { } srcPath) {
            (newSrcPath, changed1) = AppConfigurationUtility.GetRelativePath(rootDir, srcPath);
            if (changed1 || string.Equals(that.SourcePath, newSrcPath, StringComparison.Ordinal)) {
                that.SourcePath = newSrcPath;
                changed1 = true;
            }
        }
        if (that.TargetPath is { } dstPath) {
            (newDstPath, changed2) = AppConfigurationUtility.GetRelativePath(rootDir, dstPath);
            if (changed2 || string.Equals(that.TargetPath, newDstPath, StringComparison.Ordinal)) {
                that.TargetPath = newDstPath;
                changed2 = true;
            }
        }
        return changed1 || changed2;
    }

    public static ContentMapping? CreateContentMapping(
        this CopyProjectMap that,
        string rootDir,
        ExcludeSettings excludeSettings) {
        if (that.SourcePath is { } srcPath
            && that.TargetPath is { } dstPath) {
            AppConfigurationUtility.GetRelativePath(rootDir, srcPath);

            var srcRepoContentList = RepoContentList.Create(
                rootDir: rootDir,
                repoDir: srcPath,
                excludeSettings: excludeSettings);

            var dstRepoContentList = RepoContentList.Create(
                rootDir: rootDir,
                repoDir: dstPath,
                excludeSettings: excludeSettings);

            RepoContentList? diffRepoContentList;
            if (that.DiffPath is { Length: > 0 } diffPath) {
                diffRepoContentList = RepoContentList.Create(
                    rootDir: rootDir,
                    repoDir: diffPath,
                    excludeSettings: excludeSettings);

            } else {
                diffRepoContentList = null;
            }

            return new ContentMapping(
                that,
                srcRepoContentList,
                dstRepoContentList,
                diffRepoContentList);

        } else {
            return null;
        }
    }

    public static ContentMapping? LoadProjectFormFile(
        this CopyProjectMap project,
        AppConfigurationService appConfigurationService,
        ExcludeSettings excludeSettings) {
        if (project.SettingsName is { Length: > 0 } settingsName) {
            var rootFolder = appConfigurationService.GetRootFolder();
            var settingsNameJson = AppConfigurationUtility.EnsureJsonExtension(project.SettingsName);
            var listCopyFileSettings = appConfigurationService.LoadCopyFileSettings(settingsNameJson);

            if (project.CreateContentMapping(rootFolder, excludeSettings) is { } contentMapping) {
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