using Microsoft.AspNetCore.Mvc;
using WebApplication6.Backend.Models;
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
        if (request.DisplayMode != PhotoDisplayCollection.PhotoDisplayMode.Carousel) throw new NotImplementedException();

        if (!request.PhotoDisplayIds.Any()) return BadRequest();
        
        var collection = await albumItemRepository.CreateCarouselPhotoDisplayCollection(albumId, request.PhotoDisplayIds);
        if (collection is null)
        {
            return NotFound();
        }

        IAlbumItemRepository.AlbumItemDto albumItem = collection;
        return albumItem;
    }

    [HttpPost("dissolve/{photoDisplayCollectionId:int}")]
    public async Task<ActionResult<IEnumerable<IAlbumItemRepository.PhotoDisplayAlbumItemDto>>> Dissolve(int albumId, int photoDisplayCollectionId)
    {
        var orphanedItems = await albumItemRepository.DissolvePhotoDisplayCollection(albumId, photoDisplayCollectionId);
        
        if (orphanedItems is null) return NotFound();
        
        return Ok(orphanedItems);
    }

    
    /* GET */
    
    
    /* PUT, PATCH */
    [HttpPatch]
    public async Task Delete(int albumId, int itemId)
    {
        throw new NotImplementedException();
    }
    
    /* DELETE */






    public sealed record CreatePhotoDisplayCollectionRequest(IReadOnlyList<int> PhotoDisplayIds, PhotoDisplayCollection.PhotoDisplayMode DisplayMode = PhotoDisplayCollection.PhotoDisplayMode.Carousel);
}
