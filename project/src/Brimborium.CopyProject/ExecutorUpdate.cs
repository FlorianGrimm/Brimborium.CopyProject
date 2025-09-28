
namespace Brimborium.CopyProject;

public class ExecutorUpdate : Executor {
    public override async Task<int> RunAsync() {
        System.Console.Out.WriteLine("ExecutorUpdate");
        await Task.CompletedTask;
        return 0;
    }
}