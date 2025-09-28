#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace TUnit.Playwright;

public class ContextTest : BrowserTest {
    public ContextTest() {
    }

    public ContextTest(BrowserTypeLaunchOptions options) : base(options) {
    }

    public IBrowserContext Context { get; private set; } = null!;

    public virtual BrowserNewContextOptions ContextOptions(TestContext testContext) {
        return new() {
            Locale = "en-US",
            ColorScheme = ColorScheme.Light,
        };
    }

    [Before(HookType.Test, "", 0)]
    public async Task ContextSetup(TestContext testContext) {
        if (this.Browser == null) {
            throw new InvalidOperationException($"Browser is not initialized. This may indicate that {nameof(BrowserTest)}.{nameof(BrowserSetup)} did not execute properly.");
        }

        this.Context = await this.NewContext(this.ContextOptions(testContext)).ConfigureAwait(false);
    }
}
