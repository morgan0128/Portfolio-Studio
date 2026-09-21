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
        var albumRepository = new AlbumRepository(context);
        var albumItemRepository = new AlbumItemRepository(context);

        Assert.True(await albumItemRepository.AddPhotoToAlbumAsync(3002, 2, TestContext.Current.CancellationToken));
        Assert.True(await albumItemRepository.AddPhotoToAlbumAsync(3002, 3, TestContext.Current.CancellationToken));
        Assert.Equal(new int?[] { 1, 2 }, (await albumItemRepository.GetAlbumPhotosAsync(3002)).Select(p => p.Order));
        Assert.True(await albumItemRepository.ReorderPhotoInAlbum(3002, 3, 0));
        context.ChangeTracker.Clear();

        var items = await context.AlbumItems.Where(ai => ai.AlbumId == 3002).OrderBy(ai => ai.Order)
            .ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(new[] { 0, 1, 2 }, items.Select(ai => ai.Order));
        Assert.IsType<PhotoDisplay>(items[0]);
        Assert.Equal(item.Id, Assert.IsType<NonPhotoAlbumItem>(items[1]).Id);
        Assert.IsType<PhotoDisplay>(items[2]);
        Assert.Equal(new[] { 3, 2 }, (await albumItemRepository.GetAlbumPhotosAsync(3002)).Select(p => p.Id));

        Assert.True(await new PhotoRepository(context).DeletePhotoByIdAsync(2));
        context.ChangeTracker.Clear();
        Assert.NotNull(await context.AlbumItems.FindAsync([item.Id], TestContext.Current.CancellationToken));
        Assert.Equal(2, await context.AlbumItems.CountAsync(ai => ai.AlbumId == 3002, TestContext.Current.CancellationToken));
        Assert.True(await albumRepository.DeleteAlbumByIdAsync(3002));
        Assert.False(await context.AlbumItems.AnyAsync(ai => ai.AlbumId == 3002, TestContext.Current.CancellationToken));
        Assert.NotNull(await context.Photos.FindAsync([3], TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("PhotoDisplayCollection", "FK_AlbumItems_AlbumItems_AlbumId_CollectionId_ItemType")]
    [InlineData("NonPhoto", "CK_AlbumItems_CollectionItemType")]
    public async Task PhotoDisplay_DatabaseRejectsNonCollectionOwner(string itemType, string constraintName)
    {
        await using var context = CreateContextWithNonPhotoItems();
        await using var transaction = await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var item = new NonPhotoAlbumItem { AlbumId = 3002, Order = 0 };
        context.AlbumItems.Add(item);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        context.ChangeTracker.Clear();
        var photoDisplay = new PhotoDisplay { AlbumId = 3002, PhotoId = 2, PhotoDisplayCollectionId = item.Id, Order = 0 };
        context.PhotoDisplays.Add(photoDisplay);
        context.Entry(photoDisplay).Property<string>("CollectionItemType").CurrentValue = itemType;

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(TestContext.Current.CancellationToken));
        Assert.Equal(constraintName, Assert.IsType<PostgresException>(exception.InnerException).ConstraintName);
    }

    [Fact]
    public async Task PhotoDisplayCollection_DatabaseRequiresDisplayMode()
    {
        await using var context = Fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);

        context.PhotoDisplayCollections.Add(new PhotoDisplayCollection { AlbumId = 3002, Order = 0 });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var exception = await Assert.ThrowsAsync<PostgresException>(() => context.Database.ExecuteSqlRawAsync(
            "UPDATE \"AlbumItems\" SET \"DisplayMode\" = NULL WHERE \"AlbumId\" = 3002",
            TestContext.Current.CancellationToken));
        Assert.Equal("CK_AlbumItems_PhotoDisplayCollection_Mode", exception.ConstraintName);
    }

    [Fact]
    public async Task PhotoDisplays_RootAndCollectionOrdersRemainIndependent()
    {
        await using var context = Fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var item = new PhotoDisplayCollection
        {
            AlbumId = 3002, Order = 0,
            PhotoDisplays =
            [
                new PhotoDisplay { PhotoId = 2, Order = 0, DisplaysName = false },
                new PhotoDisplay { PhotoId = 3, Order = 8 }
            ]
        };
        context.PhotoDisplayCollections.Add(item);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var albumItemRepository = new AlbumItemRepository(context);

        Assert.True(await albumItemRepository.AddPhotoToAlbumAsync(3002, 4, TestContext.Current.CancellationToken));
        Assert.Equal(1, await context.PhotoDisplays.Where(pd => pd.AlbumId == 3002 && pd.PhotoId == 4)
            .Select(pd => pd.Order).SingleAsync(TestContext.Current.CancellationToken));
        Assert.True(await albumItemRepository.ReorderAlbumItem(3002, item.Id, 1));
        Assert.Equal(new[] { 0, 8 }, item.PhotoDisplays.OrderBy(pd => pd.Order).Select(pd => pd.Order));
        Assert.Equal(1, item.Order);

        var toMove = item.PhotoDisplays.Single(pd => pd.PhotoId == 3);
        Assert.True(await albumItemRepository.ReorderPhotoDisplayInCollection(3002, item.Id, toMove.Id, 0));
        Assert.Equal(1, item.Order);
        Assert.Equal(new[] { 3, 2 }, item.PhotoDisplays.OrderBy(pd => pd.Order).Select(pd => pd.PhotoId));
        Assert.Equal(new[] { 0, 1 }, item.PhotoDisplays.OrderBy(pd => pd.Order).Select(pd => pd.Order));
        Assert.False(await albumItemRepository.ReorderAlbumItem(3002, toMove.Id, 0));
        Assert.False(await albumItemRepository.ReorderPhotoDisplayInCollection(1, item.Id, toMove.Id, 0));
        Assert.False(await albumItemRepository.ReorderPhotoDisplayInCollection(3002, item.Id + 9999, toMove.Id, 0));

        var photos = (await albumItemRepository.GetAlbumPhotosAsync(3002)).ToList();
        Assert.Equal(new[] { 4, 3, 2 }, photos.Select(p => p.Id));
        Assert.Equal(new int?[] { 0, 1, 1 }, photos.Select(p => p.Order));
        Assert.False(photos[2].displaysName);
        Assert.True(await albumItemRepository.ReorderPhotoInAlbum(3002, 2, 0));
        Assert.Equal(new[] { 4, 2, 3 }, (await albumItemRepository.GetAlbumPhotosAsync(3002)).Select(p => p.Id));
        Assert.Equal(1, item.Order);
    }

    [Fact]
    public async Task PhotoDisplay_DatabaseRejectsDuplicatePhotoAcrossStandaloneAndCollection()
    {
        await using var context = Fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        context.PhotoDisplayCollections.Add(new PhotoDisplayCollection
        {
            AlbumId = 1, Order = 5,
            PhotoDisplays = [new PhotoDisplay { PhotoId = 2, Order = 0 }]
        });

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(TestContext.Current.CancellationToken));
        Assert.Equal("UX_AlbumItems_AlbumId_PhotoId", Assert.IsType<PostgresException>(exception.InnerException).ConstraintName);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PhotoDisplayCollection_DeletingCollectionOrLastPhotoRemovesItsDisplays(bool deletePhoto)
    {
        await using var context = Fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var item = new PhotoDisplayCollection
        {
            AlbumId = 3002, Order = 0,
            PhotoDisplays = [new PhotoDisplay { PhotoId = 2, Order = 0 }]
        };
        context.PhotoDisplayCollections.Add(item);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        context.ChangeTracker.Clear();

        if (deletePhoto)
        {
            Assert.True(await new PhotoRepository(context).DeletePhotoByIdAsync(2));
        }
        else
        {
            context.PhotoDisplayCollections.Remove(await context.PhotoDisplayCollections
                .SingleAsync(pdc => pdc.Id == item.Id, TestContext.Current.CancellationToken));
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        Assert.False(await context.AlbumItems.AnyAsync(ai => ai.AlbumId == 3002, TestContext.Current.CancellationToken));
        Assert.Equal(!deletePhoto, await context.Photos.AnyAsync(p => p.Id == 2, TestContext.Current.CancellationToken));
        Assert.NotNull(await context.Photos.FindAsync([3], TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("PhotoId", "CK_AlbumItems_PhotoDisplay_Required")]
    [InlineData("CollectionItemType", "CK_AlbumItems_CollectionItemType")]
    public async Task PhotoDisplay_DatabaseRejectsNullRequiredValues(string column, string constraintName)
    {
        await using var context = Fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        context.PhotoDisplayCollections.Add(new PhotoDisplayCollection
        {
            AlbumId = 3002, Order = 0,
            PhotoDisplays = [new PhotoDisplay { PhotoId = 2, Order = 0 }]
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var columnIdentifier = context.GetService<ISqlGenerationHelper>().DelimitIdentifier(column);
        var sql = "UPDATE \"AlbumItems\" SET " + columnIdentifier + " = NULL WHERE \"AlbumId\" = 3002 AND \"ItemType\" = 'PhotoDisplay'";

        var exception = await Assert.ThrowsAsync<PostgresException>(() => context.Database.ExecuteSqlRawAsync(sql, TestContext.Current.CancellationToken));
        Assert.Equal(constraintName, exception.ConstraintName);
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
