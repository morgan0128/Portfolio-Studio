#nullable enable
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
    [InlineData("712", null, PageLayoutPreset.Spooky)]
    [InlineData(null, null, PageLayoutPreset.Cozy)]
    [InlineData(null, null, null)]
    public async Task SaveAlbumAsync_GeneralExpectedCases_SavesAlbumAndReturnsAsDto(string? description, 
        string? navTitle, PageLayoutPreset? preset)
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
        var albumId = albumDto.Id;
        Assert.Equal(parameterAlbum.Id, albumId);
        
        expectedDto = expectedDto with { Id = albumDto.Id };
        Assert.Equal(expectedDto, albumDto);
        
        var contextAlbum = await GetAlbumStateFromContextById(context, albumId);
        Assert.NotNull(contextAlbum);
        Assert.Equal(expectedDto, CreateExpectedAlbumToDto(contextAlbum));
    }
    
    [Theory]
    [InlineData(false, -2)]
    [InlineData(false, -1)]
    [InlineData(false, 0)]
    [InlineData(false, 4)]
    [InlineData(false, 5)]
    [InlineData(true, -2)]
    [InlineData(true, -1)]
    [InlineData(true, 0)]
    [InlineData(true, 4)]
    [InlineData(true, 5)]
    public async Task SaveAlbumAsync_AlbumSuppliedPublishedAndOrNavbarOrder_RevertsValuesAsOutlinedThenSavesAlbumAndReturnsAsDto(bool published, int navbarOrder)
    {
        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var parameterAlbum = new Album { Name = "VisibleAlbum?", NavTitle = "I am hiding", Published = published, NavbarOrder = navbarOrder};
        
        var albumDto = await repository.SaveAlbumAsync(parameterAlbum);
        Assert.NotNull(albumDto);
        var albumId = albumDto.Id;
        Assert.Equal(albumId, parameterAlbum.Id);
        
        var contextAlbum = await GetAlbumStateFromContextById(context, albumId);
        Assert.NotNull(contextAlbum);
        
        Assert.Equal("I am hiding", albumDto.NavTitle);
        Assert.False(albumDto.Published);
        Assert.False(contextAlbum.Published);
        Assert.Equal(-1, albumDto.NavbarOrder);
        Assert.Equal(-1, contextAlbum.NavbarOrder);
    }
    
    [Theory]
    [InlineData(7)]
    [InlineData(-7)]
    public async Task SaveAlbumAsync_AlbumSuppliedWithNotSentinelValueId_CancelsOperationAndReturnsNull(int suppliedId)
    {
        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);
        
        var contextAlbum = await GetAlbumStateFromContextById(context, suppliedId);
        Assert.Null(contextAlbum);

        var parameterAlbum = new Album { Id = suppliedId, Name = "MyAlbum71341", NavTitle = "Album71322" };
        var albumDto = await repository.SaveAlbumAsync(parameterAlbum);
        Assert.Null(albumDto);

        contextAlbum = await GetAlbumStateFromContextById(context, suppliedId);
        Assert.Null(contextAlbum);
        contextAlbum = await context.Albums.FirstOrDefaultAsync(a => a.Name == "MyAlbum71341" && a.NavTitle == "Album71322");
        Assert.Null(contextAlbum);
    }

    [Fact]
    public async Task SaveAlbumAsync_AlbumAlreadyExists_ReturnsNullAndLeavesAlbumUnchanged()
    {
        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var albumPreSave = await GetAlbumStateFromContextById(context, 1);
        Assert.NotNull(albumPreSave);
        var priorDto = CreateExpectedAlbumToDto(albumPreSave);
        albumPreSave.NavTitle = "2026csgrad";

        var albumDto = await repository.SaveAlbumAsync(albumPreSave);
        Assert.Null(albumDto);

        var albumPostSave = await GetAlbumStateFromContextById(context, 1);
        Assert.NotNull(albumPostSave);

        Assert.Equivalent(priorDto, CreateExpectedAlbumToDto(albumPostSave));
    }
    
    /* SetLayoutPresetAsync */
    [Fact]
    public async Task SetLayoutPresetAsync_AlbumExists_SavesChangeAndReturnsAlbumAsDto()
    {
        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);

        var parameterAlbum1 = new Album { Name = "test1name" };
        var parameterAlbum2 = new Album { Name = "test2name", LayoutPreset = PageLayoutPreset.Cozy };
        context.Albums.AddRange(parameterAlbum1, parameterAlbum2);
        await context.SaveChangesAsync();

        var id1 = parameterAlbum1.Id;
        var id2 = parameterAlbum2.Id;
        var expectedDto1 = CreateExpectedAlbumToDto(parameterAlbum1);
        var expectedDto2 = CreateExpectedAlbumToDto(parameterAlbum2);
        context.ChangeTracker.Clear();

        var album1 = await repository.SetLayoutPresetAsync(id1, PageLayoutPreset.Spooky);
        expectedDto1 = expectedDto1 with { LayoutPreset = PageLayoutPreset.Spooky };
        Assert.Equal(expectedDto1, album1);
        var contextAlbum1 = await GetAlbumStateFromContextById(context, id1);
        var contextAlbum2 = await GetAlbumStateFromContextById(context, id2);
        Assert.NotNull(contextAlbum1);
        Assert.NotNull(contextAlbum2);
        Assert.Equal(expectedDto1, CreateExpectedAlbumToDto(contextAlbum1));
        Assert.Equal(expectedDto2, CreateExpectedAlbumToDto(contextAlbum2));

        var album2 = await repository.SetLayoutPresetAsync(id2, PageLayoutPreset.Default);
        expectedDto2 = expectedDto2 with { LayoutPreset = PageLayoutPreset.Default };
        Assert.Equal(expectedDto2, album2);
        contextAlbum1 = await GetAlbumStateFromContextById(context, id1);
        contextAlbum2 = await GetAlbumStateFromContextById(context, id2);
        Assert.NotNull(contextAlbum1);
        Assert.NotNull(contextAlbum2);
        Assert.Equal(expectedDto1, CreateExpectedAlbumToDto(contextAlbum1));
        Assert.Equal(expectedDto2, CreateExpectedAlbumToDto(contextAlbum2));
    }

    
    [Fact]
    public async Task SetLayoutPresetAsync_AlbumDoesNotExist_ReturnsNull()
    {
        using var context = Fixture.CreateContext();
        var repository = new AlbumRepository(context);
        
        var albumDto = await repository.SetLayoutPresetAsync(-1, PageLayoutPreset.Spooky);
        Assert.Null(albumDto);
    }
    
    /*
     * Deeper Integration Tests
     */
    // [Fact]
    // public async Task SaveAlbums_AlbumGetterCountsReflectExpected()
    // {
    //     using var context = Fixture.CreateContext();
    //     var repository = new AlbumRepository(context);
    //     
    //     
    //     var parameterAlbum1 = new Album { Name = "MyAlbum71", NavTitle = "MyAlbum", Published = true, NavbarOrder = 3 }; // Expect: Published = false, NavbarOrder = -1
    //     var parameterAlbum2 = new Album { Name = "Album", NavTitle = "My", Published = true }; // Expect: Published = false
    //     var parameterAlbum3 = new Album { Name = "2003", NavTitle = "BornThen", Published = false, NavbarOrder = 4 }; // Expect: NavbarOrder = -1
    //     var parameterAlbum4 = new Album { Name = "AlbumOfMine", Id = 77 }; // Expect: Album is not saved
    //     
    //     var albumCountPrior = (await repository.GetAlbumIdsAsync()).Count();
    //     var publishedCountPrior = (await repository.GetPublishedAlbumsAsync()).Count();
    //     var publishedNavExcludedCountPrior = (await repository.GetPublishedNavExcludedAlbumsAsync()).Count();
    //     var navIncludedCountPrior = (await repository.GetNavIncludedAlbumsAsync()).Count();
    //
    //     await repository.SaveAlbumAsync(parameterAlbum1);
    //     await repository.SaveAlbumAsync(parameterAlbum2);
    //     await repository.SaveAlbumAsync(parameterAlbum3);
    //     await repository.SaveAlbumAsync(parameterAlbum4); // Expect: Operation cancelled, no save occurred
    //
    //     var albumCountPost = (await repository.GetAlbumIdsAsync()).Count();
    //     Assert.Equal((albumCountPrior + 3), albumCountPost);
    //     
    //     var publishedCountPost = (await repository.GetPublishedAlbumsAsync()).Count();
    //     var publishedNavExcludedCountPost = (await repository.GetPublishedNavExcludedAlbumsAsync()).Count();
    //     var navIncludedCountPost = (await repository.GetNavIncludedAlbumsAsync()).Count();
    //     Assert.Equal(publishedCountPrior, publishedCountPost);
    //     Assert.Equal(publishedNavExcludedCountPrior, publishedNavExcludedCountPost);
    //     Assert.Equal(navIncludedCountPrior, navIncludedCountPost);
    // }
    
    
    
    

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

    private static async Task<Album?> GetAlbumStateFromContextById(ApplicationDbContext context, int id) => await context.Albums.AsNoTracking().SingleOrDefaultAsync(a => a.Id == id);

}
