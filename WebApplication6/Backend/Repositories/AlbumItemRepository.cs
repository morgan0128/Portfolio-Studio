using Microsoft.EntityFrameworkCore;
using WebApplication6.Backend.Data;
using WebApplication6.Backend.Models;

namespace WebApplication6.Backend.Repositories;

public class AlbumItemRepository(ApplicationDbContext context) : IAlbumItemRepository
{
    public async Task<IEnumerable<IAlbumItemRepository.PhotoDisplayDto>> GetAlbumPhotosAsync(int id)
    {
        try
        {
            var album = await context.Albums
                .Where(a => a.Id == id)
                .SingleAsync();

            var photoDisplays = await context.PhotoDisplays
                .Include(pd => pd.Photo).ThenInclude(p => p.Image)
                .Include(pd => pd.PhotoDisplayCollection)
                .Where(pd => pd.AlbumId == album.Id)
                .OrderBy(pd => pd.PhotoDisplayCollection == null ? pd.Order : pd.PhotoDisplayCollection.Order)
                .ThenBy(pd => pd.Order)
                .ToListAsync();

            var photos = photoDisplays
                .Select(pd => new IAlbumItemRepository.PhotoDisplayDto(
                    pd.Photo.Id,
                    pd.Photo.Name,
                    pd.Photo.Description,
                    pd.Photo.YearContentCreated,
                    pd.Photo.Image,
                    pd.PhotoDisplayCollection?.Order ?? pd.Order,
                    pd.DisplaysName,
                    pd.DisplaysDescription,
                    pd.DisplaysYearContentCreated
                ))
                .ToList();

            return photos;
        }
        catch (Exception)
        {
            // return new List<IAlbumRepository.AlbumPhotoDto>();
            throw;
        }
    }
    
    public async Task<bool> AddPhotoToAlbumAsync(int albumId, int photoId, CancellationToken cancellationToken = default)
    {
        const int maximumAttempts = 3;
        for (var attempt = 1; attempt <= maximumAttempts; attempt++)
        {
            var albumItems = ItemsInScope(albumId);
            
            var nextOrder = -1;
            if (!albumItems.Any())
            {
                nextOrder = 0;
            }
            else
            {
                nextOrder = (await albumItems.MaxAsync(ai => ai.Order, cancellationToken)) + 1;
            }

            var photoDisplay = new PhotoDisplay
            {
                AlbumId = albumId,
                PhotoId = photoId,
                Order = nextOrder,
                DisplaysName = true,
                DisplaysDescription = true,
                DisplaysYearContentCreated = true
            };

            context.PhotoDisplays.Add(photoDisplay);

            try
            {
                await context.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateException exception)
            {
                // do not track photoDisplay that violated db constraint
                context.Entry(photoDisplay).State = EntityState.Detached;

                if (attempt == maximumAttempts)
                    throw;
            }
        }

        return false;
    }
    
    public async Task<bool> ReorderPhotoInAlbum(int albumId, int photoId, int newOrder)
    {
        var photoDisplay = await context.PhotoDisplays
            .SingleOrDefaultAsync(pd => pd.AlbumId == albumId && pd.PhotoId == photoId);
        if (photoDisplay == null) return false;

        return await ReorderItem(albumId, photoDisplay.Id, newOrder, photoDisplay.PhotoDisplayCollectionId);
    }
    
    public Task<bool> ReorderAlbumItem(int albumId, int itemId, int newOrder)
    {
        return ReorderItem(albumId, itemId, newOrder);
    }

    public Task<bool> ReorderPhotoDisplayInCollection(int albumId, int photoDisplayCollectionId, int photoDisplayId, int newOrder)
    {
        return ReorderItem(albumId, photoDisplayId, newOrder, photoDisplayCollectionId);
    }
    
