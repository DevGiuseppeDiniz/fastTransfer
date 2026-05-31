using FastTransfer.Domain.Transfers;

namespace FastTransfer.Tests.Transfers;

public sealed class TransferTests
{
    [Fact]
    public void CreateGeneratesPublicIdAndAppliesExpiration()
    {
        var createdAt = DateTimeOffset.Parse("2026-05-31T12:00:00Z");
        var expiresAt = createdAt.AddDays(7);

        var transfer = Transfer.Create("Sprint files", ownerId: null, createdAt, expiresAt);

        Assert.False(string.IsNullOrWhiteSpace(transfer.PublicId));
        Assert.Equal("Sprint files", transfer.Title);
        Assert.Equal(expiresAt, transfer.ExpiresAt);
        Assert.Equal(TransferStatus.Draft, transfer.Status);
    }

    [Fact]
    public void AddFileSanitizesFileNameAndMarksTransferAsUploading()
    {
        var createdAt = DateTimeOffset.Parse("2026-05-31T12:00:00Z");
        var transfer = Transfer.Create("Build", ownerId: null, createdAt, createdAt.AddDays(7));

        var file = transfer.AddFile(
            @"C:\temp\archive.zip",
            "transfers/public-id/archive.zip",
            "application/zip",
            1024,
            createdAt);

        Assert.Equal("archive.zip", file.FileName);
        Assert.Equal(TransferStatus.Uploading, transfer.Status);
        Assert.Single(transfer.Files);
    }
}
