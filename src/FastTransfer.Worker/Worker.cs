namespace FastTransfer.Worker;

using FastTransfer.Domain.Transfers;
using FastTransfer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public sealed class Worker(
    ILogger<Worker> logger,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(15));

        logger.LogInformation("FastTransfer worker started.");

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await MarkExpiredTransfersAsync(stoppingToken);
        }
    }

    private async Task MarkExpiredTransfersAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FastTransferDbContext>();
        var now = DateTimeOffset.UtcNow;

        var updatedTransfers = await dbContext.Transfers
            .Where(transfer =>
                transfer.ExpiresAt <= now &&
                transfer.Status != TransferStatus.Expired &&
                transfer.Status != TransferStatus.Deleted)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(transfer => transfer.Status, TransferStatus.Expired), cancellationToken);

        if (updatedTransfers > 0)
        {
            logger.LogInformation("Marked {Count} expired transfers.", updatedTransfers);
        }
    }
}