    private async Task<bool> ReorderItem(int albumId, int itemId, int newOrder, int? photoDisplayCollectionId = null)
    {
        var albumItems = await ItemsInScope(albumId, photoDisplayCollectionId)
            .OrderBy(ai => ai.Order)
            .ToListAsync();

        if (albumItems.Count == 0) return false; // this should not be reached from frontend

        if (newOrder < 0)
        {
            // recognize that an operation occurred by normalizing the order, but violates constraint
            await NormalizeOrder(albumId, photoDisplayCollectionId);
            return true;
        }

        var toMove = albumItems.Find(ai => ai.Id == itemId);
        
        if (toMove == null) return false; // this should not be reached from frontend
        
        var ofOrder = albumItems.Find(ai => ai.Order == newOrder);
        
        if (ofOrder == null)
        {
            toMove.Order = newOrder;
            await context.SaveChangesAsync();
            await NormalizeOrder(albumId, photoDisplayCollectionId);
            return true;
        }
        
        if (ofOrder.Id == toMove.Id)
        {
            // recognize that an operation occurred by normalizing the order, but do nothing to grant
            await NormalizeOrder(albumId, photoDisplayCollectionId);
            return true;
        }
        
        var index = albumItems.IndexOf(ofOrder);
        if (toMove.Order < ofOrder.Order)
        {
            /* toMove.Order < ofOrder.Order; as such the user expects that this operation moves 'toMove' after 'ofOrder' */

            
            // normalize first: need to pack the Order of AlbumItems preceding ofOrder as tightly as possible (limited by 0)
            await NormalizeOrder(albumId, photoDisplayCollectionId);
            
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
        
        await NormalizeOrder(albumId, photoDisplayCollectionId);

        return true;
    }
        
    private async Task NormalizeOrder(int albumId, int? photoDisplayCollectionId)
    {
        var albumItems = await ItemsInScope(albumId, photoDisplayCollectionId)
            .OrderBy(ai => ai.Order)
            .ToListAsync();
    
        for (var i = 0; i < albumItems.Count; i++)
        {
            albumItems[i].Order = i;
        }
    
        await context.SaveChangesAsync();
    }
    
    private IQueryable<AlbumItem> ItemsInScope(int albumId, int? photoDisplayCollectionId = null)
    {
        if (photoDisplayCollectionId != null)
        {
            return context.PhotoDisplays
                .Where(pd => pd.AlbumId == albumId && pd.PhotoDisplayCollectionId == photoDisplayCollectionId)
                .Cast<AlbumItem>();
        }

        return context.AlbumItems
            .Where(ai => ai.AlbumId == albumId && (!(ai is PhotoDisplay) || ((PhotoDisplay)ai).PhotoDisplayCollectionId == null));
    }

    // TODO return type
    public async Task<bool> ModifyFieldsDisplayed(int albumId, int photoDisplayId,
        IAlbumItemRepository.PhotoDisplayFieldsDisplayedRequest request)
    {
        var photoDisplay = await context.PhotoDisplays
            .FindAsync(photoDisplayId);
        if (photoDisplay == null) return false;

        photoDisplay.DisplaysName = request.DisplaysName ?? photoDisplay.DisplaysName;
        photoDisplay.DisplaysDescription = request.DisplaysDescription ?? photoDisplay.DisplaysDescription;
        photoDisplay.DisplaysYearContentCreated = request.DisplaysYearContentCreated ?? photoDisplay.DisplaysYearContentCreated;
        await context.SaveChangesAsync();

        return true;
    }
    
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
    
    // public async Task<List<PhotoDisplay>> FetchPhotoDisplaysByIds(int albumId, List<int> photoDisplayIds)
    // {
    //     var photoDisplays = await context.PhotoDisplays
    //         .Where(p => p.AlbumId == albumId && photoDisplayIds.Contains(p.Id))
    //         .ToListAsync();
    //
    //     return photoDisplays;
    // }
    
    // public async Task<PhotoDisplayCollection> CreateCarouselPhotoDisplayCollection(int albumId, List<PhotoDisplay> photoDisplays)
    // {
    //     if (photoDisplays.Exists(pd => pd.AlbumId != albumId))
    //     {
    //         throw new Exception();
    //     }
    //     
    //     var displayCollection = new PhotoDisplayCollection();
    //     displayCollection.DisplayMode = PhotoDisplayCollection.PhotoDisplayMode.Carousel;
    //     displayCollection.PhotoDisplays = photoDisplays;
    //
    //     
    //     await context.SaveChangesAsync();
    //     return displayCollection;
    // }
    
    
}