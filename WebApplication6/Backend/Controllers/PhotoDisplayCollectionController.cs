using Microsoft.AspNetCore.Mvc;
using WebApplication6.Backend.Repositories;

namespace WebApplication6.Backend.Controllers;


[ApiController]
[Route("api/album/{id:int}/photo-display-collection")]
public class PhotoDisplayCollectionController(IAlbumItemRepository albumItemRepository) : ControllerBase
{
    /* POST */
    // [HttpPost]
    // public async Task<ActionResult<IAlbumItemRepository.PhotoDisplayCollectionDto?>> Post(int id, CreatePhotoDisplayCollectionRequest request)
    // {
    //     throw new NotImplementedException();
    // }

    
    /* GET */
    
    
    /* PUT, PATCH */
    
    
    /* DELETE */






    public sealed record CreatePhotoDisplayCollectionRequest(List<IAlbumItemRepository.PhotoDisplayDto> PhotoDisplays);
}