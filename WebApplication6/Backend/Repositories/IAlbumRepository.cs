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
    
    /// <include file="../Documentation/repositories.xml" path='Repositories/Album[@name="update-nav-order-async"]/*' />
    Task<(UpdateNavOrderOutcome, IEnumerable<AlbumDto>)> UpdateNavOrderAsync(int id, int newNavOrder);
    
    /// <include file="../Documentation/repositories.xml" path='Repositories/Album[@name="swap-albums-in-nav-async"]/*' />
    Task<(SwapInNavOutcome, IEnumerable<AlbumDto>?)> SwapAlbumsInNavAsync(int albumId1, int albumId2);
    
    /// <include file="../Documentation/repositories.xml" path='Repositories/Album[@name="get-published-nav-excluded-albums-async"]/*' />
    Task<IEnumerable<AlbumDto>> GetPublishedNavExcludedAlbumsAsync();
    
    /// <include file="../Documentation/repositories.xml" path='Repositories/Album[@name="get-nav-included-albums-async"]/*' />
    Task<IEnumerable<AlbumDto>> GetNavIncludedAlbumsAsync();
    
    /// <include file="../Documentation/repositories.xml" path='Repositories/Album[@name="publish-album-async"]/*' />
    Task<AlbumDto?> PublishAlbumAsync(int id, int? navOrder);
    
    /// <include file="../Documentation/repositories.xml" path='Repositories/Album[@name="unpublish-album-async"]/*' />
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

    public enum UpdateNavOrderOutcome
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