using System.Security.Cryptography;

namespace FastTransfer.Domain.Transfers;

public sealed class Transfer
{
    private readonly List<TransferFile> _files = [];

    private Transfer()
    {
    }

    private Transfer(string? title, string? ownerId, DateTimeOffset createdAt, DateTimeOffset expiresAt)
    {
        if (expiresAt <= createdAt)
        {
            throw new ArgumentException("Transfer expiration must be after creation.", nameof(expiresAt));
        }

        Id = Guid.NewGuid();
        PublicId = CreatePublicId();
        Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim();
        OwnerId = string.IsNullOrWhiteSpace(ownerId) ? null : ownerId.Trim();
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        Status = TransferStatus.Draft;
    }

    public Guid Id { get; private set; }

    public string PublicId { get; private set; } = string.Empty;

    public string? Title { get; private set; }

    public string? OwnerId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? ReadyAt { get; private set; }

    public TransferStatus Status { get; private set; }

    public IReadOnlyCollection<TransferFile> Files => _files.AsReadOnly();

    public static Transfer Create(string? title, string? ownerId, DateTimeOffset createdAt, DateTimeOffset expiresAt)
    {
        return new Transfer(title, ownerId, createdAt, expiresAt);
    }

    public TransferFile AddFile(
        string fileName,
        string objectKey,
        string contentType,
        long sizeBytes,
        DateTimeOffset createdAt)
    {
        var file = TransferFile.Create(Id, fileName, objectKey, contentType, sizeBytes, createdAt);
        _files.Add(file);
        Status = TransferStatus.Uploading;

        return file;
    }

    public void MarkReady(DateTimeOffset readyAt)
    {
        if (_files.Count == 0)
        {
            throw new InvalidOperationException("A transfer without files cannot be marked as ready.");
        }

        ReadyAt = readyAt;
        Status = TransferStatus.Ready;
    }

    public void MarkExpired(DateTimeOffset now)
    {
        if (now < ExpiresAt)
        {
            throw new InvalidOperationException("A transfer cannot expire before its expiration date.");
        }

        Status = TransferStatus.Expired;
    }

    private static string CreatePublicId()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(12)).ToLowerInvariant();
    }
}
