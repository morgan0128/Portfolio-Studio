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

public class AlbumRepositoryTransactionalModifyDataTests(TransactionalTestDatabaseFixture dbFixture) : IClassFixture<TransactionalTestDatabaseFixture>, IDisposable
{
    public TransactionalTestDatabaseFixture Fixture { get; } = dbFixture;
    public void Dispose() => Fixture.Cleanup();

    /* DeleteAlbumByIdAsync */
    [Fact]
    public async Task DeleteAlbumByIdAsync_AlbumExists_AlbumInNavbarIsDeletedAndNavbarPropertiesMaintained()
    {
        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);
        
        var album = await repository.GetAlbumByIdAsync(3);
        Assert.NotNull(album);
        
        var navAlbumDtosPrior = (await repository.GetNavIncludedAlbumsAsync()).ToList();
        Assert.Collection(navAlbumDtosPrior,
            element1Inspector =>
            {
                Assert.Equal(1, element1Inspector.Id);
                Assert.Equal(0, element1Inspector.NavbarOrder);
            },
            element2Inspector =>
            {
                Assert.Equal(3, element2Inspector.Id);
                Assert.Equal(1, element2Inspector.NavbarOrder);
            },
            element3Inspector =>
            {
                Assert.Equal(4, element3Inspector.Id);
                Assert.Equal(2, element3Inspector.NavbarOrder);
            });
        
        var success = await repository.DeleteAlbumByIdAsync(3);
        Assert.True(success);
        album = await repository.GetAlbumByIdAsync(3);
        Assert.Null(album);
        
        var navAlbumDtosPost = (await repository.GetNavIncludedAlbumsAsync()).ToList();
        Assert.Collection(navAlbumDtosPost,
            element1Inspector =>
            {
                Assert.Equal(1, element1Inspector.Id);
                Assert.Equal(0, element1Inspector.NavbarOrder);
            },            
            element2Inspector =>
            {
                Assert.Equal(4, element2Inspector.Id);
                Assert.Equal(1, element2Inspector.NavbarOrder);
            });
    }
    
    [Fact]
    public async Task DeleteAlbumByIdAsync_AlbumDoesNotExist_CancelsOperationReturnsFalse()
    {
        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var albumCountPrior = await repository.GetAlbumsCountAsync();
        
        var album = await repository.GetAlbumByIdAsync(-1);
        Assert.Null(album);
        
        var success = await repository.DeleteAlbumByIdAsync(-1);
        Assert.False(success);

        var albumCountPost = await repository.GetAlbumsCountAsync();
        Assert.Equal(albumCountPrior, albumCountPost);
    }
    
    /* UpdateNavOrderAsync */
    // TODO
    // [Fact]
    // public async Task UpdateNavOrderAsync_NegativeNewNavOrder_AlwaysExcludesFromNavbarOnSuccess()
    // {
    //     using var context = Fixture.CreateContext();
    //     var repository = new AlbumRepository(context);
    //
    //     var navAlbums = (await repository.GetNavIncludedAlbumsAsync()).ToList();
    //     if (navAlbums.Count() != 0)
    //     {
    //         var exclude = navAlbums[0];
    //         await repository.UpdateNavOrderAsync(exclude.Id, -1);
    //     }
    // }
    
    


}
