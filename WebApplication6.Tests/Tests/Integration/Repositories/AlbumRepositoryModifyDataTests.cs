using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebApplication6.Backend.Data;
using WebApplication6.Backend.Models;
using WebApplication6.Backend.Repositories;
using Xunit;

namespace WebApplication6.Tests.Tests.Integration.Repositories;

public class AlbumRepositoryModifyDataTests(TestDatabaseFixture dbFixture) : IClassFixture<TestDatabaseFixture>
{
    public TestDatabaseFixture Fixture { get; } = dbFixture;

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

    [Fact]
    public async Task AlbumPresentation_PublishAndQuery_UsesAlbumIdentityAndDefaults()
    {
        await using var context = Fixture.CreateContext();
        await using var transaction =
            await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var albums = await CreatePresentationAlbumsAsync(context, 2);
        var repository = new AlbumRepository(context);

        Assert.Equal(-1, albums[0].NavbarOrder);
        Assert.False(albums[0].Published);
        Assert.Equal(PageLayoutPreset.Default, albums[0].LayoutPreset);
        Assert.Equal(IAlbumRepository.AssignNavOrderOutcome.NotPublished,
            (await repository.AssignAlbumInNavAsync(albums[0].Id, 0)).Item1);

        albums[1].NavTitle = "";
        var firstPublished = await repository.PublishAlbumAsync(albums[0].Id, 4);
        Assert.Equal(0, firstPublished!.NavbarOrder);
        var secondPublished = await repository.PublishAlbumAsync(albums[1].Id, null);
        Assert.Equal(-1, secondPublished!.NavbarOrder);
        Assert.Equal(albums[1].Name, secondPublished.NavTitle);
        Assert.Equal(PageLayoutPreset.Spooky,
            (await repository.SetLayoutPresetAsync(albums[0].Id, PageLayoutPreset.Spooky))!.LayoutPreset);

        context.ChangeTracker.Clear();
        var published = (await repository.GetAllPublishedAsync()).ToList();
        Assert.Equal(2, published.Count);
        var inNav = Assert.Single(await repository.GetPublishedInNavbarOrdered());
        Assert.Equal(albums[0].Id, inNav.Id);
        Assert.Equal(albums[0].NavTitle, inNav.NavTitle);
        Assert.Equal(PageLayoutPreset.Spooky, inNav.LayoutPreset);
        Assert.Equal(albums[1].Id, Assert.Single(await repository.GetPublishedNotInNavbar()).Id);
        Assert.Equal(albums[0].Name, (await repository.GetAlbumByIdAsync(albums[0].Id))!.Name);
    }

