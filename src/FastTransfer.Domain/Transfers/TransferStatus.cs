namespace FastTransfer.Domain.Transfers;

public enum TransferStatus
{
    Draft = 0,
    Uploading = 1,
    Ready = 2,
    Expired = 3,
    Deleted = 4,
    Failed = 5
}
