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
    
    Task<AlbumDto> SaveAlbumAsync(Album album);
    
    /// <returns>true on success, or false on not found</returns>
    Task<bool> DeleteAlbumByIdAsync(int id);

    Task<IEnumerable<AlbumDto>> GetAllPublishedAsync();
    
    Task<AlbumDto?> SetLayoutPresetAsync(int albumId, PageLayoutPreset layout);
    
    Task<(AssignNavOrderOutcome, IEnumerable<AlbumDto>?)> AssignAlbumInNavAsync(int albumId, int newNavOrder);
    
    Task<(SwapInNavOutcome, IEnumerable<IAlbumRepository.AlbumDto>?)> SwapAlbumsInNavOrderAsync(int albumId1, int albumId2);
    
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

    public enum AssignNavOrderOutcome
    {
        Success,
        OutOfBounds,
        NotPublished,
        NotFound,
        NavbarFull
    }


    public enum SwapInNavOutcome
    {
        Success,
        IgnoredSwapInPlace,
        NotFoundInNav
    }
}