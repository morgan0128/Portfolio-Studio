using WebApplication6.Backend.Models;

namespace WebApplication6.Backend.Repositories;

public interface IAlbumItemRepository
{
    // Task<bool> IsSingularAlbumPhotoDisplayItemContain
    
    Task<bool> CreateCarouselAlbumPhotoDisplayItem(List<AlbumPhoto> albumPhotos);
}