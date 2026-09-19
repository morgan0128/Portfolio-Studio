using Microsoft.EntityFrameworkCore;
using WebApplication6.Backend.Models;

namespace WebApplication6.Backend.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    
    public DbSet<Image> Images => Set<Image>();
    
    public DbSet<Photo> Photos => Set<Photo>();
    
    public DbSet<Album> Albums => Set<Album>();

    public DbSet<AlbumPhoto> AlbumPhotos => Set<AlbumPhoto>();

    public DbSet<AlbumItem> AlbumItems => Set<AlbumItem>();

    public DbSet<AlbumPhotoDisplayItem> AlbumPhotoDisplayItems => Set<AlbumPhotoDisplayItem>();

    public DbSet<UntrackedFile> UntrackedFiles => Set<UntrackedFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AlbumItem>(albumItem =>
        {
            albumItem.ToTable("AlbumItems",
                table =>
                {
                    table.HasCheckConstraint("CK_AlbumItems_Order_NonNegative", "\"Order\" >= 0");
                    table.HasCheckConstraint("CK_AlbumItems_PhotoDisplayMode_Range",
                        "\"ItemType\" <> 'PhotoDisplay' OR (\"DisplayMode\" IS NOT NULL AND \"DisplayMode\" BETWEEN 0 AND 1)");
                });

            albumItem.HasDiscriminator<string>("ItemType")
                .HasValue<AlbumPhotoDisplayItem>("PhotoDisplay");
            albumItem.Property<string>("ItemType").HasMaxLength(32);
            albumItem.HasAlternateKey(nameof(AlbumItem.AlbumId), nameof(AlbumItem.Id), "ItemType");

            albumItem.HasOne(ai => ai.Album)
                .WithMany(a => a.AlbumItems)
                .HasForeignKey(ai => ai.AlbumId);

            albumItem.HasIndex(ai => new { ai.AlbumId, ai.Order })
                .IsUnique()
                .HasDatabaseName("UX_AlbumItems_AlbumId_Order");
        });

        modelBuilder.Entity<Album>()
            .HasMany(a => a.Photos)
            .WithMany(p => p.Albums)
            .UsingEntity<AlbumPhoto>(
                right => right
                    .HasOne(ap => ap.Photo)
                    .WithMany(p => p.AlbumPhotos)
                    .HasForeignKey(ap => ap.PhotoId)
                    .HasConstraintName("FK_AlbumPhoto_Photos_PhotosId"),

                left => left
                    .HasOne(ap => ap.Album)
                    .WithMany(a => a.AlbumPhotos)
                    .HasForeignKey(ap => ap.AlbumId)
                    .HasConstraintName("FK_AlbumPhoto_Albums_AlbumsId"),

                join =>
                {
                    join.ToTable("AlbumPhoto",
                        table =>
                        {
                            table.HasCheckConstraint("CK_AlbumPhoto_Order_NonNegative", "\"Order\" >= 0");
                            table.HasCheckConstraint("CK_AlbumPhoto_ItemType_PhotoDisplay", "\"AlbumItemType\" = 'PhotoDisplay'");
                        });

                    join.HasKey(ap => new { ap.AlbumId, ap.PhotoId })
                        .HasName("PK_AlbumPhoto");

                    join.Property(ap => ap.AlbumId)
                        .HasColumnName("AlbumsId");

                    join.Property(ap => ap.PhotoId)
                        .HasColumnName("PhotosId");

                    join.HasIndex(ap => ap.PhotoId)
                        .HasDatabaseName("IX_AlbumPhoto_PhotosId");

                    join.HasIndex(ap => new { ap.AlbumPhotoDisplayItemId, ap.Order })
                        .IsUnique()
                        .HasDatabaseName("UX_AlbumPhoto_AlbumPhotoDisplayItemId_Order");

                    // Include the discriminator so photos can only belong to photo-display items.
                    join.Property<string>("AlbumItemType")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasDefaultValue("PhotoDisplay");

                    join.HasOne(ap => ap.AlbumPhotoDisplayItem)
                        .WithMany(apdi => apdi.AlbumPhotos)
                        .HasForeignKey(nameof(AlbumPhoto.AlbumId), nameof(AlbumPhoto.AlbumPhotoDisplayItemId), "AlbumItemType")
                        .HasPrincipalKey(nameof(AlbumItem.AlbumId), nameof(AlbumItem.Id), "ItemType")
                        .HasConstraintName("FK_AlbumPhoto_AlbumItems_AlbumId_ItemId_ItemType");

                    join.Property(ap => ap.DisplaysName)
                        .HasDefaultValue(true);

                    join.Property(ap => ap.DisplaysDescription)
                        .HasDefaultValue(true);

                    join.Property(ap => ap.DisplaysYearContentCreated)
                        .HasDefaultValue(true);
                });

        modelBuilder.Entity<Album>(album =>
        {
            album.ToTable("Albums", table =>
            {
                table.HasCheckConstraint(
                    "CK_Albums_NavbarOrder_Range",
                    "\"NavbarOrder\" BETWEEN -1 AND 4");
                table.HasCheckConstraint(
                    "CK_Albums_Unpublished_NoNavbar",
                    "\"Published\" = TRUE OR \"NavbarOrder\" = -1");
            });

            // Defaults also apply when the migration adds these columns to existing albums.
            album.Property(a => a.NavTitle).HasDefaultValue("");
            album.Property(a => a.Published).HasDefaultValue(false);
            album.Property(a => a.NavbarOrder).HasDefaultValue(-1).HasSentinel(-1);
            album.Property(a => a.LayoutPreset).HasDefaultValue(PageLayoutPreset.Default);

            album.HasIndex(a => a.NavbarOrder)
                .IsUnique()
                .HasFilter("\"NavbarOrder\" >= 0")
                .HasDatabaseName("UX_Albums_NavbarOrder_NonNegative");
        });
    }
}
