using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using WebApplication6.Backend.Models;

namespace WebApplication6.Backend.Repositories;

public interface IAlbumRepository
{
    /// <include file="../Documentation/repositories.xml" path='Repositories/Album[@name="get-albums-async"]/*' />
    Task<IEnumerable<AlbumDto>> GetAlbumsAsync();

    /// <include file="../Documentation/repositories.xml" path='Repositories/Album[@name="get-album-ids-async"]/*' />
    Task<IEnumerable<int>> GetAlbumIdsAsync();

    /// <include file="../Documentation/repositories.xml" path='Repositories/Album[@name="get-albums-count-async"]/*' />
    Task<int> GetAlbumsCountAsync();
    
    /// <include file="../Documentation/repositories.xml" path='Repositories/Album[@name="get-album-by-id-async"]/*' />
    Task<AlbumDto?> GetAlbumByIdAsync(int id);
    
    /// <include file="../Documentation/repositories.xml" path='Repositories/Album[@name="save-album-async"]/*' />
    Task<AlbumDto> SaveAlbumAsync(Album album);
    
    /// <include file="../Documentation/repositories.xml" path='Repositories/Album[@name="delete-album-by-id-async"]/*' />
    Task<bool> DeleteAlbumByIdAsync(int id);

    /// <include file="../Documentation/repositories.xml" path='Repositories/Album[@name="get-published-albums-async"]/*' />
    Task<IEnumerable<AlbumDto>> GetPublishedAlbumsAsync();
    
    /// <include file="../Documentation/repositories.xml" path='Repositories/Album[@name="set-layout-preset-async"]/*' />
    Task<AlbumDto?> SetLayoutPresetAsync(int id, PageLayoutPreset layout);
    
    Task<(AssignNavOrderOutcome, IEnumerable<AlbumDto>?)> AssignAlbumInNavAsync(int id, int newNavOrder);
    
    Task<(SwapInNavOutcome, IEnumerable<AlbumDto>?)> SwapAlbumsInNavAsync(int albumId1, int albumId2);
    
    Task<IEnumerable<AlbumDto>> GetPublishedNavExcludedAlbumsAsync();
    
    /// <include file="../Documentation/repositories.xml" path='Repositories/Album[@name="get-nav-included-albums-async"]/*' />
    Task<IEnumerable<AlbumDto>> GetNavIncludedAlbumsAsync();
    
    Task<AlbumDto?> PublishAlbumAsync(int id, int? navOrder);
    
    Task<AlbumDto?> UnpublishAlbumAsync(int id);

    
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