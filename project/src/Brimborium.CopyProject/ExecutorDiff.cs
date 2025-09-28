
namespace Brimborium.CopyProject;

public class ExecutorDiff : Executor {
    public override async Task<int> RunAsync() {
        System.Console.Out.WriteLine("ExecutorDiff");
        await Task.CompletedTask;
        return 0;
    }
}
