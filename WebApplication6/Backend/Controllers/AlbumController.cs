using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebApplication6.Backend.Models;
using WebApplication6.Backend.Repositories;
using WebApplication6.Backend.Services;

namespace WebApplication6.Backend.Controllers;

[ApiController]
[Route("api/album")]
public sealed class AlbumController(IAlbumRepository albumRepository, IUploadPhotoService uploadPhotoService)
    : ControllerBase
{
    /* POST */
    [HttpPost]
    public async Task<ActionResult<int>> PostAlbum(CreateAlbumItemRequest albumRequest)
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
            Name = name,
            Description = albumRequest.Description,
            NavTitle = name.Length > 20 ? name[..20] : name
        };

        var id = await albumRepository.SaveAlbumAsync(album);
        if (id is null)
        {
            return Problem();
        }

        return id;
    }
    
    
    /* GET */
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<Album>>> GetAllAlbums()
    {
        var albums = await albumRepository.GetAllAlbumsAsync();
        return Ok(albums);
    }


    [HttpGet("all-ids")]
    public async Task<ActionResult<IEnumerable<int>>> GetAllAlbumsIds()
    {
        var albumIds = await albumRepository.GetAllAlbumsIdsAsync();
        return Ok(albumIds);
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<Album>> GetAlbumById(int id)
    {
        var album = await albumRepository.GetAlbumByIdAsync(id);
        if (album == null)
        {
            return NotFound();
        }

        return Ok(album);
    }
    
    
    [HttpGet("published")]
    public Task<IEnumerable<IAlbumRepository.AlbumDto>> GetAllPublishedAlbums() => albumRepository.GetAllPublishedAsync();
    
    
    [HttpGet("published/not-in-nav")]
    public Task<IEnumerable<IAlbumRepository.AlbumDto>> GetPublishedNotInNav() => albumRepository.GetPublishedNotInNavbar();
    
    
    [HttpGet("published/in-nav/ordered")]
    public Task<IEnumerable<IAlbumRepository.AlbumDto>> GetPublishedAndInNavOrdered() => albumRepository.GetPublishedInNavbarOrdered();
    
    
    [HttpGet("styling-enums")]
    public PageLayoutPreset[] GetPageLayoutPresets() => Enum.GetValues<PageLayoutPreset>();
    
    
    /* PUT, PATCH */
    [HttpPatch("{albumId:int}/modify/layout-preset")]
    public async Task<IActionResult> UpdateLayoutPreset(int albumId, [FromBody] UpdateLayoutPresetRequest request)
    {
        var applied = await albumRepository.SetLayoutPresetAsync(albumId, request.LayoutPreset);
        return applied ? NoContent() : NotFound();
    }
    
    
    [HttpPatch("{albumId:int}/modify/nav-order")]
    public async Task<IActionResult> AssignNavOrder(int albumId, [FromBody] NavOrderRequest request)
    {
        var reordered = await albumRepository.AssignAlbumInNavAsync(albumId, request.NavOrder);
        return reordered ? Ok() : Problem();
    }
    
    
    [HttpPatch("remove-from-nav/{albumId:int}")]
    public async Task<IActionResult> RemoveFromNav(int albumId)
    {
        var removed = await albumRepository.AssignAlbumInNavAsync(albumId, -1);
        return removed ? Ok() : Problem();
    }
    
    
    [HttpPatch("publish/{albumId:int}")]
    public Task<int?> PublishAlbum(int albumId, int? navOrder) => albumRepository.PublishAlbumAsync(albumId, navOrder);

    
    [HttpPatch("unpublish/{albumId:int}")]
    public async Task<IActionResult> UnpublishAlbum(int albumId)
    {
        var unpublished = await albumRepository.UnpublishAlbumAsync(albumId);
        return unpublished ? Ok() : Problem();
    }

    
    [HttpPatch("{albumId:int}/modify")]
    public async Task<IActionResult> ModifyAlbumPresentation(
        int albumId, IAlbumRepository.UpdateAlbumPresentationDto model)
    {
        var modified = await albumRepository.UpdateAlbumPresentationAsync(albumId, model);
        return modified ? Ok() : Problem();
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

    public sealed record CreateAlbumItemRequest(string? Name, string? Description);
    public sealed record UpdateLayoutPresetRequest(PageLayoutPreset LayoutPreset);
    public sealed record NavOrderRequest(int NavOrder);
    public sealed record NavOrderSwapRequest(int AlbumId1, int AlbumId2);

}