using Microsoft.AspNetCore.Mvc;
using WebApplication6.Backend.Repositories;

namespace WebApplication6.Backend.Controllers;

[ApiController]
[Route("api/albums/{albumId:int}/items")]
public sealed class AlbumItemController(IAlbumItemRepository albumItemRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<IAlbumItemRepository.AlbumItemDto>>> Get(int albumId)
    {
        var items = await albumItemRepository.GetAlbumItems(albumId);
        return items.ToList();
    }
    
    [HttpPost("reorder")]
    public async Task<IActionResult> RootLevelReorder(int albumId, RootLevelReorderRequest request)
    {
        var reordering = await albumItemRepository.ReorderAlbumItem(albumId, request.AlbumItemId, request.OrderDestination);
        return reordering switch
        {
            IAlbumItemRepository.ReorderOutcome.NoAlbumItems => Conflict(),
            IAlbumItemRepository.ReorderOutcome.ItemNotFound => NotFound(),
            IAlbumItemRepository.ReorderOutcome.Success => Ok(),
            _ => Problem(statusCode: 500)
        };
    }
    
    
    /* request data type objects */
    public sealed record RootLevelReorderRequest(int AlbumItemId, int OrderDestination);

    public sealed record AlbumItemDto(int AlbumItemId, int Order);
}
