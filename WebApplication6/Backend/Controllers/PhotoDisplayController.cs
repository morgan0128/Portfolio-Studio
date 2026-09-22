using Microsoft.AspNetCore.Mvc;
using WebApplication6.Backend.Repositories;
using WebApplication6.Backend.Services;

namespace WebApplication6.Backend.Controllers;

[ApiController]
[Route("api/album/{albumId:int}/photo-display")]
public sealed class PhotoDisplayController(IAlbumRepository albumRepository, IAlbumItemRepository albumItemRepository,
    IUploadPhotoService uploadPhotoService) : ControllerBase
{
    /* POST */
    [HttpPost]
    public async Task<IActionResult> UploadImagePostPhotoPostPhotoDisplay(int albumId, [FromForm] CombinedPhotoSpecDto combinedPhotoSpec)
    {
        var file = combinedPhotoSpec.File;

        var photoSpec = new PhotoSpecDto(combinedPhotoSpec.Name, combinedPhotoSpec.Description,
            combinedPhotoSpec.YearContentCreated);

        if (file.Length == 0)
        {
            return BadRequest("File upload fail");
        }

        var album = await albumRepository.GetAlbumByIdAsync(albumId);
        if (album == null)
        {
            return new ForbidResult();
        }

        var photoResult = await uploadPhotoService.UploadPhoto(file, photoSpec);
        if (photoResult == null) return Problem();

        var photoToAlbum = await albumItemRepository.AddPhotoToAlbumAsync(album.Id, photoResult.Value);
        if (!photoToAlbum) return Problem();

        return Ok();
    }
    
    
    /* GET */
    [HttpGet]
    public async Task<IEnumerable<IAlbumItemRepository.PhotoDisplayDto>> Get(int albumId)
    {
        var photos = await albumItemRepository.GetAlbumPhotoDisplays(albumId);
        return photos;
    }
    
    
    /* PUT, PATCH */
    [HttpPatch("{photoId:int}/fields-displayed")]
    public async Task<IActionResult> ModifyFieldsDisplayed(int albumId, int photoId, 
        IAlbumItemRepository.PhotoDisplayFieldsDisplayedRequest request)
    {
        var result = await albumItemRepository.ModifyFieldsDisplayed(albumId, photoId, request);
        return result switch
        {
            true => Ok(),
            false => NotFound()
        };
    }
    
    
    // [HttpPatch("{photoId:int}/DisplaysName")]
    // public async Task<IActionResult> ToggleDisplaysName(int albumId, int photoId)
    // {
    //     var request = await albumItemRepository.ToggleDisplaysName(albumId, photoId);
    //     return request switch
    //     {
    //         true => Ok(),
    //         false => Problem()
    //     };
    // }
    //
    // [HttpPatch("{photoId:int}/DisplaysDescription")]
    // public async Task<IActionResult> ToggleDisplaysDescription(int albumId, int photoId)
    // {
    //     var request = await albumItemRepository.ToggleDisplaysDescription(albumId, photoId);
    //     return request switch
    //     {
    //         true => Ok(),
    //         false => Problem()
    //     };
    // }
    //
    // [HttpPatch("{photoId:int}/displaysYearCC")]
    // public async Task<IActionResult> ToggleDisplaysYearContentCreated(int albumId, int photoId)
    // {
    //     var request = await albumItemRepository.ToggleDisplaysYearContentCreated(albumId, photoId);
    //     return request switch
    //     {
    //         true => Ok(),
    //         false => Problem()
    //     };
    // }

    
}