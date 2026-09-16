using Microsoft.AspNetCore.Mvc;
using WebApplication6.Backend.Models;

namespace WebApplication6.Backend.Repositories;

public interface IPortfolioPageRepository
{
    Task<IEnumerable<PortfolioPageDto>> GetAllPublishedAsync();
    
    Task<PortfolioPageDto?> GetPortfolioPageByIdAsync(int id);
    
    Task<PortfolioPageDto?> GetPortfolioPageByAlbumAsync(int albumId);
    
    /// <returns>Id of the saved portfolio page on success, or null on exception thrown or failure.</returns>
    Task<PortfolioPageDto?> SavePortfolioPageAsync(PortfolioPage portfolioPage);

    Task<bool> SetPortfolioPageLayoutPresetAsync(int ppId, PageLayoutPreset layout);
    
    Task<bool> AssignPortfolioPageInNavAsync(int ppId, int newNavOrder);

    Task<bool> SwapPortfolioPagesInNavOrderAsync(int ppId1, int ppId2);

    Task<IEnumerable<PortfolioPageDto>> GetPublishedNotInNavbar();

    Task<IEnumerable<PortfolioPageDto>> GetPublishedInNavbarOrdered();

    /// <param name="ppId"></param>
    /// <param name="navOrder"></param>
    /// <returns>Returns NavbarOrder of PortfolioPage associated with ppId on published successfully, or null</returns>
    Task<int?> PublishPortfolioPageAsync(int ppId, int? navOrder);
    
    Task<bool> UnpublishPortfolioPageAsync(int ppId);

    Task<bool> UpdatePortfolioPageAsync(int ppId, UpdatePortfolioPageDto model);
    
    /// <returns>true on success, or false on not found</returns>
    Task<bool> DeletePortfolioPageAsync(int id);
    
    // Task<IEnumerable<int>> Get

    public sealed record UpdatePortfolioPageDto(string? NavTitle, string? Title);
    
    public sealed record PortfolioPageDto(
        int Id,
        string NavTitle,
        string Title,
        bool Published,
        int NavbarOrder,
        int AlbumId,
        PageLayoutPreset LayoutPreset);
    
}