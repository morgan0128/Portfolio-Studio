using Microsoft.AspNetCore.Mvc;
using WebApplication6.Backend.Repositories;
using WebApplication6.Backend.Services;
namespace WebApplication6.Backend.Controllers;

[ApiController]
[Route("api/photos")]
public sealed class PhotoController(IPhotoRepository photoRepository, IUploadPhotoService uploadPhotoService) : ControllerBase
{
    /* POST */
    [HttpPost]
    public async Task<ActionResult<IPhotoRepository.PhotoDto>> UploadImagePostPhoto([FromForm] CombinedPhotoSpecDto combinedPhotoSpec)
    {
        var file = combinedPhotoSpec.File;

        var photoSpec = new PhotoSpecDto(combinedPhotoSpec.Name, combinedPhotoSpec.Description,
            combinedPhotoSpec.YearContentCreated);

        if (file.Length == 0)
        {
            return BadRequest("File upload fail");
        }
        
        var photoResult = await uploadPhotoService.UploadPhoto(file, photoSpec);
        if (photoResult == null) return Problem(statusCode: 500, title: "Internal Server Error", detail: "Photo failed to upload.");

        var photo = await photoRepository.GetPhotoByIdAsync(photoResult.Value);
        if (photo is null) return Problem(statusCode: 500, title: "Internal Server Error", detail: "Failed to retrieve photo.");

        return Ok(photo);
    }

    /* GET */
    [HttpGet]
    public async Task<ActionResult<IEnumerable<IPhotoRepository.PhotoDto>>> Get()
    {
        var photos = await photoRepository.GetAllPhotosAsync();
        return Ok(photos);
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<IPhotoRepository.PhotoDto>> Get(int id)
    {
        var photo = await photoRepository.GetPhotoByIdAsync(id);
        return (photo != null) ? Ok(photo) : NotFound();
    }


    /* DELETE */
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var success = await photoRepository.DeletePhotoByIdAsync(id);
        return success ? Ok() : Problem(statusCode: 500, title: "Internal Server Error", detail: "Failed to delete photo.");
    }

}