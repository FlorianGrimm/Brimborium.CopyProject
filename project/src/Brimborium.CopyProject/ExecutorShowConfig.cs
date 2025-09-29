
namespace Brimborium.CopyProject;

public class ExecutorShowConfig : Executor {
    private readonly AppConfigurationService _AppConfigurationService;

    public ExecutorShowConfig(AppConfigurationService appConfigurationService) {
        this._AppConfigurationService = appConfigurationService;
    }

    public override async Task<int> RunAsync() {
        System.Console.Out.WriteLine("ShowConfig");
        var appConfigurationService = this._AppConfigurationService;
        var rootFolder = appConfigurationService.GetRootFolder();
        var copyProjectSettings = appConfigurationService.LoadCopyProjectSettings();
        var excludeSettings = copyProjectSettings.GetExcludeSettings();

        bool changedCopyProjectSetting = copyProjectSettings.Normalize(appConfigurationService, excludeSettings);
        if (changedCopyProjectSetting) {
            System.Console.Out.WriteLine($"CopyProjectSettings changed - save to file.");
            appConfigurationService.SaveCopyProjectSettings(copyProjectSettings);
        }

        for (int idxProject = 0; idxProject < copyProjectSettings.Projects.Count; idxProject++) {
            var project = copyProjectSettings.Projects[idxProject];
            var contentMapping = project.LoadProjectFormFile(appConfigurationService, excludeSettings);
            if (contentMapping is null) {
                System.Console.Error.WriteLine($"{idxProject}: Cannot create ContentMapping for project with SettingsName '{project.SettingsName}'.");
                continue;
            };
        }

        await Task.CompletedTask;
        return 0;
    }
}