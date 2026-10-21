using Microsoft.AspNetCore.Mvc;
using WebApplication6.Backend.Models;

namespace WebApplication6.Backend.Repositories;

public interface IPhotoRepository
{
    Task<IEnumerable<PhotoDto>> GetAllPhotosAsync();

    Task<IEnumerable<int>> GetAllPhotosIdsAsync();
    
    Task<PhotoDto?> GetPhotoByIdAsync(int id);

    /// <returns>The Id of the saved photo on success, or null on exception thrown or failure.</returns>
    Task<int?> SavePhotoAsync(Photo photo);

    Task<PhotoDto?> PatchPhotoFieldsAsync(int id, PhotoEditFieldsRequestDto request);
    
    /// <returns>true on success, or false</returns>
    Task<bool> DeletePhotoByIdAsync(int id);
    
    public static PhotoDto ToPhotoDto(Photo photo)
    {
        return new PhotoDto(
            photo.Id,
            new ImageDto(
                photo.Image.Id,
                photo.Image.FileName,
                photo.Image.ContentType,
                photo.Image.FileSize,
                photo.Image.StorageFileName,
                photo.Image.Url,
                photo.Image.AltText,
                photo.Image.Width,
                photo.Image.Height),
            photo.Name,
            photo.Description,
            photo.YearContentCreated);
    }
    
    public sealed record PhotoDto(
        int Id,
        ImageDto Image,
        string Name,
        string Description,
        int? YearContentCreated
    );

    public sealed record ImageDto(
        int Id,
        string FileName,
        string ContentType,
        long? FileSize,
        string StorageFileName,
        string Url,
        string AltText,
        int Width,
        int Height
    );

    public sealed record PhotoEditFieldsRequestDto(
        string? Name,
        string? Description,
        int? YearContentCreated
    );
}