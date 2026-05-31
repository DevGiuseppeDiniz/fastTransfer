using FastTransfer.Application.Abstractions.Storage;
using FastTransfer.Infrastructure.Persistence;
using FastTransfer.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace FastTransfer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFastTransferInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is required.");
        }

        services.AddDbContext<FastTransferDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddOptions<MinioStorageOptions>()
            .Bind(configuration.GetSection(MinioStorageOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Endpoint), "MinIO endpoint is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.AccessKey), "MinIO access key is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.SecretKey), "MinIO secret key is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.BucketName), "MinIO bucket name is required.")
            .ValidateOnStart();

        services.AddSingleton<IMinioClient>(serviceProvider =>
        {
            var options = serviceProvider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<MinioStorageOptions>>()
                .Value;

            var minioClient = new MinioClient()
                .WithEndpoint(options.Endpoint)
                .WithCredentials(options.AccessKey, options.SecretKey);

            if (options.UseSsl)
            {
                minioClient = minioClient.WithSSL();
            }

            return minioClient.Build();
        });

        services.AddScoped<IObjectStorage, MinioObjectStorage>();

        return services;
    }
}
