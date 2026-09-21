using Microsoft.AspNetCore.Mvc;
using WebApplication6.Backend.Models;
using WebApplication6.Backend.Repositories;


namespace WebApplication6.Backend.Controllers;

[ApiController]
[Route("api/album")]
public sealed class AlbumController(IAlbumRepository albumRepository)
    : ControllerBase
{
    /* POST */
    [HttpPost]
    public async Task<ActionResult<int>> Post(CreateAlbumRequest albumRequest)
    {
        var name = albumRequest.Name?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            var number = await albumRepository.GetTotalNumberAlbums();
            number++;
            name = "Unnamed Album #" + number;
        }

        var album = new Album
        {
            Name = name, Description = albumRequest.Description, NavTitle = name.Length > 20 ? name[..20] : name
        };

        var id = await albumRepository.SaveAlbumAsync(album);
        if (id is null) return Problem();

        return id;
    }
    
    
    /* GET */
    [HttpGet]
    public async Task<ActionResult<IEnumerable<IAlbumRepository.AlbumDto>>> Get()
    {
        var albums = await albumRepository.GetAllAlbumsAsync();
        return Ok(albums);
    }


    [HttpGet("ids")]
    public async Task<ActionResult<IEnumerable<int>>> GetIds()
    {
        var albumIds = await albumRepository.GetAllAlbumsIdsAsync();
        return Ok(albumIds);
    }


    [HttpGet("{id:int}")]
    public async Task<ActionResult<IAlbumRepository.AlbumDto>> Get(int id)
    {
        var album = await albumRepository.GetAlbumByIdAsync(id);
        return ((album != null) ? Ok(album) : NotFound());
    }
    
    [HttpGet("styling-enums")]
    public PageLayoutPreset[] GetPageLayoutPresets() => Enum.GetValues<PageLayoutPreset>();
    
    
    
    [HttpGet("published")]
    public Task<IEnumerable<IAlbumRepository.AlbumDto>> GetAllPublishedAlbums() => albumRepository.GetAllPublishedAsync();
    
    
    [HttpGet("published/not-in-nav")]
    public Task<IEnumerable<IAlbumRepository.AlbumDto>> GetPublishedNotInNav() => albumRepository.GetPublishedNotInNavbar();
    
    
    [HttpGet("nav-ordered")]
    public Task<IEnumerable<IAlbumRepository.AlbumDto>> GetNavAlbumsOrdered() => albumRepository.GetPublishedInNavbarOrdered();
    
    
    
    
    /* PUT, PATCH */
    [HttpPatch("{id:int}/modify/layout-preset")]
    public async Task<IActionResult> UpdateLayoutPreset(int id, [FromBody] UpdateLayoutPresetRequest request)
    {
        var applied = await albumRepository.SetLayoutPresetAsync(id, request.LayoutPreset);
        return applied ? NoContent() : NotFound();
    }
    
    [HttpPatch("{id:int}/visibility")]
    public async Task<ActionResult<IAlbumRepository.AlbumDto>> ModifyAlbumVisibility(int id, AlbumVisibilityRequest request)
    {
        var albumDto = request.Published ?
            await albumRepository.PublishAlbumAsync(id, request.NavOrder) :
            await albumRepository.UnpublishAlbumAsync(id);
        
        return (albumDto is not null) ? Ok(albumDto) : NotFound();
    }
    
    
    [HttpPatch("{id:int}/nav-order")]
    public async Task<IActionResult> AssignNavOrder(int id, [FromBody] NavOrderRequest request)
    {
        var reordered = await albumRepository.AssignAlbumInNavAsync(id, request.NavOrder);
        return reordered ? Ok() : Problem();
    }
    
    
    [HttpPatch("{id:int}/remove-from-nav")]
    public async Task<IActionResult> RemoveFromNav(int id)
    {
        var removed = await albumRepository.AssignAlbumInNavAsync(id, -1);
        return removed ? Ok() : Problem();
    }
    
    
    [HttpPatch("modify/nav-order/swap")]
    public async Task<IActionResult> SwapAlbumsInNav([FromBody] NavOrderSwapRequest request)
    {
        var swapped = await albumRepository.SwapAlbumsInNavOrderAsync(request.AlbumId1, request.AlbumId2);
        return swapped ? Ok() : Problem();
    }
    
    
    /* DELETE */
    [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlbumById(int id)
        {
            var status = await albumRepository.DeleteAlbumByIdAsync(id);
            return status switch
            {
                true => Ok(),
                false => Problem()
            };
        }
        
        
    
    /* request data type objects */
    public sealed record CreateAlbumRequest(string? Name, string? Description);
    public sealed record UpdateLayoutPresetRequest(PageLayoutPreset LayoutPreset);
    
    
    public sealed record AlbumVisibilityRequest(bool Published, int? NavOrder);
    public sealed record NavOrderRequest(int NavOrder);
    public sealed record NavOrderSwapRequest(int AlbumId1, int AlbumId2);

}