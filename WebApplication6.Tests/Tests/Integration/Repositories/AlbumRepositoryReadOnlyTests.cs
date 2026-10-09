using System.Linq;
using System.Threading.Tasks;
using WebApplication6.Backend.Models;
using WebApplication6.Backend.Repositories;
using Xunit;

namespace WebApplication6.Tests.Tests.Integration.Repositories;

// Tests against test database.
public class AlbumRepositoryReadOnlyTests(TestDatabaseFixture dbFixture) : IClassFixture<TestDatabaseFixture>
{
    public TestDatabaseFixture Fixture { get; } = dbFixture;
    
    /* GetAllAlbumsAsync */
    [Fact]
    public async Task GetAllAlbumsAsync_ReturnsExpected()
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
    [Fact]
    public async Task GetAllAlbumIdsAsync_ReturnsExpected()
    {
        await using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var albumIds = await repository.GetAllAlbumIdsAsync();

        Assert.NotNull(albumIds);
        Assert.Collection(albumIds,
            element1Inspector => Assert.Equal(1, element1Inspector),
            element2Inspector => Assert.Equal(3002, element2Inspector)
        );
    }
    
    /* GetTotalNumberAlbumsAsync */
    [Fact]
    public async Task GetTotalNumberAlbumsAsync_ReturnsExpected()
    {
        await using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var numAlbums = await repository.GetTotalNumberAlbumsAsync();

        Assert.Equal(2, numAlbums);
    }
    
    /* GetAlbumByIdAsync */
    [Fact]
    public async Task GetAlbumByIdAsync_ReturnsExpected()
    {
        await using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var albumDefault = new Album();
        
        var expected = new IAlbumRepository.AlbumDto(Id: 3002, Name: "Unnamed", Description: albumDefault.Description, 
            NavTitle: albumDefault.NavTitle, Published: albumDefault.Published, LayoutPreset: PageLayoutPreset.Default,
            NavbarOrder: albumDefault.NavbarOrder);
        var albumDto = await repository.GetAlbumByIdAsync(3002);
        
        Assert.Equivalent(expected, albumDto);
    }



}
