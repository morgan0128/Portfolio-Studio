using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using WebApplication6.Backend.Models;
using WebApplication6.Backend.Repositories;

namespace WebApplication6.Backend.Controllers;

[ApiController]
[Route("api/Portfolio")]
public sealed class PortfolioPageController(IPortfolioPageRepository portfolioRepository) : ControllerBase
{

    [HttpGet("published")]
    public async Task<IEnumerable<IPortfolioPageRepository.PortfolioPageDto>> GetAllPublishedPortfolioPages()
    {
        var pps = await portfolioRepository.GetAllPublishedAsync();
        return pps;
    }

    [HttpGet("{id:int}")]
    public async Task<IPortfolioPageRepository.PortfolioPageDto?> GetPortfolioPageById(int id)
    {
        var pp = await portfolioRepository.GetPortfolioPageByIdAsync(id);
        return pp;
    }
    
    // [HttpGet("by-album/{albumId:int}")]
    // public async Task<IPortfolioPageRepository.PortfolioPageDto?> GetPortfolioPageByAlbumId(int albumId)
    // {
    //     var pp = await portfolioRepository.GetPortfolioPageByAlbumAsync(albumId);
    //     return pp;
    // }
    
    private async Task<IPortfolioPageRepository.PortfolioPageDto?> PostPortfolioPage(FetchOrCreateUsingAlbumDto albumRequest)
    {
        var pp = new PortfolioPage();
        pp.AlbumId = albumRequest.albumId;
        
        pp.Title = albumRequest.Name.Trim();
        if (pp.Title.Length > 80)
        {
            pp.Title = pp.Title[..80];
        }

        pp.NavTitle = pp.Title;
        if (pp.NavTitle.Length > 20)
        {
            pp.NavTitle = pp.NavTitle[..20];
        }

        
        var saved = await portfolioRepository.SavePortfolioPageAsync(pp);
        return saved;
    }

    [HttpPost("fetch-or-create")]
    public async Task<ActionResult<IPortfolioPageRepository.PortfolioPageDto?>> FetchOrCreatePortfolioPage(FetchOrCreateUsingAlbumDto correspondingAlbum)
    {
        try
        {
            var pp = await portfolioRepository.GetPortfolioPageByAlbumAsync(correspondingAlbum.albumId);
            if (pp != null)
            {
                return pp;
            }

            var created = await PostPortfolioPage(correspondingAlbum);
            return created;

        }
        catch (AggregateException e)
        {
            var innerExceptions = e.InnerExceptions;
            using (var exEnumerator = innerExceptions.GetEnumerator())
            {
                while (exEnumerator.MoveNext())
                {
                    if (exEnumerator.Current.GetType() != typeof(KeyNotFoundException)) continue;
                    
                    var keyNotFound = (KeyNotFoundException)exEnumerator.Current;
                    return new BadRequestResult();
                }
                throw;
            }
        }
    }
        

    [HttpGet("preview")]
    public async Task<IActionResult> PreviewLayout(PageLayoutPreset layout)
    {
        // TODO not yet implemented
        return Ok();
    }

    [HttpPatch("{id:int}/modify/layout-preset")]
    public async Task<IActionResult> UpdateLayoutPreset(int id, [FromBody] UpdateLayoutPresetRequest request)
    {
        var applied = await portfolioRepository.SetPortfolioPageLayoutPresetAsync(id, request.LayoutPreset);
        return applied ? NoContent() : NotFound();
    }

    [HttpPatch("{id:int}/modify/nav-order")]
    public async Task<IActionResult> AssignNavOrder(int id, int navOrder)
    {
        var reordered = await portfolioRepository.ReorderPortfolioPageInNavAsync(id, navOrder);

        return reordered ? Ok() : Problem();
    }

    [HttpGet("published/not-in-nav")]
    public async Task<IEnumerable<IPortfolioPageRepository.PortfolioPageDto>> GetPublishedNotInNav()
    {
        var pages = await portfolioRepository.GetPublishedNotInNavbar();
        return pages;
    }
    
    [HttpGet("published/in-nav/ordered")]
    public async Task<IEnumerable<IPortfolioPageRepository.PortfolioPageDto>> GetPublishedAndInNavOrdered()
    {
        var pages = await portfolioRepository.GetPublishedInNavbarOrdered();
        return pages;
    }

    [HttpPatch("remove-from-nav/{id:int}")]
    public async Task<IActionResult> RemoveFromNav(int id)
    {
        var pp = await portfolioRepository.ReorderPortfolioPageInNavAsync(id, -1);
        return pp switch
        {
            false => Problem(),
            true => Ok()
        };
    }

    [HttpPatch("publish/{id:int}")]
    public async Task<int?> PublishPortfolioPage(int id, int? navOrder)
    {
        var assignedNavOrder = await portfolioRepository.PublishPortfolioPageAsync(id, navOrder);
        return assignedNavOrder;
    }

    [HttpPatch("unpublish/{id:int}")]
    public async Task<IActionResult> UnpublishPortfolioPage(int id)
    {
        var unpublished = await portfolioRepository.UnpublishPortfolioPageAsync(id);
        return unpublished switch
        {
            false => Problem(),
            true => Ok()
        };
    }

    [HttpPatch("{id:int}/modify")]
    public async Task<IActionResult> ModifyPortfolioPage(int id, IPortfolioPageRepository.UpdatePortfolioPageDto model)
    {
        var modified = await portfolioRepository.UpdatePortfolioPageAsync(id, model);
        return modified switch
        {
            false => Problem(),
            true => Ok()
        };
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePortfolioPage(int id)
    {
        var deleted = await portfolioRepository.DeletePortfolioPageAsync(id);
        return deleted switch
        {
            false => Problem(),
            true => Ok()
        };
    }

    [HttpGet("styling-enums")]
    public PageLayoutPreset[] GetPageLayoutPresets()
    {
        // var enumNames = Enum.GetNames<PageLayoutPreset>();
        // var enumValues = Enum.GetValuesAsUnderlyingType<PageLayoutPreset>().Cast<int>().ToList();
        // (string, int)[] enumTuples = [];
        //
        // var length = enumNames.Length;
        // if (enumValues.Count != length) return enumTuples;
        //
        // for (var i = 0; i < length; i++)
        // {
        //     enumTuples[i] = (enumNames[i], enumValues[i]);
        // }
        //
        // return enumTuples;
        return Enum.GetValues<PageLayoutPreset>();
    }
    

    public sealed record FetchOrCreateUsingAlbumDto(int albumId, string Name);
    
    public sealed record UpdateLayoutPresetRequest(PageLayoutPreset LayoutPreset);
}