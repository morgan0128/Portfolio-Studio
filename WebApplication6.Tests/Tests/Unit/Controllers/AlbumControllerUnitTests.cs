using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApplication6.Backend.Controllers;
using WebApplication6.Backend.Models;
using WebApplication6.Backend.Repositories;
using Xunit;

namespace WebApplication6.Tests.Tests.Unit.Controllers;

public class AlbumControllerUnitTests
{
    [Fact]
    public async Task PostAlbum_OnSuccessReturnsCreatedStatusCodeHavingIdRouteValueWithDto()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        var requestPayload = new AlbumController.CreateAlbumRequest(Name: "MyAlbum", Description: "desc");
        var expectedDto = new IAlbumRepository.AlbumDto(Id: 23, Name: "MyAlbum", Description: "desc", NavTitle: "MyAlbum");
        repositoryMock.Setup(r => r.SaveAlbumAsync(It.IsAny<Album>()))
            .ReturnsAsync(expectedDto);
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.Post(requestPayload);

        var createdResult = Assert.IsType<CreatedAtActionResult>(response.Result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Same(expectedDto, createdResult.Value);
        Assert.Equal(nameof(AlbumController.Get), createdResult.ActionName);
        Assert.NotNull(createdResult.RouteValues);
        Assert.Equal(expectedDto.Id, createdResult.RouteValues["id"]);
    }
    
    [Fact]
    public async Task PostAlbum_AlbumRequestNameContainsWhitespace_TrimsLeadingAndTrailingWhitespace()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        var requestPayload = new AlbumController.CreateAlbumRequest(Name: "   My    Album\n", Description: "desc");
        var expectedDto = new IAlbumRepository.AlbumDto(Id: 42, Name: "My    Album");
        repositoryMock.Setup(r => r.SaveAlbumAsync(It.IsAny<Album>())).ReturnsAsync(expectedDto);
        var controller = new AlbumController(repositoryMock.Object);

        await controller.Post(requestPayload);

        repositoryMock.Verify(
            r => r.SaveAlbumAsync(It.Is<Album>(album => album.Name == "My    Album")),
            Times.Once());
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    \n \t\t\n")]
    public async Task PostAlbum_AlbumRequestNameIsUnacceptable_RenamedAsExpected([CanBeNull] string inputName)
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        var requestPayload = new AlbumController.CreateAlbumRequest(Name: inputName, Description: "desc");
        var expectedDto = new IAlbumRepository.AlbumDto(Id: 21, Name: "whatever");
        repositoryMock.Setup(r => r.SaveAlbumAsync(It.IsAny<Album>())).ReturnsAsync(expectedDto);
        var controller = new AlbumController(repositoryMock.Object);

        await controller.Post(requestPayload);

