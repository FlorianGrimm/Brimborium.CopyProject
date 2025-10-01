namespace Brimborium.CopyProject;

public class ExecutorHelp : Executor {
    public override Task<int> RunAsync() {
        System.Console.WriteLine("Brimborium.CopyProject");
        System.Console.WriteLine("Arguments: Action [options]");
        System.Console.WriteLine("Actions:");
        System.Console.WriteLine("  ScanFolder - scan the source folder and create configuration file.");
        System.Console.WriteLine("  Copy - copy the files as specified in the configuration file.");
        System.Console.WriteLine("  Update - update the configuration file.");
        System.Console.WriteLine("  Diff - show the differences between the source and the target.");
        System.Console.WriteLine("  Patch - patch the target.");
        System.Console.WriteLine("");
        System.Console.WriteLine("Options:");
        System.Console.WriteLine("  RootMarkerFile");
        System.Console.WriteLine("  RootFolder");
        System.Console.WriteLine("  SettingsFile");
        System.Console.WriteLine("  SettingsFolder");
        return Task.FromResult(0);
    }
}
