using WebApplication6.Backend.Data;
using WebApplication6.Backend.Models;

namespace WebApplication6.Backend.Repositories;

public class AlbumItemRepository(ApplicationDbContext context) : IAlbumItemRepository
{
    public async Task<bool> CreateCarouselAlbumPhotoDisplayItem(List<AlbumPhoto> albumPhotos)
    {
        var displayItem = new AlbumPhotoDisplayItem();
        displayItem.DisplayMode = AlbumPhotoDisplayItem.PhotoDisplayMode.Carousel;
        displayItem.AlbumPhotos = albumPhotos;

        await context.SaveChangesAsync();
        return true;
    }
}