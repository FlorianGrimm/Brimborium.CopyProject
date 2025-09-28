
namespace Brimborium.CopyProject;

public class ExecutorShowConfig : Executor {
    public override async Task<int> RunAsync() {
        System.Console.Out.WriteLine("ExecutorShowConfig");
        await Task.CompletedTask;
        return 0;
    }
}