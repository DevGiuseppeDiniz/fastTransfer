namespace FastTransfer.Infrastructure.Storage;

public sealed class MinioStorageOptions
{
    public const string SectionName = "Storage:Minio";

    public string Endpoint { get; init; } = "localhost:9000";

    public string AccessKey { get; init; } = "fasttransfer";

    public string SecretKey { get; init; } = "fasttransfer_dev";

    public string BucketName { get; init; } = "fasttransfer";

    public bool UseSsl { get; init; }
}
