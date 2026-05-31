namespace FastTransfer.Domain.Transfers;

public sealed class UploadSession
{
    private UploadSession()
    {
    }

    private UploadSession(Guid transferId, Guid transferFileId, string protocol, DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        TransferId = transferId;
        TransferFileId = transferFileId;
        Protocol = protocol;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Guid TransferId { get; private set; }

    public Guid TransferFileId { get; private set; }

    public string Protocol { get; private set; } = string.Empty;

    public long BytesReceived { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public static UploadSession Create(Guid transferId, Guid transferFileId, string protocol, DateTimeOffset createdAt)
    {
        return new UploadSession(transferId, transferFileId, protocol, createdAt);
    }

    public void ReportProgress(long bytesReceived)
    {
        if (bytesReceived < BytesReceived)
        {
            return;
        }

        BytesReceived = bytesReceived;
    }

    public void Complete(DateTimeOffset completedAt)
    {
        CompletedAt = completedAt;
    }
}
