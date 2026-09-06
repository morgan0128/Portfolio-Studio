using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication6.Backend.Data;
using WebApplication6.Backend.Models;

namespace WebApplication6.Backend.Repositories;

public class PortfolioPageRepository(ApplicationDbContext context) : IPortfolioPageRepository
{
    public async Task<IEnumerable<IPortfolioPageRepository.PortfolioPageDto>> GetAllPublishedAsync()
    {
        var publishedPages = await context.PortfolioPages
            .Where(pp => pp.Published == true)
            .Select(pp => PortfolioPageToDto(pp))
            .ToListAsync();

        return publishedPages;
    }

    public async Task<IPortfolioPageRepository.PortfolioPageDto?> GetPortfolioPageByIdAsync(int id)
    {
        var portfolio = await context.PortfolioPages
            .FindAsync(id);
        return (portfolio == null) ? null : PortfolioPageToDto(portfolio);
    }

    public async Task<IPortfolioPageRepository.PortfolioPageDto?> GetPortfolioPageByAlbumAsync(int albumId)
    {
        var albumExists = await context.Albums
            .FindAsync(albumId);
        if (albumExists == null)
        {
            throw new KeyNotFoundException();
        }
        var portfolio = await context.PortfolioPages
            .Where(pp => pp.AlbumId == albumId)
            .FirstOrDefaultAsync();
        
        return (portfolio == null) ? null : PortfolioPageToDto(portfolio);
    }

    public async Task<IPortfolioPageRepository.PortfolioPageDto?> SavePortfolioPageAsync(PortfolioPage portfolioPage)
    {
        context.PortfolioPages.Add(portfolioPage);
        try
        {
            await context.SaveChangesAsync();
            return PortfolioPageToDto(portfolioPage);
        }
        catch (Exception)
        {
            return null;
        }
        
    }

    public async Task<bool> SetPortfolioPageLayoutPresetAsync(int ppId, PageLayoutPreset layout)
    {
        var pp = await context.PortfolioPages.FindAsync(ppId);
        if (pp == null) return false;

        pp.LayoutPreset = layout;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReorderPortfolioPageInNavAsync(int ppId, int newNavOrder)
    {
        // Magic numbers: these represent the bounds of the range attribute on PortfolioPage.NavbarOrder. TODO Refactor.
        if (newNavOrder is < -1 or > 4) return false;
            
        var toReorder = await context.PortfolioPages
            .FindAsync(ppId);
        if (toReorder == null || toReorder.NavbarOrder == newNavOrder) return false;
        
        if (newNavOrder == -1)
        {
            toReorder.NavbarOrder = newNavOrder;
            await context.SaveChangesAsync();
            return true;
        }
        
        var occupyingPage = await context.PortfolioPages
            .Where(p => p.NavbarOrder == newNavOrder)
            .FirstOrDefaultAsync();
        if (occupyingPage != null) return false;

        toReorder.NavbarOrder = newNavOrder;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<IPortfolioPageRepository.PortfolioPageDto>> GetPublishedNotInNavbar()
    {
        var pages = await context.PortfolioPages
            .Select(pp => PortfolioPageToDto(pp))
            .Where(pp => pp.Published && pp.NavbarOrder == -1)
            .ToListAsync();

        return pages;
    }

    public async Task<IEnumerable<IPortfolioPageRepository.PortfolioPageDto>> GetPublishedInNavbarOrdered()
    {
        var pages = await context.PortfolioPages
            .AsNoTracking()
            .Where(pp => pp.Published && pp.NavbarOrder != -1)
            .OrderBy(pp => pp.NavbarOrder)
            .Select(pp => PortfolioPageToDto(pp))
            .ToListAsync();

        return pages;
    }

    public async Task<int?> PublishPortfolioPageAsync(int ppId, int? navOrder)
    {
        var toPublish = await context.PortfolioPages
            .FindAsync(ppId);
        if (toPublish == null || toPublish.Published) return null;
        
        await ReorderPortfolioPageInNavAsync(ppId, ((navOrder is >= -1 and <= 4) ? navOrder.Value : -1));

        toPublish.Published = true;
        await context.SaveChangesAsync();
        return toPublish.NavbarOrder;
    }
    
    public async Task<bool> UnpublishPortfolioPageAsync(int ppId)
    {
        var toUnpublish = await context.PortfolioPages
            .FindAsync(ppId);
        if (toUnpublish is not { Published: true }) return false;
        
        toUnpublish.Published = false;
        toUnpublish.NavbarOrder = -1;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdatePortfolioPageAsync(int ppId, IPortfolioPageRepository.UpdatePortfolioPageDto model)
    {
        if (model is { NavTitle: null, Title: null }) return false;

        var pPage = await context.PortfolioPages
            .FindAsync(ppId);
        if (pPage is null) return false;
        
        if (model.Title is not null)
        {
            pPage.Title = model.Title;
        }
        if (model.NavTitle is not null)
        {
            pPage.NavTitle = model.NavTitle;
        }

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeletePortfolioPageAsync(int id)
    {
        var pPage = await context.PortfolioPages
            .FindAsync(id);

        if (pPage is null) return false;

        context.PortfolioPages.Remove(pPage);
        await context.SaveChangesAsync();

        return true;
    }
    
    public static IPortfolioPageRepository.PortfolioPageDto PortfolioPageToDto(PortfolioPage p)
    {
        return new IPortfolioPageRepository.PortfolioPageDto(p.Id, p.NavTitle, p.Title, p.Published, p.NavbarOrder, p.AlbumId, p.LayoutPreset);
    }
}