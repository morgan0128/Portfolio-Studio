using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebApplication6.Backend.Models;
using WebApplication6.Backend.Repositories;
using WebApplication6.Backend.Services;

namespace WebApplication6.Backend.Controllers;

[ApiController]
[Route("api/Album")]
public sealed class AlbumController(IAlbumRepository albumRepository, IUploadPhotoService uploadPhotoService)
    : ControllerBase
{
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


    [HttpPost("{id:int}/upload")]
    public async Task<IActionResult> UploadPhotoToAlbum(int id, [FromForm] CombinedPhotoSpecDto combinedPhotoSpec)
    {
        var file = combinedPhotoSpec.File;

        var photoSpec = new PhotoSpecDto(combinedPhotoSpec.Name, combinedPhotoSpec.Description,
            combinedPhotoSpec.YearContentCreated);

        if (file.Length == 0)
        {
            return BadRequest("File upload fail");
        }

        var album = await albumRepository.GetAlbumByIdAsync(id);
        if (album == null)
        {
            return new ForbidResult();
        }

        var photoResult = await uploadPhotoService.UploadPhoto(album, file, photoSpec);
        if (photoResult == null) return Problem();

        var photoToAlbum = await albumRepository.AddPhotoToAlbumAsync(album.Id, photoResult.Value);
        if (!photoToAlbum) return Problem();

        return Ok();
    }

    [HttpGet("{id:int}/photos")]
    public async Task<IEnumerable<IAlbumRepository.AlbumPhotoDto>> GetAlbumPhotos(int id)
    {
        var photos = await albumRepository.GetAlbumPhotosAsync(id);
        return photos;
    }

    [HttpPut("{id:int}/{photoId:int}/reorder/{toDest:int}")]
    public async Task<IActionResult> ReorderPhoto(int id, int photoId, int toDest)
    {
        var reordering = await albumRepository.ReorderPhotoInAlbum(id, photoId, toDest);
        return reordering switch
        {
            true => Ok(),
            false => Problem()
        };
    }

    [HttpPatch("{id:int}/{photoId:int}/displaysName")]
    public async Task<IActionResult> ToggleDisplaysName(int id, int photoId)
    {
        var request = await albumRepository.ToggleDisplaysName(id, photoId);
        return request switch
        {
            true => Ok(),
            false => Problem()
        };
    }
    
    [HttpPatch("{id:int}/{photoId:int}/displaysDescription")]
    public async Task<IActionResult> ToggleDisplaysDescription(int id, int photoId)
    {
        var request = await albumRepository.ToggleDisplaysDescription(id, photoId);
        return request switch
        {
            true => Ok(),
            false => Problem()
        };
    }
    
    [HttpPatch("{id:int}/{photoId:int}/displaysYearCC")]
    public async Task<IActionResult> ToggleDisplaysYearContentCreated(int id, int photoId)
    {
        var request = await albumRepository.ToggleDisplaysYearContenCreated(id, photoId);
        return request switch
        {
            true => Ok(),
            false => Problem()
        };
    }


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

}