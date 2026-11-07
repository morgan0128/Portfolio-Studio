using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApplication6.Backend.Models;
using WebApplication6.Backend.Repositories;

namespace WebApplication6.Tests.Tests.Integration.Repositories.AlbumRepositoryTransactionalModifyDataTests.Base;

public abstract class AlbumRepositoryTransactionalModifyDataTestClass(TransactionalTestDatabaseFixture dbFixture) : IDisposable
{
    public TransactionalTestDatabaseFixture Fixture { get; } = dbFixture;
    public void Dispose() => Fixture.Cleanup();
    
    protected async Task<List<IAlbumRepository.AlbumDto>> GetAlbumStatesFromDatabaseAsync()
    {
        using var context = Fixture.CreateContext();
        var albums = await context.Albums
            .AsNoTracking()
            .ToListAsync();
        return [.. albums.Select(CreateExpectedAlbumToDto)];
    }

    /// <summary>
    ///     Test version of AlbumRepository.AlbumToDto(Album album), kept independent/hardcoded:
    ///     results in less chance of a dto mapping bug slipping through.
    /// </summary>
    /// <returns>
    ///     An AlbumDto, which represents the expected Album-to-dto mapping utilized in AlbumRepository.
    /// </returns>
    protected static IAlbumRepository.AlbumDto CreateExpectedAlbumToDto(Album album)
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