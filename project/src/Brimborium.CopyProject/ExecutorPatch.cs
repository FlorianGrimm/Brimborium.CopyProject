
namespace Brimborium.CopyProject;

public class ExecutorPatch : Executor {
    public override async Task<int> RunAsync() {
        System.Console.Out.WriteLine("ExecutorPatch");
        await Task.CompletedTask;
        return 0;
    }
}