// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.EntityFrameworkCore;
// using WebApplication6.Backend.Models;
// using WebApplication6.Backend.Repositories;
// using Xunit;
//
// namespace WebApplication6.Tests.Tests.Integration.Repositories;
//
// // Tests against test database.
// public class AlbumRepositoryReadOnlyTests(TestDatabaseFixture dbFixture) : IClassFixture<TestDatabaseFixture>
// {
//     public TestDatabaseFixture Fixture { get; } = dbFixture;
//
//     [Fact]
//     public async Task GetAllAlbumsAsync_ReturnsExpectedValue()
//     {
//         await using var context = Fixture.CreateContext();
//         var repository = new AlbumRepository(context);
//
//         var albums = await repository.GetAllAlbumsAsync();
//         var albumsList = albums.ToList();
//         
//         Assert.NotNull(albums);
//         Assert.Collection(albumsList,
//             element1Inspector => Assert.Equal(1, element1Inspector.Id),
//             element2Inspector => Assert.Equal(3002, element2Inspector.Id)
//         );
//     }
//
//     [Fact]
//     public async Task GetAlbumPhotosAsync_ReturnsExistingAlbumOrderAndDisplaySettings()
//     {
//         await using var context = Fixture.CreateContext();
//         var repository = new AlbumItemRepository(context);
//
//         var photos = (await repository.GetAlbumPhotosAsync(1)).ToList();
//
//         Assert.Equal(new[] { 4, 2, 3, 64 }, photos.Select(p => p.PhotoId));
//         Assert.Equal(new int?[] { 0, 1, 2, 4 }, photos.Select(p => p.Order));
//         Assert.All(photos, photo =>
//         {
//             Assert.NotNull(photo.Image);
//             Assert.True(photo.DisplaysName);
//             Assert.True(photo.DisplaysDescription);
//             Assert.True(photo.DisplaysYearContentCreated);
//         });
//         Assert.Empty(await repository.GetAlbumPhotosAsync(3002));
//     }
//
//     [Theory]
//     [InlineData(1, -1, 0)]
//     [InlineData(1, 63, 0)]
//     [InlineData(1, -1, 2)]
//     [InlineData(1, 63, 2)]
//     public async Task ReorderPhotoInAlbum_InvalidKeyAlbumPhotoDoesNotExistPhoto_ReturnsFalse(int albumId, int photoId, int newOrder)
//     {
//         await using var context = Fixture.CreateContext();
//         var repository = new AlbumItemRepository(context);
//         
//         // double-check our inputs:
//         // test should not fail based on invalid albumId; want to use valid albumId
//         var album = await context.Albums.FindAsync([albumId], TestContext.Current.CancellationToken);
//         Assert.NotNull(album);
//         
//         var photo = await context.Photos.FindAsync([photoId], TestContext.Current.CancellationToken);
//         Assert.Null(photo);
//
//         var task = await repository.ReorderPhotoInAlbum(albumId, photoId, newOrder);
//         Assert.False(task);
//     }
//     
//     [Theory]
//     [InlineData(-1, 3, 0)]
//     [InlineData(22, 3, 0)]
//     [InlineData(-1, 3, 2)]
//     [InlineData(22, 3, 2)]
//     public async Task ReorderPhotoInAlbum_InvalidKeyAlbumPhotoDoesNotExistAlbum_ReturnsFalse(int albumId, int photoId, int newOrder)
//     {
//         await using var context = Fixture.CreateContext();
//         var repository = new AlbumItemRepository(context);
//         
//         // double-check our inputs:
//         // test should not fail based on invalid photoId; want to use valid photoId
//         var photo = await context.Photos.FindAsync([photoId], TestContext.Current.CancellationToken);
//         Assert.NotNull(photo);
//         
//         var album = await context.Albums.FindAsync([albumId], TestContext.Current.CancellationToken);
//         Assert.Null(album);
//
//         
//         var task = await repository.ReorderPhotoInAlbum(albumId, photoId, newOrder);
//         Assert.False(task);
//     }
//     
//     [Theory]
//     [InlineData(3002, 3, 0)]
//     [InlineData(3002, 4, 0)]
//     public async Task ReorderPhotoInAlbum_InvalidKeyAlbumPhotoAlthoughBothExist_ReturnsFalse(int albumId, int photoId, int newOrder)
//     {
//         await using var context = Fixture.CreateContext();
//         var repository = new AlbumItemRepository(context);
//
//         // double-check our inputs:
//         var album = await context.Albums.FindAsync([albumId], TestContext.Current.CancellationToken);
//         Assert.NotNull(album);
//         var photo = await context.Photos.FindAsync([photoId], TestContext.Current.CancellationToken);
//         Assert.NotNull(photo);
//
//         var task = await repository.ReorderPhotoInAlbum(albumId, photoId, newOrder);
//         Assert.False(task);
//     }
//     
//
// }