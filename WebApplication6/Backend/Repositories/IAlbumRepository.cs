using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using WebApplication6.Backend.Models;

namespace WebApplication6.Backend.Repositories;

public interface IAlbumRepository
{
    Task<IEnumerable<Album>> GetAllAlbumsAsync();

    Task<IEnumerable<int>> GetAllAlbumsIdsAsync();

    Task<int> GetTotalNumberAlbums();
    
    Task<Album?> GetAlbumByIdAsync(int id);
    
    /// <returns>Id of the saved album on success, or null on exception thrown or failure.</returns>
    Task<int?> SaveAlbumAsync(Album album);


    // /// <returns>true on success, or false.</returns>
    // Task<bool> AddPhotoToAlbum(int albumId, int photoId);



    // Task<bool> AddPhotoToAlbumAsync(int albumId, int photoId, CancellationToken cancellationToken = default);
    
    /// <returns>true on success, or false on not found</returns>
    Task<bool> DeleteAlbumByIdAsync(int id);

    // /// <summary>
    // /// Retrieves all photos for the queried Album
    // /// </summary>
    // /// <param name="id">The Id of the Album row to query.</param>
    // /// <returns>A (nullable) IEnumerable of AlbumPhotoDto with no guarantee that they have a correct or an explicit ordering</returns>
    // Task<IEnumerable<AlbumPhotoDto>> GetAlbumPhotosAsync(int id); // TODO: Make Task<IAsyncEnumerable....> instead, once have more time to look into.
    
    // /// <param name="albumId"></param>
    // /// <param name="photoId"></param>
    // /// <param name="newOrder"></param>
    // /// <returns>true if any Order value may have been modified (regardless of if the 'order' of photos left unchanged), or false</returns>
    // Task<bool> ReorderPhotoInAlbum(int albumId, int photoId, int newOrder);
    
    // Task<bool> ReorderAlbumItem(int albumId, int itemId, int newOrder);

    // Task<bool> ReorderPhotoDisplayInCollection(int albumId, int photoDisplayCollectionId, int photoDisplayId, int newOrder);

    // Task<bool> ToggleDisplaysName(int albumId, int photoId);
    //
    // Task<bool> ToggleDisplaysDescription(int albumId, int photoId);
    //
    // Task<bool> ToggleDisplaysYearContentCreated(int albumId, int photoId);

    Task<IEnumerable<AlbumDto>> GetAllPublishedAsync();
    
    Task<bool> SetLayoutPresetAsync(int albumId, PageLayoutPreset layout);
    
    Task<bool> AssignAlbumInNavAsync(int albumId, int newNavOrder);
    
    Task<bool> SwapAlbumsInNavOrderAsync(int albumId1, int albumId2);
    
    Task<IEnumerable<AlbumDto>> GetPublishedNotInNavbar();
    
    Task<IEnumerable<AlbumDto>> GetPublishedInNavbarOrdered();
    
    Task<int?> PublishAlbumAsync(int albumId, int? navOrder);
    
    Task<bool> UnpublishAlbumAsync(int albumId);
    
    Task<bool> UpdateAlbumPresentationAsync(int albumId, UpdateAlbumPresentationDto model);

    public sealed record UpdateAlbumPresentationDto([MaxLength(20)] string? NavTitle);

    public sealed record AlbumDto(
        int Id,
        string? Name,
        string? Description,
        string NavTitle,
        bool Published,
        int NavbarOrder,
        PageLayoutPreset LayoutPreset
    );
    
    // public sealed record AlbumPhotoDto(
    //     int Id,
    //     string? Name,
    //     string? Description,
    //     int? YearContentCreated,
    //     Image Image,
    //     int? Order,
    //     bool displaysName = true,
    //     bool displaysDescription = true,
    //     bool displaysYearCC = true
    // );
}