using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebApplication6.Backend.Models;
using WebApplication6.Backend.Repositories;

namespace WebApplication6.Backend.Controllers;

[ApiController]
[Route("api/album-item")]
public sealed class AlbumItemController(IAlbumItemRepository albumItemRepository) : ControllerBase
{
    // [HttpPost("{albumId:int}/create/photo-display-carousel")]
    // public async Task<PhotoDisplayCollection?> CreatePhotoDisplayCollection(int albumId, [FromBody] int[] photoDisplayIds)
    // {
    //     var ids = photoDisplayIds.ToList();
    //     var photoDisplays = await repository.FetchPhotoDisplaysByIds(albumId, ids);
    //     if (ids.Count != photoDisplays.Count)
    //     {
    //         return null;
    //     }
    //
    //     var displayCollection = await repository.CreateCarouselPhotoDisplayCollection(albumId, photoDisplays);
    //     return displayCollection;
    // }
    
    [HttpPut("{itemId:int}/reorder/{toDest:int}")]
    public async Task<IActionResult> ReorderPhoto(int albumId, int photoId, int toDest)
    {
        var reordering = await albumItemRepository.ReorderPhotoInAlbum(albumId, photoId, toDest);
        return reordering switch
        {
            true => Ok(),
            false => Problem()
        };
    }
}