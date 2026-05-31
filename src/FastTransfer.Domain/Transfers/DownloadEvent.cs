namespace FastTransfer.Domain.Transfers;

public sealed class DownloadEvent
{
    private DownloadEvent()
    {
    }

    private DownloadEvent(Guid transferId, string? ipAddress, string? userAgent, DateTimeOffset downloadedAt)
    {
        Id = Guid.NewGuid();
        TransferId = transferId;
        IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress.Trim();
        UserAgent = string.IsNullOrWhiteSpace(userAgent) ? null : userAgent.Trim();
        DownloadedAt = downloadedAt;
    }

    public Guid Id { get; private set; }

    public Guid TransferId { get; private set; }

    public string? IpAddress { get; private set; }

    public string? UserAgent { get; private set; }

    public DateTimeOffset DownloadedAt { get; private set; }

    public static DownloadEvent Create(Guid transferId, string? ipAddress, string? userAgent, DateTimeOffset downloadedAt)
    {
        return new DownloadEvent(transferId, ipAddress, userAgent, downloadedAt);
    }
}
