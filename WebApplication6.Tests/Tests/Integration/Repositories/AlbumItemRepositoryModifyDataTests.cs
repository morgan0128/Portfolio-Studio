using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using WebApplication6.Backend.Data;
using WebApplication6.Backend.Models;
using WebApplication6.Backend.Repositories;
using Xunit;

namespace WebApplication6.Tests.Tests.Integration.Repositories;

public class AlbumItemRepositoryModifyDataTests(TestDatabaseFixture dbFixture) : IClassFixture<TestDatabaseFixture>
{
    public TestDatabaseFixture Fixture { get; } = dbFixture;
    
    // TODO: Move AlbumItems_MixedTypes_ShareOrderAndPreserveNonPhotoItemsWhenDeletingPhotos
    //// [Fact]
    // public async Task AlbumItems_MixedTypes_ShareOrderAndPreserveNonPhotoItemsWhenDeletingPhotos()
    // {
    //     await using var context = CreateContextWithNonPhotoItems();
    //     await using var transaction = await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
    //     var item = new NonPhotoAlbumItem { AlbumId = 3002, Order = 0 };
    //     context.AlbumItems.Add(item);
    //     await context.SaveChangesAsync(TestContext.Current.CancellationToken);
    //     var albumRepository = new AlbumRepository(context);
    //     var albumItemRepository = new AlbumItemRepository(context);
    //
    //     Assert.True(await albumItemRepository.AddPhotoToAlbumAsync(3002, 2, TestContext.Current.CancellationToken));
    //     Assert.True(await albumItemRepository.AddPhotoToAlbumAsync(3002, 3, TestContext.Current.CancellationToken));
    //     Assert.Equal(new[] { 1, 2 }, (await albumItemRepository.GetAlbumItems(3002)).Select(photo => photo.Order));
    //     var photoDisplayId = await context.PhotoDisplays.Where(pd => pd.AlbumId == 3002 && pd.PhotoId == 3)
    //         .Select(pd => pd.Id).SingleAsync(TestContext.Current.CancellationToken);
    //     Assert.Equal(IAlbumItemRepository.ReorderOutcome.Success,
    //         await albumItemRepository.ReorderAlbumItem(3002, photoDisplayId, 0));
    //     context.ChangeTracker.Clear();
    //
    //     var items = await context.AlbumItems.Where(ai => ai.AlbumId == 3002).OrderBy(ai => ai.Order)
    //         .ToListAsync(TestContext.Current.CancellationToken);
    //     Assert.Equal(new[] { 0, 1, 2 }, items.Select(ai => ai.Order));
    //     Assert.IsType<PhotoDisplay>(items[0]);
    //     Assert.Equal(item.Id, Assert.IsType<NonPhotoAlbumItem>(items[1]).Id);
    //     Assert.IsType<PhotoDisplay>(items[2]);
    //     Assert.Equal(new[] { 3, 2 }, (await albumItemRepository.GetAlbumItems(3002))
    //         .OfType<IAlbumItemRepository.PhotoDisplayAlbumItemDto>().Select(photo => photo.Content.Photo.Id));
    //
    //     Assert.True(await new PhotoRepository(context).DeletePhotoByIdAsync(2));
    //     context.ChangeTracker.Clear();
    //     Assert.NotNull(await context.AlbumItems.FindAsync([item.Id], TestContext.Current.CancellationToken));
    //     Assert.Equal(2, await context.AlbumItems.CountAsync(ai => ai.AlbumId == 3002, TestContext.Current.CancellationToken));
    //     Assert.True(await albumRepository.DeleteAlbumByIdAsync(3002));
    //     Assert.False(await context.AlbumItems.AnyAsync(ai => ai.AlbumId == 3002, TestContext.Current.CancellationToken));
    //     Assert.NotNull(await context.Photos.FindAsync([3], TestContext.Current.CancellationToken));
    // }

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
        var repository = new AlbumItemRepository(context);

        Assert.True(await repository.AddPhotoToAlbumAsync(3002, 4, TestContext.Current.CancellationToken));
        Assert.Equal(1, await context.PhotoDisplays.Where(pd => pd.AlbumId == 3002 && pd.PhotoId == 4)
            .Select(pd => pd.Order).SingleAsync(TestContext.Current.CancellationToken));
        Assert.Equal(IAlbumItemRepository.ReorderOutcome.Success,
            await repository.ReorderAlbumItem(3002, item.Id, 1));
        Assert.Equal(new[] { 0, 8 }, item.PhotoDisplays.OrderBy(pd => pd.Order).Select(pd => pd.Order));
        Assert.Equal(1, item.Order);

