using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using WebApplication6.Backend.Models;

namespace WebApplication6.Backend.Repositories;

public interface IAlbumRepository
{
    Task<IEnumerable<AlbumDto>> GetAllAlbumsAsync();

    Task<IEnumerable<int>> GetAllAlbumsIdsAsync();

    Task<int> GetTotalNumberAlbums();
    
    Task<AlbumDto?> GetAlbumByIdAsync(int id);
    
    /// <returns>Id of the saved album on success, or null on exception thrown or failure.</returns>
    Task<int?> SaveAlbumAsync(Album album);
    
    /// <returns>true on success, or false on not found</returns>
    Task<bool> DeleteAlbumByIdAsync(int id);

    Task<IEnumerable<AlbumDto>> GetAllPublishedAsync();
    
    Task<bool> SetLayoutPresetAsync(int albumId, PageLayoutPreset layout);
    
    Task<bool> AssignAlbumInNavAsync(int albumId, int newNavOrder);
    
    Task<bool> SwapAlbumsInNavOrderAsync(int albumId1, int albumId2);
    
    Task<IEnumerable<AlbumDto>> GetPublishedNotInNavbar();
    
    Task<IEnumerable<AlbumDto>> GetPublishedInNavbarOrdered();
    
    Task<AlbumDto?> PublishAlbumAsync(int albumId, int? navOrder);
    
    Task<AlbumDto?> UnpublishAlbumAsync(int albumId);

    
    public sealed record AlbumDto(
        int Id,
        string? Name,
        string? Description = null,
        string NavTitle = "",
        bool Published = false,
        PageLayoutPreset LayoutPreset = PageLayoutPreset.Default,
        int NavbarOrder = -1
        );
    
}