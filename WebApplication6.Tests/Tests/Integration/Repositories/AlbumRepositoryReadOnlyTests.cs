using System.Linq;
using System.Threading.Tasks;
using WebApplication6.Backend.Models;
using WebApplication6.Backend.Repositories;
using Xunit;

namespace WebApplication6.Tests.Tests.Integration.Repositories;

// Tests against test database.
public class AlbumRepositoryReadOnlyTests(PrimaryTestDatabaseFixture dbFixture) : IClassFixture<PrimaryTestDatabaseFixture>
{
    public PrimaryTestDatabaseFixture Fixture { get; } = dbFixture;
    
    /* GetAlbumsAsync */
    [Fact]
    public async Task GetAlbumsAsync_ReturnsExpected()
    {
        await using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var albums = await repository.GetAlbumsAsync();
        var albumsList = albums.OrderBy(album => album.Id).ToList();

        Assert.NotNull(albums);
        Assert.Collection(albumsList,
            element1Inspector =>
            {
                Assert.Equal(1, element1Inspector.Id);
                Assert.Equal("MyPhotoAlbum", element1Inspector.Name);
                Assert.Equal("Here are all of my pictures", element1Inspector.Description);
                Assert.Equal("MyPhotoAlbum", element1Inspector.NavTitle);
                Assert.True(element1Inspector.Published);
                Assert.Equal(0, element1Inspector.NavbarOrder);
            },
            element2Inspector =>
            {
                Assert.Equal(2, element2Inspector.Id);
                Assert.Equal("HelloAlbum", element2Inspector.Name);
                Assert.Equal("Hello", element2Inspector.NavTitle);
            },
            element3Inspector =>
            {
                Assert.Equal(3, element3Inspector.Id);
                Assert.Equal(1, element3Inspector.NavbarOrder);
            },
            element4Inspector =>
            {
                Assert.Equal(5, element4Inspector.Id);
                Assert.True(element4Inspector.Published);
                Assert.Equal(-1, element4Inspector.NavbarOrder);
            },
            element5Inspector =>
            {
                Assert.Equal(32, element5Inspector.Id);
                Assert.False(element5Inspector.Published);
                Assert.Equal(-1, element5Inspector.NavbarOrder);
            });
    }
    
    /* GetAllAlbumsIdsAsync */
    [Fact]
    public async Task GetAlbumIdsAsync_ReturnsExpected()
    {
        await using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var albumIds = await repository.GetAlbumIdsAsync();
        Assert.NotNull(albumIds);
        
        var albumIdsList = albumIds.Order().ToList();
        Assert.Collection(albumIdsList,
            element1Inspector => Assert.Equal(1, element1Inspector),
            element2Inspector => Assert.Equal(2, element2Inspector),
            element3Inspector => Assert.Equal(3, element3Inspector),
            element4Inspector => Assert.Equal(5, element4Inspector),
            element5Inspector => Assert.Equal(32, element5Inspector)
        );
    }
    
    /* GetAlbumsCountAsync */
    [Fact]
    public async Task GetAlbumsCountAsync_ReturnsExpected()
    {
        await using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var numAlbums = await repository.GetAlbumsCountAsync();

        Assert.Equal(5, numAlbums);
    }
    
    /* GetAlbumByIdAsync */
    [Fact]
    public async Task GetAlbumByIdAsync_AlbumExists_ReturnsExpected()
    {
        await using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var albumDefault = new Album();
        
        var expected = new IAlbumRepository.AlbumDto(Id: 32, Name: "Unnamed", Description: albumDefault.Description, 
            NavTitle: albumDefault.NavTitle, Published: false, LayoutPreset: albumDefault.LayoutPreset,
            NavbarOrder: -1);
        var albumDto = await repository.GetAlbumByIdAsync(32);
        
        Assert.Equivalent(expected, albumDto);
    }

    [Theory] 
    [InlineData(-1)] 
    [InlineData(0)] 
    [InlineData(4)] 
    public async Task GetAlbumByIdAsync_AlbumDoesNotExist_ReturnsNull(int id)
    {
        await using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);
        
        var albumDto = await repository.GetAlbumByIdAsync(id);
        
        Assert.Null(albumDto);
    }

    /* GetPublishedAlbumsAsync */
    [Fact]
    public async Task GetPublishedAlbumsAsync_ReturnsExpected()
    {
        await using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var albums = await repository.GetPublishedAlbumsAsync();
        var albumsList = albums.OrderBy(album => album.Id).ToList();

        Assert.NotNull(albums);
        Assert.Collection(albumsList,
            element1Inspector =>
            {
                Assert.Equal(1, element1Inspector.Id);
                Assert.True(element1Inspector.Published);
                Assert.Equal(0, element1Inspector.NavbarOrder);
            },
            element2Inspector =>
            {
                Assert.Equal(2, element2Inspector.Id);
                Assert.True(element2Inspector.Published);
                Assert.Equal(2, element2Inspector.NavbarOrder);
            },
            element3Inspector =>
            {
                Assert.Equal(3, element3Inspector.Id);
                Assert.True(element3Inspector.Published);
                Assert.Equal(1, element3Inspector.NavbarOrder);
            },
            element4Inspector =>
            {
                Assert.Equal(5, element4Inspector.Id);
                Assert.True(element4Inspector.Published);
                Assert.Equal(-1, element4Inspector.NavbarOrder);
            });
    }
    
    /* GetPublishedNavExcludedAlbumsAsync */
    [Fact]
    public async Task GetPublishedNavExcludedAlbumsAsync_ReturnsExpected()
    {
        await using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var albums = await repository.GetPublishedNavExcludedAlbumsAsync();
        var albumsList = albums.OrderBy(album => album.Id).ToList();

        Assert.NotNull(albums);
        var single = Assert.Single(albumsList);
        Assert.Equal(5, single.Id);
        Assert.True(single.Published);
        Assert.Equal(-1, single.NavbarOrder);
    }
    
    /* GetNavIncludedAlbumsAsync */
    [Fact]
    public async Task GetNavIncludedAlbumsAsync_ReturnsExpected()
    {
        await using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var albums = await repository.GetNavIncludedAlbumsAsync();
        var albumsList = albums.ToList();

        Assert.NotNull(albums);
        
        // Requirement: Should be ordered by Album.NavbarOrder (ascending)
        Assert.Collection(albumsList,
            element1Inspector =>
            {
                Assert.Equal(1, element1Inspector.Id);
                Assert.True(element1Inspector.Published);
                Assert.Equal(0, element1Inspector.NavbarOrder);
            },
            element2Inspector =>
            {
                Assert.Equal(3, element2Inspector.Id);
                Assert.True(element2Inspector.Published);
                Assert.Equal(1, element2Inspector.NavbarOrder);
            },
            element3Inspector =>
            {
                Assert.Equal(2, element3Inspector.Id);
                Assert.True(element3Inspector.Published);
                Assert.Equal(2, element3Inspector.NavbarOrder);
            });
    }


}
