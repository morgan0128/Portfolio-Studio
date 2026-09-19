using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication6.Backend.Data;
using WebApplication6.Backend.Models;

namespace WebApplication6.Backend.Repositories;

public class PhotoRepository(ApplicationDbContext context) : IPhotoRepository
{
    public async Task<IEnumerable<Photo>> GetAllPhotosAsync()
    {
        var photos = await context.Photos
            .ToListAsync();

        return photos;
    }

    public async Task<IEnumerable<int>> GetAllPhotosIdsAsync()
    {
        var photoIds = await context.Photos
            .Select(p => p.Id)
            .ToListAsync();

        return photoIds;
    }

    public async Task<Photo?> GetPhotoByIdAsync(int id)
    {
        var photo = await context.Photos
            .FindAsync(id);

        return photo;
    }

    public async Task<int?> SavePhotoAsync(Photo photo)
    {
        context.Photos.Add(photo);
        
        try
        {
            await context.SaveChangesAsync();
            return photo.Id;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> DeletePhotoByIdAsync(int id)
    {
        var photo = await context.Photos.FindAsync(id);
        if (photo == null) return false;

        var albumPhotoDisplayItems = await context.AlbumPhotoDisplayItems
            .Where(apdi => apdi.AlbumPhotos.Any(ap => ap.PhotoId == id)
                         && apdi.AlbumPhotos.All(ap => ap.PhotoId == id))
            .ToListAsync();
        context.AlbumPhotoDisplayItems.RemoveRange(albumPhotoDisplayItems);

        context.Photos.Remove(photo);

        try
        {
            await context.SaveChangesAsync();
            return true;
        }
        catch (AggregateException)
        {
            return false;
        }
    }
}