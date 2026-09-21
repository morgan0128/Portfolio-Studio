// using Microsoft.AspNetCore.Mvc;
// using WebApplication6.Backend.Models;
// using WebApplication6.Backend.Repositories;
//
// namespace WebApplication6.Backend.Controllers;
//
// [ApiController]
// [Route("api/Portfolio")]
// public sealed class PortfolioPageControllerOld(IPortfolioPageRepository portfolioRepository) : ControllerBase
// {
//     // [HttpGet("published")]
//     // public Task<IEnumerable<IPortfolioPageRepository.AlbumDto>> GetAllPublishedAlbums()
//     //     => portfolioRepository.GetAllPublishedAsync();
//
//     // [HttpGet("{albumId:int}")]
//     // public async Task<ActionResult<IPortfolioPageRepository.AlbumDto>> GetAlbum(int albumId)
//     // {
//     //     var album = await portfolioRepository.GetAlbumByIdAsync(albumId);
//     //     return album == null ? NotFound() : Ok(album);
//     // }
//
//     // [HttpPatch("{albumId:int}/modify/layout-preset")]
//     // public async Task<IActionResult> UpdateLayoutPreset(int albumId, [FromBody] UpdateLayoutPresetRequest request)
//     // {
//     //     var applied = await portfolioRepository.SetLayoutPresetAsync(albumId, request.LayoutPreset);
//     //     return applied ? NoContent() : NotFound();
//     // }
//
//     // [HttpPatch("{albumId:int}/modify/nav-order")]
//     // public async Task<IActionResult> AssignNavOrder(int albumId, [FromBody] NavOrderRequest request)
//     // {
//     //     var reordered = await portfolioRepository.AssignAlbumInNavAsync(albumId, request.NavOrder);
//     //     return reordered ? Ok() : Problem();
//     // }
//
//     // [HttpGet("published/not-in-nav")]
//     // public Task<IEnumerable<IPortfolioPageRepository.AlbumDto>> GetPublishedNotInNav()
//     //     => portfolioRepository.GetPublishedNotInNavbar();
//
//     // [HttpGet("published/in-nav/ordered")]
//     // public Task<IEnumerable<IPortfolioPageRepository.AlbumDto>> GetPublishedAndInNavOrdered()
//     //     => portfolioRepository.GetPublishedInNavbarOrdered();
//
//     // [HttpPatch("remove-from-nav/{albumId:int}")]
//     // public async Task<IActionResult> RemoveFromNav(int albumId)
//     // {
//     //     var removed = await portfolioRepository.AssignAlbumInNavAsync(albumId, -1);
//     //     return removed ? Ok() : Problem();
//     // }
//
//     // [HttpPatch("publish/{albumId:int}")]
//     // public Task<int?> PublishAlbum(int albumId, int? navOrder)
//     //     => portfolioRepository.PublishAlbumAsync(albumId, navOrder);
//     //
//     // [HttpPatch("unpublish/{albumId:int}")]
//     // public async Task<IActionResult> UnpublishAlbum(int albumId)
//     // {
//     //     var unpublished = await portfolioRepository.UnpublishAlbumAsync(albumId);
//     //     return unpublished ? Ok() : Problem();
//     // }
//     //
//     // [HttpPatch("{albumId:int}/modify")]
//     // public async Task<IActionResult> ModifyAlbumPresentation(
//     //     int albumId, IPortfolioPageRepository.UpdateAlbumPresentationDto model)
//     // {
//     //     var modified = await portfolioRepository.UpdateAlbumPresentationAsync(albumId, model);
//     //     return modified ? Ok() : Problem();
//     // }
//     //
//     // [HttpPatch("modify/nav-order/swap")]
//     // public async Task<IActionResult> SwapAlbumsInNav([FromBody] NavOrderSwapRequest request)
//     // {
//     //     var swapped = await portfolioRepository.SwapAlbumsInNavOrderAsync(request.AlbumId1, request.AlbumId2);
//     //     return swapped ? Ok() : Problem();
//     // }
//
//     // [HttpGet("styling-enums")]
//     // public PageLayoutPreset[] GetPageLayoutPresets() => Enum.GetValues<PageLayoutPreset>();
//
//     // public sealed record UpdateLayoutPresetRequest(PageLayoutPreset LayoutPreset);
//     // public sealed record NavOrderRequest(int NavOrder);
//     // public sealed record NavOrderSwapRequest(int AlbumId1, int AlbumId2);
// }
