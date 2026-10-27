using System;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
        await using var context = await CreateIsolatedContextAsync();
        var albums = await CreatePresentationAlbumsAsync(context, 2);
        var repository = new AlbumRepository(context);

        Assert.Equal(-1, albums[0].NavbarOrder);
        Assert.False(albums[0].Published);
        Assert.Equal(PageLayoutPreset.Default, albums[0].LayoutPreset);
        Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.NotPublished,
            (await repository.UpdateNavOrderAsync(albums[0].Id, 0)).Item1);

        albums[1].NavTitle = "";
        var firstPublished = await repository.PublishAlbumAsync(albums[0].Id, 4);
        Assert.Equal(0, firstPublished!.NavbarOrder);
        var secondPublished = await repository.PublishAlbumAsync(albums[1].Id, null);
        Assert.Equal(-1, secondPublished!.NavbarOrder);
        Assert.Equal(albums[1].Name, secondPublished.NavTitle);
        Assert.Equal(PageLayoutPreset.Spooky,
            (await repository.SetLayoutPresetAsync(albums[0].Id, PageLayoutPreset.Spooky))!.LayoutPreset);

        context.ChangeTracker.Clear();
        var published = (await repository.GetPublishedAlbumsAsync()).ToList();
        Assert.Equal(2, published.Count);
        var inNav = Assert.Single(await repository.GetNavIncludedAlbumsAsync());
        Assert.Equal(albums[0].Id, inNav.Id);
        Assert.Equal(albums[0].NavTitle, inNav.NavTitle);
        Assert.Equal(PageLayoutPreset.Spooky, inNav.LayoutPreset);
        Assert.Equal(albums[1].Id, Assert.Single(await repository.GetPublishedNavExcludedAlbumsAsync()).Id);
        Assert.Equal(albums[0].Name, (await repository.GetAlbumByIdAsync(albums[0].Id))!.Name);
    }

    [Fact]
    public async Task AlbumPresentation_FullNavbar_AllowsReorderAndSwapButRejectsSixthEntry()
    {
        await using var context = await CreateIsolatedContextAsync();
        var albums = await CreatePresentationAlbumsAsync(context, 6);
        var repository = new AlbumRepository(context);
        foreach (var album in albums)
            Assert.True((await repository.PublishAlbumAsync(album.Id, null))!.Published);
        for (var index = 0; index < 5; index++)
            Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success,
                (await repository.UpdateNavOrderAsync(albums[index].Id, index)).Item1);

        Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.NavbarFull,
            (await repository.UpdateNavOrderAsync(albums[5].Id, 0)).Item1);
        Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success,
            (await repository.UpdateNavOrderAsync(albums[0].Id, 4)).Item1);
        Assert.Equal(IAlbumRepository.SwapInNavOutcome.Success,
            (await repository.SwapAlbumsInNavAsync(albums[0].Id, albums[1].Id)).Item1);

        context.ChangeTracker.Clear();
        var navbar = (await repository.GetNavIncludedAlbumsAsync()).ToList();
        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, navbar.Select(album => album.NavbarOrder));
        Assert.Equal(new[] { albums[0].Id, albums[2].Id, albums[3].Id, albums[4].Id, albums[1].Id },
            navbar.Select(album => album.Id));
    }

    [Fact]
    public async Task AlbumPresentation_UnpublishRemoveAndDelete_NormalizesNavbarAndPreservesPhotos()
    {
        await using var context = await CreateIsolatedContextAsync();
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
        Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success,
            (await repository.UpdateNavOrderAsync(albums[2].Id, -1)).Item1);
        Assert.True(albums[2].Published);
        Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success,
            (await repository.UpdateNavOrderAsync(albums[2].Id, 1)).Item1);
        Assert.True(await new AlbumRepository(context).DeleteAlbumByIdAsync(albums[1].Id));

        context.ChangeTracker.Clear();
        var remaining = Assert.Single(await repository.GetNavIncludedAlbumsAsync());
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



    [Theory]
    [InlineData("delete")]
    [InlineData("update")]
    [InlineData("swap")]
    [InlineData("publish")]
    [InlineData("unpublish")]
    public async Task NavbarOperation_RejectsCallerManagedTransaction(string operation)
    {
        await using var context = await CreateIsolatedContextAsync();
        await SeedNavbarAsync(context);
        await using var transaction = await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(() => RunNavbarOperationAsync(context, operation));

        Assert.Same(transaction, context.Database.CurrentTransaction);
    }

    [Theory]
    [InlineData("delete")]
    [InlineData("update")]
    [InlineData("swap")]
    [InlineData("publish")]
    [InlineData("unpublish")]
    [InlineData("publish-unchanged")]
    [InlineData("unpublish-unchanged")]
    public async Task NavbarOperation_CommitFailureThrowsAndRollsBack(string operation)
    {
        var interceptor = new CommitFailureInterceptor();
        await using var context = await CreateIsolatedContextAsync(interceptor);
        await SeedNavbarAsync(context);
        var repository = new AlbumRepository(context);
        var before = (await repository.GetAlbumsAsync()).OrderBy(album => album.Id).ToList();
        interceptor.FailCommit = true;

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => RunNavbarOperationAsync(context, operation));

        Assert.Equal("Simulated commit failure.", exception.Message);
        Assert.Null(context.Database.CurrentTransaction);
        context.ChangeTracker.Clear();
        Assert.Equal(before, (await repository.GetAlbumsAsync()).OrderBy(album => album.Id).ToList());
    }

    private async Task<ApplicationDbContext> CreateIsolatedContextAsync(params IInterceptor[] interceptors)
    {
        await using var source = Fixture.CreateContext();
        var images = await source.Images.AsNoTracking().ToListAsync(TestContext.Current.CancellationToken);
        var photos = await source.Photos.AsNoTracking().ToListAsync(TestContext.Current.CancellationToken);
        var context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(source.Database.GetConnectionString()).AddInterceptors(interceptors).Options);
        try
        {
            // Temporary tables isolate committed repository operations from the shared fixture.
            await context.Database.OpenConnectionAsync(TestContext.Current.CancellationToken);
            await context.Database.ExecuteSqlRawAsync(
                context.Database.GenerateCreateScript().Replace("CREATE TABLE ", "CREATE TEMP TABLE "),
                TestContext.Current.CancellationToken);
            context.Images.AddRange(images);
            context.Photos.AddRange(photos);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            return context;
        }
        catch
        {
            await context.DisposeAsync();
            throw;
        }
    }

    private static async Task SeedNavbarAsync(ApplicationDbContext context)
    {
        var albums = await CreatePresentationAlbumsAsync(context, 3);
        for (var index = 0; index < 2; index++)
        {
            albums[index].Published = true;
            albums[index].NavbarOrder = index;
        }
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private static Task RunNavbarOperationAsync(ApplicationDbContext context, string operation)
    {
        var repository = new AlbumRepository(context);
        return operation switch
        {
            "delete" => repository.DeleteAlbumByIdAsync(7000),
            "update" => repository.UpdateNavOrderAsync(7000, 1),
            "swap" => repository.SwapAlbumsInNavAsync(7000, 7001),
            "publish" => repository.PublishAlbumAsync(7002, 2),
            "unpublish" => repository.UnpublishAlbumAsync(7000),
            "publish-unchanged" => repository.PublishAlbumAsync(7000, null),
            "unpublish-unchanged" => repository.UnpublishAlbumAsync(7002),
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };
    }

    private sealed class CommitFailureInterceptor : DbTransactionInterceptor
    {
        public bool FailCommit { get; set; }

        public override ValueTask<InterceptionResult> TransactionCommittingAsync(DbTransaction transaction,
            TransactionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
        {
            if (FailCommit) throw new InvalidOperationException("Simulated commit failure.");
            return ValueTask.FromResult(result);
        }
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
