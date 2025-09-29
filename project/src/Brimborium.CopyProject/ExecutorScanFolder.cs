
namespace Brimborium.CopyProject;

public class ExecutorScanFolder : Executor {
    private readonly AppConfigurationService _AppConfigurationService;

    public ExecutorScanFolder(AppConfigurationService appConfigurationService) {
        this._AppConfigurationService = appConfigurationService;
    }
    public override async Task<int> RunAsync() {
        System.Console.Out.WriteLine("ExecutorScanFolder");
        var rootFolder = this._AppConfigurationService.GetRootFolder();
        System.Console.Out.WriteLine(rootFolder);
        await Task.CompletedTask;
        return 0;
    }
}
