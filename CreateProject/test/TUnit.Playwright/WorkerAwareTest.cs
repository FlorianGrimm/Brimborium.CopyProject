#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace TUnit.Playwright;

public class WorkerAwareTest : ITestRegisteredEventReceiver {
    private class Worker {
        private static int _LastWorkedIndex = 0;
        public readonly int WorkerIndex = Interlocked.Increment(ref _LastWorkedIndex);
        public readonly Dictionary<string, IWorkerService> Services = [];
    }

    public virtual bool UseDefaultParallelLimiter => true;

    private static readonly ConcurrentStack<Worker> _AllWorkers = [];
    private Worker? _CurrentWorker;

    public int WorkerIndex { get; internal set; }

    public async Task<T> RegisterService<T>(string name, Func<Task<T>> factory) where T : class, IWorkerService {
        Worker currentWorker = (this._CurrentWorker ??= new());
        if (!currentWorker.Services.ContainsKey(name)) {
            currentWorker.Services[name] = await factory().ConfigureAwait(false);
        }

        return (currentWorker.Services[name] as T)!;
    }

    [Before(HookType.Test, "", 0)]
    public void WorkerSetup() {
        if (!_AllWorkers.TryPop(out this._CurrentWorker)) {
            this._CurrentWorker = new();
        }

        this.WorkerIndex = this._CurrentWorker.WorkerIndex;
    }

    [After(HookType.Test, "", 0)]
    public async Task WorkerTeardown(TestContext testContext) {
        if (this.TestOk(testContext)) {
            if (this._CurrentWorker is { } currentWorker) {
                foreach (var kv in currentWorker.Services) {
                    await kv.Value.ResetAsync().ConfigureAwait(false);
                }
                _AllWorkers.Push(currentWorker);
            }
        } else {
            if (this._CurrentWorker is { } currentWorker) {
                foreach (var kv in currentWorker.Services) {
                    await kv.Value.DisposeAsync().ConfigureAwait(false);
                }
                currentWorker.Services.Clear();
            }
        }
    }

    protected bool TestOk(TestContext testContext) {
        return testContext.Result?.State is TestState.Passed or TestState.Skipped;
    }

    public ValueTask OnTestRegistered(TestRegisteredContext context) {
        if (this.UseDefaultParallelLimiter) {
            context.SetParallelLimiter(new DefaultPlaywrightParallelLimiter());
        }

        return default(ValueTask);
    }

    int IEventReceiver.Order => 0;
}
