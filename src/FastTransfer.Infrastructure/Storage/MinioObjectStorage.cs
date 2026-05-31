using FastTransfer.Application.Abstractions.Storage;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace FastTransfer.Infrastructure.Storage;

public sealed class MinioObjectStorage(
    IMinioClient client,
    IOptions<MinioStorageOptions> options) : IObjectStorage
{
    private readonly MinioStorageOptions _options = options.Value;

    public async Task PutAsync(
        string objectKey,
        Stream content,
        long contentLength,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        await EnsureBucketExistsAsync(cancellationToken);

        var args = new PutObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectKey)
            .WithStreamData(content)
            .WithObjectSize(contentLength)
            .WithContentType(contentType);

        await client.PutObjectAsync(args, cancellationToken);
    }

    public async Task<Stream> GetAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        var output = new MemoryStream();
        var args = new GetObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectKey)
            .WithCallbackStream(stream => stream.CopyTo(output));

        await client.GetObjectAsync(args, cancellationToken);
        output.Position = 0;

        return output;
    }

    public async Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        var args = new RemoveObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectKey);

        await client.RemoveObjectAsync(args, cancellationToken);
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        var existsArgs = new BucketExistsArgs()
            .WithBucket(_options.BucketName);

        if (await client.BucketExistsAsync(existsArgs, cancellationToken))
        {
            return;
        }

        var makeArgs = new MakeBucketArgs()
            .WithBucket(_options.BucketName);

        await client.MakeBucketAsync(makeArgs, cancellationToken);
    }
}
