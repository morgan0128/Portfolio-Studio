using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication6.Backend.Models;
using WebApplication6.Backend.Repositories;
using WebApplication6.Tests.Tests.Integration.Repositories.AlbumRepositoryTransactionalModifyDataTests.Base;
using Xunit;

namespace WebApplication6.Tests.Tests.Integration.Repositories.AlbumRepositoryTransactionalModifyDataTests;

[Collection("TransactionalTests")]
public class UpdateNavOrderAsyncTests(TransactionalTestDatabaseFixture dbFixture)
    : AlbumRepositoryTransactionalModifyDataTestClass(dbFixture)
{

    [Theory]
    [InlineData(new[] { 2, 3 }, -1, new[] { 2, 3 })]
    [InlineData(new[] { 2, 3 }, 0, new[] { 1, 2, 3 })]
    [InlineData(new[] { 2, 3 }, 4, new[] { 2, 3, 1 })]
    [InlineData(new[] { 1, 3, 2 }, -1, new[] { 3, 2 })]
    [InlineData(new[] { 1, 3, 2 }, 0, new[] { 1, 3, 2 })]
    [InlineData(new[] { 1, 3, 2 }, 2, new[] { 3, 2, 1 })]
    [InlineData(new[] { 1, 3, 2 }, 4, new[] { 3, 2, 1 })]
    [InlineData(new[] { 2, 3, 4, 5, 1 }, -1, new[] { 2, 3, 4, 5 })]
    [InlineData(new[] { 2, 3, 4, 5, 1 }, 0, new[] { 1, 2, 3, 4, 5 })]
    public async Task SatisfactoryConditionsToModifyNavbar_OrderingUpdatedAndExpectedDtoNavbarReturnedWithSuccess(
        int[] initialNavbarIds, int newNavOrderAlbumOne, int[] expectedNavbarIds)
    {
        if (newNavOrderAlbumOne is < -1 or > 4) Assert.Fail();

        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var initialNavbar = initialNavbarIds.ToList();
        var album1 = new Album { Name = "One", Published = true, Id = 1, NavbarOrder = initialNavbar.IndexOf(1) };
        var album2 = new Album { Name = "Two", Published = true, Id = 2, NavbarOrder = initialNavbar.IndexOf(2) };
        var album3 = new Album { Name = "Three", Published = true, Id = 3, NavbarOrder = initialNavbar.IndexOf(3) };
        var album4 = new Album { Name = "Four", Published = true, Id = 4, NavbarOrder = initialNavbar.IndexOf(4) };
        var album5 = new Album { Name = "Five", Published = true, Id = 5, NavbarOrder = initialNavbar.IndexOf(5) };
        var albums = new List<Album> { album1, album2, album3, album4, album5 };
        context.AddRange(albums);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var albumDtosPrior = albums.ToDictionary(album => album.Id, CreateExpectedAlbumToDto);
        context.ChangeTracker.Clear();

        var update = await repository.UpdateNavOrderAsync(album1.Id, newNavOrderAlbumOne);
        Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success, update.Item1);

        var expectedNavbar = expectedNavbarIds.ToList();
        var expectedAlbumDtos = albumDtosPrior.Values
            .Select(album => album with { NavbarOrder = expectedNavbar.IndexOf(album.Id) })
            .ToList();
        Assert.Equal(expectedAlbumDtos.Where(album => album.NavbarOrder >= 0).OrderBy(album => album.NavbarOrder),
            update.Item2);

        var albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Equivalent(expectedAlbumDtos, albumDtosPost, true);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-2)]
    [InlineData(5)]
    [InlineData(int.MaxValue)]
    public async Task OutOfBoundsNewNavOrder_OperationCancelledAndExpectedDtoNavbarReturnedWithOutOfBounds(
        int newNavOrder)
    {
        if (newNavOrder is >= -1 and <= 4) Assert.Fail();

        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var album1 = new Album { Name = "One", Published = true, Id = 1, NavbarOrder = 0 };
        var album2 = new Album { Name = "Two", Published = true, Id = 2, NavbarOrder = 1 };
        var album3 = new Album { Name = "Three", Published = true, Id = 3, NavbarOrder = 2 };
        var album4 = new Album { Name = "Four", Published = true, Id = 4, NavbarOrder = 3 };
        var albums = new List<Album> { album1, album2, album3, album4 };
        context.AddRange(albums);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken); // Navbar is not yet full.

        var albumDtosPrior = albums.ToDictionary(album => album.Id, CreateExpectedAlbumToDto);
        context.ChangeTracker.Clear();

        /* Does not exist Album of Id 7. Failure conditions met: OutOfBounds; NotFound. */
        var update = await repository.UpdateNavOrderAsync(7, newNavOrder);
        Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.OutOfBounds, update.Item1);
        Assert.Equal(albumDtosPrior.Values.Where(album => album.NavbarOrder >= 0).OrderBy(album => album.NavbarOrder),
            update.Item2);
        var albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Equivalent(albumDtosPrior.Values, albumDtosPost, true);


        var album7 = new Album { Name = "Seven", Published = false, Id = 7, NavbarOrder = -1 };
        context.Add(album7);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        albumDtosPrior.Add(album7.Id, CreateExpectedAlbumToDto(album7));
        /* Now exists !Published Album of Id 7. Failure conditions met: OutOfBounds; NotPublished. */
        update = await repository.UpdateNavOrderAsync(7, newNavOrder);
        Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.OutOfBounds, update.Item1);
        Assert.Equal(albumDtosPrior.Values.Where(album => album.NavbarOrder >= 0).OrderBy(album => album.NavbarOrder),
            update.Item2);
        albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Equivalent(albumDtosPrior.Values, albumDtosPost, true);


        album7.Published = true;
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        albumDtosPrior[7] = albumDtosPrior[7] with { Published = true };
        /* Album of Id 7 now satisfies Published. Failure conditions met: OutOfBounds. */
        update = await repository.UpdateNavOrderAsync(7, newNavOrder);
        Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.OutOfBounds, update.Item1);
        Assert.Equal(albumDtosPrior.Values.Where(album => album.NavbarOrder >= 0).OrderBy(album => album.NavbarOrder),
            update.Item2);
        albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Equivalent(albumDtosPrior.Values, albumDtosPost, true);



        var album5 = new Album { Name = "Five", Published = true, Id = 5, NavbarOrder = 4 };
        context.Add(album5);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        albumDtosPrior.Add(album5.Id, CreateExpectedAlbumToDto(album5));
        /* Navbar is now full. Failure conditions met: OutOfBounds; NavbarFull (with Album of id not already in Navbar). */
        update = await repository.UpdateNavOrderAsync(7, newNavOrder);
        Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.OutOfBounds, update.Item1);
        Assert.Equal(albumDtosPrior.Values.Where(album => album.NavbarOrder >= 0).OrderBy(album => album.NavbarOrder),
            update.Item2);
        albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Equivalent(albumDtosPrior.Values, albumDtosPost, true);



        album7.Published = false;
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        albumDtosPrior[7] = albumDtosPrior[7] with { Published = false };
        /* Failure conditions met: OutOfBounds; NavbarFull (with Album of id not already in Navbar), NotPublished */
        update = await repository.UpdateNavOrderAsync(7, newNavOrder);
        Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.OutOfBounds, update.Item1);
        Assert.Equal(albumDtosPrior.Values.Where(album => album.NavbarOrder >= 0).OrderBy(album => album.NavbarOrder),
            update.Item2);
        albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Equivalent(albumDtosPrior.Values, albumDtosPost, true);



        context.Remove(album7);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        albumDtosPrior.Remove(album7.Id);
        /* Failure conditions met: OutOfBounds; NavbarFull (with Album of id not already in Navbar), NotFound */
        update = await repository.UpdateNavOrderAsync(7, newNavOrder);
        Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.OutOfBounds, update.Item1);
        Assert.Equal(albumDtosPrior.Values.Where(album => album.NavbarOrder >= 0).OrderBy(album => album.NavbarOrder),
            update.Item2);
        albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Equivalent(albumDtosPrior.Values, albumDtosPost, true);
    }

    [Fact]
    public async Task
        IncludeInNavWithNotOutOfBoundsAndAlbumDoesNotExist_OperationCancelledAndExpectedDtoNavbarReturnedWithNotFound()
    {
        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var update = await repository.UpdateNavOrderAsync(7, 0);
        Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.NotFound, update.Item1);
        Assert.Empty(update.Item2);

        var albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Empty(albumDtosPost);

        var album1 = new Album { Name = "One", Published = true, Id = 1, NavbarOrder = 0 };
        var album2 = new Album { Name = "Two", Published = true, Id = 2, NavbarOrder = 1 };
        var album3 = new Album { Name = "Three", Published = true, Id = 3, NavbarOrder = 2 };
        var album4 = new Album { Name = "Four", Published = true, Id = 4, NavbarOrder = 3 };
        var album5 = new Album { Name = "Five", Published = true, Id = 5, NavbarOrder = 4 };
        var albums = new List<Album> { album1, album2, album3, album4, album5 };
        context.AddRange(albums);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken); // Our 'test navbar' is full.

        var albumDtosPrior = albums.ToDictionary(album => album.Id, CreateExpectedAlbumToDto);
        context.ChangeTracker.Clear();

        update = await repository.UpdateNavOrderAsync(7, 0);
        Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.NotFound, update.Item1);
        Assert.Equal(albumDtosPrior.Values.OrderBy(album => album.NavbarOrder), update.Item2);

        albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Equivalent(albumDtosPrior.Values, albumDtosPost, true);
    }

    [Fact]
    public async Task
        IncludeInNavWithNotOutOfBoundsAndAlbumIsNotPublished_OperationCancelledAndExpectedDtoNavbarReturnedWithNotPublished()
    {
        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var album1 = new Album { Name = "One", Published = true, Id = 1, NavbarOrder = 0 };
        var album2 = new Album { Name = "Two", Published = true, Id = 2, NavbarOrder = 1 };
        var album3 = new Album { Name = "Three", Published = true, Id = 3, NavbarOrder = 2 };
        var album4 = new Album { Name = "Four", Published = true, Id = 4, NavbarOrder = 3 };
        var album5 = new Album { Name = "Five", Published = false, Id = 5, NavbarOrder = -1 };
        var albums = new List<Album> { album1, album2, album3, album4, album5 };
        context.AddRange(albums);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken); // Our 'test navbar' is not yet full.

        var albumDtosPrior = albums.ToDictionary(album => album.Id, CreateExpectedAlbumToDto);
        context.ChangeTracker.Clear();

        var update = await repository.UpdateNavOrderAsync(5, 0);
        Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.NotPublished, update.Item1);
        Assert.Equal(albumDtosPrior.Values.Where(album => album.NavbarOrder >= 0).OrderBy(album => album.NavbarOrder),
            update.Item2);

        var albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Equivalent(albumDtosPrior.Values, albumDtosPost, true);

        var album6 = new Album { Name = "Six", Published = true, Id = 6, NavbarOrder = 4 };
        context.Add(album6);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken); // Our 'test navbar' is full.

        albumDtosPrior.Add(album6.Id, CreateExpectedAlbumToDto(album6));

        update = await repository.UpdateNavOrderAsync(5, 0);
        Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.NotPublished, update.Item1);
        Assert.Equal(albumDtosPrior.Values.Where(album => album.NavbarOrder >= 0).OrderBy(album => album.NavbarOrder),
            update.Item2);

        albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Equivalent(albumDtosPrior.Values, albumDtosPost, true);
    }

    [Fact]
    public async Task
        IncludeInNavWithAllConditionsSatisfiedExceptNavbarIsFull_OperationCancelledAndExpectedDtoNavbarReturnedWithNavbarFull()
    {
        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var album1 = new Album { Name = "One", Published = true, Id = 1, NavbarOrder = 0 };
        var album2 = new Album { Name = "Two", Published = true, Id = 2, NavbarOrder = 1 };
        var album3 = new Album { Name = "Three", Published = true, Id = 3, NavbarOrder = 2 };
        var album4 = new Album { Name = "Four", Published = true, Id = 4, NavbarOrder = 3 };
        var album5 = new Album { Name = "Five", Published = true, Id = 5, NavbarOrder = 4 };
        var album6 = new Album { Name = "Six", Published = true, Id = 6, NavbarOrder = -1 };
        var albums = new List<Album> { album1, album2, album3, album4, album5, album6 };
        context.AddRange(albums);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken); // Our 'test navbar' is full.

        var albumDtosPrior = albums.ToDictionary(album => album.Id, CreateExpectedAlbumToDto);
        context.ChangeTracker.Clear();

        var update = await repository.UpdateNavOrderAsync(6, 0);
        Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.NavbarFull, update.Item1);
        Assert.Equal(albumDtosPrior.Values.Where(album => album.NavbarOrder >= 0).OrderBy(album => album.NavbarOrder),
            update.Item2);

        var albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Equivalent(albumDtosPrior.Values, albumDtosPost, true);
    }

    [Fact]
    public async Task ExcludePublishedAlbumFromNav_ReturnedTaskResultAlwaysIndicatesSuccess()
    {
        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var album1 = new Album { Name = "One", Published = true, Id = 1, NavbarOrder = 0 };
        var album2 = new Album { Name = "Two", Published = true, Id = 2, NavbarOrder = 1 };
        var album3 = new Album { Name = "Three", Published = true, Id = 3, NavbarOrder = 2 };
        var album4 = new Album { Name = "Four", Published = true, Id = 4, NavbarOrder = 3 };
        var album5 = new Album { Name = "Five", Published = true, Id = 5, NavbarOrder = 4 };
        var album6 = new Album { Name = "Six", Published = true, Id = 6, NavbarOrder = -1 };
        var albums = new List<Album> { album1, album2, album3, album4, album5, album6 };
        context.AddRange(albums);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken); // Our 'test navbar' is full.

        var albumDtosPrior = albums.ToDictionary(album => album.Id, CreateExpectedAlbumToDto);
        context.ChangeTracker.Clear();

        var update = await repository.UpdateNavOrderAsync(6, -1);
        Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success, update.Item1);
        Assert.Equal(albumDtosPrior.Values.Where(album => album.NavbarOrder >= 0).OrderBy(album => album.NavbarOrder),
            update.Item2);

        var albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Equivalent(albumDtosPrior.Values, albumDtosPost, true);

        update = await repository.UpdateNavOrderAsync(3, -1);
        Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success, update.Item1);

        var expectedAlbumDtos = new List<IAlbumRepository.AlbumDto>
        {
            albumDtosPrior[1],
            albumDtosPrior[2],
            albumDtosPrior[3] with { NavbarOrder = -1 },
            albumDtosPrior[4] with { NavbarOrder = 2 },
            albumDtosPrior[5] with { NavbarOrder = 3 },
            albumDtosPrior[6]
        };
        Assert.Equal(expectedAlbumDtos.Where(album => album.NavbarOrder >= 0).OrderBy(album => album.NavbarOrder),
            update.Item2);

        albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Equivalent(expectedAlbumDtos, albumDtosPost, true);
    }

    [Fact]
    public async Task
        SuccessfulExcludeFromNavbarOperations_OrderingPropertiesMaintainedAndExpectedDtoNavbarReturnedWithSuccess()
    {
        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var album1 = new Album { Name = "One", Published = true, Id = 1, NavbarOrder = -1 };
        var album2 = new Album
            { Name = "Two", Published = true, Id = 2, NavbarOrder = 0, Description = "descriptive description" };
        var album3 = new Album { Name = "Three", Published = true, Id = 3, NavbarOrder = 2, NavTitle = "3Three3" };
        var album4 = new Album
            { Name = "Four", Published = true, Id = 4, NavbarOrder = 1, LayoutPreset = PageLayoutPreset.Cozy };
        var album5 = new Album
        {
            Name = "Five", Published = true, Id = 5, NavbarOrder = 4, Description = "I am 5",
            NavTitle = "Navigate to 5", LayoutPreset = PageLayoutPreset.Spooky
        };
        var album6 = new Album { Name = "Six", Published = true, Id = 6, NavbarOrder = 3 };
        var albums = new List<Album> { album1, album2, album3, album4, album5, album6 };
        context.AddRange(albums);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var albumDtosPrior = albums.ToDictionary(album => album.Id, CreateExpectedAlbumToDto);
        context.ChangeTracker.Clear();

        var update = await repository.UpdateNavOrderAsync(1, -1);
        Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success, update.Item1);
        Assert.Collection(update.Item2,
            albumOfOrder0 => { Assert.Equal(albumDtosPrior[2], albumOfOrder0); },
            albumOfOrder1 => { Assert.Equal(albumDtosPrior[4], albumOfOrder1); },
            albumOfOrder2 => { Assert.Equal(albumDtosPrior[3], albumOfOrder2); },
            albumOfOrder3 => { Assert.Equal(albumDtosPrior[6], albumOfOrder3); },
            albumOfOrder4 => { Assert.Equal(albumDtosPrior[5], albumOfOrder4); }
        );

        var albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Equivalent(new List<IAlbumRepository.AlbumDto>
        {
            albumDtosPrior[1],
            albumDtosPrior[2],
            albumDtosPrior[3],
            albumDtosPrior[4],
            albumDtosPrior[5],
            albumDtosPrior[6]
        }, albumDtosPost, true);

        update = await repository.UpdateNavOrderAsync(2, -1);
        Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success, update.Item1);
        Assert.Collection(update.Item2,
            albumOfOrder0 => { Assert.Equal(albumDtosPrior[4] with { NavbarOrder = 0 }, albumOfOrder0); },
            albumOfOrder1 => { Assert.Equal(albumDtosPrior[3] with { NavbarOrder = 1 }, albumOfOrder1); },
            albumOfOrder2 => { Assert.Equal(albumDtosPrior[6] with { NavbarOrder = 2 }, albumOfOrder2); },
            albumOfOrder3 => { Assert.Equal(albumDtosPrior[5] with { NavbarOrder = 3 }, albumOfOrder3); }
        );

        albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Equivalent(new List<IAlbumRepository.AlbumDto>
        {
            albumDtosPrior[1],
            albumDtosPrior[2] with { NavbarOrder = -1 },
            albumDtosPrior[3] with { NavbarOrder = 1 },
            albumDtosPrior[4] with { NavbarOrder = 0 },
            albumDtosPrior[5] with { NavbarOrder = 3 },
            albumDtosPrior[6] with { NavbarOrder = 2 }
        }, albumDtosPost, true);

        update = await repository.UpdateNavOrderAsync(6, -1);
        Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success, update.Item1);
        Assert.Collection(update.Item2,
            albumOfOrder0 => { Assert.Equal(albumDtosPrior[4] with { NavbarOrder = 0 }, albumOfOrder0); },
            albumOfOrder1 => { Assert.Equal(albumDtosPrior[3] with { NavbarOrder = 1 }, albumOfOrder1); },
            albumOfOrder2 => { Assert.Equal(albumDtosPrior[5] with { NavbarOrder = 2 }, albumOfOrder2); }
        );

        albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Equivalent(new List<IAlbumRepository.AlbumDto>
        {
            albumDtosPrior[1],
            albumDtosPrior[2] with { NavbarOrder = -1 },
            albumDtosPrior[3] with { NavbarOrder = 1 },
            albumDtosPrior[4] with { NavbarOrder = 0 },
            albumDtosPrior[5] with { NavbarOrder = 2 },
            albumDtosPrior[6] with { NavbarOrder = -1 }
        }, albumDtosPost, true);

        update = await repository.UpdateNavOrderAsync(4, -1);
        Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success, update.Item1);
        Assert.Collection(update.Item2,
            albumOfOrder0 => { Assert.Equal(albumDtosPrior[3] with { NavbarOrder = 0 }, albumOfOrder0); },
            albumOfOrder1 => { Assert.Equal(albumDtosPrior[5] with { NavbarOrder = 1 }, albumOfOrder1); }
        );

        albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Equivalent(new List<IAlbumRepository.AlbumDto>
        {
            albumDtosPrior[1],
            albumDtosPrior[2] with { NavbarOrder = -1 },
            albumDtosPrior[3] with { NavbarOrder = 0 },
            albumDtosPrior[4] with { NavbarOrder = -1 },
            albumDtosPrior[5] with { NavbarOrder = 1 },
            albumDtosPrior[6] with { NavbarOrder = -1 }
        }, albumDtosPost, true);

        update = await repository.UpdateNavOrderAsync(5, -1);
        Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success, update.Item1);
        Assert.Collection(update.Item2,
            albumOfOrder0 => { Assert.Equal(albumDtosPrior[3] with { NavbarOrder = 0 }, albumOfOrder0); });

        albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Equivalent(new List<IAlbumRepository.AlbumDto>
        {
            albumDtosPrior[1],
            albumDtosPrior[2] with { NavbarOrder = -1 },
            albumDtosPrior[3] with { NavbarOrder = 0 },
            albumDtosPrior[4] with { NavbarOrder = -1 },
            albumDtosPrior[5] with { NavbarOrder = -1 },
            albumDtosPrior[6] with { NavbarOrder = -1 }
        }, albumDtosPost, true);

        update = await repository.UpdateNavOrderAsync(3, -1);
        Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success, update.Item1);
        Assert.Empty(update.Item2);

        albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Equivalent(new List<IAlbumRepository.AlbumDto>
        {
            albumDtosPrior[1],
            albumDtosPrior[2] with { NavbarOrder = -1 },
            albumDtosPrior[3] with { NavbarOrder = -1 },
            albumDtosPrior[4] with { NavbarOrder = -1 },
            albumDtosPrior[5] with { NavbarOrder = -1 },
            albumDtosPrior[6] with { NavbarOrder = -1 }
        }, albumDtosPost, true);
    }
}