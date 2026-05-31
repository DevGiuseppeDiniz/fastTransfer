namespace FastTransfer.Domain.Transfers;

public sealed class TransferFile
{
    private TransferFile()
    {
    }

    private TransferFile(
        Guid transferId,
        string fileName,
        string objectKey,
        string contentType,
        long sizeBytes,
        DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name is required.", nameof(fileName));
        }

        if (string.IsNullOrWhiteSpace(objectKey))
        {
            throw new ArgumentException("Object key is required.", nameof(objectKey));
        }

        if (sizeBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sizeBytes), "File size must be greater than zero.");
        }

        Id = Guid.NewGuid();
        TransferId = transferId;
        FileName = Path.GetFileName(fileName);
        ObjectKey = objectKey;
        ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType.Trim();
        SizeBytes = sizeBytes;
        CreatedAt = createdAt;
        Status = TransferFileStatus.Pending;
    }

    public Guid Id { get; private set; }

    public Guid TransferId { get; private set; }

    public string FileName { get; private set; } = string.Empty;

    public string ObjectKey { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = "application/octet-stream";

    public long SizeBytes { get; private set; }

    public string? ChecksumSha256 { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UploadedAt { get; private set; }

    public TransferFileStatus Status { get; private set; }

    public static TransferFile Create(
        Guid transferId,
        string fileName,
        string objectKey,
        string contentType,
        long sizeBytes,
        DateTimeOffset createdAt)
    {
        return new TransferFile(transferId, fileName, objectKey, contentType, sizeBytes, createdAt);
    }

    public void MarkUploaded(DateTimeOffset uploadedAt, string? checksumSha256)
    {
        UploadedAt = uploadedAt;
        ChecksumSha256 = string.IsNullOrWhiteSpace(checksumSha256) ? null : checksumSha256.Trim();
        Status = TransferFileStatus.Uploaded;
    }

    public void MarkFailed()
    {
        Status = TransferFileStatus.Failed;
    }
}
