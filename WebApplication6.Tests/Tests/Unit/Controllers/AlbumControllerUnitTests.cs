using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApplication6.Backend.Controllers;
using WebApplication6.Backend.Repositories;
using Xunit;

namespace WebApplication6.Tests.Tests.Unit.Controllers;

public class AlbumControllerUnitTests
{
    [Fact]
    public async Task GetAllAlbums_SingleAlbumList_ReturnsSameSingleAlbumInList()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        var album = new IAlbumRepository.AlbumDto(1, "album");
        var myAlbums = new List<IAlbumRepository.AlbumDto> { album };
        repositoryMock.Setup(r => r.GetAllAlbumsAsync()).ReturnsAsync(myAlbums);
        var controller = new AlbumController(repositoryMock.Object);
        
        var albums = await controller.Get();

        var okResult = Assert.IsType<OkObjectResult>(albums.Result);
        var returnedAlbums = Assert.IsType<IEnumerable<IAlbumRepository.AlbumDto>>(okResult.Value, exactMatch: false);
        var item = Assert.Single(returnedAlbums);
        Assert.Equal(album, item);
    }
}
