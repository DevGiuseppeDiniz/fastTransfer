using System.Text;
using FastTransfer.Api.Hubs;
using FastTransfer.Application.Transfers;
using FastTransfer.Domain.Transfers;
using FastTransfer.Infrastructure;
using FastTransfer.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddFastTransferInfrastructure(builder.Configuration);

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:3000"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var signalRBuilder = builder.Services.AddSignalR();
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");

if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    signalRBuilder.AddStackExchangeRedis(redisConnectionString);
}

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? new JwtOptions();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = !string.IsNullOrWhiteSpace(jwtOptions.Audience),
            ValidAudience = jwtOptions.Audience,
            ValidateIssuer = !string.IsNullOrWhiteSpace(jwtOptions.Issuer),
            ValidIssuer = jwtOptions.Issuer,
            ValidateIssuerSigningKey = !string.IsNullOrWhiteSpace(jwtOptions.SigningKey),
            IssuerSigningKey = string.IsNullOrWhiteSpace(jwtOptions.SigningKey)
                ? null
                : new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapHub<UploadProgressHub>("/hubs/upload-progress");

app.MapGet("/", () => Results.Ok(new
{
    service = "FastTransfer API",
    status = "running"
}));

var transfers = app.MapGroup("/api/transfers")
    .WithTags("Transfers");

transfers.MapPost("/", async (
    CreateTransferRequest request,
    FastTransferDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var validationErrors = ValidateCreateTransferRequest(request);

    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    var createdAt = DateTimeOffset.UtcNow;
    var expiresInDays = Math.Clamp(request.ExpiresInDays ?? 7, 1, 30);
    var transfer = Transfer.Create(
        request.Title,
        ownerId: null,
        createdAt,
        createdAt.AddDays(expiresInDays));

    foreach (var file in request.Files)
    {
        transfer.AddFile(
            file.FileName,
            BuildObjectKey(transfer.PublicId, file.FileName),
            NormalizeContentType(file.ContentType),
            file.SizeBytes,
            createdAt);
    }

    dbContext.Transfers.Add(transfer);
    await dbContext.SaveChangesAsync(cancellationToken);

    return Results.Created(
        $"/api/transfers/{transfer.PublicId}",
        CreateTransferResponse.FromTransfer(transfer));
});

transfers.MapGet("/{publicId}", async (
    string publicId,
    FastTransferDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var transfer = await dbContext.Transfers
        .Include(item => item.Files)
        .FirstOrDefaultAsync(item => item.PublicId == publicId, cancellationToken);

    return transfer is null
        ? Results.NotFound()
        : Results.Ok(TransferDetailsResponse.FromTransfer(transfer));
});

app.Run();

static Dictionary<string, string[]> ValidateCreateTransferRequest(CreateTransferRequest request)
{
    var errors = new Dictionary<string, string[]>();

    if (request.Files is null || request.Files.Count == 0)
    {
        errors["files"] = ["At least one file is required."];
        return errors;
    }

    var invalidFiles = request.Files
        .Select((file, index) => new { File = file, Index = index })
        .Where(item => string.IsNullOrWhiteSpace(item.File.FileName) || item.File.SizeBytes <= 0)
        .Select(item => $"files[{item.Index}]")
        .ToArray();

    if (invalidFiles.Length > 0)
    {
        errors["files"] = invalidFiles;
    }

    return errors;
}

static string BuildObjectKey(string transferPublicId, string fileName)
{
    var safeFileName = Path.GetFileName(fileName);

    return $"transfers/{transferPublicId}/{Guid.NewGuid():N}/{safeFileName}";
}

static string NormalizeContentType(string contentType)
{
    return string.IsNullOrWhiteSpace(contentType)
        ? "application/octet-stream"
        : contentType.Trim();
}

internal sealed class JwtOptions
{
    public const string SectionName = "Authentication:Jwt";

    public string? Issuer { get; init; }

    public string? Audience { get; init; }

    public string? SigningKey { get; init; }
}