        var toMove = item.PhotoDisplays.Single(pd => pd.PhotoId == 3);
        Assert.Equal(IAlbumItemRepository.ReorderOutcome.Success,
            await repository.ReorderPhotoDisplayInCollection(3002, item.Id, toMove.Id, 0));
        Assert.Equal(1, item.Order);
        Assert.Equal(new[] { 3, 2 }, item.PhotoDisplays.OrderBy(pd => pd.Order).Select(pd => pd.PhotoId));
        Assert.Equal(new[] { 0, 1 }, item.PhotoDisplays.OrderBy(pd => pd.Order).Select(pd => pd.Order));
        Assert.Equal(IAlbumItemRepository.ReorderOutcome.ItemNotFound,
            await repository.ReorderAlbumItem(3002, toMove.Id, 0));
        Assert.Equal(IAlbumItemRepository.ReorderOutcome.NoAlbumItems,
            await repository.ReorderPhotoDisplayInCollection(1, item.Id, toMove.Id, 0));
        Assert.Equal(IAlbumItemRepository.ReorderOutcome.NoAlbumItems,
            await repository.ReorderPhotoDisplayInCollection(3002, item.Id + 9999, toMove.Id, 0));

        var albumItems = (await repository.GetAlbumItems(3002)).ToList();
        Assert.Equal(2, albumItems.Count);
        var standalonePhoto = Assert.IsType<IAlbumItemRepository.PhotoDisplayAlbumItemDto>(albumItems[0]);
        var collection = Assert.IsType<IAlbumItemRepository.PhotoDisplayCollectionAlbumItemDto>(albumItems[1]);
        Assert.Equal(4, standalonePhoto.Content.Photo.Id);
        Assert.Equal(0, standalonePhoto.Order);
        Assert.Equal(1, collection.Order);
        Assert.Equal(new[] { 3, 2 }, collection.Content.PhotoDisplays.Select(pd => pd.Content.Photo.Id));
        Assert.Equal(new[] { 0, 1 }, collection.Content.PhotoDisplays.Select(pd => pd.Order));
        Assert.False(collection.Content.PhotoDisplays[1].Content.DisplaysName);

        var photoToMove = item.PhotoDisplays.Single(pd => pd.PhotoId == 2);
        Assert.Equal(IAlbumItemRepository.ReorderOutcome.Success,
            await repository.ReorderPhotoDisplayInCollection(3002, item.Id, photoToMove.Id, 0));
        collection = Assert.IsType<IAlbumItemRepository.PhotoDisplayCollectionAlbumItemDto>(
            (await repository.GetAlbumItems(3002)).Last());
        Assert.Equal(new[] { 2, 3 }, collection.Content.PhotoDisplays.Select(pd => pd.Content.Photo.Id));
        Assert.Equal(new[] { 0, 1 }, collection.Content.PhotoDisplays.Select(pd => pd.Order));
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
    
        [Theory]
    [InlineData(1, -1, 0)]
    [InlineData(1, 63, 0)]
    [InlineData(1, -1, 2)]
    [InlineData(1, 63, 2)]
    public async Task ReorderAlbumItem_InvalidItemId_ReturnsItemNotFound(int albumId, int itemId, int newOrder)
    {
        await using var context = Fixture.CreateContext();
        var repository = new AlbumItemRepository(context);

        // Confirm the album exists, but the requested item does not.
        Assert.NotNull(await context.Albums.FindAsync([albumId], TestContext.Current.CancellationToken));
        Assert.False(await context.AlbumItems.AnyAsync(item => item.Id == itemId, TestContext.Current.CancellationToken));

        var outcome = await repository.ReorderAlbumItem(albumId, itemId, newOrder);
        Assert.Equal(IAlbumItemRepository.ReorderOutcome.ItemNotFound, outcome);
    }

    [Theory]
    [InlineData(-1, 3, 0)]
    [InlineData(22, 3, 0)]
    [InlineData(-1, 3, 2)]
    [InlineData(22, 3, 2)]
    public async Task ReorderAlbumItem_AlbumDoesNotExist_ReturnsNoAlbumItems(int albumId, int photoId, int newOrder)
    {
        await using var context = Fixture.CreateContext();
        var repository = new AlbumItemRepository(context);

        // Confirm the item exists in a different album, but the requested album does not.
        var item = await context.PhotoDisplays.SingleAsync(
            display => display.AlbumId == 1 && display.PhotoId == photoId,
            TestContext.Current.CancellationToken);
        Assert.Null(await context.Albums.FindAsync([albumId], TestContext.Current.CancellationToken));

        var outcome = await repository.ReorderAlbumItem(albumId, item.Id, newOrder);
        Assert.Equal(IAlbumItemRepository.ReorderOutcome.NoAlbumItems, outcome);
    }

