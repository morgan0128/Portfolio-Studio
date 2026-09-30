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
