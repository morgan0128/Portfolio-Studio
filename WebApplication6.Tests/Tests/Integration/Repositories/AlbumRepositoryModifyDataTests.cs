using System;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WebApplication6.Backend.Data;
using WebApplication6.Backend.Models;
using WebApplication6.Backend.Repositories;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;
using XunitException = Xunit.MicrosoftTestingPlatform.XunitException;

namespace WebApplication6.Tests.Tests.Integration.Repositories;

public class AlbumRepositoryModifyDataTests(PrimaryTestDatabaseFixture dbFixture)
    : IClassFixture<PrimaryTestDatabaseFixture>
{
    public PrimaryTestDatabaseFixture Fixture { get; } = dbFixture;
    
    /* SaveAlbumAsync */
    [Theory]
    [InlineData(null, "Album712", PageLayoutPreset.Default)]
    [InlineData("", null, PageLayoutPreset.Cozy)]
    [InlineData("Hello Album712", "", null)]
    [InlineData("Hi Album712", "Album712", PageLayoutPreset.Spooky)]
    [InlineData("", null, PageLayoutPreset.Spooky)]
    [InlineData(null, "", null)]
    [InlineData(null, null, PageLayoutPreset.Cozy)]
    [InlineData(null, null, null)]
    public async Task SaveAlbumAsync_GeneralExpectedCases_SavesAlbumAndReturnsAsDto([CanBeNull] string description, 
        [CanBeNull] string navTitle, PageLayoutPreset? preset)
    {
        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var parameterAlbum = new Album { Name = "MyAlbum712" };
        if (description is not null) parameterAlbum.Description = description;
        if (navTitle is not null) parameterAlbum.NavTitle = navTitle;
        if (preset is not null) parameterAlbum.LayoutPreset = preset.Value;
        var expectedDto = CreateExpectedAlbumToDto(parameterAlbum);

        var albumDto = await repository.SaveAlbumAsync(parameterAlbum);
        Assert.NotNull(albumDto);
        
        var contextAlbum = await context.Albums
            .AsNoTracking()
            .SingleAsync(a => a.Id == albumDto.Id);
        
        Assert.True(albumDto.Id > 0);
        Assert.Equal(parameterAlbum.Id, albumDto.Id);
        
        expectedDto = expectedDto with { Id = albumDto.Id };
        Assert.Equal(expectedDto, albumDto);
        Assert.Equal(expectedDto, CreateExpectedAlbumToDto(contextAlbum));
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
        var contextAlbum = await context.Albums
            .AsNoTracking()
            .SingleAsync(a => a.Name == "VisibleAlbum?" && a.NavTitle == "I am hiding");
        
        Assert.NotNull(albumDto);
        Assert.Equal("I am hiding", albumDto.NavTitle);
        Assert.False(albumDto.Published);
        Assert.False(contextAlbum.Published);
        Assert.Equal(-1, albumDto.NavbarOrder);
        Assert.Equal(-1, contextAlbum.NavbarOrder);
    }
    
    // Careful: do NOT input Sentinel for Album.Id
    [Theory]
    [InlineData(7)]
    [InlineData(-7)]
    public async Task SaveAlbumAsync_AlbumIsSuppliedWithId_CancelsOperationAndReturnsNull(int suppliedId)
    {
        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var parameterAlbum = new Album { Id = suppliedId, Name = "MyAlbum713", NavTitle = "Album713" };
        
        var albumDto = await repository.SaveAlbumAsync(parameterAlbum);
        var contextAlbum = await context.Albums.FirstOrDefaultAsync(a => a.Name == "MyAlbum713");

        Assert.Null(albumDto);
        Assert.Null(contextAlbum);
    }
    
    /* SetLayoutPresetAsync */
    [Theory]
    [InlineData(1, PageLayoutPreset.Default)]
    [InlineData(1, PageLayoutPreset.Spooky)]
    [InlineData(3, PageLayoutPreset.Default)]
    public async Task SetLayoutPresetAsync_AlbumExists_SavesChangeAndReturnsAlbumAsDto(int id, PageLayoutPreset preset)
    {
        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var originalAlbum = await context.Albums
            .AsNoTracking()
            .SingleAsync(a => a.Id == id);
        var expectedDto = CreateExpectedAlbumToDto(originalAlbum) with { LayoutPreset = preset };

        var albumDto = await repository.SetLayoutPresetAsync(id, preset);

        Assert.NotNull(albumDto);
        Assert.Equal(expectedDto, albumDto);

        var contextAlbum = await context.Albums
            .AsNoTracking()
            .SingleAsync(a => a.Id == id);

        Assert.Equal(expectedDto, CreateExpectedAlbumToDto(contextAlbum));
    }
    
    [Fact]
    public async Task SetLayoutPresetAsync_AlbumDoesNotExist_ReturnsNull()
    {
        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var albumDto = await repository.SetLayoutPresetAsync(-1, PageLayoutPreset.Spooky);
        Assert.Null(albumDto);
    }
    
    
    
    

    /// <summary>
    ///     Test version of AlbumRepository.AlbumToDto(Album album), kept independent/hardcoded:
    ///     results in less chance of a dto mapping bug slipping through.
    /// </summary>
    /// <returns>
    ///     An AlbumDto, which represents the expected Album-to-dto mapping utilized in AlbumRepository.
    /// </returns>
    private static IAlbumRepository.AlbumDto CreateExpectedAlbumToDto(Album album)
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
