using System;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WebApplication6.Backend.Data;
using WebApplication6.Backend.Models;
using WebApplication6.Backend.Repositories;
using Xunit;

namespace WebApplication6.Tests.Tests.Integration.Repositories;

public class AlbumRepositoryModifyDataTests(PrimaryTestDatabaseFixture dbFixture)
    : IClassFixture<PrimaryTestDatabaseFixture>
{
    public PrimaryTestDatabaseFixture Fixture { get; } = dbFixture;
    
    /* SaveAlbumAsync */
    [Fact]
    public async Task SaveAlbumAsync_ExpectedCase_SavesAlbumAndReturnsAsDto()
    {
        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var parameterAlbum = new Album { Name = "MyAlbum712", NavTitle = "Album712" };
        
        var albumDto = await repository.SaveAlbumAsync(parameterAlbum);
        var contextAlbum = await context.Albums.SingleAsync(a => a.Name == "MyAlbum712");

        Assert.NotNull(albumDto);
        Assert.Equal("Album712", albumDto.NavTitle);
        Assert.Equal(albumDto, TestAlbumToDto(contextAlbum));
    }
    
    [Theory]
    [InlineData(false, -1)]
    [InlineData(false, 4)]
    [InlineData(null, 0)]
    [InlineData(true, 0)]
    [InlineData(true, 4)]
    [InlineData(true, 5)]
    [InlineData(true, null)]
    public async Task SaveAlbumAsync_AlbumSuppliedPublishedAndOrNavbarOrder_RevertsValuesAsOutlinedThenSavesAlbumAndReturnsAsDto(bool? published, int? navbarOrder)
    {
        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var parameterAlbum = new Album { Name = "VisibleAlbum?", NavTitle = "I am hiding" };
        if (published is not null) parameterAlbum.Published = published.Value;
        if (navbarOrder is not null) parameterAlbum.NavbarOrder = navbarOrder.Value;
        
        var albumDto = await repository.SaveAlbumAsync(parameterAlbum);
        var contextAlbum = await context.Albums.SingleAsync(a => a.Name == "VisibleAlbum?");
        
        Assert.NotNull(albumDto);
        Assert.Equal("I am hiding", albumDto.NavTitle);
        Assert.False(albumDto.Published);
        Assert.False(contextAlbum.Published);
        Assert.Equal(-1, albumDto.NavbarOrder);
        Assert.Equal(-1, contextAlbum.NavbarOrder);
    }
    
    [Fact]
    public async Task SaveAlbumAsync_AlbumIsSuppliedWithId_CancelsOperationAndReturnsNull()
    {
        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var parameterAlbum = new Album { Id = 7, Name = "MyAlbum713", NavTitle = "Album713" };
        
        var albumDto = await repository.SaveAlbumAsync(parameterAlbum);
        var contextAlbum = await context.Albums.FirstOrDefaultAsync(a => a.Name == "MyAlbum713");

        Assert.Null(albumDto);
        Assert.Null(contextAlbum);
    }
    
    /* SetLayoutPresetAsync */
    [Theory]
    [InlineData(PageLayoutPreset.Default)]
    [InlineData(PageLayoutPreset.Spooky)]
    public async Task SetLayoutPresetAsync_AlbumExists_SavesChangeAndReturnsAlbumAsDto(PageLayoutPreset preset)
    {
        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var albumDto = await repository.SetLayoutPresetAsync(1, preset);
        var contextAlbum = await context.Albums.FindAsync(1);
        
        Assert.NotNull(albumDto);
        Assert.Equal(preset, albumDto.LayoutPreset);
        Assert.Equal(albumDto, TestAlbumToDto(contextAlbum));
    }
    
    [Fact]
    public async Task SetLayoutPresetAsync_AlbumDoesNotExist_ReturnsNull()
    {
        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var albumDto = await repository.SetLayoutPresetAsync(-1, PageLayoutPreset.Spooky);
        Assert.Null(albumDto);
    }
    
    
    
    
    
    // TODO better approach?
    private static IAlbumRepository.AlbumDto TestAlbumToDto(Album album)
    {
        return new IAlbumRepository.AlbumDto
        (
            Id: album.Id,
            Name: album.Name,
            Description: album.Description,
            NavTitle: album.NavTitle,
            Published: album.Published,
            LayoutPreset: album.LayoutPreset, NavbarOrder: album.NavbarOrder);
    }

}
