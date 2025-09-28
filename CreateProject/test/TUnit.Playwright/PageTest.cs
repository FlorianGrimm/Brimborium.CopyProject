#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace TUnit.Playwright;

public class PageTest : ContextTest {
    public PageTest() {
    }

    public PageTest(BrowserTypeLaunchOptions options) : base(options) {
    }

    public IPage Page { get; private set; } = null!;

    [Before(HookType.Test, "", 0)]
    public async Task PageSetup() {
        if (this.Context == null) {
            throw new InvalidOperationException($"Browser context is not initialized. This may indicate that {nameof(ContextTest)}.{nameof(ContextSetup)} did not execute properly.");
        }

        this.Page = await this.Context.NewPageAsync().ConfigureAwait(false);
    }
}
