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

    public async Task<bool> AssignPortfolioPageInNavAsync(int ppId, int newNavOrder)
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
            await NormalizeNavOrder();
            return true;
        }

        var pages = await context.PortfolioPages
            .Where(pp => pp.NavbarOrder > -1)
            .OrderBy(pp => pp.NavbarOrder)
            .ToListAsync();

        if (pages.Count == 5) return false; // Navbar is full

        var occupyingPage = pages.Find(pp => pp.NavbarOrder == newNavOrder);
        if (occupyingPage == null)
        {
            toReorder.NavbarOrder = newNavOrder;
            await context.SaveChangesAsync();
            await NormalizeNavOrder();
            return true;
        }

        // similar process to NormalizeNavOrder() - TODO: optimize case where we enter this block of code - by merging logic directly with NormalizeNavOrder logic in a new helper
        var orders = new List<(int, int)>(); // (int ppId, int newNavOrder)
        
        foreach (var pp in pages)
        {
            if (pp.NavbarOrder >= newNavOrder)
            {
                orders.Add((pp.Id, ((pp.NavbarOrder + 1 <= 4) ? pp.NavbarOrder + 1 : 4)));
                pp.NavbarOrder = -1;
            }
        }

        await context.SaveChangesAsync();

        toReorder.NavbarOrder = newNavOrder;
        foreach (var reordering in orders)
        {
            pages.Find(pp => pp.Id == reordering.Item1)?.NavbarOrder = reordering.Item2;
        }

        await context.SaveChangesAsync();
        await NormalizeNavOrder(); // TODO: redundancies.
        return true;
        
    }

    public async Task<bool> SwapPortfolioPagesInNavOrderAsync(int ppId1, int ppId2)
    {
        var pp1 = await context.PortfolioPages.FindAsync(ppId1);
        var pp2 = await context.PortfolioPages.FindAsync(ppId2);
        if (pp1 == null) return false;
        if (pp2 == null) return false;

        var orderTmp1 = pp1.NavbarOrder;
        var orderTmp2 = pp2.NavbarOrder;
        pp1.NavbarOrder = -1;
        pp2.NavbarOrder = -1;
        await context.SaveChangesAsync();

        pp1.NavbarOrder = orderTmp2;
        pp2.NavbarOrder = orderTmp1;
        await context.SaveChangesAsync();

        await NormalizeNavOrder();
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
        
        await AssignPortfolioPageInNavAsync(ppId, ((navOrder is >= -1 and <= 4) ? navOrder.Value : -1));

        toPublish.Published = true;
        await context.SaveChangesAsync();
        
        await NormalizeNavOrder();
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
        
        await NormalizeNavOrder();
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
        
        await NormalizeNavOrder();
        return true;
    }
    
    private async Task NormalizeNavOrder()
    {
        var pages = await context.PortfolioPages
            .Where(pp => pp.NavbarOrder > -1)
            .OrderBy(pp => pp.NavbarOrder)
            .ToListAsync();

        var orders = new List<(int, int)>(); // (int ppId, int newNavOrder)
        
        var counter = 0;
        var flagged = false;
        foreach (var pp in pages)
        {
            if (pp.NavbarOrder != counter)
            {
                flagged = true;
            }
            orders.Add((pp.Id, counter));
            counter++;
        }
        if (!flagged) return;
        
        
        foreach (var pp in pages)
        {
            pp.NavbarOrder = -1;
        }
        await context.SaveChangesAsync();
        
        foreach (var reordering in orders)
        {
            pages.Find(pp => pp.Id == reordering.Item1)?.NavbarOrder = reordering.Item2;
        }

        await context.SaveChangesAsync();
    }
    
    public static IPortfolioPageRepository.PortfolioPageDto PortfolioPageToDto(PortfolioPage p)
    {
        return new IPortfolioPageRepository.PortfolioPageDto(p.Id, p.NavTitle, p.Title, p.Published, p.NavbarOrder, p.AlbumId, p.LayoutPreset);
    }
}