    [Fact]
    public async Task AlbumPresentation_FullNavbar_AllowsReorderAndSwapButRejectsSixthEntry()
    {
        await using var context = Fixture.CreateContext();
        await using var transaction =
            await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var albums = await CreatePresentationAlbumsAsync(context, 6);
        var repository = new AlbumRepository(context);
        foreach (var album in albums)
            Assert.True((await repository.PublishAlbumAsync(album.Id, null))!.Published);
        for (var index = 0; index < 5; index++)
            Assert.Equal(IAlbumRepository.AssignNavOrderOutcome.Success,
                (await repository.AssignAlbumInNavAsync(albums[index].Id, index)).Item1);

        Assert.Equal(IAlbumRepository.AssignNavOrderOutcome.NavbarFull,
            (await repository.AssignAlbumInNavAsync(albums[5].Id, 0)).Item1);
        Assert.Equal(IAlbumRepository.AssignNavOrderOutcome.Success,
            (await repository.AssignAlbumInNavAsync(albums[0].Id, 4)).Item1);
        Assert.Equal(IAlbumRepository.SwapInNavOutcome.Success,
            (await repository.SwapAlbumsInNavOrderAsync(albums[0].Id, albums[1].Id)).Item1);

        context.ChangeTracker.Clear();
        var navbar = (await repository.GetPublishedInNavbarOrdered()).ToList();
        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, navbar.Select(album => album.NavbarOrder));
        Assert.Equal(new[] { albums[0].Id, albums[2].Id, albums[3].Id, albums[4].Id, albums[1].Id },
            navbar.Select(album => album.Id));
    }

    [Fact]
    public async Task AlbumPresentation_UnpublishRemoveAndDelete_NormalizesNavbarAndPreservesPhotos()
    {
        await using var context = Fixture.CreateContext();
        await using var transaction =
            await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var albums = await CreatePresentationAlbumsAsync(context, 3);
        context.PhotoDisplays.Add(new PhotoDisplay
        {
            AlbumId = albums[1].Id, PhotoId = 2, Order = 0,
            PhotoDisplayCollection = new PhotoDisplayCollection { AlbumId = albums[1].Id, Order = 0 }
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var repository = new AlbumRepository(context);
        for (var index = 0; index < albums.Length; index++)
            Assert.Equal(index, (await repository.PublishAlbumAsync(albums[index].Id, index))!.NavbarOrder);

        var unpublished = await repository.UnpublishAlbumAsync(albums[0].Id);
        Assert.False(unpublished!.Published);
        Assert.Equal(-1, unpublished.NavbarOrder);
        Assert.Equal(IAlbumRepository.AssignNavOrderOutcome.Success,
            (await repository.AssignAlbumInNavAsync(albums[2].Id, -1)).Item1);
        Assert.True(albums[2].Published);
        Assert.Equal(IAlbumRepository.AssignNavOrderOutcome.Success,
            (await repository.AssignAlbumInNavAsync(albums[2].Id, 1)).Item1);
        Assert.True(await new AlbumRepository(context).DeleteAlbumByIdAsync(albums[1].Id));

        context.ChangeTracker.Clear();
        var remaining = Assert.Single(await repository.GetPublishedInNavbarOrdered());
        Assert.Equal(albums[2].Id, remaining.Id);
        Assert.Equal(0, remaining.NavbarOrder);
        Assert.NotNull(await context.Photos.FindAsync([2], TestContext.Current.CancellationToken));
        Assert.False(await context.PhotoDisplays.AnyAsync(display => display.AlbumId == albums[1].Id,
            TestContext.Current.CancellationToken));
        Assert.False(await context.PhotoDisplayCollections.AnyAsync(collection => collection.AlbumId == albums[1].Id,
            TestContext.Current.CancellationToken));
        Assert.Null(await repository.GetAlbumByIdAsync(albums[1].Id));
    }

    [Fact]
    public async Task AlbumPresentation_ExplicitFirstNavbarPosition_IsSavedOnInsert()
    {
        await using var context = Fixture.CreateContext();
        await using var transaction =
            await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var album = new Album { Id = 7000, Name = "First", Published = true, NavbarOrder = 0 };
        context.Albums.Add(album);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        context.ChangeTracker.Clear();

        Assert.Equal(0,
            (await context.Albums.FindAsync([album.Id], TestContext.Current.CancellationToken))!.NavbarOrder);
    }

    [Theory]
    [InlineData(false, 0, "valid")]
    [InlineData(true, -2, "valid")]
    [InlineData(true, 5, "valid")]
    [InlineData(false, -1, "123456789012345678901")]
    public async Task AlbumPresentation_DatabaseRejectsInvalidPresentation(bool published, int order, string navTitle)
    {
        await using var context = Fixture.CreateContext();
        await using var transaction =
            await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        context.Albums.Add(new Album
        {
            Id = 7000, Name = "Invalid presentation", Published = published,
            NavbarOrder = order, NavTitle = navTitle
        });

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            context.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AlbumPresentation_DatabaseRejectsDuplicateNavbarPosition()
    {
        await using var context = Fixture.CreateContext();
        await using var transaction =
            await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var albums = await CreatePresentationAlbumsAsync(context, 2);
        foreach (var album in albums)
        {
            album.Published = true;
            album.NavbarOrder = 0;
        }

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            context.SaveChangesAsync(TestContext.Current.CancellationToken));
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

    private static async Task<Album[]> CreatePresentationAlbumsAsync(ApplicationDbContext context, int count)
    {
        var albums = Enumerable.Range(0, count).Select(index => new Album
        {
            Id = 7000 + index,
            Name = $"Gallery {index}",
            NavTitle = $"Gallery {index}"
        }).ToArray();
        context.Albums.AddRange(albums);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        return albums;
    }
}
