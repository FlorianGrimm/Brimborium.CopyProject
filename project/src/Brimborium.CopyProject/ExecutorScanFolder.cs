
namespace Brimborium.CopyProject;

public class ExecutorScanFolder : Executor {
    public override async Task<int> RunAsync() {
        System.Console.Out.WriteLine("ExecutorScanFolder");
        await Task.CompletedTask;
        return 0;
    }
}
