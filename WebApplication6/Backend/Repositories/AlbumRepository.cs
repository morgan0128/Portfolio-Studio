using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WebApplication6.Backend.Data;
using WebApplication6.Backend.Models;

namespace WebApplication6.Backend.Repositories;

public class AlbumRepository(ApplicationDbContext context) : IAlbumRepository
{
    public async Task<IEnumerable<Album>> GetAllAlbumsAsync()
    {
        var albums = await context.Albums
            .ToListAsync();
        
        return albums;
    }

    public async Task<IEnumerable<int>> GetAllAlbumsIdsAsync()
    {
        var albumIds = await context.Albums
            .AsNoTracking()
            .Select(a => a.Id)
            .ToListAsync();
        
        return albumIds;
    }

    public async Task<int> GetTotalNumberAlbums()
    {
        var amount = await context.Albums
            .CountAsync();

        return amount;
    }

    public async Task<Album?> GetAlbumByIdAsync(int id)
    {
        var album = await context.Albums
            .FindAsync(id);
        
        return album;
    }

    public async Task<int?> SaveAlbumAsync(Album album)
    {
        context.Albums.Add(album);

        try
        {
            await context.SaveChangesAsync();
            return album.Id;
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    // public async Task<bool> AddPhotoToAlbumAsync(int albumId, int photoId, CancellationToken cancellationToken = default)
    // {
    //     const int maximumAttempts = 3;
    //
    //     for (var attempt = 1; attempt <= maximumAttempts; attempt++)
    //     {
    //         var albumItems = ItemsInScope(albumId);
    //         
    //         var nextOrder = -1;
    //         if (!albumItems.Any())
    //         {
    //             nextOrder = 0;
    //         }
    //         else
    //         {
    //             nextOrder = (await albumItems.MaxAsync(ai => ai.Order, cancellationToken)) + 1;
    //         }
    //
    //
    //         var photoDisplay = new PhotoDisplay
    //         {
    //             AlbumId = albumId,
    //             PhotoId = photoId,
    //             Order = nextOrder,
    //             DisplaysName = true,
    //             DisplaysDescription = true,
    //             DisplaysYearContentCreated = true
    //         };
    //
    //         context.PhotoDisplays.Add(photoDisplay);
    //
    //         try
    //         {
    //             await context.SaveChangesAsync(cancellationToken);
    //             return true;
    //         }
    //         catch (DbUpdateException exception)
    //         {
    //             // do not track photoDisplay that violated db constraint
    //             context.Entry(photoDisplay).State = EntityState.Detached;
    //
    //             if (attempt == maximumAttempts)
    //                 throw;
    //         }
    //     }
    //
    //     return false;
    // }
    
    

    public async Task<bool> DeleteAlbumByIdAsync(int id)
    {
        var album = await context.Albums.FindAsync(id);
        if (album == null) return false;

        await using var transaction = context.Database.CurrentTransaction == null
            ? await context.Database.BeginTransactionAsync()
            : null;

        context.Albums.Remove(album);
        await context.SaveChangesAsync();

        // Navbar positions remain contiguous after deleting a displayed album.
        var navbarAlbums = await context.Albums
            .Where(a => a.NavbarOrder >= 0)
            .OrderBy(a => a.NavbarOrder)
            .ToListAsync();
        foreach (var navbarAlbum in navbarAlbums)
        {
            navbarAlbum.NavbarOrder = -1;
        }
        await context.SaveChangesAsync();
        for (var index = 0; index < navbarAlbums.Count; index++)
        {
            navbarAlbums[index].NavbarOrder = index;
        }
        await context.SaveChangesAsync();

        if (transaction != null) await transaction.CommitAsync();
        return true;
    }

    // public async Task<IEnumerable<IAlbumRepository.AlbumPhotoDto>> GetAlbumPhotosAsync(int id)
    // {
    //     try
    //     {
    //         var album = await context.Albums
    //             .Where(a => a.Id == id)
    //             .SingleAsync();
    //
    //         var photoDisplays = await context.PhotoDisplays
    //             .Include(pd => pd.Photo).ThenInclude(p => p.Image)
    //             .Include(pd => pd.PhotoDisplayCollection)
    //             .Where(pd => pd.AlbumId == album.Id)
    //             .OrderBy(pd => pd.PhotoDisplayCollection == null ? pd.Order : pd.PhotoDisplayCollection.Order)
    //             .ThenBy(pd => pd.Order)
    //             .ToListAsync();
    //
    //         var photos = photoDisplays
    //             .Select(pd => new IAlbumRepository.AlbumPhotoDto(
    //                 pd.Photo.Id,
    //                 pd.Photo.Name,
    //                 pd.Photo.Description,
    //                 pd.Photo.YearContentCreated,
    //                 pd.Photo.Image,
    //                 pd.PhotoDisplayCollection?.Order ?? pd.Order,
    //                 pd.DisplaysName,
    //                 pd.DisplaysDescription,
    //                 pd.DisplaysYearContentCreated
    //             ))
    //             .ToList();
    //
    //         return photos;
    //     }
    //     catch (Exception)
    //     {
    //         // return new List<IAlbumRepository.AlbumPhotoDto>();
    //         throw;
    //     }
    // }

    // public async Task<bool> ReorderPhotoInAlbum(int albumId, int photoId, int newOrder)
    // {
    //     var photoDisplay = await context.PhotoDisplays
    //         .SingleOrDefaultAsync(pd => pd.AlbumId == albumId && pd.PhotoId == photoId);
    //     if (photoDisplay == null) return false;
    //
    //     return await ReorderItem(albumId, photoDisplay.Id, newOrder, photoDisplay.PhotoDisplayCollectionId);
    // }

    // public Task<bool> ReorderAlbumItem(int albumId, int itemId, int newOrder)
    // {
    //     return ReorderItem(albumId, itemId, newOrder);
    // }
    //
    // public Task<bool> ReorderPhotoDisplayInCollection(int albumId, int photoDisplayCollectionId, int photoDisplayId, int newOrder)
    // {
    //     return ReorderItem(albumId, photoDisplayId, newOrder, photoDisplayCollectionId);
    // }

    // private IQueryable<AlbumItem> ItemsInScope(int albumId, int? photoDisplayCollectionId = null)
    // {
    //     if (photoDisplayCollectionId != null)
    //     {
    //         return context.PhotoDisplays
    //             .Where(pd => pd.AlbumId == albumId && pd.PhotoDisplayCollectionId == photoDisplayCollectionId)
    //             .Cast<AlbumItem>();
    //     }
    //
    //     return context.AlbumItems
    //         .Where(ai => ai.AlbumId == albumId && (!(ai is PhotoDisplay) || ((PhotoDisplay)ai).PhotoDisplayCollectionId == null));
    // }

    // private async Task<bool> ReorderItem(int albumId, int itemId, int newOrder, int? photoDisplayCollectionId = null)
    // {
    //     var albumItems = await ItemsInScope(albumId, photoDisplayCollectionId)
    //         .OrderBy(ai => ai.Order)
    //         .ToListAsync();
    //
    //     if (albumItems.Count == 0) return false; // this should not be reached from frontend
    //
    //     if (newOrder < 0)
    //     {
    //         // recognize that an operation occurred by normalizing the order, but violates constraint
    //         await NormalizeOrder(albumId, photoDisplayCollectionId);
    //         return true;
    //     }
    //
    //     var toMove = albumItems.Find(ai => ai.Id == itemId);
    //     
    //     if (toMove == null) return false; // this should not be reached from frontend
    //     
    //     var ofOrder = albumItems.Find(ai => ai.Order == newOrder);
    //     
    //     if (ofOrder == null)
    //     {
    //         toMove.Order = newOrder;
    //         await context.SaveChangesAsync();
    //         await NormalizeOrder(albumId, photoDisplayCollectionId);
    //         return true;
    //     }
    //     
    //     if (ofOrder.Id == toMove.Id)
    //     {
    //         // recognize that an operation occurred by normalizing the order, but do nothing to grant
    //         await NormalizeOrder(albumId, photoDisplayCollectionId);
    //         return true;
    //     }
    //     
    //     var index = albumItems.IndexOf(ofOrder);
    //     if (toMove.Order < ofOrder.Order)
    //     {
    //         /* toMove.Order < ofOrder.Order; as such the user expects that this operation moves 'toMove' after 'ofOrder' */
    //
    //         
    //         // normalize first: need to pack the Order of AlbumItems preceding ofOrder as tightly as possible (limited by 0)
    //         await NormalizeOrder(albumId, photoDisplayCollectionId);
    //         
    //         // normalized, so no longer want to use newOrder
    //         var newOrderNormalized = albumItems[index].Order;
    //
    //         var lowerBound = albumItems.IndexOf(toMove) + 1;
    //         var upperBound = index;
    //         toMove.Order = albumItems[^1].Order + 1; // temporary reassignment
    //
    //         for (var i = lowerBound; i <= upperBound; i++)
    //         {
    //             var moveMeBackward = albumItems[i];
    //             moveMeBackward.Order = moveMeBackward.Order - 1;
    //         }
    //         await context.SaveChangesAsync(); // avoid circular dependency
    //
    //         toMove.Order = newOrderNormalized;
    //         await context.SaveChangesAsync();
    //
    //         return true; // order already normalized
    //     }
    //
    //     /* toMove.Order > ofOrder.Order; as such the user expects that this operation moves 'toMove' before 'ofOrder' */
    //     while (index < albumItems.Count)
    //     {
    //         var moveMeForward = albumItems[index];
    //         moveMeForward.Order = moveMeForward.Order + 1;
    //         index++;
    //     }
    //     
    //     await context.SaveChangesAsync(); // avoid circular dependency
    //
    //     toMove.Order = newOrder;
    //     await context.SaveChangesAsync();
    //     
    //     await NormalizeOrder(albumId, photoDisplayCollectionId);
    //
    //     return true;
    // }

    // private async Task NormalizeOrder(int albumId, int? photoDisplayCollectionId)
    // {
    //     var albumItems = await ItemsInScope(albumId, photoDisplayCollectionId)
    //         .OrderBy(ai => ai.Order)
    //         .ToListAsync();
    //
    //     for (var i = 0; i < albumItems.Count; i++)
    //     {
    //         albumItems[i].Order = i;
    //     }
    //
    //     await context.SaveChangesAsync();
    // }

    // public async Task<bool> ToggleDisplaysName(int albumId, int photoId)
    // {
    //     var ap = await context.PhotoDisplays
    //         .SingleOrDefaultAsync(pd => pd.AlbumId == albumId && pd.PhotoId == photoId);
    //
    //     if (ap == null) return false;
    //     
    //     ap.DisplaysName = !ap.DisplaysName;
    //     await context.SaveChangesAsync();
    //     return true;
    // }
    //
    // public async Task<bool> ToggleDisplaysDescription(int albumId, int photoId)
    // {
    //     var ap = await context.PhotoDisplays
    //         .SingleOrDefaultAsync(pd => pd.AlbumId == albumId && pd.PhotoId == photoId);
    //
    //     if (ap == null) return false;
    //     
    //     ap.DisplaysDescription = !ap.DisplaysDescription;
    //     await context.SaveChangesAsync();
    //     return true;
    // }
    //
    // public async Task<bool> ToggleDisplaysYearContentCreated(int albumId, int photoId)
    // {
    //     var ap = await context.PhotoDisplays
    //         .SingleOrDefaultAsync(pd => pd.AlbumId == albumId && pd.PhotoId == photoId);
    //
    //     if (ap == null) return false;
    //     
    //     ap.DisplaysYearContentCreated = !ap.DisplaysYearContentCreated;
    //     await context.SaveChangesAsync();
    //     return true;
    // }
    
    private static readonly Expression<Func<Album, IAlbumRepository.AlbumDto>> ToDto =
        album => new IAlbumRepository.AlbumDto(
            album.Id, album.Name, album.Description, album.NavTitle,
            album.Published, album.NavbarOrder, album.LayoutPreset);

    public async Task<IEnumerable<IAlbumRepository.AlbumDto>> GetAllPublishedAsync()
        => await context.Albums.AsNoTracking().Where(album => album.Published).Select(ToDto).ToListAsync();

    public async Task<bool> SetLayoutPresetAsync(int albumId, PageLayoutPreset layout)
    {
        if (!Enum.IsDefined(layout)) return false;
        var album = await context.Albums.FindAsync(albumId);
        if (album == null) return false;

        album.LayoutPreset = layout;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignAlbumInNavAsync(int albumId, int newNavOrder)
    {
        if (newNavOrder is < -1 or > 4) return false;
        var album = await context.Albums.FindAsync(albumId);
        if (album == null || (newNavOrder >= 0 && !album.Published)) return false;

        var albums = await GetNavbarAlbumsAsync();
        albums.Remove(album);
        if (newNavOrder >= 0)
        {
            if (albums.Count >= 5) return false;
            albums.Insert(Math.Min(newNavOrder, albums.Count), album);
        }

        await SaveNavbarOrderAsync(albums);
        return true;
    }

    public async Task<bool> SwapAlbumsInNavOrderAsync(int albumId1, int albumId2)
    {
        if (albumId1 == albumId2) return false;
        var albums = await GetNavbarAlbumsAsync();
        var index1 = albums.FindIndex(album => album.Id == albumId1);
        var index2 = albums.FindIndex(album => album.Id == albumId2);
        if (index1 < 0 || index2 < 0) return false;

        (albums[index1], albums[index2]) = (albums[index2], albums[index1]);
        await SaveNavbarOrderAsync(albums);
        return true;
    }

    public async Task<IEnumerable<IAlbumRepository.AlbumDto>> GetPublishedNotInNavbar()
        => await context.Albums.AsNoTracking().Where(album => album.Published && album.NavbarOrder == -1)
            .Select(ToDto).ToListAsync();

    public async Task<IEnumerable<IAlbumRepository.AlbumDto>> GetPublishedInNavbarOrdered()
        => await context.Albums.AsNoTracking().Where(album => album.Published && album.NavbarOrder >= 0)
            .OrderBy(album => album.NavbarOrder).Select(ToDto).ToListAsync();

    public async Task<int?> PublishAlbumAsync(int albumId, int? navOrder)
    {
        var album = await context.Albums.FindAsync(albumId);
        if (album == null || album.Published) return null;

        var albums = await GetNavbarAlbumsAsync();
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
        return album.NavbarOrder;
    }

    public async Task<bool> UnpublishAlbumAsync(int albumId)
    {
        var album = await context.Albums.FindAsync(albumId);
        if (album is not { Published: true }) return false;

        var albums = await GetNavbarAlbumsAsync();
        albums.Remove(album);
        album.Published = false;
        await SaveNavbarOrderAsync(albums);
        return true;
    }

    public async Task<bool> UpdateAlbumPresentationAsync(
        int albumId, IAlbumRepository.UpdateAlbumPresentationDto model)
    {
        if (model.NavTitle is null || model.NavTitle.Length > 20) return false;
        var album = await context.Albums.FindAsync(albumId);
        if (album == null) return false;

        album.NavTitle = model.NavTitle;
        await context.SaveChangesAsync();
        return true;
    }

    private Task<List<Album>> GetNavbarAlbumsAsync()
        => context.Albums.Where(album => album.NavbarOrder >= 0)
            .OrderBy(album => album.NavbarOrder).ToListAsync();

    private async Task SaveNavbarOrderAsync(List<Album> orderedAlbums)
    {
        // Free occupied positions before assigning the new order. Keep both saves atomic.
        await using var transaction = context.Database.CurrentTransaction == null
            ? await context.Database.BeginTransactionAsync()
            : null;

        var currentNavbar = await GetNavbarAlbumsAsync();
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

        if (transaction != null) await transaction.CommitAsync();
    }
    

}
