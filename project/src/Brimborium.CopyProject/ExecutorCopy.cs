using Microsoft.Extensions.Logging;

namespace Brimborium.CopyProject;

public class ExecutorCopy : Executor {
    private readonly AppConfigurationService _AppConfigurationService;
    private readonly ILogger<ExecutorCopy> _Logger;

    public ExecutorCopy(
        AppConfigurationService appConfigurationService,
        ILogger<ExecutorCopy> logger
        ) {
        this._AppConfigurationService = appConfigurationService;
        this._Logger = logger;
    }

    public override async Task<int> RunAsync() {
        this._Logger.StartCopy();
        var appConfigurationService = this._AppConfigurationService;
        var rootFolder = appConfigurationService.GetRootFolder();
        var copyProjectSettings = appConfigurationService.LoadCopyProjectSettings();
        var excludeSettings = copyProjectSettings.GetExcludeSettings();

        bool changedCopyProjectSetting = false;
        for(int idxProject = 0; idxProject < copyProjectSettings.Projects.Count; idxProject++) {
            var project = copyProjectSettings.Projects[idxProject];
            if (project.Normalize(idxProject, rootFolder)) {
                changedCopyProjectSetting = true;
            }
            var contentMapping = project.LoadProjectFormFile(appConfigurationService, excludeSettings, this._Logger);
            if (contentMapping is null) {
                this._Logger.LoadProjectFormFileFailed(idxProject, project.SettingsName);
                continue;
            }
            contentMapping.Dst.RescanFolder();
            contentMapping.ExecuteAction();
            // contentMapping.SaveCopyFileSettings(this._AppConfigurationService);
        }

        if (changedCopyProjectSetting) {
            this._Logger.CopyProjectSettingsChanged();
            appConfigurationService.SaveCopyProjectSettings(copyProjectSettings);
        }

        await Task.CompletedTask;
        return 0;
    }
}