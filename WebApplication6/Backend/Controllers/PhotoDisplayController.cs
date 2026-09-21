using Microsoft.AspNetCore.Mvc;
using WebApplication6.Backend.Repositories;
using WebApplication6.Backend.Services;

namespace WebApplication6.Backend.Controllers;

[ApiController]
[Route("api/{albumId:int}/photo-display")]
public sealed class PhotoDisplayController(IAlbumRepository albumRepository, IAlbumItemRepository albumItemRepository,
    IUploadPhotoService uploadPhotoService) : ControllerBase
{
    /* POST */
    [HttpPost]
    public async Task<IActionResult> Post(int albumId, [FromForm] CombinedPhotoSpecDto combinedPhotoSpec)
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

        var photoResult = await uploadPhotoService.UploadPhoto(album, file, photoSpec);
        if (photoResult == null) return Problem();

        var photoToAlbum = await albumItemRepository.AddPhotoToAlbumAsync(album.Id, photoResult.Value);
        if (!photoToAlbum) return Problem();

        return Ok();
    }
    
    
    /* GET */
    [HttpGet]
    public async Task<IEnumerable<IAlbumItemRepository.PhotoDisplayDto>> Get(int albumId)
    {
        var photos = await albumItemRepository.GetAlbumPhotosAsync(albumId);
        return photos;
    }
    
    
    /* PUT, PATCH */
    [HttpPatch("{photoId:int}/displaysName")]
    public async Task<IActionResult> ToggleDisplaysName(int id, int photoId)
    {
        var request = await albumItemRepository.ToggleDisplaysName(id, photoId);
        return request switch
        {
            true => Ok(),
            false => Problem()
        };
    }
    
    [HttpPatch("{photoId:int}/displaysDescription")]
    public async Task<IActionResult> ToggleDisplaysDescription(int id, int photoId)
    {
        var request = await albumItemRepository.ToggleDisplaysDescription(id, photoId);
        return request switch
        {
            true => Ok(),
            false => Problem()
        };
    }
    
    [HttpPatch("{photoId:int}/displaysYearCC")]
    public async Task<IActionResult> ToggleDisplaysYearContentCreated(int id, int photoId)
    {
        var request = await albumItemRepository.ToggleDisplaysYearContentCreated(id, photoId);
        return request switch
        {
            true => Ok(),
            false => Problem()
        };
    }
}