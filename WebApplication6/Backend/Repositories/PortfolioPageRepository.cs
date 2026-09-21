// using System.Linq.Expressions;
// using Microsoft.EntityFrameworkCore;
// using WebApplication6.Backend.Data;
// using WebApplication6.Backend.Models;
//
// namespace WebApplication6.Backend.Repositories;
//
// public class PortfolioPageRepository(ApplicationDbContext context) : IPortfolioPageRepository
// {
//     // private static readonly Expression<Func<Album, IPortfolioPageRepository.AlbumDto>> ToDto =
//     //     album => new IPortfolioPageRepository.AlbumDto(
//     //         album.Id, album.Name, album.Description, album.NavTitle,
//     //         album.Published, album.NavbarOrder, album.LayoutPreset);
//     //
//     // public async Task<IEnumerable<IPortfolioPageRepository.AlbumDto>> GetAllPublishedAsync()
//     //     => await context.Albums.AsNoTracking().Where(album => album.Published).Select(ToDto).ToListAsync();
//     //
//     // public Task<IPortfolioPageRepository.AlbumDto?> GetAlbumByIdAsync(int albumId)
//     //     => context.Albums.AsNoTracking().Where(album => album.Id == albumId).Select(ToDto).SingleOrDefaultAsync();
//     //
//     // public async Task<bool> SetLayoutPresetAsync(int albumId, PageLayoutPreset layout)
//     // {
//     //     if (!Enum.IsDefined(layout)) return false;
//     //     var album = await context.Albums.FindAsync(albumId);
//     //     if (album == null) return false;
//     //
//     //     album.LayoutPreset = layout;
//     //     await context.SaveChangesAsync();
//     //     return true;
//     // }
//     //
//     // public async Task<bool> AssignAlbumInNavAsync(int albumId, int newNavOrder)
//     // {
//     //     if (newNavOrder is < -1 or > 4) return false;
//     //     var album = await context.Albums.FindAsync(albumId);
//     //     if (album == null || (newNavOrder >= 0 && !album.Published)) return false;
//     //
//     //     var albums = await GetNavbarAlbumsAsync();
//     //     albums.Remove(album);
//     //     if (newNavOrder >= 0)
//     //     {
//     //         if (albums.Count >= 5) return false;
//     //         albums.Insert(Math.Min(newNavOrder, albums.Count), album);
//     //     }
//     //
//     //     await SaveNavbarOrderAsync(albums);
//     //     return true;
//     // }
//     //
//     // public async Task<bool> SwapAlbumsInNavOrderAsync(int albumId1, int albumId2)
//     // {
//     //     if (albumId1 == albumId2) return false;
//     //     var albums = await GetNavbarAlbumsAsync();
//     //     var index1 = albums.FindIndex(album => album.Id == albumId1);
//     //     var index2 = albums.FindIndex(album => album.Id == albumId2);
//     //     if (index1 < 0 || index2 < 0) return false;
//     //
//     //     (albums[index1], albums[index2]) = (albums[index2], albums[index1]);
//     //     await SaveNavbarOrderAsync(albums);
//     //     return true;
//     // }
//     //
//     // public async Task<IEnumerable<IPortfolioPageRepository.AlbumDto>> GetPublishedNotInNavbar()
//     //     => await context.Albums.AsNoTracking().Where(album => album.Published && album.NavbarOrder == -1)
//     //         .Select(ToDto).ToListAsync();
//     //
//     // public async Task<IEnumerable<IPortfolioPageRepository.AlbumDto>> GetPublishedInNavbarOrdered()
//     //     => await context.Albums.AsNoTracking().Where(album => album.Published && album.NavbarOrder >= 0)
//     //         .OrderBy(album => album.NavbarOrder).Select(ToDto).ToListAsync();
//     //
//     // public async Task<int?> PublishAlbumAsync(int albumId, int? navOrder)
//     // {
//     //     var album = await context.Albums.FindAsync(albumId);
//     //     if (album == null || album.Published) return null;
//     //
//     //     var albums = await GetNavbarAlbumsAsync();
//     //     if (navOrder is >= 0 and <= 4 && albums.Count < 5)
//     //     {
//     //         albums.Insert(Math.Min(navOrder.Value, albums.Count), album);
//     //     }
//     //
//     //     // Existing albums receive an empty NavTitle when the new column is added.
//     //     if (string.IsNullOrWhiteSpace(album.NavTitle))
//     //     {
//     //         var name = album.Name?.Trim() ?? "";
//     //         album.NavTitle = name.Length > 20 ? name[..20] : name;
//     //     }
//     //
//     //     // Publish before assigning a navbar position to satisfy the database constraint.
//     //     album.Published = true;
//     //     await SaveNavbarOrderAsync(albums);
//     //     return album.NavbarOrder;
//     // }
//     //
//     // public async Task<bool> UnpublishAlbumAsync(int albumId)
//     // {
//     //     var album = await context.Albums.FindAsync(albumId);
//     //     if (album is not { Published: true }) return false;
//     //
//     //     var albums = await GetNavbarAlbumsAsync();
//     //     albums.Remove(album);
//     //     album.Published = false;
//     //     await SaveNavbarOrderAsync(albums);
//     //     return true;
//     // }
//     //
//     // public async Task<bool> UpdateAlbumPresentationAsync(
//     //     int albumId, IPortfolioPageRepository.UpdateAlbumPresentationDto model)
//     // {
//     //     if (model.NavTitle is null || model.NavTitle.Length > 20) return false;
//     //     var album = await context.Albums.FindAsync(albumId);
//     //     if (album == null) return false;
//     //
//     //     album.NavTitle = model.NavTitle;
//     //     await context.SaveChangesAsync();
//     //     return true;
//     // }
//     //
//     // private Task<List<Album>> GetNavbarAlbumsAsync()
//     //     => context.Albums.Where(album => album.NavbarOrder >= 0)
//     //         .OrderBy(album => album.NavbarOrder).ToListAsync();
//     //
//     // private async Task SaveNavbarOrderAsync(List<Album> orderedAlbums)
//     // {
//     //     // Free occupied positions before assigning the new order. Keep both saves atomic.
//     //     await using var transaction = context.Database.CurrentTransaction == null
//     //         ? await context.Database.BeginTransactionAsync()
//     //         : null;
//     //
//     //     var currentNavbar = await GetNavbarAlbumsAsync();
//     //     foreach (var album in currentNavbar)
//     //     {
//     //         album.NavbarOrder = -1;
//     //     }
//     //     await context.SaveChangesAsync();
//     //
//     //     for (var index = 0; index < orderedAlbums.Count; index++)
//     //     {
//     //         orderedAlbums[index].NavbarOrder = index;
//     //     }
//     //     await context.SaveChangesAsync();
//     //
//     //     if (transaction != null) await transaction.CommitAsync();
//     // }
// }
