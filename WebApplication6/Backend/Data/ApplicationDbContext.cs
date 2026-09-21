using Microsoft.EntityFrameworkCore;
using WebApplication6.Backend.Models;

namespace WebApplication6.Backend.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    
    public DbSet<Image> Images => Set<Image>();
    
    public DbSet<Photo> Photos => Set<Photo>();
    
    public DbSet<Album> Albums => Set<Album>();

    public DbSet<AlbumItem> AlbumItems => Set<AlbumItem>();

    public DbSet<PhotoDisplay> PhotoDisplays => Set<PhotoDisplay>();

    public DbSet<PhotoDisplayCollection> PhotoDisplayCollections => Set<PhotoDisplayCollection>();

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
                    table.HasCheckConstraint("CK_AlbumItems_PhotoDisplay_Required",
                        "\"ItemType\" <> 'PhotoDisplay' OR (\"PhotoId\" IS NOT NULL AND \"DisplaysName\" IS NOT NULL AND \"DisplaysDescription\" IS NOT NULL AND \"DisplaysYearContentCreated\" IS NOT NULL)");
                    table.HasCheckConstraint("CK_AlbumItems_PhotoDisplayCollection_Mode",
                        "\"ItemType\" <> 'PhotoDisplayCollection' OR (\"DisplayMode\" IS NOT NULL AND \"DisplayMode\" BETWEEN 0 AND 1)");
                    table.HasCheckConstraint("CK_AlbumItems_PhotoDisplay_Columns",
                        "\"ItemType\" = 'PhotoDisplay' OR (\"PhotoId\" IS NULL AND \"PhotoDisplayCollectionId\" IS NULL)");
                    table.HasCheckConstraint("CK_AlbumItems_CollectionItemType",
                        "\"PhotoDisplayCollectionId\" IS NULL OR (\"CollectionItemType\" IS NOT NULL AND \"CollectionItemType\" = 'PhotoDisplayCollection')");
                });

            albumItem.HasDiscriminator<string>("ItemType")
                .HasValue<PhotoDisplay>("PhotoDisplay")
                .HasValue<PhotoDisplayCollection>("PhotoDisplayCollection");
            albumItem.Property<string>("ItemType").HasMaxLength(32);
            albumItem.HasAlternateKey(nameof(AlbumItem.AlbumId), nameof(AlbumItem.Id), "ItemType");

            albumItem.HasOne(ai => ai.Album)
                .WithMany(a => a.AlbumItems)
                .HasForeignKey(ai => ai.AlbumId);

            albumItem.HasIndex(ai => new { ai.AlbumId, ai.Order })
                .IsUnique()
                .HasFilter("\"PhotoDisplayCollectionId\" IS NULL")
                .HasDatabaseName("UX_AlbumItems_AlbumId_Order");
        });

        modelBuilder.Entity<PhotoDisplay>(photoDisplay =>
        {
            photoDisplay.HasOne(pd => pd.Photo)
                .WithMany(p => p.PhotoDisplays)
                .HasForeignKey(pd => pd.PhotoId);

            photoDisplay.HasIndex(pd => new { pd.AlbumId, pd.PhotoId })
                .IsUnique()
                .HasFilter("\"ItemType\" = 'PhotoDisplay'")
                .HasDatabaseName("UX_AlbumItems_AlbumId_PhotoId");

            photoDisplay.HasIndex(pd => new { pd.PhotoDisplayCollectionId, pd.Order })
                .IsUnique()
                .HasFilter("\"PhotoDisplayCollectionId\" IS NOT NULL")
                .HasDatabaseName("UX_AlbumItems_PhotoDisplayCollectionId_Order");

            // Include the album and discriminator so a display can only join a collection in its own album.
            photoDisplay.Property<string>("CollectionItemType")
                .IsRequired()
                .HasMaxLength(32)
                .HasDefaultValue("PhotoDisplayCollection");

            photoDisplay.HasOne(pd => pd.PhotoDisplayCollection)
                .WithMany(pdc => pdc.PhotoDisplays)
                .HasForeignKey(nameof(PhotoDisplay.AlbumId), nameof(PhotoDisplay.PhotoDisplayCollectionId), "CollectionItemType")
                .HasPrincipalKey(nameof(AlbumItem.AlbumId), nameof(AlbumItem.Id), "ItemType")
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_AlbumItems_AlbumItems_AlbumId_CollectionId_ItemType");

            photoDisplay.Property(pd => pd.DisplaysName).HasDefaultValue(true);
            photoDisplay.Property(pd => pd.DisplaysDescription).HasDefaultValue(true);
            photoDisplay.Property(pd => pd.DisplaysYearContentCreated).HasDefaultValue(true);
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
