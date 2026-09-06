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

    public DbSet<PortfolioPage> PortfolioPages => Set<PortfolioPage>();

    public DbSet<UntrackedFile> UntrackedFiles => Set<UntrackedFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
                        table => table.HasCheckConstraint(
                            "CK_AlbumPhoto_Order_NonNegative",
                            "\"Order\" >= 0"));

                    join.HasKey(ap => new { ap.AlbumId, ap.PhotoId })
                        .HasName("PK_AlbumPhoto");

                    join.Property(ap => ap.AlbumId)
                        .HasColumnName("AlbumsId");

                    join.Property(ap => ap.PhotoId)
                        .HasColumnName("PhotosId");

                    join.HasIndex(ap => ap.PhotoId)
                        .HasDatabaseName("IX_AlbumPhoto_PhotosId");

                    join.HasIndex(ap => new { ap.AlbumId, ap.Order })
                        .IsUnique()
                        .HasDatabaseName("UX_AlbumPhoto_AlbumsId_Order");

                    join.Property(ap => ap.DisplaysName)
                        .HasDefaultValue(true);

                    join.Property(ap => ap.DisplaysDescription)
                        .HasDefaultValue(true);

                    join.Property(ap => ap.DisplaysYearContentCreated)
                        .HasDefaultValue(true);
                });

        modelBuilder.Entity<PortfolioPage>(portfolioPage =>
        {
            portfolioPage.ToTable("PortfolioPages",
                table => table.HasCheckConstraint("CK_PortfolioPages_NavbarOrder_Range", "\"NavbarOrder\" BETWEEN -1 AND 4"));

            portfolioPage.HasIndex(page => page.NavbarOrder)
                .IsUnique()
                .HasFilter("\"NavbarOrder\" >= 0")
                .HasDatabaseName("UX_PortfolioPages_NavbarOrder_NonNegative");
        });
    }

}
