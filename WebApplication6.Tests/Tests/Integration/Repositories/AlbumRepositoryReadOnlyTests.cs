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

    
    /* GetAllAlbumsAsync */
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
    
    /* GetAllAlbumsIdsAsync */
    
    /* GetTotalNumberAlbumsAsync */
    
    /* GetAlbumByIdAsync */




}
