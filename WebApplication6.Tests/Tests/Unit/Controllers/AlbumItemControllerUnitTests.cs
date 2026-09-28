using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApplication6.Backend.Controllers;
using WebApplication6.Backend.Models;
using WebApplication6.Backend.Repositories;
using Xunit;

namespace WebApplication6.Tests.Tests.Unit.Controllers;

public class AlbumItemControllerUnitTests
{
    [Fact]
    public async Task GetItems_AlbumContainsNoItems_ReturnsEmptyList()
    {
        var repositoryMock = new Mock<IAlbumItemRepository>();
        repositoryMock.Setup(r => r.GetAlbumItems(1)).ReturnsAsync(new List<IAlbumItemRepository.AlbumItemDto>());
        var controller = new AlbumItemController(repositoryMock.Object);

        var response = await controller.Get(1);

        Assert.Null(response.Result);
        var returnedItems = Assert.IsType<List<IAlbumItemRepository.AlbumItemDto>>(response.Value);
        Assert.Empty(returnedItems);
        repositoryMock.Verify(r => r.GetAlbumItems(1), Times.Once());
    }

    [Fact]
    public async Task GetItems_AlbumContainsOneItem_ReturnsSameItemInList()
    {
        var repositoryMock = new Mock<IAlbumItemRepository>();
        var item = new IAlbumItemRepository.PhotoDisplayCollectionAlbumItemDto(25, 0,
            new IAlbumItemRepository.PhotoDisplayCollectionDto(
                PhotoDisplayCollection.PhotoDisplayMode.Carousel,
                new List<IAlbumItemRepository.PhotoDisplayAlbumItemDto>()));
        repositoryMock.Setup(r => r.GetAlbumItems(23))
            .ReturnsAsync(new List<IAlbumItemRepository.AlbumItemDto> { item });
        var controller = new AlbumItemController(repositoryMock.Object);

        var response = await controller.Get(23);

        Assert.Null(response.Result);
        var returnedItems = Assert.IsType<List<IAlbumItemRepository.AlbumItemDto>>(response.Value);
        Assert.Same(item, Assert.Single(returnedItems));
        repositoryMock.Verify(r => r.GetAlbumItems(23), Times.Once());
    }

    [Fact]
    public async Task GetItems_AlbumContainsMultipleItemTypes_ReturnsSameItemsInOrder()
    {
        var repositoryMock = new Mock<IAlbumItemRepository>();
        var collection = new IAlbumItemRepository.PhotoDisplayCollectionAlbumItemDto(25, 0,
            new IAlbumItemRepository.PhotoDisplayCollectionDto(
                PhotoDisplayCollection.PhotoDisplayMode.Carousel,
                new List<IAlbumItemRepository.PhotoDisplayAlbumItemDto>()));
        var image = new IAlbumItemRepository.ImageDto(30, "photo.jpg", "image/jpeg", 100,
            "stored.jpg", "/photos/stored.jpg", "A photo", 800, 600);
        var photo = new IAlbumItemRepository.PhotoDto(31, image, "Portrait", "Description", 2024);
        var display = new IAlbumItemRepository.PhotoDisplayAlbumItemDto(7, 1,
            new IAlbumItemRepository.PhotoDisplayDto(photo, null, true, true, true));
        repositoryMock.Setup(r => r.GetAlbumItems(23))
            .ReturnsAsync(new List<IAlbumItemRepository.AlbumItemDto> { collection, display });
        var controller = new AlbumItemController(repositoryMock.Object);

        var response = await controller.Get(23);

        Assert.Null(response.Result);
        var returnedItems = Assert.IsType<List<IAlbumItemRepository.AlbumItemDto>>(response.Value);
        Assert.Collection(returnedItems,
            item => Assert.Same(collection, item),
            item => Assert.Same(display, item));
        repositoryMock.Verify(r => r.GetAlbumItems(23), Times.Once());
    }

    [Fact]
    public async Task RootLevelReorder_Success_ReturnsOk()
    {
        var repositoryMock = new Mock<IAlbumItemRepository>();
        repositoryMock.Setup(r => r.ReorderAlbumItem(23, 7, 2))
            .ReturnsAsync(IAlbumItemRepository.ReorderOutcome.Success);
        var controller = new AlbumItemController(repositoryMock.Object);

        var response = await controller.RootLevelReorder(23,
            new AlbumItemController.RootLevelReorderRequest(7, 2));

        Assert.IsType<OkResult>(response);
        repositoryMock.Verify(r => r.ReorderAlbumItem(23, 7, 2), Times.Once());
    }

    [Fact]
    public async Task RootLevelReorder_NoAlbumItems_ReturnsConflict()
    {
        var repositoryMock = new Mock<IAlbumItemRepository>();
        repositoryMock.Setup(r => r.ReorderAlbumItem(23, 7, 2))
            .ReturnsAsync(IAlbumItemRepository.ReorderOutcome.NoAlbumItems);
        var controller = new AlbumItemController(repositoryMock.Object);

        var response = await controller.RootLevelReorder(23,
            new AlbumItemController.RootLevelReorderRequest(7, 2));

        Assert.IsType<ConflictResult>(response);
        repositoryMock.Verify(r => r.ReorderAlbumItem(23, 7, 2), Times.Once());
    }

    [Fact]
    public async Task RootLevelReorder_ItemNotFound_ReturnsNotFound()
    {
        var repositoryMock = new Mock<IAlbumItemRepository>();
        repositoryMock.Setup(r => r.ReorderAlbumItem(23, 7, 2))
            .ReturnsAsync(IAlbumItemRepository.ReorderOutcome.ItemNotFound);
        var controller = new AlbumItemController(repositoryMock.Object);

        var response = await controller.RootLevelReorder(23,
            new AlbumItemController.RootLevelReorderRequest(7, 2));

        Assert.IsType<NotFoundResult>(response);
        repositoryMock.Verify(r => r.ReorderAlbumItem(23, 7, 2), Times.Once());
    }

    [Fact]
    public async Task RootLevelReorder_UnexpectedOutcome_ReturnsServerError()
    {
        var repositoryMock = new Mock<IAlbumItemRepository>();
        repositoryMock.Setup(r => r.ReorderAlbumItem(23, 7, 2))
            .ReturnsAsync((IAlbumItemRepository.ReorderOutcome)999);
        var controller = new AlbumItemController(repositoryMock.Object);

        var response = await controller.RootLevelReorder(23,
            new AlbumItemController.RootLevelReorderRequest(7, 2));

        var problemResult = Assert.IsType<ObjectResult>(response);
        Assert.Equal(500, problemResult.StatusCode);
        repositoryMock.Verify(r => r.ReorderAlbumItem(23, 7, 2), Times.Once());
    }
}
