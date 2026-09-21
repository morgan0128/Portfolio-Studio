using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApplication6.Backend.Controllers;
using WebApplication6.Backend.Models;
using WebApplication6.Backend.Repositories;
using WebApplication6.Backend.Services;
using Xunit;

namespace WebApplication6.Tests.Tests.Unit.Controllers;

public class AlbumControllerUnitTests
{
    /*
    [Fact]
    public async Task GetAllAlbums_SingleAlbumList_ReturnsSameSingleAlbumInList()
    {
        // Arrange
        var repositoryMock = new Mock<IAlbumRepository>();
        var serviceMock = new Mock<IUploadPhotoService>();

        var image = new Image
        {
            Id = 99,
            AltText = "",
            ContentType = "image/png",
            FileName = "image",
            Height = 57,
            StorageFileName = "Somewhere",
            Url = "Iam/Somewhere",
            Width = 2
        };

        var pointlessPhoto = new Photo
        {
            Id = 67,
            Name = "I am a pointless photo",
            Description = "I have nothing important to say",
            Image = image,
            ImageId = image.Id
        };
        
        var album = new Album
        {
            Id = 1,
            Name = "MeAlbum",
            Description = "Hello I am an album",
            Photos = new List<Photo>{ pointlessPhoto }
        };

        var albumList = new List<Album> { album };
        
        repositoryMock.Setup(r => r.GetAllAlbumsAsync())
            .ReturnsAsync(albumList);

        var controller = new AlbumController(repositoryMock.Object, serviceMock.Object);
        
        // Act
        var albums = await controller.GetAllAlbums();
        
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(albums.Result);
        var returnedAlbums = Assert.IsType<IEnumerable<Album>>(okResult.Value, exactMatch: false);
        var item = Assert.Single(returnedAlbums);
        Assert.Equal(album, item);
    }


    [Fact]
    public async Task GetAllAlbums_MultipleAlbumsExist_ReturnsListOfSameAlbums()
    {
        // Arrange
        var repositoryMock = new Mock<IAlbumRepository>();
        var serviceMock = new Mock<IUploadPhotoService>();

        var image = new Image
        {
            Id = 99,
            AltText = "",
            ContentType = "image/png",
            FileName = "image",
            Height = 57,
            StorageFileName = "Somewhere",
            Url = "Iam/Somewhere",
            Width = 2
        };

        var pointlessPhoto = new Photo
        {
            Id = 67,
            Name = "I am a pointless photo",
            Description = "I have nothing important to say",
            Image = image,
            ImageId = image.Id
        };
        
        var album1 = new Album
        {
            Id = 1,
            Name = "MeAlbum",
            Description = "Hello I am an album",
            Photos = new List<Photo>{ pointlessPhoto }
        };
        
        var album2 = new Album
        {
            Id = 5,
            Name = "MeAlbum",
            Description = "Hello I am an album",
            Photos = new List<Photo>()
        };

        var albumList = new List<Album> { album1, album2 };
        
        repositoryMock.Setup(r => r.GetAllAlbumsAsync())
            .ReturnsAsync(albumList);
        
        var controller = new AlbumController(repositoryMock.Object, serviceMock.Object);
        
        // Act
        var albums = await controller.GetAllAlbums();
        
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(albums.Result);
        var returnedAlbums = Assert.IsType<IEnumerable<Album>>(okResult.Value, exactMatch: false);
        var enumerable = returnedAlbums.ToList();
        Assert.Equal(albumList, enumerable);

        var retrievedAlbum1 = enumerable.ElementAt(0);
        var retrievedAlbum2 = enumerable.ElementAt(1);
        
        // just in case default comparator is missing anything
        Assert.Equal(albumList[0], retrievedAlbum1);
        Assert.Equal(albumList[1], retrievedAlbum2);
    }

    
    [Fact]
    public async Task PostAlbum_ExpectedRepositoryBehavior_ReturnsGeneratedId()
    {
        // Arrange
        var repositoryMock = new Mock<IAlbumRepository>();
        var serviceMock = new Mock<IUploadPhotoService>();

        var albumRequest = new AlbumController.CreateAlbumItemRequest("New album", "Album description");
        var generatedId = 12;

        repositoryMock.Setup(r => r.SaveAlbumAsync(It.IsAny<Album>()))
            .ReturnsAsync(generatedId);

        var controller = new AlbumController(repositoryMock.Object, serviceMock.Object);

        // Act
        var albumId = await controller.PostAlbum(albumRequest);

        // Assert
        Assert.Equal(generatedId, albumId.Value);
        repositoryMock.Verify(r => r.SaveAlbumAsync(It.Is<Album>(album =>
            album.Name == albumRequest.Name &&
            album.Description == albumRequest.Description)));
    }
    
    
    [Fact]
    public async Task PostAlbum_SaveAlbumAsyncFailure_ReturnsProblem()
    {
        // Arrange
        var repositoryMock = new Mock<IAlbumRepository>();
        var serviceMock = new Mock<IUploadPhotoService>();

        repositoryMock.Setup(r => r.SaveAlbumAsync(It.IsAny<Album>()))
            .ReturnsAsync((int?)null);

        var controller = new AlbumController(repositoryMock.Object, serviceMock.Object);

        // Act
        var album = await controller.PostAlbum(
            new AlbumController.CreateAlbumItemRequest("New album", "Album description"));

        // Assert
        var problemResult = Assert.IsType<ObjectResult>(album.Result);
        Assert.Equal(500, problemResult.StatusCode);
        repositoryMock.Verify(r => r.SaveAlbumAsync(It.IsAny<Album>()));
    }

    [Fact]
    public async Task UploadPhotoToAlbum_InvalidAlbumId_ReturnsForbidResult()
    {
        // Arrange
        var repositoryMock = new Mock<IAlbumRepository>();
        var serviceMock = new Mock<IUploadPhotoService>();
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1);

        repositoryMock.Setup(r => r.GetAlbumByIdAsync(99))
            .ReturnsAsync((Album)null);

        var controller = new AlbumController(repositoryMock.Object, serviceMock.Object);
        var combinedPhotoSpec = new CombinedPhotoSpecDto(fileMock.Object, "Photo", "Description", 2026);

        // Act
        var result = await controller.UploadPhotoToAlbum(99, combinedPhotoSpec);

        // Assert
        Assert.IsType<ForbidResult>(result);
        repositoryMock.Verify(r => r.GetAlbumByIdAsync(99));
        serviceMock.Verify(s => s.UploadPhoto(
            It.IsAny<Album>(),
            It.IsAny<IFormFile>(),
            It.IsAny<PhotoSpecDto>()),
            Times.Never);
    }

    [Fact]
    public async Task UploadPhotoToAlbum_combinedPhotoSpecIsMissingFile_ThrowsException()
    {
        // Arrange
        var repositoryMock = new Mock<IAlbumRepository>();
        var serviceMock = new Mock<IUploadPhotoService>();
        var controller = new AlbumController(repositoryMock.Object, serviceMock.Object);
        var combinedPhotoSpec = new CombinedPhotoSpecDto(null, "Photo", "Description", 2026);

        // Act and Assert
        await Assert.ThrowsAsync<NullReferenceException>(() =>
            controller.UploadPhotoToAlbum(99, combinedPhotoSpec));
    }

    [Fact]
    public async Task UploadPhotoToAlbum_combinedPhotoSpecFileHasFileLengthZero_ReturnsBadRequest()
    {
        // Arrange
        var repositoryMock = new Mock<IAlbumRepository>();
        var serviceMock = new Mock<IUploadPhotoService>();
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);

        var controller = new AlbumController(repositoryMock.Object, serviceMock.Object);
        var combinedPhotoSpec = new CombinedPhotoSpecDto(fileMock.Object, "Photo", "Description", 2026);

        // Act
        var result = await controller.UploadPhotoToAlbum(99, combinedPhotoSpec);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("File upload fail", badRequestResult.Value);
        repositoryMock.Verify(r => r.GetAlbumByIdAsync(It.IsAny<int>()), Times.Never);
        serviceMock.Verify(s => s.UploadPhoto(
            It.IsAny<Album>(),
            It.IsAny<IFormFile>(),
            It.IsAny<PhotoSpecDto>()),
            Times.Never);
    }

    [Fact]
    public async Task UploadPhotoToAlbum_combinedPhotoSpecNullablesAllNull_ReturnsOk()
    {
        // Arrange
        var repositoryMock = new Mock<IAlbumRepository>();
        var serviceMock = new Mock<IUploadPhotoService>();
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1);

        var album = new Album { Id = 7, Name = "Album", Description = "Description" };
        repositoryMock.Setup(r => r.GetAlbumByIdAsync(album.Id))
            .ReturnsAsync(album);
        serviceMock.Setup(s => s.UploadPhoto(
                album,
                fileMock.Object,
                It.Is<PhotoSpecDto>(spec =>
                    spec.Name == null &&
                    spec.Description == null &&
                    spec.YearContentCreated == null)))
            .ReturnsAsync(14);
        repositoryMock.Setup(r => r.AddPhotoToAlbumAsync(album.Id, 14))
            .ReturnsAsync(true);

        var controller = new AlbumController(repositoryMock.Object, serviceMock.Object);
        var combinedPhotoSpec = new CombinedPhotoSpecDto(fileMock.Object, null, null, null);

        // Act
        var result = await controller.UploadPhotoToAlbum(album.Id, combinedPhotoSpec);

        // Assert
        Assert.IsType<OkResult>(result);
        serviceMock.Verify(s => s.UploadPhoto(
            album,
            fileMock.Object,
            It.Is<PhotoSpecDto>(spec =>
                spec.Name == null &&
                spec.Description == null &&
                spec.YearContentCreated == null)));
        repositoryMock.Verify(r => r.AddPhotoToAlbumAsync(album.Id, 14));
    }

    [Fact]
    public async Task UploadPhotoToAlbum_ExpectedParametersAndBehavior_ReturnsOk()
    {
        // Arrange
        var repositoryMock = new Mock<IAlbumRepository>();
        var serviceMock = new Mock<IUploadPhotoService>();
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1);

        var album = new Album { Id = 7, Name = "Album", Description = "Description" };
        var name = "Photo";
        var description = "Photo description";
        var yearContentCreated = 2026;

        repositoryMock.Setup(r => r.GetAlbumByIdAsync(album.Id))
            .ReturnsAsync(album);
        serviceMock.Setup(s => s.UploadPhoto(
                album,
                fileMock.Object,
                It.Is<PhotoSpecDto>(spec =>
                    spec.Name == name &&
                    spec.Description == description &&
                    spec.YearContentCreated == yearContentCreated)))
            .ReturnsAsync(14);
        repositoryMock.Setup(r => r.AddPhotoToAlbumAsync(album.Id, 14))
            .ReturnsAsync(true);

        var controller = new AlbumController(repositoryMock.Object, serviceMock.Object);
        var combinedPhotoSpec = new CombinedPhotoSpecDto(
            fileMock.Object,
            name,
            description,
            yearContentCreated);

        // Act
        var result = await controller.UploadPhotoToAlbum(album.Id, combinedPhotoSpec);

        // Assert
        Assert.IsType<OkResult>(result);
        repositoryMock.Verify(r => r.GetAlbumByIdAsync(album.Id));
        serviceMock.Verify(s => s.UploadPhoto(
            album,
            fileMock.Object,
            It.Is<PhotoSpecDto>(spec =>
                spec.Name == name &&
                spec.Description == description &&
                spec.YearContentCreated == yearContentCreated)));
        repositoryMock.Verify(r => r.AddPhotoToAlbumAsync(album.Id, 14));
    }

    [Fact]
    public async Task UploadPhotoToAlbum_serviceUploadPhotoReturnsNull_ReturnsProblem()
    {
        // Arrange
        var repositoryMock = new Mock<IAlbumRepository>();
        var serviceMock = new Mock<IUploadPhotoService>();
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1);

        var album = new Album { Id = 7, Name = "Album", Description = "Description" };
        repositoryMock.Setup(r => r.GetAlbumByIdAsync(album.Id))
            .ReturnsAsync(album);
        serviceMock.Setup(s => s.UploadPhoto(
                It.IsAny<Album>(),
                It.IsAny<IFormFile>(),
                It.IsAny<PhotoSpecDto>()))
            .ReturnsAsync((int?)null);

        var controller = new AlbumController(repositoryMock.Object, serviceMock.Object);
        var combinedPhotoSpec = new CombinedPhotoSpecDto(fileMock.Object, "Photo", "Description", 2026);

        // Act
        var result = await controller.UploadPhotoToAlbum(album.Id, combinedPhotoSpec);

        // Assert
        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problemResult.StatusCode);
        repositoryMock.Verify(r => r.AddPhotoToAlbumAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UploadPhotoToAlbum_repositoryAddPhotoToAlbumAsyncReturnsFalse_ReturnsProblem()
    {
        // Arrange
        var repositoryMock = new Mock<IAlbumRepository>();
        var serviceMock = new Mock<IUploadPhotoService>();
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1);

        var album = new Album { Id = 7, Name = "Album", Description = "Description" };
        repositoryMock.Setup(r => r.GetAlbumByIdAsync(album.Id))
            .ReturnsAsync(album);
        serviceMock.Setup(s => s.UploadPhoto(
                It.IsAny<Album>(),
                It.IsAny<IFormFile>(),
                It.IsAny<PhotoSpecDto>()))
            .ReturnsAsync(14);
        repositoryMock.Setup(r => r.AddPhotoToAlbumAsync(album.Id, 14))
            .ReturnsAsync(false);

        var controller = new AlbumController(repositoryMock.Object, serviceMock.Object);
        var combinedPhotoSpec = new CombinedPhotoSpecDto(fileMock.Object, "Photo", "Description", 2026);

        // Act
        var result = await controller.UploadPhotoToAlbum(album.Id, combinedPhotoSpec);

        // Assert
        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problemResult.StatusCode);
        repositoryMock.Verify(r => r.AddPhotoToAlbumAsync(album.Id, 14));
    }

    // [Fact]
    // public async Task GetAllPhotos_InvalidAlbumId_ReturnsNull()
    // {
    //     // Arrange
    //     var repositoryMock = new Mock<IAlbumRepository>();
    //     var serviceMock = new Mock<IUploadPhotoService>();
    //
    //     repositoryMock.Setup(r => r.GetAlbumPhotosAsyncEnumerable(99))
    //         .ReturnsAsync((IAsyncEnumerable<IAlbumRepository.AlbumPhotoDto>)null);
    //
    //     var controller = new AlbumController(repositoryMock.Object, serviceMock.Object);
    //
    //     // Act
    //     var photos = await controller.GetAllPhotos(99);
    //
    //     // Assert
    //     Assert.Null(photos);
    //     repositoryMock.Verify(r => r.GetAlbumPhotosAsyncEnumerable(99));
    // }

    // [Fact]
    // public async Task GetAllPhotos_AlbumHasSinglePhoto_ReturnsSameSinglePhotoInList()
    // {
    //     // Arrange
    //     var repositoryMock = new Mock<IAlbumRepository>();
    //     var serviceMock = new Mock<IUploadPhotoService>();
    //
    //     var image = new Image
    //     {
    //         Id = 99,
    //         AltText = "alt text",
    //         ContentType = "image/png",
    //         FileName = "image",
    //         Height = 57,
    //         StorageFileName = "Somewhere",
    //         Url = "Iam/Somewhere",
    //         Width = 2
    //     };
    //
    //     var photo = new IAlbumRepository.AlbumPhotoDto(
    //         1,
    //         "Photo",
    //         "Photo description",
    //         2026,
    //         image);
    //     var photoList = new List<IAlbumRepository.AlbumPhotoDto> { photo };
    //
    //     repositoryMock.Setup(r => r.GetAlbumPhotosAsyncEnumerable(1))
    //         .ReturnsAsync(photoList.ToAsyncEnumerable());
    //
    //     var controller = new AlbumController(repositoryMock.Object, serviceMock.Object);
    //
    //     // Act
    //     var photos = await controller.GetAllPhotos(1);
    //     var returnedPhotos = new List<IAlbumRepository.AlbumPhotoDto>();
    //     await foreach (var returnedPhoto in photos!)
    //     {
    //         returnedPhotos.Add(returnedPhoto);
    //     }
    //
    //     // Assert
    //     var item = Assert.Single(returnedPhotos);
    //     Assert.Equal(photo, item);
    // }

    // [Fact]
    // public async Task GetAllPhotos_AlbumHasPhotos_ReturnsListOfSamePhotos()
    // {
    //     // Arrange
    //     var repositoryMock = new Mock<IAlbumRepository>();
    //     var serviceMock = new Mock<IUploadPhotoService>();
    //
    //     var image = new Image
    //     {
    //         Id = 99,
    //         AltText = "alt text",
    //         ContentType = "image/png",
    //         FileName = "image",
    //         Height = 57,
    //         StorageFileName = "Somewhere",
    //         Url = "Iam/Somewhere",
    //         Width = 2
    //     };
    //
    //     var photo1 = new IAlbumRepository.AlbumPhotoDto(
    //         1,
    //         "Photo 1",
    //         "Photo 1 description",
    //         2026,
    //         image);
    //     var photo2 = new IAlbumRepository.AlbumPhotoDto(
    //         2,
    //         "Photo 2",
    //         "Photo 2 description",
    //         2025,
    //         image);
    //     var photoList = new List<IAlbumRepository.AlbumPhotoDto> { photo1, photo2 };
    //
    //     repositoryMock.Setup(r => r.GetAlbumPhotosAsyncEnumerable(1))
    //         .ReturnsAsync(photoList.ToAsyncEnumerable());
    //
    //     var controller = new AlbumController(repositoryMock.Object, serviceMock.Object);
    //
    //     // Act
    //     var photos = await controller.GetAllPhotos(1);
    //     var returnedPhotos = new List<IAlbumRepository.AlbumPhotoDto>();
    //     await foreach (var returnedPhoto in photos!)
    //     {
    //         returnedPhotos.Add(returnedPhoto);
    //     }
    //
    //     // Assert
    //     Assert.Equal(photoList, returnedPhotos);
    //
    //     var retrievedPhoto1 = returnedPhotos.ElementAt(0);
    //     var retrievedPhoto2 = returnedPhotos.ElementAt(1);
    //
    //     // just in case default comparator is missing anything
    //     Assert.Equal(photoList[0], retrievedPhoto1);
    //     Assert.Equal(photoList[1], retrievedPhoto2);
    // }
    */
}
