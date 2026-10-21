using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication6.Backend.Data;
using WebApplication6.Backend.Models;

namespace WebApplication6.Backend.Repositories;

public class PhotoRepository(ApplicationDbContext context) : IPhotoRepository
{
    public async Task<IEnumerable<IPhotoRepository.PhotoDto>> GetAllPhotosAsync()
    {
        var photos = await context.Photos
            .Include(p => p.Image)
            .ToListAsync();

        return photos.ConvertAll(IPhotoRepository.ToPhotoDto);
    }

    public async Task<IEnumerable<int>> GetAllPhotosIdsAsync()
    {
        var photoIds = await context.Photos
            .Select(p => p.Id)
            .ToListAsync();

        return photoIds;
    }

    public async Task<IPhotoRepository.PhotoDto?> GetPhotoByIdAsync(int id)
    {
        var photo = await context.Photos
            .FindAsync(id);

        return (photo != null) ? IPhotoRepository.ToPhotoDto(photo) : null;
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

    public async Task<IPhotoRepository.PhotoDto?> PatchPhotoFieldsAsync(int id, IPhotoRepository.PhotoEditFieldsRequestDto request)
    {
        var photo = await context.Photos
            .Include(p => p.Image)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (photo == null) return null;

        if (request.Name is not null) photo.Name = request.Name;
        if (request.Description is not null) photo.Description = request.Description;
        if (request.YearContentCreated is not null) photo.YearContentCreated = request.YearContentCreated;

        await context.SaveChangesAsync();
        return IPhotoRepository.ToPhotoDto(photo);
    }

    public async Task<bool> DeletePhotoByIdAsync(int id)
    {
        var photo = await context.Photos
            .FirstOrDefaultAsync(p => p.Id == id);
        if (photo == null) return false;

        var photoDisplayCollections = await context.PhotoDisplayCollections
            .Where(apdi => apdi.PhotoDisplays.Any(ap => ap.PhotoId == id)
                         && apdi.PhotoDisplays.All(ap => ap.PhotoId == id))
            .ToListAsync();
        context.PhotoDisplayCollections.RemoveRange(photoDisplayCollections);

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