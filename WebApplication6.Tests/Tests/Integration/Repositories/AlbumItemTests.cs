using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using WebApplication6.Backend.Data;
using WebApplication6.Backend.Models;
using WebApplication6.Backend.Repositories;
using Xunit;

namespace WebApplication6.Tests.Tests.Integration.Repositories;

public class AlbumItemTests(TestDatabaseFixture dbFixture) : IClassFixture<TestDatabaseFixture>
{
    public TestDatabaseFixture Fixture { get; } = dbFixture;

    [Fact]
    public async Task AlbumItems_MixedTypes_ShareOrderAndPreserveNonPhotoItemsWhenDeletingPhotos()
    {
        await using var context = CreateContextWithNonPhotoItems();
        await using var transaction = await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var item = new NonPhotoAlbumItem { AlbumId = 3002, Order = 0 };
        context.AlbumItems.Add(item);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var repository = new AlbumRepository(context);

        Assert.True(await repository.AddPhotoToAlbumAsync(3002, 2, TestContext.Current.CancellationToken));
        Assert.True(await repository.AddPhotoToAlbumAsync(3002, 3, TestContext.Current.CancellationToken));
        Assert.Equal(new int?[] { 1, 2 }, (await repository.GetAlbumPhotosAsync(3002)).Select(p => p.Order));
        Assert.True(await repository.ReorderPhotoInAlbum(3002, 3, 0));
        context.ChangeTracker.Clear();

        var items = await context.AlbumItems.Where(ai => ai.AlbumId == 3002).OrderBy(ai => ai.Order)
            .ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(new[] { 0, 1, 2 }, items.Select(ai => ai.Order));
        Assert.IsType<AlbumPhotoDisplayItem>(items[0]);
        Assert.Equal(item.Id, Assert.IsType<NonPhotoAlbumItem>(items[1]).Id);
        Assert.IsType<AlbumPhotoDisplayItem>(items[2]);
        Assert.Equal(new[] { 3, 2 }, (await repository.GetAlbumPhotosAsync(3002)).Select(p => p.Id));

        Assert.True(await new PhotoRepository(context).DeletePhotoByIdAsync(2));
        context.ChangeTracker.Clear();
        Assert.NotNull(await context.AlbumItems.FindAsync([item.Id], TestContext.Current.CancellationToken));
        Assert.Equal(2, await context.AlbumItems.CountAsync(ai => ai.AlbumId == 3002, TestContext.Current.CancellationToken));
        Assert.True(await repository.DeleteAlbumByIdAsync(3002));
        Assert.False(await context.AlbumItems.AnyAsync(ai => ai.AlbumId == 3002, TestContext.Current.CancellationToken));
        Assert.NotNull(await context.Photos.FindAsync([3], TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("PhotoDisplay", "FK_AlbumPhoto_AlbumItems_AlbumId_ItemId_ItemType")]
    [InlineData("NonPhoto", "CK_AlbumPhoto_ItemType_PhotoDisplay")]
    public async Task AlbumPhoto_DatabaseRejectsNonPhotoOwner(string itemType, string constraintName)
    {
        await using var context = CreateContextWithNonPhotoItems();
        await using var transaction = await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var item = new NonPhotoAlbumItem { AlbumId = 3002, Order = 0 };
        context.AlbumItems.Add(item);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        context.ChangeTracker.Clear();
        var albumPhoto = new AlbumPhoto { AlbumId = 3002, PhotoId = 2, AlbumPhotoDisplayItemId = item.Id, Order = 0 };
        context.AlbumPhotos.Add(albumPhoto);
        context.Entry(albumPhoto).Property<string>("AlbumItemType").CurrentValue = itemType;

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(TestContext.Current.CancellationToken));
        Assert.Equal(constraintName, Assert.IsType<PostgresException>(exception.InnerException).ConstraintName);
    }

    [Fact]
    public async Task AlbumPhotoDisplayItem_DatabaseRequiresDisplayMode()
    {
        await using var context = Fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);

        var exception = await Assert.ThrowsAsync<PostgresException>(() => context.Database.ExecuteSqlRawAsync(
            "UPDATE \"AlbumItems\" SET \"DisplayMode\" = NULL WHERE \"AlbumId\" = 1",
            TestContext.Current.CancellationToken));
        Assert.Equal("CK_AlbumItems_PhotoDisplayMode_Range", exception.ConstraintName);
    }

