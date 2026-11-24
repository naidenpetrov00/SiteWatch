using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace Infrastructure.Cameras.Streaming;

public sealed class MediaMtxPathReconciliationState
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _pathLocks =
        new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, byte[]> _sourceFingerprints =
        new(StringComparer.Ordinal);

    public SemaphoreSlim GetPathLock(string path) =>
        _pathLocks.GetOrAdd(path, static _ => new SemaphoreSlim(1, 1));

    public bool MatchesFingerprint(string path, ReadOnlySpan<byte> fingerprint) =>
        _sourceFingerprints.TryGetValue(path, out var existing)
        && CryptographicOperations.FixedTimeEquals(existing, fingerprint);

    public void SetFingerprint(string path, ReadOnlySpan<byte> fingerprint) =>
        _sourceFingerprints[path] = fingerprint.ToArray();
}
