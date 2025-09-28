[assembly: NotInParallel]

namespace Brimborium.CreateProject.Test.TestUtility;

public partial class TestPrepares {
    [Test, Explicit]
    public async Task ExplicitInstallPlaywright() {
        Microsoft.Playwright.Program.Main(["install"]);
        await Task.CompletedTask;
    }

    [Test]
    public Task TestVerifyChecksRun() => VerifyChecks.Run();
}