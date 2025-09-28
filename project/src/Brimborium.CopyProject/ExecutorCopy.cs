
namespace Brimborium.CopyProject;

public class ExecutorCopy : Executor {
    public override async Task<int> RunAsync() {
        System.Console.Out.WriteLine("ExecutorCopy");
        await Task.CompletedTask;
        return 0;
    }
}