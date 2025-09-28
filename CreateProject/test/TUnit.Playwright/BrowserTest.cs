#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace TUnit.Playwright;

public class BrowserTest : PlaywrightTest {
    public BrowserTest() : this(new BrowserTypeLaunchOptions()) {
    }

    public BrowserTest(BrowserTypeLaunchOptions options) {
        this._Options = options;
    }

    public IBrowser Browser { get; internal set; } = null!;

    private readonly List<IBrowserContext> _Contexts = [];
    private readonly BrowserTypeLaunchOptions _Options;

    protected virtual BrowserTypeLaunchOptions ConfigureBrowserTypeLaunchOptions(BrowserTypeLaunchOptions options) {
        return options;
    }

    public async Task<IBrowserContext> NewContext(BrowserNewContextOptions options) {
        var context = await this.Browser.NewContextAsync(options).ConfigureAwait(false);
        this._Contexts.Add(context);
        return context;
    }

    [Before(HookType.Test, "", 0)]
    public async Task BrowserSetup() {
        if (this.BrowserType == null) {
            throw new InvalidOperationException($"BrowserType is not initialized. This may indicate that {nameof(PlaywrightTest)}.{nameof(Playwright)} is not initialized or {nameof(PlaywrightTest)}.{nameof(PlaywrightSetup)} did not execute properly.");
        }

        var options = this.ConfigureBrowserTypeLaunchOptions(this._Options);
        var service = await BrowserService.Register(this, this.BrowserType, options).ConfigureAwait(false);
        this.Browser = service.Browser;
    }

    [After(HookType.Test, "", 0)]
    public async Task BrowserTearDown(TestContext testContext) {
        if (this.TestOk(testContext)) {
            foreach (var context in this._Contexts) {
                await context.CloseAsync().ConfigureAwait(false);
            }
        }
        this._Contexts.Clear();
        this.Browser = null!;
    }
}
