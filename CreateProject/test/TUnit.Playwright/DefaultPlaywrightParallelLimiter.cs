#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace TUnit.Playwright;

public sealed class DefaultPlaywrightParallelLimiter : IParallelLimit {
    private static readonly int _StaticallyInitializedLimit = GetLimit();

    public int Limit => _StaticallyInitializedLimit;

    private static int GetLimit() {
        var limit = Math.Max(Environment.ProcessorCount, 2);

        Console.WriteLine(@$"Default playwright parallel limiter set to {limit}");

        return limit;
    }
}
