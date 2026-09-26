using Microsoft.AspNetCore.Mvc;
using WebApplication6.Backend.Repositories;

namespace WebApplication6.Backend.Controllers;


[ApiController]
[Route("api/albums/{albumId:int}/photo-display-collection-items")]
public class PhotoDisplayCollectionController(IAlbumItemRepository albumItemRepository) : ControllerBase
{
    /* POST */
    [HttpPost]
    public async Task<ActionResult<IAlbumItemRepository.AlbumItemDto>> Post(int albumId, CreatePhotoDisplayCollectionRequest request)
    {
        var collection = await albumItemRepository.CreateCarouselPhotoDisplayCollection(albumId, request.PhotoDisplayIds);
        if (collection is null)
        {
            return NotFound();
        }

        IAlbumItemRepository.AlbumItemDto albumItem = collection;
        return albumItem;
    }

    
    /* GET */
    
    
    /* PUT, PATCH */
    
    
    /* DELETE */






    public sealed record CreatePhotoDisplayCollectionRequest(IReadOnlyList<int> PhotoDisplayIds);
}
