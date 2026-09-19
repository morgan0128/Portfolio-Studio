using System.Collections;
using Microsoft.AspNetCore.Mvc;
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

    // public async Task<bool> AddPhotoToAlbum(int albumId, int photoId)
    // {
    //     var album = await context.Albums.FindAsync(albumId);
    //     if (album == null)
    //     {
    //         return false;
    //     }
    //
    //     var photo = await context.Photos.FindAsync(photoId);
    //     if (photo == null)
    //     {
    //         return false;
    //     }
    //
    //     album.Photos.Add(photo);
    //     try
    //     {
    //         await context.SaveChangesAsync();
    //         return true;
    //     }
    //     catch (Exception)
    //     {
    //         return false;
    //     }
    //
    // }
    
    public async Task<bool> AddPhotoToAlbumAsync(int albumId, int photoId, CancellationToken cancellationToken = default)
    {
        const int maximumAttempts = 3;

        for (var attempt = 1; attempt <= maximumAttempts; attempt++)
        {
            var albumItems = context.AlbumItems.Where(ai => ai.AlbumId == albumId);
            var nextOrder = -1;
            if (!albumItems.Any())
            {
                nextOrder = 0;
            }
            else
            {
                nextOrder = (await albumItems.MaxAsync(ai => ai.Order, cancellationToken)) + 1;
            }


            var albumPhotoDisplayItem = new AlbumPhotoDisplayItem
            {
                AlbumId = albumId,
                Order = nextOrder
            };

            var albumPhoto = new AlbumPhoto
            {
                AlbumId = albumId,
                PhotoId = photoId,
                AlbumPhotoDisplayItem = albumPhotoDisplayItem,
                Order = 0,
                DisplaysName = true,
                DisplaysDescription = true,
                DisplaysYearContentCreated = true
            };

            context.AlbumPhotos.Add(albumPhoto);

            try
            {
                await context.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateException exception)
            {
                // do not track albumPhoto that violated db constraint
                context.Entry(albumPhoto).State = EntityState.Detached; 
                context.Entry(albumPhotoDisplayItem).State = EntityState.Detached;

                if (attempt == maximumAttempts)
                    throw;
            }
        }

        return false;
    }
    
    

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

    public async Task<IEnumerable<IAlbumRepository.AlbumPhotoDto>> GetAlbumPhotosAsync(int id)
    {
        try
        {
            var album = await context.Albums
                .Include(a => a.Photos).ThenInclude(p => p.Image)
                .Where(a => a.Id == id)
                .SingleAsync();

            if (album.Photos.Count == 0) return new List<IAlbumRepository.AlbumPhotoDto>();

            var albumPhotos = await context.AlbumPhotos
                .Include(ap => ap.AlbumPhotoDisplayItem)
                .Where(ap => ap.AlbumId == album.Id)
                .ToListAsync();

            if (albumPhotos.Count == 0) throw new Exception("Issue in AlbumPhotos table.");

            var photos = album.Photos
                .Join(
                    albumPhotos,
                    (p => p.Id),
                    (ap => ap.PhotoId),
                    (photo, albumPhoto) => new IAlbumRepository.AlbumPhotoDto(
                        photo.Id,
                        photo.Name,
                        photo.Description,
                        photo.YearContentCreated,
                        photo.Image,
                        albumPhoto.AlbumPhotoDisplayItem.Order,
                        albumPhoto.DisplaysName,
                        albumPhoto.DisplaysDescription,
                        albumPhoto.DisplaysYearContentCreated
                    )
                )
                .OrderBy(photos => photos.Order)
                .ToList();
            
            return photos;
        }
        catch (Exception)
        {
            // return new List<IAlbumRepository.AlbumPhotoDto>();
            throw;
        }
    }

    public async Task<bool> ReorderPhotoInAlbum(int albumId, int photoId, int newOrder)
    {
        var albumItems = await context.AlbumItems
            .Include(ai => ((AlbumPhotoDisplayItem)ai).AlbumPhotos)
            .Where(ai => ai.AlbumId == albumId)
            .OrderBy(ai => ai.Order)
            .ToListAsync();

        if (albumItems.Count == 0) return false; // this should not be reached from frontend

        if (newOrder < 0)
        {
            // recognize that an operation occurred by normalizing the order, but violates constraint
            await NormalizeOrder(albumId);
            return true;
        }

        var toMove = albumItems.Find(ai => ai is AlbumPhotoDisplayItem item && item.AlbumPhotos.Any(ap => ap.PhotoId == photoId));
        
        if (toMove == null) return false; // this should not be reached from frontend
        
        var ofOrder = albumItems.Find(ai => ai.Order == newOrder);
        
        if (ofOrder == null)
        {
            toMove.Order = newOrder;
            await context.SaveChangesAsync();
            await NormalizeOrder(albumId);
            return true;
        }
        
        if (ofOrder.Id == toMove.Id)
        {
            // recognize that an operation occurred by normalizing the order, but do nothing to grant
            await NormalizeOrder(albumId);
            return true;
        }
        
        var index = albumItems.IndexOf(ofOrder);
        if (toMove.Order < ofOrder.Order)
        {
            /* toMove.Order < ofOrder.Order; as such the user expects that this operation moves 'toMove' after 'ofOrder' */

            
            // normalize first: need to pack the Order of AlbumItems preceding ofOrder as tightly as possible (limited by 0)
            await NormalizeOrder(albumId);
            
            // normalized, so no longer want to use newOrder
            var newOrderNormalized = albumItems[index].Order;

            var lowerBound = albumItems.IndexOf(toMove) + 1;
            var upperBound = index;
            toMove.Order = albumItems[^1].Order + 1; // temporary reassignment

            for (var i = lowerBound; i <= upperBound; i++)
            {
                var moveMeBackward = albumItems[i];
                moveMeBackward.Order = moveMeBackward.Order - 1;
            }
            await context.SaveChangesAsync(); // avoid circular dependency

            toMove.Order = newOrderNormalized;
            await context.SaveChangesAsync();

            return true; // order already normalized
        }

        /* toMove.Order > ofOrder.Order; as such the user expects that this operation moves 'toMove' before 'ofOrder' */
        while (index < albumItems.Count)
        {
            var moveMeForward = albumItems[index];
            moveMeForward.Order = moveMeForward.Order + 1;
            index++;
        }
        
        await context.SaveChangesAsync(); // avoid circular dependency

        toMove.Order = newOrder;
        await context.SaveChangesAsync();
        
        await NormalizeOrder(albumId);

        return true;
    }

    private async Task NormalizeOrder(int albumId)
    {
        var albumItems = await context.AlbumItems
            .Where(ai => ai.AlbumId == albumId)
            .OrderBy(ai => ai.Order)
            .ToListAsync();
    
        for (var i = 0; i < albumItems.Count; i++)
        {
            albumItems[i].Order = i;
        }
    
        await context.SaveChangesAsync();
    }

    public async Task<bool> ToggleDisplaysName(int albumId, int photoId)
    {
        var ap = await context.AlbumPhotos
            .FindAsync(albumId, photoId);

        if (ap == null) return false;
        
        ap.DisplaysName = !ap.DisplaysName;
        await context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> ToggleDisplaysDescription(int albumId, int photoId)
    {
        var ap = await context.AlbumPhotos
            .FindAsync(albumId, photoId);

        if (ap == null) return false;
        
        ap.DisplaysDescription = !ap.DisplaysDescription;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleDisplaysYearContenCreated(int albumId, int photoId)
    {
        var ap = await context.AlbumPhotos
            .FindAsync(albumId, photoId);

        if (ap == null) return false;
        
        ap.DisplaysYearContentCreated = !ap.DisplaysYearContentCreated;
        await context.SaveChangesAsync();
        return true;
    }
    

}
