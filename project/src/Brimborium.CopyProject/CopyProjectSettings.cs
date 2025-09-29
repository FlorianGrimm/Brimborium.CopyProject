namespace Brimborium.CopyProject;

public sealed class CopyProjectSettings {
    public List<string> ExcludeFolderNames { get; set; } = new();
    public List<CopyProjectMap> Projects { get; set; } = new();
}

public static class CopyProjectSettingsExtension {
    public static ExcludeSettings GetExcludeSettings(this CopyProjectSettings that)
        => new ExcludeSettings(that.ExcludeFolderNames, null);

    public static bool Normalize(
        this CopyProjectSettings that,
        AppConfigurationService appConfigurationService,
        ExcludeSettings excludeSettings) {
        bool result = false;
        var rootFolder = appConfigurationService.GetRootFolder();
        for (int idxProject = 0; idxProject < that.Projects.Count; idxProject++) {
            CopyProjectMap project = that.Projects[idxProject];
            if (!(project.SettingsName is { Length: > 0 } settingsName)) {
                throw new InvalidOperationException($"{idxProject} SettungsName is empty.");
            }
            if (string.IsNullOrEmpty(project.SourcePath)
                || string.IsNullOrEmpty(project.TargetPath)) {
                throw new InvalidOperationException($"{project.SettingsName} - SourcePath and TargetPath must be set.");
            }
            if (project.Normalize(rootFolder)) {
                result = true;
            }
        }
        return result;
    }

    public static List<ContentMapping> LoadProjectsFromFile(
        this CopyProjectSettings that,
        AppConfigurationService appConfigurationService,
        ExcludeSettings excludeSettings) {
        List<ContentMapping> result = [];
        var rootFolder = appConfigurationService.GetRootFolder();
        foreach (var project in that.Projects) {
            var contentMapping = project.LoadProjectFormFile(appConfigurationService, excludeSettings);
            if (contentMapping is not null) { 
                result.Add(contentMapping);
            }
        }
        return result;
    }
}