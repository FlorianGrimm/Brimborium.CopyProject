
using Microsoft.Extensions.Logging;

namespace Brimborium.CopyProject;

public class ExecutorShowConfig : Executor {
    private readonly AppConfigurationService _AppConfigurationService;
    private readonly ILogger<ExecutorDiff> _Logger;

    public ExecutorShowConfig(
        AppConfigurationService appConfigurationService,
        ILogger<ExecutorDiff> logger) {
        this._AppConfigurationService = appConfigurationService;
        this._Logger = logger;
    }

    public override async Task<int> RunAsync() {
        this._Logger.StartShowConfig();
        var appConfigurationService = this._AppConfigurationService;
        var rootFolder = appConfigurationService.GetRootFolder();
        var copyProjectSettings = appConfigurationService.LoadCopyProjectSettings();
        var excludeSettings = copyProjectSettings.GetExcludeSettings();

        bool changedCopyProjectSetting = false;
        for (int idxProject = 0; idxProject < copyProjectSettings.Projects.Count; idxProject++) {
            var project = copyProjectSettings.Projects[idxProject];
            if (project.Normalize(idxProject, rootFolder)) {
                changedCopyProjectSetting = true;
            }

            var contentMapping = project.LoadProjectFormFile(appConfigurationService, excludeSettings, this._Logger);
            if (contentMapping is null) {
                this._Logger.LoadProjectFormFileFailed(idxProject, project.SettingsName);
                continue;
            };
        }
        if (changedCopyProjectSetting) {
            this._Logger.CopyProjectSettingsChanged();
            appConfigurationService.SaveCopyProjectSettings(copyProjectSettings);
        }

        await Task.CompletedTask;
        return 0;
    }
}