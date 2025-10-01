using Microsoft.Extensions.Logging;

namespace Brimborium.CopyProject;

public sealed class CopyProjectSettings {
    public List<string> ExcludeFolderNames { get; set; } = new();
    public List<CopyProjectMap> Projects { get; set; } = new();
}

public static class CopyProjectSettingsExtension {
    public static ExcludeSettings GetExcludeSettings(this CopyProjectSettings that)
        => new ExcludeSettings(
            new string[] { ".git", ".github", ".vscode", "bin", "obj", "artifacts", "node_modules" },
            that.ExcludeFolderNames);

    public static bool Normalize(
        this CopyProjectSettings that,
        AppConfigurationService appConfigurationService) {
        bool result = false;
        var rootFolder = appConfigurationService.GetRootFolder();
        for (int idxProject = 0; idxProject < that.Projects.Count; idxProject++) {
            CopyProjectMap project = that.Projects[idxProject];
            if (project.Normalize(idxProject, rootFolder)) {
                result = true;
            }
        }
        return result;
    }

    public static List<ContentMapping> LoadProjectsFromFile(
        this CopyProjectSettings that,
        AppConfigurationService appConfigurationService,
        ExcludeSettings excludeSettings,
        ILogger logger) {
        List<ContentMapping> result = [];
        var rootFolder = appConfigurationService.GetRootFolder();
        foreach (var project in that.Projects) {
            var contentMapping = project.LoadProjectFormFile(appConfigurationService, excludeSettings, logger);
            if (contentMapping is not null) {
                result.Add(contentMapping);
            }
        }
        return result;
    }
}