namespace Brimborium.CreateProject.Test.TestUtility;

public class AppPageTest : PageTest {
    [ClassDataSource<WebApplicationFactoryIntegration>(Shared = SharedType.PerTestSession)]
    public required WebApplicationFactoryIntegration WebApplicationFactory { get; init; }

    protected override BrowserTypeLaunchOptions ConfigureBrowserTypeLaunchOptions(BrowserTypeLaunchOptions options) {
#if false
        if (System.Diagnostics.Debugger.IsAttached) {
#pragma warning disable CS0612 // Type or member is obsolete
            return new BrowserTypeLaunchOptions(options) {
                Devtools = true,
                Headless = false,
            };
#pragma warning restore CS0612 // Type or member is obsolete
        }
#endif
#if true
        if (System.Diagnostics.Debugger.IsAttached) {
            // System.Environment.SetEnvironmentVariable("PWDBG", "1");
            return new BrowserTypeLaunchOptions(options) {
                Headless = false,
            };
        }
#endif
        return options;
    }
}
