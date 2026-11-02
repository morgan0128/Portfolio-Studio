using System;
using System.Collections.Generic;
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

[TestClass(DisableParallelization = true)]
public class AlbumRepositoryTransactionalModifyDataTests(TransactionalTestDatabaseFixture dbFixture) : IClassFixture<TransactionalTestDatabaseFixture>, IDisposable
{
    public TransactionalTestDatabaseFixture Fixture { get; } = dbFixture;
    public void Dispose() => Fixture.Cleanup();

    /* DeleteAlbumByIdAsync */
    [Fact]
    public async Task DeleteAlbumByIdAsync_AlbumExists_AlbumInNavbarIsDeletedAndNavbarOrderingPropertiesMaintained()
    {
        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var album1 = new Album { Name = "One", Published = true, Id = 1, NavbarOrder = -1 };
        var album2 = new Album { Name = "Two", Published = true, Id = 2, NavbarOrder = 0 };
        var album3 = new Album { Name = "Three", Published = true, Id = 3, NavbarOrder = 2 };
        var album4 = new Album { Name = "Four", Published = true, Id = 4, NavbarOrder = 1 };
        var album5 = new Album { Name = "Five", Published = true, Id = 5, NavbarOrder = 4 };
        var album6 = new Album { Name = "Six", Published = true, Id = 6, NavbarOrder = 3 };
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

    // [Fact]
    // public async Task SwapAlbumsInNavAsync_

    /* UpdateNavOrderAsync */
    [Fact]
    public async Task UpdateNavOrderAsync_SuccessWithRemovingNavbarIncludedItems_AlbumRemovedFromNavbarAndOrderingPropertiesMaintained()
    {
        //TODO also check update.Item2 in this method
        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var album1 = new Album { Name = "One", Published = true, Id = 1, NavbarOrder = -1 };
        var album2 = new Album { Name = "Two", Published = true, Id = 2, NavbarOrder = 0 };
        var album3 = new Album { Name = "Three", Published = true, Id = 3, NavbarOrder = 2 };
        var album4 = new Album { Name = "Four", Published = true, Id = 4, NavbarOrder = 1 };
        var album5 = new Album { Name = "Five", Published = true, Id = 5, NavbarOrder = 4 };
        var album6 = new Album { Name = "Six", Published = true, Id = 6, NavbarOrder = 3 };
        var albums = new List<Album> { album1, album2, album3, album4, album5, album6 };
        context.AddRange(albums);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var albumDtosPrior = albums.ToDictionary(album => album.Id, CreateExpectedAlbumToDto);
        context.ChangeTracker.Clear();

        var update = await repository.UpdateNavOrderAsync(1, -1);
        Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success, update.Item1);

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
    
    // [Fact]
    // public async Task UpdateNavOrderAsync_SuccessWithNegativeNewNavOrder_AlwaysExcludesFromNavbarOnSuccess()
    // {
    //     using var context = Fixture.CreateContext();
    //     var repository = new AlbumRepository(context);
    //     
    //     var album1 = new Album { Name = "One", Published = true, Id = 1, NavbarOrder = -1 };
    //     var album2 = new Album { Name = "Two", Published = true, Id = 2, NavbarOrder = 0 };
    //     var album3 = new Album { Name = "Three", Published = true, Id = 3, NavbarOrder = 1 };
    //     var albums = new List<Album> { album1, album2, album3 };
    //     context.AddRange(albums);
    //     await context.SaveChangesAsync();
    //
    //     var albumDtosPrior = albums.ToDictionary(album => album.Id, CreateExpectedAlbumToDto);
    //     context.ChangeTracker.Clear();
    //     
    //     var update = await repository.UpdateNavOrderAsync(1, -1);
    //     Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success, update.Item1);
    //
    //     var albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
    //     Assert.Equivalent(new List<IAlbumRepository.AlbumDto>
    //     {
    //         albumDtosPrior[1],
    //         albumDtosPrior[2],
    //         albumDtosPrior[3]
    //     }, albumDtosPost, true);
    //
    //     update = await repository.UpdateNavOrderAsync(2, -1);
    //     Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success, update.Item1);
    //
    //     albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
    //     Assert.Equivalent(new List<IAlbumRepository.AlbumDto>
    //     {
    //         albumDtosPrior[1],
    //         albumDtosPrior[2] with { NavbarOrder = -1 },
    //         albumDtosPrior[3] with { NavbarOrder = 0 }
    //     }, albumDtosPost, true);
    // }

    private async Task<List<IAlbumRepository.AlbumDto>> GetAlbumStatesFromDatabaseAsync()
    {
        using var context = Fixture.CreateContext();
        var albums = await context.Albums
            .AsNoTracking()
            .ToListAsync();
        return [.. albums.Select(CreateExpectedAlbumToDto)];
    }

    /// <summary>
    ///     Test version of AlbumRepository.AlbumToDto(Album album), kept independent/hardcoded:
    ///     results in less chance of a dto mapping bug slipping through.
    /// </summary>
    /// <returns>
    ///     An AlbumDto, which represents the expected Album-to-dto mapping utilized in AlbumRepository.
    /// </returns>
    private static IAlbumRepository.AlbumDto CreateExpectedAlbumToDto(Album album)
    {
        return new IAlbumRepository.AlbumDto
        (
            Id: album.Id,
            Name: album.Name,
            Description: album.Description,
            NavTitle: album.NavTitle,
            Published: album.Published,
            LayoutPreset: album.LayoutPreset, NavbarOrder: album.NavbarOrder);
    }




}
