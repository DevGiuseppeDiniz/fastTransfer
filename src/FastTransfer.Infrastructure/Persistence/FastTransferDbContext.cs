using FastTransfer.Domain.Transfers;
using Microsoft.EntityFrameworkCore;

namespace FastTransfer.Infrastructure.Persistence;

public sealed class FastTransferDbContext(DbContextOptions<FastTransferDbContext> options) : DbContext(options)
{
    public DbSet<Transfer> Transfers => Set<Transfer>();

    public DbSet<TransferFile> TransferFiles => Set<TransferFile>();

    public DbSet<UploadSession> UploadSessions => Set<UploadSession>();

    public DbSet<DownloadEvent> DownloadEvents => Set<DownloadEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transfer>(entity =>
        {
            entity.ToTable("transfers");

            entity.HasKey(transfer => transfer.Id);

            entity.Property(transfer => transfer.PublicId)
                .HasMaxLength(32)
                .IsRequired();

            entity.HasIndex(transfer => transfer.PublicId)
                .IsUnique();

            entity.Property(transfer => transfer.Title)
                .HasMaxLength(180);

            entity.Property(transfer => transfer.OwnerId)
                .HasMaxLength(128);

            entity.Property(transfer => transfer.Status)
                .HasConversion<string>()
                .HasMaxLength(32);

            entity.HasMany(transfer => transfer.Files)
                .WithOne()
                .HasForeignKey(file => file.TransferId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Navigation(transfer => transfer.Files)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<TransferFile>(entity =>
        {
            entity.ToTable("transfer_files");

            entity.HasKey(file => file.Id);

            entity.Property(file => file.FileName)
                .HasMaxLength(512)
                .IsRequired();

            entity.Property(file => file.ObjectKey)
                .HasMaxLength(1024)
                .IsRequired();

            entity.HasIndex(file => file.ObjectKey)
                .IsUnique();

            entity.Property(file => file.ContentType)
                .HasMaxLength(160)
                .IsRequired();

            entity.Property(file => file.ChecksumSha256)
                .HasMaxLength(64);

            entity.Property(file => file.Status)
                .HasConversion<string>()
                .HasMaxLength(32);
        });

        modelBuilder.Entity<UploadSession>(entity =>
        {
            entity.ToTable("upload_sessions");

            entity.HasKey(session => session.Id);

            entity.Property(session => session.Protocol)
                .HasMaxLength(64)
                .IsRequired();

            entity.HasIndex(session => new { session.TransferId, session.TransferFileId });
        });

        modelBuilder.Entity<DownloadEvent>(entity =>
        {
            entity.ToTable("download_events");

            entity.HasKey(download => download.Id);

            entity.Property(download => download.IpAddress)
                .HasMaxLength(64);

            entity.Property(download => download.UserAgent)
                .HasMaxLength(512);

            entity.HasIndex(download => download.TransferId);
        });
    }
}