        repositoryMock.Verify(
            r => r.SaveAlbumAsync(It.Is<Album>(album => album.Name.StartsWith("Unnamed Album #"))),
            Times.Once());
    }

    [Theory]
    [InlineData(" \tMy    Album\r\n", "My    Album", "My    Album")]
    [InlineData("catfoxdogblueredhen", "catfoxdogblueredhen", "catfoxdogblueredhen")] // name length 19
    [InlineData("catfoxdogblueredhens", "catfoxdogblueredhens", "catfoxdogblueredhens")] // name length 20
    [InlineData(" \tcatfoxdogblueredhens\r\n", "catfoxdogblueredhens", "catfoxdogblueredhens")] // name length 20 after trimming
    [InlineData("catfoxdogblueredhensh", "catfoxdogblueredhensh", "catfoxdogblueredhens")] // name length 21
    [InlineData(" \tcatfoxdogblueredhensh\r\n", "catfoxdogblueredhensh", "catfoxdogblueredhens")] // name length 21 after trimming
    public async Task PostAlbum_NavTitleIsNameTrimmedThenTruncatedPastTwentyCharacters(string inputName, string expectedName, string expectedNavTitle)
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        var requestPayload = new AlbumController.CreateAlbumRequest(inputName, "desc");
        repositoryMock.Setup(r => r.SaveAlbumAsync(It.IsAny<Album>()))
            .ReturnsAsync(new IAlbumRepository.AlbumDto(42, "Repository result"));

        var controller = new AlbumController(repositoryMock.Object);

        await controller.Post(requestPayload);

        repositoryMock.Verify(r => r.SaveAlbumAsync(It.Is<Album>(
                album => album.Name == expectedName && album.NavTitle == expectedNavTitle)), Times.Once());
    }
    
    [Theory]
    [InlineData(150, true)]
    [InlineData(151, false)]
    public void CreateAlbumRequest_NameLength_ValidatesBoundary(int length, bool expectedValid)
    {
        var constructor = typeof(AlbumController.CreateAlbumRequest).GetConstructor([typeof(string), typeof(string)]);
        Assert.NotNull(constructor);

        var nameParameter = constructor.GetParameters()[0];
        var attribute = nameParameter.GetCustomAttribute<StringLengthAttribute>();
        Assert.NotNull(attribute);

        var name = new string('x', length);
        Assert.Equal(expectedValid, attribute.IsValid(name));
    }
    
    [Theory]
    [InlineData(400, true)]
    [InlineData(401, false)]
    public void CreateAlbumRequest_DescriptionLength_ValidatesBoundary(int length, bool expectedValid)
    {
        var constructor = typeof(AlbumController.CreateAlbumRequest).GetConstructor([typeof(string), typeof(string)]);
        Assert.NotNull(constructor);

        var descriptionParameter = constructor.GetParameters()[1];
        var attribute = descriptionParameter.GetCustomAttribute<StringLengthAttribute>();
        Assert.NotNull(attribute);

        var description = new string('x', length);
        Assert.Equal(expectedValid, attribute.IsValid(description));
    }
    
    [Fact]
    public async Task GetAllAlbums_EmptyAlbumList_ReturnsEmptyAlbumList()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        repositoryMock.Setup(r => r.GetAllAlbumsAsync()).ReturnsAsync(new List<IAlbumRepository.AlbumDto>());
        var controller = new AlbumController(repositoryMock.Object);
        
        var albums = await controller.Get();

        var okResult = Assert.IsType<OkObjectResult>(albums.Result);
        var returnedAlbums = Assert.IsType<IEnumerable<IAlbumRepository.AlbumDto>>(okResult.Value, exactMatch: false);
        Assert.Empty(returnedAlbums);
    }
    
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
    
    [Fact]
    public async Task GetAllAlbums_MultipleAlbumsList_ReturnsSameAlbumsInList()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        var album1 = new IAlbumRepository.AlbumDto(1, "album");
        var album2 = new IAlbumRepository.AlbumDto(2, "hello album");
        var album3 = new IAlbumRepository.AlbumDto(4, "album album");
        var myAlbums = new List<IAlbumRepository.AlbumDto> { album1, album2, album3 };
        repositoryMock.Setup(r => r.GetAllAlbumsAsync()).ReturnsAsync(myAlbums);
        var controller = new AlbumController(repositoryMock.Object);
        
        var albums = await controller.Get();

        var okResult = Assert.IsType<OkObjectResult>(albums.Result);
        var returnedAlbums = Assert.IsType<IEnumerable<IAlbumRepository.AlbumDto>>(okResult.Value, exactMatch: false);
        Assert.Collection(returnedAlbums,
            element1Inspector =>
            {
                Assert.Equal(1, element1Inspector.Id);
                Assert.Equal("album", element1Inspector.Name);
            },
            element2Inspector =>
            {
                Assert.Equal(2, element2Inspector.Id);
                Assert.Equal("hello album", element2Inspector.Name);
            },
            element3Inspector =>
            {
                Assert.Equal(4, element3Inspector.Id);
                Assert.Equal("album album", element3Inspector.Name);
            });
    }
    

}
