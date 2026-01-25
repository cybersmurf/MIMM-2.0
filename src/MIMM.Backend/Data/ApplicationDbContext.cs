using Microsoft.EntityFrameworkCore;
using MIMM.Shared.Entities;

namespace MIMM.Backend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<JournalEntry> Entries { get; set; } = null!;
    public DbSet<LastFmToken> LastFmTokens { get; set; } = null!;
    public DbSet<MusicBrainzArtist> MusicBrainzArtists { get; set; } = null!;
    public DbSet<MusicBrainzRelease> MusicBrainzReleases { get; set; } = null!;
    public DbSet<MusicBrainzRecording> MusicBrainzRecordings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(100);
            entity.Property(e => e.Language).HasMaxLength(5).HasDefaultValue("en");
            entity.Property(e => e.TimeZone).HasMaxLength(50);
            
            // Soft delete query filter
            entity.HasQueryFilter(e => e.DeletedAt == null);
            
            // Relationships
            entity.HasMany(e => e.Entries)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.LastFmToken)
                .WithOne(t => t.User)
                .HasForeignKey<LastFmToken>(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // JournalEntry configuration
        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SongTitle).HasMaxLength(255).IsRequired();
            entity.Property(e => e.ArtistName).HasMaxLength(255).IsRequired(false);
            entity.Property(e => e.AlbumName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.SongId).HasMaxLength(100);
            entity.Property(e => e.CoverUrl).HasMaxLength(500);
            entity.Property(e => e.Source).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(2000);
            
            // Indexes for common queries
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.HasIndex(e => e.Source);
            
            // Array column for somatic tags (PostgreSQL specific)
            entity.Property(e => e.SomaticTags)
                .HasColumnType("text[]")
                .IsRequired();
        });

        // LastFmToken configuration
        modelBuilder.Entity<LastFmToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionKey).HasMaxLength(255).IsRequired();
            entity.Property(e => e.LastFmUsername).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // MusicBrainz artist cache
        modelBuilder.Entity<MusicBrainzArtist>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(36);
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
        });

        // MusicBrainz release cache
        modelBuilder.Entity<MusicBrainzRelease>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(36);
            entity.Property(e => e.Title).HasMaxLength(255).IsRequired();
            entity.Property(e => e.ReleaseDate).HasMaxLength(100);
            entity.Property(e => e.CoverArtUrl).HasMaxLength(500);
        });

        // MusicBrainz recording cache
        modelBuilder.Entity<MusicBrainzRecording>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(36);
            entity.Property(e => e.Title).HasMaxLength(255).IsRequired();
            entity.Property(e => e.ArtistId).HasMaxLength(36);
            entity.Property(e => e.ArtistName).HasMaxLength(255);
            entity.Property(e => e.ReleaseId).HasMaxLength(36);
            entity.Property(e => e.ReleaseTitle).HasMaxLength(255);
            entity.Property(e => e.CoverUrl).HasMaxLength(500);
            entity.HasIndex(e => e.ArtistId);
            entity.HasIndex(e => e.ReleaseId);
        });
    }
}
