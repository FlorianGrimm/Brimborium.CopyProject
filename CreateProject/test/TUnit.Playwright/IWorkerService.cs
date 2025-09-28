#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace TUnit.Playwright;

public interface IWorkerService {
    public Task ResetAsync();
    public Task DisposeAsync();
}
