using System.Collections.Concurrent;

namespace Infrastructure.RetailerExtractionProfiles.Services;

public sealed class RetailerExtractionHostGate
{
    private static readonly TimeSpan MinimumInterval = TimeSpan.FromSeconds(2);
    private readonly ConcurrentDictionary<string, HostState> states =
        new(StringComparer.OrdinalIgnoreCase);

    public async ValueTask<IAsyncDisposable> AcquireAsync(
        string host,
        CancellationToken cancellationToken)
    {
        var state = states.GetOrAdd(host, _ => new HostState());
        await state.Semaphore.WaitAsync(cancellationToken);
        try
        {
            var delay = state.LastStartedAt + MinimumInterval - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken);
            }
            state.LastStartedAt = DateTimeOffset.UtcNow;
            return new HostLease(state.Semaphore);
        }
        catch
        {
            state.Semaphore.Release();
            throw;
        }
    }

    private sealed class HostState
    {
        public SemaphoreSlim Semaphore { get; } = new(1, 1);
        public DateTimeOffset LastStartedAt { get; set; } = DateTimeOffset.MinValue;
    }

    private sealed class HostLease(SemaphoreSlim semaphore) : IAsyncDisposable
    {
        private bool disposed;

        public ValueTask DisposeAsync()
        {
            if (!disposed)
            {
                disposed = true;
                semaphore.Release();
            }
            return ValueTask.CompletedTask;
        }
    }
}
