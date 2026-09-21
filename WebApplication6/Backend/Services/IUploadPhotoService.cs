using Microsoft.AspNetCore.Mvc;
using WebApplication6.Backend.Models;

namespace WebApplication6.Backend.Services;

public interface IUploadPhotoService
{
    /// <summary>
    /// Stores image file 'file' on file host, creates individual, associated db entries for new Image, Photo.
    /// WARN: Does not create AlbumPhoto (composite key) db row.
    /// </summary>
    /// <param name="album"></param>
    /// <param name="file"></param>
    /// <param name="photoSpec"></param>
    /// <returns>Id of newly created Photo on success, or null</returns>
    Task<int?> UploadPhoto(IFormFile file, PhotoSpecDto photoSpec);
}

// Specification for a photo based on user input
public record PhotoSpecDto(string? Name, string? Description, int? YearContentCreated);

public record CombinedPhotoSpecDto(IFormFile File, string? Name, string? Description, int? YearContentCreated);