    [Theory]
    [InlineData(3002, 3, 0)]
    [InlineData(3002, 4, 0)]
    public async Task ReorderAlbumItem_ItemBelongsToAnotherAlbum_ReturnsItemNotFound(int albumId, int photoId, int newOrder)
    {
        await using var context = Fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var repository = new AlbumItemRepository(context);
        
        Assert.NotNull(await context.Albums.FindAsync([albumId], TestContext.Current.CancellationToken));
        Assert.True(await repository.AddPhotoToAlbumAsync(albumId, 2, TestContext.Current.CancellationToken));
        var item = await context.PhotoDisplays.SingleAsync(
            display => display.AlbumId == 1 && display.PhotoId == photoId,
            TestContext.Current.CancellationToken);

        var outcome = await repository.ReorderAlbumItem(albumId, item.Id, newOrder);
        Assert.Equal(IAlbumItemRepository.ReorderOutcome.ItemNotFound, outcome);
    }
    
        [Theory]
    [InlineData(4, -1)]
    [InlineData(4, -99)]
    [InlineData(3, -7)]
    [InlineData(64, -9999)]
    public async Task ReorderAlbumItem_NegativeOrder_NormalizesAndReturnsSuccess(int photoId, int newOrder)
    {
        await using var context = Fixture.CreateContext();
        await using var transaction =
            await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var itemId = await GetRootPhotoDisplayId(context, 1, photoId);

        Assert.Equal(IAlbumItemRepository.ReorderOutcome.Success,
            await new AlbumItemRepository(context).ReorderAlbumItem(1, itemId, newOrder));
        await AssertRootPhotoOrder(context, 1, 4, 2, 3, 64);
    }

    [Theory]
    [InlineData(4, 0)]
    [InlineData(2, 1)]
    [InlineData(3, 2)]
    [InlineData(64, 4)]
    public async Task ReorderAlbumItem_SameOrder_NormalizesAndReturnsSuccess(int photoId, int newOrder)
    {
        await using var context = Fixture.CreateContext();
        await using var transaction =
            await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var itemId = await GetRootPhotoDisplayId(context, 1, photoId);

        Assert.Equal(IAlbumItemRepository.ReorderOutcome.Success,
            await new AlbumItemRepository(context).ReorderAlbumItem(1, itemId, newOrder));
        await AssertRootPhotoOrder(context, 1, 4, 2, 3, 64);
    }

    [Theory]
    [InlineData(4, 3, new[] { 2, 3, 4, 64 })]
    [InlineData(2, 4, new[] { 4, 3, 64, 2 })]
    [InlineData(64, 5, new[] { 4, 2, 3, 64 })]
    public async Task ReorderAlbumItem_MovesAndNormalizes(int photoId, int newOrder, int[] expectedPhotoIds)
    {
        await using var context = Fixture.CreateContext();
        await using var transaction =
            await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var itemId = await GetRootPhotoDisplayId(context, 1, photoId);

        Assert.Equal(IAlbumItemRepository.ReorderOutcome.Success,
            await new AlbumItemRepository(context).ReorderAlbumItem(1, itemId, newOrder));
        await AssertRootPhotoOrder(context, 1, expectedPhotoIds);
    }

    [Fact]
    public async Task ReorderAlbumItem_FirstMovesAcrossMultiplePositions()
    {
        await using var context = Fixture.CreateContext();
        await using var transaction =
            await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var repository = new AlbumItemRepository(context);
        var itemId = await GetRootPhotoDisplayId(context, 1, 4);

        Assert.Equal(IAlbumItemRepository.ReorderOutcome.Success, await repository.ReorderAlbumItem(1, itemId, 1));
        Assert.Equal(IAlbumItemRepository.ReorderOutcome.Success, await repository.ReorderAlbumItem(1, itemId, 2));
        Assert.Equal(IAlbumItemRepository.ReorderOutcome.Success, await repository.ReorderAlbumItem(1, itemId, 4));
        await AssertRootPhotoOrder(context, 1, 2, 3, 64, 4);
    }

