# FastTransfer

FastTransfer is a modern file-transfer platform built with .NET, ASP.NET Core, PostgreSQL, SignalR, and S3-compatible object storage.

## Architecture

- `FastTransfer.Api`: ASP.NET Core Web API, JWT authentication, SignalR, public endpoints.
- `FastTransfer.Application`: use-case contracts, DTOs, storage abstractions.
- `FastTransfer.Domain`: transfer aggregate, files, upload sessions, download events.
- `FastTransfer.Infrastructure`: EF Core, PostgreSQL, MinIO/S3-compatible storage.
- `FastTransfer.Worker`: background jobs for expiration and cleanup.
- `FastTransfer.Tests`: domain and application tests.

## Local Services

Start PostgreSQL, MinIO, and Redis:

```powershell
docker compose up -d
```

MinIO console:

```text
http://localhost:9001
user: fasttransfer
password: fasttransfer_dev
```

## Development

Restore and build:

```powershell
dotnet restore
dotnet build
```

Apply database migrations:

```powershell
dotnet ef database update --project src/FastTransfer.Infrastructure --startup-project src/FastTransfer.Api
```

Run the API:

```powershell
dotnet run --project src/FastTransfer.Api
```

Health check:

```text
http://localhost:5011/health
```

Create a transfer draft:

```http
POST /api/transfers
Content-Type: application/json

{
  "title": "Project files",
  "expiresInDays": 7,
  "files": [
    {
      "fileName": "archive.zip",
      "contentType": "application/zip",
      "sizeBytes": 1024
    }
  ]
}
```

## Next Milestones

- Implement database bootstrap for local development.
- Implement streaming upload endpoint.
- Add presigned object-storage upload URLs.
- Emit SignalR upload progress events.
- Add Next.js frontend.
- Add GitHub Actions build and test pipeline.
