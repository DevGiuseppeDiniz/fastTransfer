namespace FastTransfer.Application.Abstractions.Storage;

public interface IObjectStorage
{
    Task PutAsync(
        string objectKey,
        Stream content,
        long contentLength,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<Stream> GetAsync(string objectKey, CancellationToken cancellationToken = default);

    Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default);
}