    [Fact]
    public async Task AddPhotoToAlbumAsync_CreatesSingletonItemsAndPreservesDisplaySettings()
    {
        await using var context = Fixture.CreateContext();
        await using var transaction =
            await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var repository = new AlbumItemRepository(context);

        Assert.True(await repository.AddPhotoToAlbumAsync(3002, 2, TestContext.Current.CancellationToken));
        Assert.True(await repository.AddPhotoToAlbumAsync(3002, 3, TestContext.Current.CancellationToken));
        var firstDisplayId = await GetRootPhotoDisplayId(context, 3002, 2);
        Assert.True(await repository.ModifyFieldsDisplayed(3002, firstDisplayId,
            new IAlbumItemRepository.PhotoDisplayFieldsDisplayedRequest(false, false, false)));
        context.ChangeTracker.Clear();

        var photoDisplays = await context.PhotoDisplays.Where(display => display.AlbumId == 3002)
            .OrderBy(display => display.Order).ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(new[] { 0, 1 }, photoDisplays.Select(display => display.Order));
        Assert.All(photoDisplays, display => Assert.Null(display.PhotoDisplayCollectionId));

        var photos = (await repository.GetAlbumItems(3002))
            .Select(item => Assert.IsType<IAlbumItemRepository.PhotoDisplayAlbumItemDto>(item)).ToList();
        Assert.Equal(new[] { 2, 3 }, photos.Select(item => item.Content.Photo.Id));
        Assert.Equal(new[] { 0, 1 }, photos.Select(item => item.Order));
        Assert.False(photos[0].Content.DisplaysName);
        Assert.False(photos[0].Content.DisplaysDescription);
        Assert.False(photos[0].Content.DisplaysYearContentCreated);
        Assert.True(photos[1].Content.DisplaysName);
        Assert.True(photos[1].Content.DisplaysDescription);
        Assert.True(photos[1].Content.DisplaysYearContentCreated);
    }

