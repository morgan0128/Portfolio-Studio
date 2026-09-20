using Microsoft.AspNetCore.Mvc;
using WebApplication6.Backend.Repositories;

namespace WebApplication6.Backend.Controllers;

public class AlbumPhotoDisplayItemController(IAlbumItemRepository repository)
{
    public IAlbumItemRepository Repository { get; } = repository;

    // [HttpPost("{albumId: int}/create/photo-display-carousel")]
    // public async Task<IActionResult> MergeSingularPhotosIntoCarousel(int albumId, [FromBody] int[] albumPhotoIds)
    // {
    //     
    // }
}