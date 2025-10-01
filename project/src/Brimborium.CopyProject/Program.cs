
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Brimborium.CopyProject;

public class Program {
    public static async Task<int> Main(string[] args) {
        try {
            return await new Program().RunAsync(args);
        } catch (System.Exception error) {
            System.Console.Error.WriteLine(error.ToString());
            return 1;
        }
    }

    public async Task<int> RunAsync(string[] args) {

        // parse args
        var (command, remainingArgs) = this.SplitAction(args);

        // create configuration
        var configuration = this.CreateConfigurationBuilder(remainingArgs, new ConfigurationBuilder()).Build();

        // create service provider
        var serviceBuilder = new ServiceCollection();
        this.ConfigureServiceBuilder(configuration, serviceBuilder);
        serviceBuilder.AddLogging((loggingBuilder) => this.ConfigureLogging(configuration, serviceBuilder, loggingBuilder));
        var serviceProvider = serviceBuilder.BuildServiceProvider();

        // create executor
        Executor executor = command switch {
            ApplicationCommand.Help => serviceProvider.GetRequiredService<ExecutorHelp>(),
            ApplicationCommand.ScanFolder => serviceProvider.GetRequiredService<ExecutorScanFolder>(),
            ApplicationCommand.Copy => serviceProvider.GetRequiredService<ExecutorCopy>(),
            ApplicationCommand.Update => serviceProvider.GetRequiredService<ExecutorUpdate>(),
            ApplicationCommand.Diff => serviceProvider.GetRequiredService<ExecutorDiff>(),
            _ => serviceProvider.GetRequiredService<ExecutorHelp>(),
        };
        // and run
        return await executor.RunAsync();
    }

    public virtual (ApplicationCommand command, string[] remainingArgs) SplitAction(string[] args) {
        var listArgs = args.ToList();
        ApplicationCommand command = ApplicationCommand.Help;
        if (listArgs.Count > 0) {
            var actionName = listArgs[0];
            command = actionName.ToLowerInvariant() switch {
                "scan" => ApplicationCommand.ScanFolder,
                "scanfolder" => ApplicationCommand.ScanFolder,
                "copy" => ApplicationCommand.Copy,
                "update" => ApplicationCommand.Update,
                "diff" => ApplicationCommand.Diff,
                "showconfig" => ApplicationCommand.ShowConfig,
                "help" => ApplicationCommand.Help,
                _ => ApplicationCommand.Unknown,
            };
            if (command == ApplicationCommand.Unknown) {
                System.Console.Error.WriteLine($"Unknown action: {actionName}");
                command = ApplicationCommand.Help;
            }
            listArgs.RemoveAt(0);
        } else {
            command = ApplicationCommand.Help;
        }
        var remainingArgs = listArgs.ToArray();
        return (command: command, remainingArgs: remainingArgs);
    }

    public virtual IConfigurationBuilder CreateConfigurationBuilder(string[] args, ConfigurationBuilder configurationBuilder)
    => configurationBuilder
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
        .AddCommandLine(args);

    public virtual void ConfigureServiceBuilder(
        IConfiguration configuration,
        ServiceCollection serviceBuilder) {
        serviceBuilder.AddSingleton<IConfiguration>(configuration);
        serviceBuilder.AddOptions<AppConfiguration>().BindConfiguration("");
        serviceBuilder.AddSingleton<ExecutorScanFolder>();
        serviceBuilder.AddSingleton<ExecutorCopy>();
        serviceBuilder.AddSingleton<ExecutorUpdate>();
        serviceBuilder.AddSingleton<ExecutorDiff>();
        serviceBuilder.AddSingleton<ExecutorHelp>();
        serviceBuilder.AddSingleton<AppConfigurationService>();
        serviceBuilder.AddSingleton<FileSettingService>();
        serviceBuilder.AddSingleton<FileSystemService>();
    }

    public virtual void ConfigureLogging(
        IConfigurationRoot configuration,
        IServiceCollection serviceBuilder,
        ILoggingBuilder loggingBuilder) {
        loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
        loggingBuilder.AddConsole();
    }
}