    [Fact]
    public async Task AlbumItemsMigration_PreservesPhotosAndFlagsThroughUpgradeAndRollback()
    {
        const string previousMigration = "20260918131712_album_absorbed_portfoliopage";
        await using var source = Fixture.CreateContext();
        var connectionString = new NpgsqlConnectionStringBuilder(source.Database.GetConnectionString());
        Assert.Equal("webapplication6test", connectionString.Database);
        var schema = "album_items_test_" + Guid.NewGuid().ToString("N");
        connectionString.SearchPath = schema;
        var schemaIdentifier = source.GetService<ISqlGenerationHelper>().DelimitIdentifier(schema);
        var createSchemaSql = "CREATE SCHEMA " + schemaIdentifier;
        var dropSchemaSql = "DROP SCHEMA " + schemaIdentifier + " CASCADE";
        await source.Database.ExecuteSqlRawAsync(createSchemaSql, TestContext.Current.CancellationToken);
        try
        {
            await using var context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(connectionString.ConnectionString).Options);
            var migrator = context.GetService<IMigrator>();
            await migrator.MigrateAsync(previousMigration, TestContext.Current.CancellationToken);
            await context.Database.ExecuteSqlRawAsync("""
                INSERT INTO "Images" ("Id", "AltText", "ContentType", "FileName", "StorageFileName", "Url", "Height", "Width")
                VALUES (9000, 'test', 'image/png', 'test.png', 'test.png', 'test.png', 1, 1);
                INSERT INTO "Photos" ("Id", "ImageId") VALUES (9001, 9000), (9002, 9000), (9003, 9000);
                INSERT INTO "Albums" ("Id", "Name") VALUES (9000, 'First'), (9001, 'Second');
                INSERT INTO "AlbumPhoto" ("AlbumsId", "PhotosId", "Order", "DisplaysName", "DisplaysDescription", "DisplaysYearContentCreated")
                VALUES (9000, 9001, 4, false, true, false), (9000, 9002, 0, true, false, true), (9001, 9001, 7, true, true, false);
                """, TestContext.Current.CancellationToken);

            await migrator.MigrateAsync(cancellationToken: TestContext.Current.CancellationToken);
            Assert.False(context.Database.HasPendingModelChanges());
            var albumPhotos = await context.AlbumPhotos.Include(ap => ap.AlbumPhotoDisplayItem)
                .OrderBy(ap => ap.AlbumId).ThenBy(ap => ap.AlbumPhotoDisplayItem.Order)
                .ToListAsync(TestContext.Current.CancellationToken);
            Assert.Equal(new[] { 9002, 9001, 9001 }, albumPhotos.Select(ap => ap.PhotoId));
            Assert.Equal(new[] { 0, 4, 7 }, albumPhotos.Select(ap => ap.AlbumPhotoDisplayItem.Order));
            Assert.Equal(3, albumPhotos.Select(ap => ap.AlbumPhotoDisplayItemId).Distinct().Count());
            Assert.All(albumPhotos, ap =>
            {
                Assert.Equal(0, ap.Order);
                Assert.Equal(ap.AlbumId, ap.AlbumPhotoDisplayItem.AlbumId);
                Assert.Equal(AlbumPhotoDisplayItem.PhotoDisplayMode.Static, ap.AlbumPhotoDisplayItem.DisplayMode);
            });
            Assert.Equal(new[] { true, false, true }, albumPhotos.Select(ap => ap.DisplaysName));
            Assert.Equal(new[] { false, true, true }, albumPhotos.Select(ap => ap.DisplaysDescription));
            Assert.Equal(new[] { true, false, false }, albumPhotos.Select(ap => ap.DisplaysYearContentCreated));
            Assert.Equal(3, await context.Photos.CountAsync(TestContext.Current.CancellationToken));

            var repository = new AlbumRepository(context);
            Assert.True(await repository.AddPhotoToAlbumAsync(9000, 9003, TestContext.Current.CancellationToken));
            var added = await context.AlbumPhotos.Include(ap => ap.AlbumPhotoDisplayItem)
                .SingleAsync(ap => ap.AlbumId == 9000 && ap.PhotoId == 9003, TestContext.Current.CancellationToken);
            Assert.Equal(5, added.AlbumPhotoDisplayItem.Order);
            var item = added.AlbumPhotoDisplayItem;
            added.AlbumPhotoDisplayItem = albumPhotos[1].AlbumPhotoDisplayItem;
            added.Order = 1;
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            context.AlbumItems.Remove(item);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            context.ChangeTracker.Clear();

            await migrator.MigrateAsync(previousMigration, TestContext.Current.CancellationToken);
            var photoIds = await context.Database.SqlQueryRaw<int>("""
                SELECT "PhotosId" AS "Value" FROM "AlbumPhoto" WHERE "AlbumsId" = 9000 ORDER BY "Order"
                """).ToListAsync(TestContext.Current.CancellationToken);
            Assert.Equal(new[] { 9002, 9001, 9003 }, photoIds);
            Assert.False(await context.AlbumPhotos.Where(ap => ap.AlbumId == 9000 && ap.PhotoId == 9001)
                .Select(ap => ap.DisplaysName).SingleAsync(TestContext.Current.CancellationToken));
            await migrator.MigrateAsync(cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal(4, await context.AlbumItems.CountAsync(TestContext.Current.CancellationToken));
            Assert.Equal(4, await context.AlbumPhotos.CountAsync(TestContext.Current.CancellationToken));
        }
        finally
        {
            await source.Database.ExecuteSqlRawAsync(dropSchemaSql, CancellationToken.None);
        }
    }

    private ApplicationDbContext CreateContextWithNonPhotoItems()
    {
        using var context = Fixture.CreateContext();
        return new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(context.Database.GetConnectionString())
            .ReplaceService<IModelCustomizer, AlbumItemModelCustomizer>()
            .Options);
    }

    private sealed class NonPhotoAlbumItem : AlbumItem;

    private sealed class AlbumItemModelCustomizer(ModelCustomizerDependencies dependencies) : ModelCustomizer(dependencies)
    {
        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);
            modelBuilder.Entity<AlbumItem>().HasDiscriminator<string>("ItemType")
                .HasValue<NonPhotoAlbumItem>("NonPhoto");
        }
    }
}
