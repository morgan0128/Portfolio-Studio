using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebApplication6.Backend.Models;
using WebApplication6.Backend.Repositories;


namespace WebApplication6.Backend.Controllers;

[ApiController]
[Route("api/albums")]
public sealed class AlbumController(IAlbumRepository albumRepository)
    : ControllerBase
{
    /* POST */
    [HttpPost]
    public async Task<ActionResult<IAlbumRepository.AlbumDto>> Post(CreateAlbumRequest albumRequest)
    { 
        var name = albumRequest.Name?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            var number = await albumRepository.GetAlbumsCountAsync();
            number++;
            name = "Unnamed Album #" + number;
        }

        var album = new Album
        {
            Name = name, Description = albumRequest.Description, NavTitle = name.Length > 20 ? name[..20] : name
        };

        var dto = await albumRepository.SaveAlbumAsync(album);
        return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
    }
    
    
    /* GET */
    [HttpGet]
    public async Task<ActionResult<IEnumerable<IAlbumRepository.AlbumDto>>> Get()
    {
        var albums = await albumRepository.GetAlbumsAsync();
        return Ok(albums);
    }


    [HttpGet("ids")]
    public async Task<ActionResult<IEnumerable<int>>> GetIds()
    {
        var albumIds = await albumRepository.GetAlbumIdsAsync();
        return Ok(albumIds);
    }


    [HttpGet("{id:int}")]
    public async Task<ActionResult<IAlbumRepository.AlbumDto>> Get(int id)
    {
        var album = await albumRepository.GetAlbumByIdAsync(id);
        return ((album != null) ? Ok(album) : NotFound());
    }
    
    [HttpGet("layout-presets")]
    public PageLayoutPreset[] GetPageLayoutPresets() => Enum.GetValues<PageLayoutPreset>();



    [HttpGet("published")]
    public async Task<ActionResult<IEnumerable<IAlbumRepository.AlbumDto>>> GetAllPublishedAlbums()
    {
        var published = await albumRepository.GetPublishedAlbumsAsync();
        return Ok(published);
    }


    [HttpGet("published/not-in-nav")]
    public async Task<ActionResult<IEnumerable<IAlbumRepository.AlbumDto>>> GetPublishedNotInNav()
    {
        var publishedExcludedInNav = await albumRepository.GetPublishedNavExcludedAlbumsAsync();
        return Ok(publishedExcludedInNav);
    }


    [HttpGet("nav-items-ordered")]
    public async Task<ActionResult<IEnumerable<IAlbumRepository.AlbumDto>>> GetNavAlbumsOrdered()
    {
        var navItemsOrdered = await albumRepository.GetNavIncludedAlbumsAsync();
        return Ok(navItemsOrdered);
    }
    
    
    
    
    /* PUT, PATCH */
    [HttpPatch("{id:int}/layout-preset")]
    public async Task<ActionResult<IAlbumRepository.AlbumDto>> UpdateLayoutPreset(int id, [FromBody] UpdateLayoutPresetRequest request)
    {
        if (!Enum.IsDefined(request.LayoutPreset)) return BadRequest();
        var applied = await albumRepository.SetLayoutPresetAsync(id, request.LayoutPreset);
        return (applied is not null) ? Ok(applied) : NotFound();
    }
    
    [HttpPatch("{id:int}/visibility")]
    public async Task<ActionResult<IAlbumRepository.AlbumDto>> ModifyAlbumVisibility(int id, AlbumVisibilityRequest request)
    {
        if (!request.Publish)
        {
            var unpublishResult = await albumRepository.UnpublishAlbumAsync(id);
            return (unpublishResult is not null) ? Ok(unpublishResult) : NotFound();
        }

        var publishResult = await albumRepository.PublishAlbumAsync(id, request.NavOrder);
        return publishResult.Item1 switch
        {
            IAlbumRepository.PublishAlbumOutcome.NavExcludedSuccess => Ok(publishResult.Item2),
            IAlbumRepository.PublishAlbumOutcome.NavIncludedSuccess => Ok(publishResult.Item2),
            IAlbumRepository.PublishAlbumOutcome.AlbumAlreadyPublished => Conflict(publishResult.Item2),
            IAlbumRepository.PublishAlbumOutcome.NotFound => NotFound(publishResult.Item2),
            _ => Problem(statusCode: 500)
        };

    }
    
    
    [HttpPatch("{id:int}/nav-order")]
    public async Task<ActionResult<IEnumerable<IAlbumRepository.AlbumDto>>> AssignNavOrder(int id, [FromBody] NavOrderRequest request)
    {
        var reordered = await albumRepository.UpdateNavOrderAsync(id, request.NavOrder);
        return reordered.Item1 switch
        {
            IAlbumRepository.UpdateNavOrderOutcome.Success => Ok(reordered.Item2),
            IAlbumRepository.UpdateNavOrderOutcome.OutOfBounds => BadRequest(reordered.Item2),
            IAlbumRepository.UpdateNavOrderOutcome.NotPublished => Conflict(reordered.Item2),
            IAlbumRepository.UpdateNavOrderOutcome.NotFound => NotFound(reordered.Item2),
            IAlbumRepository.UpdateNavOrderOutcome.NavbarFull => Conflict(reordered.Item2),
            _ => Problem(statusCode: 500)
        };
    }
    
    
    [HttpPatch("{id:int}/remove-from-nav")]
    public async Task<ActionResult<IEnumerable<IAlbumRepository.AlbumDto>>> RemoveFromNav(int id)
    {
        var removed = await albumRepository.UpdateNavOrderAsync(id, -1);
        return removed.Item1 switch
        {
            IAlbumRepository.UpdateNavOrderOutcome.Success => Ok(removed.Item2),
            IAlbumRepository.UpdateNavOrderOutcome.NotPublished => Conflict(removed.Item2),
            IAlbumRepository.UpdateNavOrderOutcome.NotFound => NotFound(removed.Item2),
            _ => Problem(statusCode: 500)
        };
    }
    
    
    [HttpPatch("nav-order/swap")]
    public async Task<IActionResult> SwapAlbumsInNav([FromBody] NavOrderSwapRequest request)
    {
        var swapped = await albumRepository.SwapAlbumsInNavAsync(request.AlbumId1, request.AlbumId2);
        return swapped.Item1 switch
        {
            IAlbumRepository.SwapInNavOutcome.Success => Ok(swapped.Item2),
            IAlbumRepository.SwapInNavOutcome.IgnoredSwapInPlace => BadRequest(),
            IAlbumRepository.SwapInNavOutcome.NotFoundInNav => BadRequest(),
            _ => Problem(statusCode: 500)
        };
    }
    
    
    /* DELETE */
    [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlbumById(int id)
        {
            var status = await albumRepository.DeleteAlbumByIdAsync(id);
            return status switch
            {
                true => NoContent(),
                false => NotFound()
            };
        }
        
        
    
    /* request data type objects */
    public sealed record CreateAlbumRequest([StringLength(150)] string? Name = null, [StringLength(400)] string? Description = null);
    public sealed record UpdateLayoutPresetRequest(PageLayoutPreset LayoutPreset);
    
    
    public sealed record AlbumVisibilityRequest(bool Publish, int? NavOrder);
    public sealed record NavOrderRequest(int NavOrder);
    public sealed record NavOrderSwapRequest(int AlbumId1, int AlbumId2);

}