    [Fact]
    public async Task AddPhotoToAlbumAsync_DuplicatePhoto_DoesNotLeaveAnEmptyItem()
    {
        await using var context = Fixture.CreateContext();
        await using var transaction =
            await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var repository = new AlbumItemRepository(context);

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() =>
            repository.AddPhotoToAlbumAsync(1, 2, TestContext.Current.CancellationToken));
        Assert.Equal("UX_AlbumItems_AlbumId_PhotoId",
            Assert.IsType<PostgresException>(exception.InnerException).ConstraintName);
        Assert.Equal(4,
            await context.AlbumItems.CountAsync(item => item.AlbumId == 1, TestContext.Current.CancellationToken));
        Assert.Equal(4,
            await context.PhotoDisplays.CountAsync(display => display.AlbumId == 1,
                TestContext.Current.CancellationToken));
        Assert.DoesNotContain(context.ChangeTracker.Entries(), entry => entry.State == EntityState.Added);
        Assert.True(await repository.AddPhotoToAlbumAsync(3002, 2, TestContext.Current.CancellationToken));
    }

    private ApplicationDbContext CreateContextWithNonPhotoItems()
    {
        using var context = Fixture.CreateContext();
        return new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(context.Database.GetConnectionString())
            .ReplaceService<IModelCustomizer, AlbumItemModelCustomizer>()
            .Options);
    }

    private static Task<int> GetRootPhotoDisplayId(ApplicationDbContext context, int albumId, int photoId)
        => context.PhotoDisplays
            .Where(display => display.AlbumId == albumId && display.PhotoId == photoId &&
                              display.PhotoDisplayCollectionId == null)
            .Select(display => display.Id)
            .SingleAsync(TestContext.Current.CancellationToken);

    private static async Task AssertRootPhotoOrder(ApplicationDbContext context, int albumId, params int[] photoIds)
    {
        context.ChangeTracker.Clear();
        var displays = await context.PhotoDisplays
            .Where(display => display.AlbumId == albumId && display.PhotoDisplayCollectionId == null)
            .OrderBy(display => display.Order)
            .ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(photoIds, displays.Select(display => display.PhotoId));
        Assert.Equal(Enumerable.Range(0, photoIds.Length), displays.Select(display => display.Order));
    }
    
    [Fact]
    public async Task DeletePhotoByIdAsync_RemovesEmptyItemsAndPreservesOtherPhotos()
    {
        await using var context = Fixture.CreateContext();
        await using var transaction =
            await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var collection = new PhotoDisplayCollection { AlbumId = 3002, Order = 0 };
        context.PhotoDisplays.Add(new PhotoDisplay
        {
            AlbumId = 3002, PhotoId = 2, PhotoDisplayCollection = collection, Order = 0
        });
        context.PhotoDisplays.Add(new PhotoDisplay
        {
            AlbumId = 3002, PhotoId = 3, PhotoDisplayCollection = collection, Order = 1
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var rootDisplayId = await GetRootPhotoDisplayId(context, 1, 2);
        context.ChangeTracker.Clear();

        Assert.True(await new PhotoRepository(context).DeletePhotoByIdAsync(2));
        context.ChangeTracker.Clear();

        Assert.Null(await context.Photos.FindAsync([2], TestContext.Current.CancellationToken));
        Assert.NotNull(await context.Photos.FindAsync([3], TestContext.Current.CancellationToken));
        Assert.False(await context.AlbumItems.AnyAsync(item => item.Id == rootDisplayId,
            TestContext.Current.CancellationToken));
        Assert.False(await context.PhotoDisplays.AnyAsync(display => display.PhotoId == 2,
            TestContext.Current.CancellationToken));
        var remaining = await context.PhotoDisplayCollections.Include(item => item.PhotoDisplays)
            .SingleAsync(item => item.Id == collection.Id, TestContext.Current.CancellationToken);
        Assert.Equal(3, Assert.Single(remaining.PhotoDisplays).PhotoId);
        Assert.Equal(3,
            await context.AlbumItems.CountAsync(item => item.AlbumId == 1, TestContext.Current.CancellationToken));
    }
    
    


    [Theory]
    [InlineData(1, -1, 0, "CK_AlbumItems_Order_NonNegative")]
    [InlineData(1, 3, -1, "CK_AlbumItems_PhotoDisplayCollection_Mode")]
    [InlineData(1, 3, 2, "CK_AlbumItems_PhotoDisplayCollection_Mode")]
    [InlineData(1, 0, 0, "UX_AlbumItems_AlbumId_Order")]
    [InlineData(9999, 0, 0, "FK_AlbumItems_Albums_AlbumId")]
    public async Task PhotoDisplayCollection_DatabaseRejectsInvalidItem(
        int albumId, int order, int displayMode, string constraintName)
    {
        await using var context = Fixture.CreateContext();
        await using var transaction =
            await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        context.PhotoDisplayCollections.Add(new PhotoDisplayCollection
        {
            AlbumId = albumId, Order = order, DisplayMode = (PhotoDisplayCollection.PhotoDisplayMode)displayMode
        });

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() =>
            context.SaveChangesAsync(TestContext.Current.CancellationToken));
        Assert.Equal(constraintName, Assert.IsType<PostgresException>(exception.InnerException).ConstraintName);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PhotoDisplay_DatabaseRequiresCollectionInSameAlbum(bool existingItem)
    {
        await using var context = Fixture.CreateContext();
        await using var transaction =
            await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var collection = new PhotoDisplayCollection { AlbumId = 1, Order = 5 };
        context.PhotoDisplayCollections.Add(collection);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var collectionId = existingItem ? collection.Id : 0;
        context.PhotoDisplays.Add(new PhotoDisplay
        {
            AlbumId = 3002, PhotoId = 2, PhotoDisplayCollectionId = collectionId, Order = 1
        });

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() =>
            context.SaveChangesAsync(TestContext.Current.CancellationToken));
        Assert.Equal("FK_AlbumItems_AlbumItems_AlbumId_CollectionId_ItemType",
            Assert.IsType<PostgresException>(exception.InnerException).ConstraintName);
    }

    [Theory]
    [InlineData(0, "UX_AlbumItems_PhotoDisplayCollectionId_Order")]
    [InlineData(-1, "CK_AlbumItems_Order_NonNegative")]
    public async Task PhotoDisplay_DatabaseRequiresUniqueNonNegativeOrderWithinCollection(
        int order, string constraintName)
    {
        await using var context = Fixture.CreateContext();
        await using var transaction =
            await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var collection = new PhotoDisplayCollection
        {
            AlbumId = 3002, Order = 0, DisplayMode = PhotoDisplayCollection.PhotoDisplayMode.Carousel
        };
        context.PhotoDisplays.Add(new PhotoDisplay
        {
            AlbumId = 3002, PhotoId = 2, PhotoDisplayCollection = collection, Order = 0
        });
        var display = new PhotoDisplay
        {
            AlbumId = 3002, PhotoId = 3, PhotoDisplayCollection = collection, Order = 1
        };
        context.PhotoDisplays.Add(display);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        display.Order = order;
        var exception = await Assert.ThrowsAsync<DbUpdateException>(() =>
            context.SaveChangesAsync(TestContext.Current.CancellationToken));
        Assert.Equal(constraintName, Assert.IsType<PostgresException>(exception.InnerException).ConstraintName);
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
