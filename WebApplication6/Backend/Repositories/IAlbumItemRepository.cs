using WebApplication6.Backend.Models;

namespace WebApplication6.Backend.Repositories;

public interface IAlbumItemRepository
{
    // Task<List<PhotoDisplay>> FetchPhotoDisplaysByIds(int albumId, List<int> photoDisplayIds);
    
    // Task<PhotoDisplayCollection> CreateCarouselPhotoDisplayCollection(int albumId, List<PhotoDisplay> photoDisplays);
    
    Task<IEnumerable<PhotoDisplayDto>> GetAlbumPhotosAsync(int id);
    
    Task<bool> AddPhotoToAlbumAsync(int albumId, int photoId, CancellationToken cancellationToken = default);
    
    Task<bool> ReorderPhotoInAlbum(int albumId, int photoId, int newOrder);
    
    Task<bool> ReorderAlbumItem(int albumId, int itemId, int newOrder);

    Task<bool> ReorderPhotoDisplayInCollection(int albumId, int photoDisplayCollectionId, int photoDisplayId, int newOrder);
    
    Task<bool> ToggleDisplaysName(int albumId, int photoId);
    
    Task<bool> ToggleDisplaysDescription(int albumId, int photoId);
    
    Task<bool> ToggleDisplaysYearContentCreated(int albumId, int photoId);
    
    public sealed record PhotoDisplayDto(
        int Id,
        string? Name,
        string? Description,
        int? YearContentCreated,
        Image Image,
        int? Order,
        bool displaysName = true,
        bool displaysDescription = true,
        bool displaysYearCC = true
    );
    
}