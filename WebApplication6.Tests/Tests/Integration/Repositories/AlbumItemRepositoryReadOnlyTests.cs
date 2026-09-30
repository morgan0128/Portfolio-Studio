using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using WebApplication6.Backend.Data;
using WebApplication6.Backend.Models;
using WebApplication6.Backend.Repositories;
using Xunit;

namespace WebApplication6.Tests.Tests.Integration.Repositories;

public class AlbumItemRepositoryReadOnlyTests(TestDatabaseFixture dbFixture) : IClassFixture<TestDatabaseFixture>
{
    public TestDatabaseFixture Fixture { get; } = dbFixture;

    [Fact]
    public async Task GetAlbumItems_ReturnsExistingAlbumOrderAndDisplaySettings()
    {
        await using var context = Fixture.CreateContext();
        var repository = new AlbumItemRepository(context);

        var photos = (await repository.GetAlbumItems(1))
            .Select(item => Assert.IsType<IAlbumItemRepository.PhotoDisplayAlbumItemDto>(item))
            .ToList();

        Assert.Equal(new[] { 4, 2, 3, 64 }, photos.Select(p => p.Content.Photo.Id));
        Assert.Equal(new[] { 0, 1, 2, 4 }, photos.Select(p => p.Order));
        Assert.All(photos, photo =>
        {
            Assert.NotNull(photo.Content.Photo.Image);
            Assert.True(photo.Content.DisplaysName);
            Assert.True(photo.Content.DisplaysDescription);
            Assert.True(photo.Content.DisplaysYearContentCreated);
        });
        Assert.Empty(await repository.GetAlbumItems(3002));
    }
}
