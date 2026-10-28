using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WebApplication6.Backend.Data;
using WebApplication6.Backend.Models;

namespace WebApplication6.Backend.Repositories;

public class AlbumRepository(ApplicationDbContext context) : IAlbumRepository
{
    private const long NavbarLockKey = 0x4E4156424152; // "NAVBAR"

    public async Task<IEnumerable<IAlbumRepository.AlbumDto>> GetAlbumsAsync()
    {
        var albums = await context.Albums
            .AsNoTracking()
            .Select(ToDto)
            .ToListAsync();
        
        return albums;
    }

    
    public async Task<IEnumerable<int>> GetAlbumIdsAsync()
    {
        var albumIds = await context.Albums
            .AsNoTracking()
            .Select(a => a.Id)
            .ToListAsync();
        
        return albumIds;
    }

    
    public async Task<int> GetAlbumsCountAsync()
    {
        var amount = await context.Albums
            .CountAsync();

        return amount;
    }

    
    public async Task<IAlbumRepository.AlbumDto?> GetAlbumByIdAsync(int id)
    {
        var album = await context.Albums
            .Where(album => album.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync();

        
        return album;
    }

    
    public async Task<IAlbumRepository.AlbumDto?> SaveAlbumAsync(Album album)
    {
        var idPropertySentinel = context.Model
            .FindEntityType(typeof(Album))!
            .FindProperty(nameof(Album.Id))!
            .Sentinel;
        if (!Equals(album.Id, idPropertySentinel)) return null; // Reject explicit Id
        
        if (album.NavbarOrder != -1) album.NavbarOrder = -1;
        if (album.Published) album.Published = false;
        context.Albums.Add(album);
        
        await context.SaveChangesAsync();
        return AlbumToDto(album);
    }

    
    public async Task<bool> DeleteAlbumByIdAsync(int id)
    {
        await using var transaction = await BeginNavbarTransactionAsync();
        var album = await context.Albums.FindAsync(id);
        if (album == null) return false;

        context.Albums.Remove(album);
        await context.SaveChangesAsync();
        
        var navbarAlbums = await GetNavbarAlbumsOrderedAsync();
        var albumsToMove = navbarAlbums
            .Select((navbarAlbum, index) => (Album: navbarAlbum, Order: index))
            .Where(entry => entry.Album.NavbarOrder != entry.Order)
            .ToList();
        if (albumsToMove.Count > 0)
        {
            foreach (var entry in albumsToMove)
            {
                entry.Album.NavbarOrder = -1;
            }
            await context.SaveChangesAsync();
            foreach (var entry in albumsToMove)
            {
                entry.Album.NavbarOrder = entry.Order;
            }
            await context.SaveChangesAsync();
        }

        await transaction.CommitAsync();
        return true;
    }

    
    public async Task<IEnumerable<IAlbumRepository.AlbumDto>> GetPublishedAlbumsAsync()
        => await context.Albums.AsNoTracking().Where(album => album.Published).Select(ToDto).ToListAsync();

    
    public async Task<IAlbumRepository.AlbumDto?> SetLayoutPresetAsync(int id, PageLayoutPreset layout)
    {
        var album = await context.Albums.FindAsync(id);
        if (album == null) return null;

        album.LayoutPreset = layout;
        await context.SaveChangesAsync();
        return AlbumToDto(album);
    }

    
    public async Task<(IAlbumRepository.UpdateNavOrderOutcome, IEnumerable<IAlbumRepository.AlbumDto>)> UpdateNavOrderAsync(int id, int newNavOrder)
    {
        await using var transaction = await BeginNavbarTransactionAsync();
        var albums = await GetNavbarAlbumsOrderedAsync();
        if (newNavOrder is < -1 or > 4) return (IAlbumRepository.UpdateNavOrderOutcome.OutOfBounds, albums.Select(AlbumToDto).ToList());
        var album = await context.Albums.FindAsync(id);
        if (album == null) return (IAlbumRepository.UpdateNavOrderOutcome.NotFound, albums.Select(AlbumToDto).ToList());
        if (newNavOrder >= 0 && !album.Published) return (IAlbumRepository.UpdateNavOrderOutcome.NotPublished, albums.Select(AlbumToDto).ToList());


        albums.Remove(album);
        if (newNavOrder >= 0)
        {
            if (albums.Count >= 5) return (IAlbumRepository.UpdateNavOrderOutcome.NavbarFull, albums.Select(AlbumToDto).ToList());
            albums.Insert(Math.Min(newNavOrder, albums.Count), album);
        }

        await SaveNavbarOrderAsync(albums);
        await transaction.CommitAsync();
        return (IAlbumRepository.UpdateNavOrderOutcome.Success, albums.Select(AlbumToDto).ToList());
    }

    
    public async Task<(IAlbumRepository.SwapInNavOutcome, IEnumerable<IAlbumRepository.AlbumDto>)> SwapAlbumsInNavAsync(int albumId1, int albumId2)
    {
        await using var transaction = await BeginNavbarTransactionAsync();
        var albums = await GetNavbarAlbumsOrderedAsync();
        if (albumId1 == albumId2) return (IAlbumRepository.SwapInNavOutcome.IgnoredSwapInPlace, albums.Select(AlbumToDto).ToList());
        var index1 = albums.FindIndex(album => album.Id == albumId1);
        var index2 = albums.FindIndex(album => album.Id == albumId2);
        if (index1 < 0 || index2 < 0) return (IAlbumRepository.SwapInNavOutcome.NotFoundInNav, albums.Select(AlbumToDto).ToList());

        (albums[index1], albums[index2]) = (albums[index2], albums[index1]);
        await SaveNavbarOrderAsync(albums);
        await transaction.CommitAsync();
        return (IAlbumRepository.SwapInNavOutcome.Success, albums.Select(AlbumToDto).ToList());
    }

    
    public async Task<IEnumerable<IAlbumRepository.AlbumDto>> GetPublishedNavExcludedAlbumsAsync()
        => await context.Albums.AsNoTracking().Where(album => album.Published && album.NavbarOrder == -1)
            .Select(ToDto).ToListAsync();

    
    public async Task<IEnumerable<IAlbumRepository.AlbumDto>> GetNavIncludedAlbumsAsync()
        => await context.Albums.AsNoTracking().Where(album => album.Published && album.NavbarOrder >= 0)
            .OrderBy(album => album.NavbarOrder).Select(ToDto).ToListAsync();

    
    public async Task<(IAlbumRepository.PublishAlbumOutcome, IAlbumRepository.AlbumDto?)> PublishAlbumAsync(int id, int? navOrder)
    {
        await using var transaction = await BeginNavbarTransactionAsync();
        var album = await context.Albums.FindAsync(id);
        if (album == null) return (IAlbumRepository.PublishAlbumOutcome.NotFound, null);
        if (album.Published)
        {
            return (IAlbumRepository.PublishAlbumOutcome.AlbumAlreadyPublished, AlbumToDto(album));
        }

        var albums = await GetNavbarAlbumsOrderedAsync();
        if (navOrder is >= 0 and <= 4 && albums.Count < 5)
        {
            albums.Insert(Math.Min(navOrder.Value, albums.Count), album);
        }

        // Existing albums receive an empty NavTitle when the new column is added.
        if (string.IsNullOrWhiteSpace(album.NavTitle))
        {
            var name = album.Name?.Trim() ?? "";
            album.NavTitle = name.Length > 20 ? name[..20] : name;
        }

        // Publish before assigning a navbar position to satisfy the database constraint.
        album.Published = true;
        await SaveNavbarOrderAsync(albums);
        await transaction.CommitAsync();
        var successOutcome = album.NavbarOrder == -1
            ? IAlbumRepository.PublishAlbumOutcome.NavExcludedSuccess
            : IAlbumRepository.PublishAlbumOutcome.NavIncludedSuccess;
        return (successOutcome, AlbumToDto(album));
    }

    
    public async Task<IAlbumRepository.AlbumDto?> UnpublishAlbumAsync(int id)
    {
        await using var transaction = await BeginNavbarTransactionAsync();
        var album = await context.Albums.FindAsync(id);
        if (album == null) return null;
        if (!album.Published)
        {
            await transaction.CommitAsync();
            return AlbumToDto(album);
        }

        if (album.NavbarOrder > -1)
        {
            var albums = await GetNavbarAlbumsOrderedAsync();
            albums.Remove(album);
            album.Published = false;
            await SaveNavbarOrderAsync(albums);
            await transaction.CommitAsync();
            return AlbumToDto(album);
        }
        album.Published = false;
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        return AlbumToDto(album);
    }
    
    private Task<List<Album>> GetNavbarAlbumsOrderedAsync()
        => context.Albums.Where(album => album.NavbarOrder >= 0)
            .OrderBy(album => album.NavbarOrder).ToListAsync();

    
    private async Task<IDbContextTransaction> BeginNavbarTransactionAsync()
    {
        // Each public operation owns its transaction; EF rejects an existing one (throws exception)
        var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // Share one lock across navbar writers, even when the navbar is empty.
            await context.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({NavbarLockKey})");
            return transaction;
        }
        catch
        {
            await transaction.DisposeAsync();
            throw;
        }
    }

    private async Task SaveNavbarOrderAsync(List<Album> orderedAlbums)
    {
        // The caller holds the navbar lock and transaction for both saves.
        var currentNavbar = await GetNavbarAlbumsOrderedAsync();
        foreach (var album in currentNavbar)
        {
            album.NavbarOrder = -1;
        }
        await context.SaveChangesAsync();

        for (var index = 0; index < orderedAlbums.Count; index++)
        {
            orderedAlbums[index].NavbarOrder = index;
        }
        await context.SaveChangesAsync();
    }
    
    
    private static readonly Expression<Func<Album, IAlbumRepository.AlbumDto>> ToDto = album => 
        new IAlbumRepository.AlbumDto(album.Id, album.Name, album.Description,
            album.NavTitle, album.Published, album.LayoutPreset, album.NavbarOrder);

    private static IAlbumRepository.AlbumDto AlbumToDto(Album album)
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
