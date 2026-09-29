using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApplication6.Backend.Repositories;
using Xunit;

namespace WebApplication6.Tests.Tests.Integration.Repositories;

// Tests against test database.
public class AlbumRepositoryReadOnlyTests(TestDatabaseFixture dbFixture) : IClassFixture<TestDatabaseFixture>
{
    public TestDatabaseFixture Fixture { get; } = dbFixture;

    [Fact]
    public async Task GetAllAlbumsAsync_ReturnsExpectedValue()
    {
        await using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var albums = await repository.GetAllAlbumsAsync();
        var albumsList = albums.OrderBy(album => album.Id).ToList();

        Assert.NotNull(albums);
        Assert.Collection(albumsList,
            element1Inspector => Assert.Equal(1, element1Inspector.Id),
            element2Inspector => Assert.Equal(3002, element2Inspector.Id)
        );
    }

    [Fact]
    public async Task GetAlbumItems_ReturnsExistingAlbumOrderAndDisplaySettings()
    {
        await using var context = Fixture.CreateContext();
        var repository = new AlbumItemRepository(context);

        var photos = (await repository.GetAlbumItems(1))
            .Select(item => Assert.IsType<IAlbumItemRepository.PhotoDisplayAlbumItemDto>(item))
            .ToList();

        Assert.Equal(new[] { 4, 2, 3, 64 }, photos.Select(p => p.Content.Photo.Id));
        Assert.Equal(new[] { 0, 1, 2, 4 }, photos.Select(p => p.Order));
        Assert.All(photos, photo =>
        {
            Assert.NotNull(photo.Content.Photo.Image);
            Assert.True(photo.Content.DisplaysName);
            Assert.True(photo.Content.DisplaysDescription);
            Assert.True(photo.Content.DisplaysYearContentCreated);
        });
        Assert.Empty(await repository.GetAlbumItems(3002));
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
}
