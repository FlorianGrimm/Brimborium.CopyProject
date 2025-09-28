[assembly: NotInParallel]

namespace Brimborium.CreateProject.Tests.TestUtility;

public partial class TestPrepares {
    [Test]
    public Task TestVerifyChecksRun() => VerifyChecks.Run();
}