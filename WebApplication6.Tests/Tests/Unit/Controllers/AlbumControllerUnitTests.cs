using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

    [Fact]
    public async Task GetIds_EmptyIdList_ReturnsEmptyIdList()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        repositoryMock.Setup(r => r.GetAllAlbumsIdsAsync()).ReturnsAsync(new List<int>());
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.GetIds();

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var returnedIds = Assert.IsType<IEnumerable<int>>(okResult.Value, exactMatch: false);
        Assert.Empty(returnedIds);
    }

    [Fact]
    public async Task GetIds_SingleIdList_ReturnsSameIdInList()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        repositoryMock.Setup(r => r.GetAllAlbumsIdsAsync()).ReturnsAsync(new List<int> { 23 });
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.GetIds();

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var returnedIds = Assert.IsType<IEnumerable<int>>(okResult.Value, exactMatch: false);
        Assert.Equal(23, Assert.Single(returnedIds));
    }

    [Fact]
    public async Task GetIds_MultipleIdList_ReturnsSameIdsInOrder()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        repositoryMock.Setup(r => r.GetAllAlbumsIdsAsync()).ReturnsAsync(new List<int> { 4, 12, 23 });
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.GetIds();

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var returnedIds = Assert.IsType<IEnumerable<int>>(okResult.Value, exactMatch: false);
        Assert.Collection(returnedIds,
            id => Assert.Equal(4, id),
            id => Assert.Equal(12, id),
            id => Assert.Equal(23, id));
    }

    [Fact]
    public async Task GetAlbumById_AlbumExists_ReturnsAlbum()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        var album = new IAlbumRepository.AlbumDto(23, "My Album");
        repositoryMock.Setup(r => r.GetAlbumByIdAsync(23)).ReturnsAsync(album);
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.Get(23);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(album, okResult.Value);
        repositoryMock.Verify(r => r.GetAlbumByIdAsync(23), Times.Once());
    }

    [Fact]
    public async Task GetAlbumById_AlbumDoesNotExist_ReturnsNotFound()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        repositoryMock.Setup(r => r.GetAlbumByIdAsync(23)).ReturnsAsync((IAlbumRepository.AlbumDto)null);
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.Get(23);

        Assert.IsType<NotFoundResult>(response.Result);
        repositoryMock.Verify(r => r.GetAlbumByIdAsync(23), Times.Once());
    }

    [Fact]
    public void GetPageLayoutPresets_ReturnsAllPresets()
    {
        var controller = new AlbumController(new Mock<IAlbumRepository>().Object);

        var presets = controller.GetPageLayoutPresets();

        Assert.Equal(new[] { PageLayoutPreset.Default, PageLayoutPreset.Cozy, PageLayoutPreset.Spooky }, presets);
    }

    [Fact]
    public async Task GetAllPublishedAlbums_EmptyAlbumList_ReturnsEmptyAlbumList()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        repositoryMock.Setup(r => r.GetAllPublishedAsync()).ReturnsAsync(new List<IAlbumRepository.AlbumDto>());
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.GetAllPublishedAlbums();

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var returnedAlbums = Assert.IsType<IEnumerable<IAlbumRepository.AlbumDto>>(okResult.Value, exactMatch: false);
        Assert.Empty(returnedAlbums);
    }

    [Fact]
    public async Task GetAllPublishedAlbums_MultipleAlbumsList_ReturnsSameAlbumsInList()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        var album1 = new IAlbumRepository.AlbumDto(1, "First", Published: true);
        var album2 = new IAlbumRepository.AlbumDto(2, "Second", Published: true);
        repositoryMock.Setup(r => r.GetAllPublishedAsync()).ReturnsAsync(new List<IAlbumRepository.AlbumDto> { album1, album2 });
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.GetAllPublishedAlbums();

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var returnedAlbums = Assert.IsType<IEnumerable<IAlbumRepository.AlbumDto>>(okResult.Value, exactMatch: false);
        Assert.Collection(returnedAlbums,
            album => Assert.Equal(album1, album),
            album => Assert.Equal(album2, album));
    }

    [Fact]
    public async Task GetPublishedNotInNav_EmptyAlbumList_ReturnsEmptyAlbumList()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        repositoryMock.Setup(r => r.GetPublishedNotInNavbar()).ReturnsAsync(new List<IAlbumRepository.AlbumDto>());
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.GetPublishedNotInNav();

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var returnedAlbums = Assert.IsType<IEnumerable<IAlbumRepository.AlbumDto>>(okResult.Value, exactMatch: false);
        Assert.Empty(returnedAlbums);
    }

    [Fact]
    public async Task GetPublishedNotInNav_MultipleAlbumsList_ReturnsSameAlbumsInList()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        var album1 = new IAlbumRepository.AlbumDto(1, "First", Published: true);
        var album2 = new IAlbumRepository.AlbumDto(2, "Second", Published: true);
        repositoryMock.Setup(r => r.GetPublishedNotInNavbar()).ReturnsAsync(new List<IAlbumRepository.AlbumDto> { album1, album2 });
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.GetPublishedNotInNav();

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var returnedAlbums = Assert.IsType<IEnumerable<IAlbumRepository.AlbumDto>>(okResult.Value, exactMatch: false);
        Assert.Collection(returnedAlbums,
            album => Assert.Equal(album1, album),
            album => Assert.Equal(album2, album));
    }

    [Fact]
    public async Task GetNavAlbumsOrdered_EmptyAlbumList_ReturnsEmptyAlbumList()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        repositoryMock.Setup(r => r.GetPublishedInNavbarOrdered()).ReturnsAsync(new List<IAlbumRepository.AlbumDto>());
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.GetNavAlbumsOrdered();

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var returnedAlbums = Assert.IsType<IEnumerable<IAlbumRepository.AlbumDto>>(okResult.Value, exactMatch: false);
        Assert.Empty(returnedAlbums);
    }

    [Fact]
    public async Task GetNavAlbumsOrdered_MultipleAlbumsList_ReturnsSameAlbumsInOrder()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        var album1 = new IAlbumRepository.AlbumDto(3, "First", Published: true, NavbarOrder: 0);
        var album2 = new IAlbumRepository.AlbumDto(1, "Second", Published: true, NavbarOrder: 1);
        repositoryMock.Setup(r => r.GetPublishedInNavbarOrdered()).ReturnsAsync(new List<IAlbumRepository.AlbumDto> { album1, album2 });
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.GetNavAlbumsOrdered();

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var returnedAlbums = Assert.IsType<IEnumerable<IAlbumRepository.AlbumDto>>(okResult.Value, exactMatch: false);
        Assert.Collection(returnedAlbums,
            album => Assert.Equal(album1, album),
            album => Assert.Equal(album2, album));
    }

    [Theory]
    [InlineData(PageLayoutPreset.Default)]
    [InlineData(PageLayoutPreset.Cozy)]
    [InlineData(PageLayoutPreset.Spooky)]
    public async Task UpdateLayoutPreset_ValidPreset_ReturnsUpdatedAlbum(PageLayoutPreset preset)
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        var album = new IAlbumRepository.AlbumDto(23, "My Album", LayoutPreset: preset);
        repositoryMock.Setup(r => r.SetLayoutPresetAsync(23, preset)).ReturnsAsync(album);
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.UpdateLayoutPreset(23, new AlbumController.UpdateLayoutPresetRequest(preset));

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(album, okResult.Value);
        repositoryMock.Verify(r => r.SetLayoutPresetAsync(23, preset), Times.Once());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public async Task UpdateLayoutPreset_InvalidPreset_ReturnsBadRequestWithoutSaving(int preset)
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.UpdateLayoutPreset(23,
            new AlbumController.UpdateLayoutPresetRequest((PageLayoutPreset)preset));

        Assert.IsType<BadRequestResult>(response.Result);
        repositoryMock.Verify(r => r.SetLayoutPresetAsync(It.IsAny<int>(), It.IsAny<PageLayoutPreset>()), Times.Never());
    }

    [Fact]
    public async Task UpdateLayoutPreset_AlbumDoesNotExist_ReturnsNotFound()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        repositoryMock.Setup(r => r.SetLayoutPresetAsync(23, PageLayoutPreset.Cozy))
            .ReturnsAsync((IAlbumRepository.AlbumDto)null);
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.UpdateLayoutPreset(23,
            new AlbumController.UpdateLayoutPresetRequest(PageLayoutPreset.Cozy));

        Assert.IsType<NotFoundResult>(response.Result);
        repositoryMock.Verify(r => r.SetLayoutPresetAsync(23, PageLayoutPreset.Cozy), Times.Once());
    }

    [Theory]
    [InlineData(2)]
    [InlineData(null)]
    public async Task ModifyAlbumVisibility_PublishRequested_ReturnsPublishedAlbum(int? navOrder)
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        var album = new IAlbumRepository.AlbumDto(23, "My Album", Published: true);
        repositoryMock.Setup(r => r.PublishAlbumAsync(23, navOrder)).ReturnsAsync(album);
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.ModifyAlbumVisibility(23,
            new AlbumController.AlbumVisibilityRequest(true, navOrder));

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(album, okResult.Value);
        repositoryMock.Verify(r => r.PublishAlbumAsync(23, navOrder), Times.Once());
        repositoryMock.Verify(r => r.UnpublishAlbumAsync(It.IsAny<int>()), Times.Never());
    }

    [Fact]
    public async Task ModifyAlbumVisibility_UnpublishRequested_ReturnsUnpublishedAlbum()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        var album = new IAlbumRepository.AlbumDto(23, "My Album", Published: false);
        repositoryMock.Setup(r => r.UnpublishAlbumAsync(23)).ReturnsAsync(album);
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.ModifyAlbumVisibility(23,
            new AlbumController.AlbumVisibilityRequest(false, 2));

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(album, okResult.Value);
        repositoryMock.Verify(r => r.UnpublishAlbumAsync(23), Times.Once());
        repositoryMock.Verify(r => r.PublishAlbumAsync(It.IsAny<int>(), It.IsAny<int?>()), Times.Never());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ModifyAlbumVisibility_AlbumDoesNotExist_ReturnsNotFound(bool published)
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        if (published)
            repositoryMock.Setup(r => r.PublishAlbumAsync(23, 2)).ReturnsAsync((IAlbumRepository.AlbumDto)null);
        else
            repositoryMock.Setup(r => r.UnpublishAlbumAsync(23)).ReturnsAsync((IAlbumRepository.AlbumDto)null);
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.ModifyAlbumVisibility(23,
            new AlbumController.AlbumVisibilityRequest(published, 2));

        Assert.IsType<NotFoundResult>(response.Result);
        if (published)
            repositoryMock.Verify(r => r.PublishAlbumAsync(23, 2), Times.Once());
        else
            repositoryMock.Verify(r => r.UnpublishAlbumAsync(23), Times.Once());
    }

    [Fact]
    public async Task AssignNavOrder_Success_ReturnsOrderedAlbums()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        IEnumerable<IAlbumRepository.AlbumDto> albums = new List<IAlbumRepository.AlbumDto>
        {
            new(23, "My Album", Published: true, NavbarOrder: 2)
        };
        repositoryMock.Setup(r => r.AssignAlbumInNavAsync(23, 2))
            .ReturnsAsync((IAlbumRepository.AssignNavOrderOutcome.Success, albums));
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.AssignNavOrder(23, new AlbumController.NavOrderRequest(2));

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(albums, okResult.Value);
        repositoryMock.Verify(r => r.AssignAlbumInNavAsync(23, 2), Times.Once());
    }

    [Fact]
    public async Task AssignNavOrder_OutOfBounds_ReturnsBadRequestWithAlbums()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        IEnumerable<IAlbumRepository.AlbumDto> albums = new List<IAlbumRepository.AlbumDto>();
        repositoryMock.Setup(r => r.AssignAlbumInNavAsync(23, 5))
            .ReturnsAsync((IAlbumRepository.AssignNavOrderOutcome.OutOfBounds, albums));
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.AssignNavOrder(23, new AlbumController.NavOrderRequest(5));

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Same(albums, badRequestResult.Value);
        repositoryMock.Verify(r => r.AssignAlbumInNavAsync(23, 5), Times.Once());
    }

    [Fact]
    public async Task AssignNavOrder_AlbumNotPublished_ReturnsConflictWithAlbums()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        IEnumerable<IAlbumRepository.AlbumDto> albums = new List<IAlbumRepository.AlbumDto>();
        repositoryMock.Setup(r => r.AssignAlbumInNavAsync(23, 2))
            .ReturnsAsync((IAlbumRepository.AssignNavOrderOutcome.NotPublished, albums));
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.AssignNavOrder(23, new AlbumController.NavOrderRequest(2));

        var conflictResult = Assert.IsType<ConflictObjectResult>(response.Result);
        Assert.Same(albums, conflictResult.Value);
        repositoryMock.Verify(r => r.AssignAlbumInNavAsync(23, 2), Times.Once());
    }

    [Fact]
    public async Task AssignNavOrder_AlbumDoesNotExist_ReturnsNotFoundWithAlbums()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        IEnumerable<IAlbumRepository.AlbumDto> albums = new List<IAlbumRepository.AlbumDto>();
        repositoryMock.Setup(r => r.AssignAlbumInNavAsync(23, 2))
            .ReturnsAsync((IAlbumRepository.AssignNavOrderOutcome.NotFound, albums));
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.AssignNavOrder(23, new AlbumController.NavOrderRequest(2));

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        Assert.Same(albums, notFoundResult.Value);
        repositoryMock.Verify(r => r.AssignAlbumInNavAsync(23, 2), Times.Once());
    }

    [Fact]
    public async Task AssignNavOrder_NavbarFull_ReturnsConflictWithAlbums()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        IEnumerable<IAlbumRepository.AlbumDto> albums = new List<IAlbumRepository.AlbumDto>();
        repositoryMock.Setup(r => r.AssignAlbumInNavAsync(23, 2))
            .ReturnsAsync((IAlbumRepository.AssignNavOrderOutcome.NavbarFull, albums));
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.AssignNavOrder(23, new AlbumController.NavOrderRequest(2));

        var conflictResult = Assert.IsType<ConflictObjectResult>(response.Result);
        Assert.Same(albums, conflictResult.Value);
        repositoryMock.Verify(r => r.AssignAlbumInNavAsync(23, 2), Times.Once());
    }

    [Fact]
    public async Task AssignNavOrder_UnexpectedOutcome_ReturnsServerError()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        repositoryMock.Setup(r => r.AssignAlbumInNavAsync(23, 2))
            .ReturnsAsync(((IAlbumRepository.AssignNavOrderOutcome)999, (IEnumerable<IAlbumRepository.AlbumDto>)null));
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.AssignNavOrder(23, new AlbumController.NavOrderRequest(2));

        var problemResult = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(500, problemResult.StatusCode);
    }

    [Fact]
    public async Task RemoveFromNav_Success_ReturnsOrderedAlbums()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        IEnumerable<IAlbumRepository.AlbumDto> albums = new List<IAlbumRepository.AlbumDto>
        {
            new(1, "Another Album", Published: true, NavbarOrder: 0)
        };
        repositoryMock.Setup(r => r.AssignAlbumInNavAsync(23, -1))
            .ReturnsAsync((IAlbumRepository.AssignNavOrderOutcome.Success, albums));
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.RemoveFromNav(23);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(albums, okResult.Value);
        repositoryMock.Verify(r => r.AssignAlbumInNavAsync(23, -1), Times.Once());
    }

    [Fact]
    public async Task RemoveFromNav_AlbumNotPublished_ReturnsConflictWithAlbums()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        IEnumerable<IAlbumRepository.AlbumDto> albums = new List<IAlbumRepository.AlbumDto>();
        repositoryMock.Setup(r => r.AssignAlbumInNavAsync(23, -1))
            .ReturnsAsync((IAlbumRepository.AssignNavOrderOutcome.NotPublished, albums));
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.RemoveFromNav(23);

        var conflictResult = Assert.IsType<ConflictObjectResult>(response.Result);
        Assert.Same(albums, conflictResult.Value);
        repositoryMock.Verify(r => r.AssignAlbumInNavAsync(23, -1), Times.Once());
    }

    [Fact]
    public async Task RemoveFromNav_AlbumDoesNotExist_ReturnsNotFoundWithAlbums()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        IEnumerable<IAlbumRepository.AlbumDto> albums = new List<IAlbumRepository.AlbumDto>();
        repositoryMock.Setup(r => r.AssignAlbumInNavAsync(23, -1))
            .ReturnsAsync((IAlbumRepository.AssignNavOrderOutcome.NotFound, albums));
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.RemoveFromNav(23);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        Assert.Same(albums, notFoundResult.Value);
        repositoryMock.Verify(r => r.AssignAlbumInNavAsync(23, -1), Times.Once());
    }

    [Fact]
    public async Task RemoveFromNav_UnexpectedOutcome_ReturnsServerError()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        repositoryMock.Setup(r => r.AssignAlbumInNavAsync(23, -1))
            .ReturnsAsync((IAlbumRepository.AssignNavOrderOutcome.OutOfBounds, (IEnumerable<IAlbumRepository.AlbumDto>)null));
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.RemoveFromNav(23);

        var problemResult = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(500, problemResult.StatusCode);
    }

    [Fact]
    public async Task SwapAlbumsInNav_Success_ReturnsOrderedAlbums()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        IEnumerable<IAlbumRepository.AlbumDto> albums = new List<IAlbumRepository.AlbumDto>
        {
            new(12, "Second", Published: true, NavbarOrder: 0),
            new(23, "First", Published: true, NavbarOrder: 1)
        };
        repositoryMock.Setup(r => r.SwapAlbumsInNavOrderAsync(23, 12))
            .ReturnsAsync((IAlbumRepository.SwapInNavOutcome.Success, albums));
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.SwapAlbumsInNav(new AlbumController.NavOrderSwapRequest(23, 12));

        var okResult = Assert.IsType<OkObjectResult>(response);
        Assert.Same(albums, okResult.Value);
        repositoryMock.Verify(r => r.SwapAlbumsInNavOrderAsync(23, 12), Times.Once());
    }

    [Theory]
    [InlineData(IAlbumRepository.SwapInNavOutcome.IgnoredSwapInPlace)]
    [InlineData(IAlbumRepository.SwapInNavOutcome.NotFoundInNav)]
    public async Task SwapAlbumsInNav_SwapRejected_ReturnsBadRequest(IAlbumRepository.SwapInNavOutcome outcome)
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        repositoryMock.Setup(r => r.SwapAlbumsInNavOrderAsync(23, 12))
            .ReturnsAsync((outcome, (IEnumerable<IAlbumRepository.AlbumDto>)null));
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.SwapAlbumsInNav(new AlbumController.NavOrderSwapRequest(23, 12));

        Assert.IsType<BadRequestResult>(response);
        repositoryMock.Verify(r => r.SwapAlbumsInNavOrderAsync(23, 12), Times.Once());
    }

    [Fact]
    public async Task SwapAlbumsInNav_UnexpectedOutcome_ReturnsServerError()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        repositoryMock.Setup(r => r.SwapAlbumsInNavOrderAsync(23, 12))
            .ReturnsAsync(((IAlbumRepository.SwapInNavOutcome)999, (IEnumerable<IAlbumRepository.AlbumDto>)null));
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.SwapAlbumsInNav(new AlbumController.NavOrderSwapRequest(23, 12));

        var problemResult = Assert.IsType<ObjectResult>(response);
        Assert.Equal(500, problemResult.StatusCode);
    }

    [Fact]
    public async Task DeleteAlbumById_AlbumExists_ReturnsNoContent()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        repositoryMock.Setup(r => r.DeleteAlbumByIdAsync(23)).ReturnsAsync(true);
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.DeleteAlbumById(23);

        Assert.IsType<NoContentResult>(response);
        repositoryMock.Verify(r => r.DeleteAlbumByIdAsync(23), Times.Once());
    }

    [Fact]
    public async Task DeleteAlbumById_AlbumDoesNotExist_ReturnsNotFound()
    {
        var repositoryMock = new Mock<IAlbumRepository>();
        repositoryMock.Setup(r => r.DeleteAlbumByIdAsync(23)).ReturnsAsync(false);
        var controller = new AlbumController(repositoryMock.Object);

        var response = await controller.DeleteAlbumById(23);

        Assert.IsType<NotFoundResult>(response);
        repositoryMock.Verify(r => r.DeleteAlbumByIdAsync(23), Times.Once());
    }
}
