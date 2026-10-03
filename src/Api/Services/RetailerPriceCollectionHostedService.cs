using Application.SeedWork.Interfaces;

namespace Api.Services;

public sealed class RetailerPriceCollectionHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<RetailerPriceCollectionHostedService> logger) : BackgroundService
{
    private const int ProcessingLaneCount = 4;
    private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan LeadershipRetryDelay = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan LeadershipRenewalInterval = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan LeadershipDuration = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var ownerId = Guid.NewGuid();
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (!await TryAcquireLeadershipAsync(ownerId, stoppingToken))
                    {
                        await Task.Delay(LeadershipRetryDelay, stoppingToken);
                        continue;
                    }

                    logger.LogInformation(
                        "Price-collection worker leadership acquired by {WorkerId}.",
                        ownerId);
                    using var leadershipSource = CancellationTokenSource.CreateLinkedTokenSource(
                        stoppingToken);
                    var lanes = Enumerable.Range(0, ProcessingLaneCount)
                        .Select(_ => RunLaneAsync(ownerId, leadershipSource.Token))
                        .ToArray();
                    try
                    {
                        while (!leadershipSource.IsCancellationRequested)
                        {
                            await Task.Delay(LeadershipRenewalInterval, leadershipSource.Token);
                            if (!await RenewLeadershipAsync(ownerId, leadershipSource.Token))
                            {
                                logger.LogWarning(
                                    "Price-collection worker {WorkerId} lost leadership.",
                                    ownerId);
                                leadershipSource.Cancel();
                            }
                        }
                    }
                    catch (OperationCanceledException) when (leadershipSource.IsCancellationRequested)
                    {
                    }
                    finally
                    {
                        leadershipSource.Cancel();
                        try
                        {
                            await Task.WhenAll(lanes);
                        }
                        catch (OperationCanceledException) when (leadershipSource.IsCancellationRequested)
                        {
                        }
                        await ReleaseLeadershipAsync(ownerId);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    logger.LogError(
                        exception,
                        "The price-collection worker cycle failed and will retry.");
                    await Task.Delay(LeadershipRetryDelay, stoppingToken);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private async Task RunLaneAsync(Guid ownerId, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider
                    .GetRequiredService<IRetailerPriceCollectionProcessor>();
                var processed = await processor.TryProcessNextAsync(ownerId, cancellationToken);
                if (!processed)
                {
                    await Task.Delay(IdleDelay, cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "A price-collection processing lane failed and will continue.");
                await Task.Delay(IdleDelay, cancellationToken);
            }
        }
    }

    private async Task<bool> TryAcquireLeadershipAsync(
        Guid ownerId,
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var processor = scope.ServiceProvider
            .GetRequiredService<IRetailerPriceCollectionProcessor>();
        return await processor.TryAcquireLeadershipAsync(
            ownerId,
            DateTimeOffset.UtcNow.Add(LeadershipDuration),
            cancellationToken);
    }

    private async Task<bool> RenewLeadershipAsync(
        Guid ownerId,
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var processor = scope.ServiceProvider
            .GetRequiredService<IRetailerPriceCollectionProcessor>();
        return await processor.RenewLeadershipAsync(
            ownerId,
            DateTimeOffset.UtcNow.Add(LeadershipDuration),
            cancellationToken);
    }

    private async Task ReleaseLeadershipAsync(Guid ownerId)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var processor = scope.ServiceProvider
                .GetRequiredService<IRetailerPriceCollectionProcessor>();
            await processor.ReleaseLeadershipAsync(ownerId, CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Price-collection worker {WorkerId} could not release leadership; the lease will expire.",
                ownerId);
        }
    }
}
