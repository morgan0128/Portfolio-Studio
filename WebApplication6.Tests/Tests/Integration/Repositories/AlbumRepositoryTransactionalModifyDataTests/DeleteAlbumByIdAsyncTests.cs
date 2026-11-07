using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication6.Backend.Models;
using WebApplication6.Backend.Repositories;
using WebApplication6.Tests.Tests.Integration.Repositories.AlbumRepositoryTransactionalModifyDataTests.Base;
using Xunit;

namespace WebApplication6.Tests.Tests.Integration.Repositories.AlbumRepositoryTransactionalModifyDataTests;

[Collection("TransactionalTests")]
public class DeleteAlbumByIdAsyncTests(TransactionalTestDatabaseFixture dbFixture) : AlbumRepositoryTransactionalModifyDataTestClass(dbFixture)
{
    /* DeleteAlbumByIdAsync */
    [Fact]
    public async Task DeleteAlbumByIdAsync_AlbumExists_AlbumInNavbarIsDeletedAndNavbarOrderingPropertiesMaintained()
    {
        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var album1 = new Album { Name = "One", Published = true, Id = 1, NavbarOrder = -1, Description = "descriptive description" };
        var album2 = new Album { Name = "Two", Published = true, Id = 2, NavbarOrder = 0, Description = "j", LayoutPreset = PageLayoutPreset.Cozy };
        var album3 = new Album { Name = "Three", Published = true, Id = 3, NavbarOrder = 2 };
        var album4 = new Album { Name = "Four", Published = true, Id = 4, NavbarOrder = 1, Description = "I am 4", NavTitle = "Navigate to 4", LayoutPreset = PageLayoutPreset.Spooky };
        var album5 = new Album { Name = "Five", Published = true, Id = 5, NavbarOrder = 4, NavTitle = "title for navbar" };
        var album6 = new Album { Name = "Six", Published = true, Id = 6, NavbarOrder = 3, Description = "description_here", NavTitle = "navigation" };
        var album7 = new Album { Name = "Seven", Published = false, Id = 7, NavbarOrder = -1 };
        var albums = new List<Album> { album1, album2, album3, album4, album5, album6, album7 };
        context.AddRange(albums);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var albumDtosPrior = albums.ToDictionary(album => album.Id, CreateExpectedAlbumToDto);
        context.ChangeTracker.Clear();

        var success = await repository.DeleteAlbumByIdAsync(3);
        Assert.True(success);

        var albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Equivalent(new List<IAlbumRepository.AlbumDto>
        {
            albumDtosPrior[1],
            albumDtosPrior[2],
            albumDtosPrior[4],
            albumDtosPrior[5] with { NavbarOrder = 3 },
            albumDtosPrior[6] with { NavbarOrder = 2 },
            albumDtosPrior[7]
        }, albumDtosPost, true);

        success = await repository.DeleteAlbumByIdAsync(2);
        Assert.True(success);

        albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Equivalent(new List<IAlbumRepository.AlbumDto>
        {
            albumDtosPrior[1],
            albumDtosPrior[4] with { NavbarOrder = 0 },
            albumDtosPrior[5] with { NavbarOrder = 2 },
            albumDtosPrior[6] with { NavbarOrder = 1 },
            albumDtosPrior[7]
        }, albumDtosPost, true);

        success = await repository.DeleteAlbumByIdAsync(6);
        Assert.True(success);

        albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Equivalent(new List<IAlbumRepository.AlbumDto>
        {
            albumDtosPrior[1],
            albumDtosPrior[4] with { NavbarOrder = 0 },
            albumDtosPrior[5] with { NavbarOrder = 1 },
            albumDtosPrior[7]
        }, albumDtosPost, true);

        success = await repository.DeleteAlbumByIdAsync(7);
        Assert.True(success);

        albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Equivalent(new List<IAlbumRepository.AlbumDto>
        {
            albumDtosPrior[1],
            albumDtosPrior[4] with { NavbarOrder = 0 },
            albumDtosPrior[5] with { NavbarOrder = 1 }
        }, albumDtosPost, true);

        success = await repository.DeleteAlbumByIdAsync(4);
        Assert.True(success);

        albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Equivalent(new List<IAlbumRepository.AlbumDto>
        {
            albumDtosPrior[1],
            albumDtosPrior[5] with { NavbarOrder = 0 }
        }, albumDtosPost, true);

        success = await repository.DeleteAlbumByIdAsync(5);
        Assert.True(success);

        albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Equivalent(new List<IAlbumRepository.AlbumDto>
        { 
            albumDtosPrior[1]
        }, albumDtosPost, true);
    }

    [Fact]
    public async Task DeleteAlbumByIdAsync_AlbumDoesNotExist_CancelsOperationReturnsFalse()
    {
        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var album1 = new Album { Name = "One", Published = true, Id = 1, NavbarOrder = -1 };
        var album2 = new Album { Name = "Two", Published = true, Id = 2, NavbarOrder = -1 };
        var album3 = new Album { Name = "Three", Published = true, Id = 3, NavbarOrder = -1 };
        var albums = new List<Album> { album1, album2, album3 };
        context.AddRange(albums);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var albumDtosPrior = albums.Select(CreateExpectedAlbumToDto).ToList();
        context.ChangeTracker.Clear();

        var album = await repository.GetAlbumByIdAsync(-1);
        Assert.Null(album);
        var success = await repository.DeleteAlbumByIdAsync(-1);
        Assert.False(success);

        var albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
        Assert.Equivalent(albumDtosPrior, albumDtosPost, true);
    }
}