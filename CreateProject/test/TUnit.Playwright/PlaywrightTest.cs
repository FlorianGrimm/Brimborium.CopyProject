#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace TUnit.Playwright;

public class PlaywrightTest : WorkerAwareTest {
    public virtual string BrowserName { get; } = Microsoft.Playwright.BrowserType.Chromium;
    public IBrowserType BrowserType => Playwright[this.BrowserName];

    private static readonly Task<IPlaywright> _PlaywrightTask = Microsoft.Playwright.Playwright.CreateAsync();

    public static IPlaywright Playwright { get; private set; } = null!;

    [Before(HookType.TestSession, "", 0)]
    public static async Task PlaywrightSetup() {
        Playwright = await _PlaywrightTask.ConfigureAwait(false);
        Playwright.Selectors.SetTestIdAttribute("data-testid");
    }

    [After(HookType.TestSession, "", 0)]
    public static void PlaywrightCleanup() {
        Playwright?.Dispose();
    }

    public static void SetDefaultExpectTimeout(float timeout) => Microsoft.Playwright.Assertions.SetDefaultExpectTimeout(timeout);

#pragma warning disable CA1822 // Mark members as static
    public ILocatorAssertions Expect(ILocator locator) => Microsoft.Playwright.Assertions.Expect(locator);

    public IPageAssertions Expect(IPage page) => Microsoft.Playwright.Assertions.Expect(page);

    public IAPIResponseAssertions Expect(IAPIResponse response) => Microsoft.Playwright.Assertions.Expect(response);
#pragma warning restore CA1822 // Mark members as static
}
