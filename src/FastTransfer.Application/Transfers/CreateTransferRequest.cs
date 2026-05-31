namespace FastTransfer.Application.Transfers;

public sealed record CreateTransferRequest(
    string? Title,
    int? ExpiresInDays,
    IReadOnlyList<CreateTransferFileRequest> Files);

public sealed record CreateTransferFileRequest(
    string FileName,
    string ContentType,
    long SizeBytes);
