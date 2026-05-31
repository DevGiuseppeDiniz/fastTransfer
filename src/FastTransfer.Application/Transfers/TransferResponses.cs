using FastTransfer.Domain.Transfers;

namespace FastTransfer.Application.Transfers;

public sealed record CreateTransferResponse(
    string PublicId,
    DateTimeOffset ExpiresAt,
    IReadOnlyList<TransferFileResponse> Files)
{
    public static CreateTransferResponse FromTransfer(Transfer transfer)
    {
        return new CreateTransferResponse(
            transfer.PublicId,
            transfer.ExpiresAt,
            transfer.Files.Select(TransferFileResponse.FromTransferFile).ToArray());
    }
}

public sealed record TransferDetailsResponse(
    string PublicId,
    string? Title,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    string Status,
    IReadOnlyList<TransferFileResponse> Files)
{
    public static TransferDetailsResponse FromTransfer(Transfer transfer)
    {
        return new TransferDetailsResponse(
            transfer.PublicId,
            transfer.Title,
            transfer.CreatedAt,
            transfer.ExpiresAt,
            transfer.Status.ToString(),
            transfer.Files.Select(TransferFileResponse.FromTransferFile).ToArray());
    }
}

public sealed record TransferFileResponse(
    Guid Id,
    string FileName,
    string ObjectKey,
    string ContentType,
    long SizeBytes,
    string Status)
{
    public static TransferFileResponse FromTransferFile(TransferFile file)
    {
        return new TransferFileResponse(
            file.Id,
            file.FileName,
            file.ObjectKey,
            file.ContentType,
            file.SizeBytes,
            file.Status.ToString());
    }
}
