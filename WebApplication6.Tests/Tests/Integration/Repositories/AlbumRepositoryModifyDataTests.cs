using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebApplication6.Backend.Models;
using WebApplication6.Backend.Repositories;
using Xunit;

namespace WebApplication6.Tests.Tests.Integration.Repositories;

// Tests against test database.
public class AlbumRepositoryModifyDataTests(TestDatabaseFixture dbFixture) : IClassFixture<TestDatabaseFixture>
{
    public TestDatabaseFixture Fixture { get; } = dbFixture;
    
    [Theory]
    [InlineData(1, 4, -1)]
    [InlineData(1, 4, -99)]
    [InlineData(1, 3, -7)]
    [InlineData(1, 64, -9999)]
    public async Task ReorderPhotoInAlbum_InvalidOrder_MaintainsPracticalOrderButNormalizedAndReturnsTrue(int albumId, int photoId, int newOrder)
    {
        await using var context = Fixture.CreateContext();
        await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var repository = new AlbumRepository(context);


        
        var task = await repository.ReorderPhotoInAlbum(albumId, photoId, newOrder);
        context.ChangeTracker.Clear();
        
        var albumPhotos = await context.AlbumPhotos
            .Include(ap => ap.AlbumPhotoDisplayItem)
            .Where(ap => ap.AlbumId == albumId)
            .OrderBy(ap => ap.AlbumPhotoDisplayItem.Order)
            .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.Collection(albumPhotos,
            element1Inspector =>
            {
                Assert.Equal(4, element1Inspector.PhotoId);
                Assert.Equal(0, element1Inspector.AlbumPhotoDisplayItem.Order);
            },
            element2Inspector =>
            {
                Assert.Equal(2, element2Inspector.PhotoId);
                Assert.Equal(1, element2Inspector.AlbumPhotoDisplayItem.Order);
            },
            element3Inspector =>
            {
                Assert.Equal(3, element3Inspector.PhotoId);
                Assert.Equal(2, element3Inspector.AlbumPhotoDisplayItem.Order);
            },
            element4Inspector =>
            {
                Assert.Equal(64, element4Inspector.PhotoId);
                Assert.Equal(3, element4Inspector.AlbumPhotoDisplayItem.Order);
            });
        
        Assert.True(task);
    }
    
    [Theory]
    [InlineData(1, 4, 0)]
    [InlineData(1, 2, 1)]
    [InlineData(1, 3, 2)]
    [InlineData(1, 64, 4)]
    public async Task ReorderPhotoInAlbum_SameOrder_MaintainsPracticalOrderButNormalizedAndReturnsTrue(int albumId, int photoId, int newOrder)
    {
        await using var context = Fixture.CreateContext();
        await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var repository = new AlbumRepository(context);


        
        var task = await repository.ReorderPhotoInAlbum(albumId, photoId, newOrder);
        context.ChangeTracker.Clear();
        
        var albumPhotos = await context.AlbumPhotos
            .Include(ap => ap.AlbumPhotoDisplayItem)
            .Where(ap => ap.AlbumId == albumId)
            .OrderBy(ap => ap.AlbumPhotoDisplayItem.Order)
            .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.Collection(albumPhotos,
            element1Inspector =>
            {
                Assert.Equal(4, element1Inspector.PhotoId);
                Assert.Equal(0, element1Inspector.AlbumPhotoDisplayItem.Order);
            },
            element2Inspector =>
            {
                Assert.Equal(2, element2Inspector.PhotoId);
                Assert.Equal(1, element2Inspector.AlbumPhotoDisplayItem.Order);
            },
            element3Inspector =>
            {
                Assert.Equal(3, element3Inspector.PhotoId);
                Assert.Equal(2, element3Inspector.AlbumPhotoDisplayItem.Order);
            },
            element4Inspector =>
            {
                Assert.Equal(64, element4Inspector.PhotoId);
                Assert.Equal(3, element4Inspector.AlbumPhotoDisplayItem.Order);
            });
        
        Assert.True(task);
    }
    
    [Fact]
    public async Task ReorderPhotoInAlbum_AssignOrderInBetweenGap_ReturnsExpectedAndNormalized()
    {
        const int albumId = 1;
        const int photoId = 4;
        const int newOrder = 3;
        
        await using var context = Fixture.CreateContext();
        await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var repository = new AlbumRepository(context);
        
        var task = await repository.ReorderPhotoInAlbum(albumId, photoId, newOrder);
        context.ChangeTracker.Clear();
        
        var albumPhotos = await context.AlbumPhotos
            .Include(ap => ap.AlbumPhotoDisplayItem)
            .Where(ap => ap.AlbumId == albumId)
            .OrderBy(ap => ap.AlbumPhotoDisplayItem.Order)
            .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.Collection(albumPhotos,
            element1Inspector =>
            {
                Assert.Equal(2, element1Inspector.PhotoId);
                Assert.Equal(0, element1Inspector.AlbumPhotoDisplayItem.Order);
            },
            element2Inspector =>
            {
                Assert.Equal(3, element2Inspector.PhotoId);
                Assert.Equal(1, element2Inspector.AlbumPhotoDisplayItem.Order);
            },
            element3Inspector =>
            {
                Assert.Equal(4, element3Inspector.PhotoId);
                Assert.Equal(2, element3Inspector.AlbumPhotoDisplayItem.Order);
            },
            element4Inspector =>
            {
                Assert.Equal(64, element4Inspector.PhotoId);
                Assert.Equal(3, element4Inspector.AlbumPhotoDisplayItem.Order);
            });
        
        Assert.True(task);
    }
        
    [Fact]
    public async Task ReorderPhotoInAlbum_SwapToHigherOrder_ReturnsExpectedAndNormalized()
    {
        const int albumId = 1;
        const int photoId = 2;
        const int newOrder = 4;
        
        await using var context = Fixture.CreateContext();
        await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var repository = new AlbumRepository(context);
        
        var task = await repository.ReorderPhotoInAlbum(albumId, photoId, newOrder);
        context.ChangeTracker.Clear();
        
        var albumPhotos = await context.AlbumPhotos
            .Include(ap => ap.AlbumPhotoDisplayItem)
            .Where(ap => ap.AlbumId == albumId)
            .OrderBy(ap => ap.AlbumPhotoDisplayItem.Order)
            .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.Collection(albumPhotos,
            element1Inspector =>
            {
                Assert.Equal(4, element1Inspector.PhotoId);
                Assert.Equal(0, element1Inspector.AlbumPhotoDisplayItem.Order);
            },
            element2Inspector =>
            {
                Assert.Equal(3, element2Inspector.PhotoId);
                Assert.Equal(1, element2Inspector.AlbumPhotoDisplayItem.Order);
            },
            element3Inspector =>
            {
                Assert.Equal(64, element3Inspector.PhotoId);
                Assert.Equal(2, element3Inspector.AlbumPhotoDisplayItem.Order);
            },
            element4Inspector =>
            {
                Assert.Equal(2, element4Inspector.PhotoId);
                Assert.Equal(3, element4Inspector.AlbumPhotoDisplayItem.Order);
            });
        
        Assert.True(task);
    }
    
    [Fact]
    public async Task ReorderPhotoInAlbum_LastToAboveMaxOrder_ReturnsExpectedAndNormalized()
    {
        const int albumId = 1;
        const int photoId = 64;
        const int newOrder = 5;
        
        await using var context = Fixture.CreateContext();
        await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var repository = new AlbumRepository(context);
        
        var task = await repository.ReorderPhotoInAlbum(albumId, photoId, newOrder);
        context.ChangeTracker.Clear();
        
        var albumPhotos = await context.AlbumPhotos
            .Include(ap => ap.AlbumPhotoDisplayItem)
            .Where(ap => ap.AlbumId == albumId)
            .OrderBy(ap => ap.AlbumPhotoDisplayItem.Order)
            .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.Collection(albumPhotos,
            element1Inspector =>
            {
                Assert.Equal(4, element1Inspector.PhotoId);
                Assert.Equal(0, element1Inspector.AlbumPhotoDisplayItem.Order);
            },
            element2Inspector =>
            {
                Assert.Equal(2, element2Inspector.PhotoId);
                Assert.Equal(1, element2Inspector.AlbumPhotoDisplayItem.Order);
            },
            element3Inspector =>
            {
                Assert.Equal(3, element3Inspector.PhotoId);
                Assert.Equal(2, element3Inspector.AlbumPhotoDisplayItem.Order);
            },
            element4Inspector =>
            {
                Assert.Equal(64, element4Inspector.PhotoId);
                Assert.Equal(3, element4Inspector.AlbumPhotoDisplayItem.Order);
            });
        
        Assert.True(task);
    }
    
    [Fact]
    public async Task ReorderPhotoInAlbum_FirstTakesElevator_ReturnsExpectedAndNormalized()
    {
        const int albumId = 1;
        const int photoId = 4;
        var newOrder = 1;
        
        await using var context = Fixture.CreateContext();
        await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var repository = new AlbumRepository(context);
        
        var task1 = await repository.ReorderPhotoInAlbum(albumId, photoId, newOrder);
        newOrder += 1;
        var task2 = await repository.ReorderPhotoInAlbum(albumId, photoId, newOrder);
        newOrder += 2;
        var task3 = await repository.ReorderPhotoInAlbum(albumId, photoId, newOrder);

        context.ChangeTracker.Clear();
        
        var albumPhotos = await context.AlbumPhotos
            .Include(ap => ap.AlbumPhotoDisplayItem)
            .Where(ap => ap.AlbumId == albumId)
            .OrderBy(ap => ap.AlbumPhotoDisplayItem.Order)
            .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.Collection(albumPhotos,
            element1Inspector =>
            {
                Assert.Equal(2, element1Inspector.PhotoId);
                Assert.Equal(0, element1Inspector.AlbumPhotoDisplayItem.Order);
            },
            element2Inspector =>
            {
                Assert.Equal(3, element2Inspector.PhotoId);
                Assert.Equal(1, element2Inspector.AlbumPhotoDisplayItem.Order);
            },
            element3Inspector =>
            {
                Assert.Equal(64, element3Inspector.PhotoId);
                Assert.Equal(2, element3Inspector.AlbumPhotoDisplayItem.Order);
            },
            element4Inspector =>
            {
                Assert.Equal(4, element4Inspector.PhotoId);
                Assert.Equal(3, element4Inspector.AlbumPhotoDisplayItem.Order);
            });
        
        Assert.True(task1);
        Assert.True(task2);
        Assert.True(task3);
    }
    


    [Fact]
    public async Task AddPhotoToAlbumAsync_CreatesSingletonItemsAndPreservesDisplaySettings()
    {
        await using var context = Fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var repository = new AlbumRepository(context);

        Assert.True(await repository.AddPhotoToAlbumAsync(3002, 2, TestContext.Current.CancellationToken));
        Assert.True(await repository.AddPhotoToAlbumAsync(3002, 3, TestContext.Current.CancellationToken));
        Assert.True(await repository.ToggleDisplaysName(3002, 2));
        Assert.True(await repository.ToggleDisplaysDescription(3002, 2));
        Assert.True(await repository.ToggleDisplaysYearContenCreated(3002, 2));
        context.ChangeTracker.Clear();

        var albumPhotoDisplayItems = await context.AlbumPhotoDisplayItems
            .Include(ag => ag.AlbumPhotos)
            .Where(ag => ag.AlbumId == 3002)
            .OrderBy(ag => ag.Order)
            .ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(new[] { 0, 1 }, albumPhotoDisplayItems.Select(ag => ag.Order));
        Assert.All(albumPhotoDisplayItems, albumPhotoDisplayItem =>
        {
            Assert.Equal(AlbumPhotoDisplayItem.PhotoDisplayMode.Static, albumPhotoDisplayItem.DisplayMode);
            Assert.Equal(0, Assert.Single(albumPhotoDisplayItem.AlbumPhotos).Order);
        });

        var photos = (await repository.GetAlbumPhotosAsync(3002)).ToList();
        Assert.Equal(new[] { 2, 3 }, photos.Select(p => p.Id));
        Assert.Equal(new int?[] { 0, 1 }, photos.Select(p => p.Order));
        Assert.False(photos[0].displaysName);
        Assert.False(photos[0].displaysDescription);
        Assert.False(photos[0].displaysYearCC);
        Assert.True(photos[1].displaysName);
        Assert.True(photos[1].displaysDescription);
        Assert.True(photos[1].displaysYearCC);
    }

    [Fact]
    public async Task AddPhotoToAlbumAsync_DuplicatePhoto_DoesNotLeaveAnEmptyItem()
    {
        await using var context = Fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var repository = new AlbumRepository(context);

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => repository.AddPhotoToAlbumAsync(1, 2, TestContext.Current.CancellationToken));
        Assert.Equal("PK_AlbumPhoto", Assert.IsType<PostgresException>(exception.InnerException).ConstraintName);

        Assert.Equal(4, await context.AlbumPhotoDisplayItems.CountAsync(ag => ag.AlbumId == 1, TestContext.Current.CancellationToken));
        Assert.Equal(4, await context.AlbumPhotos.CountAsync(ap => ap.AlbumId == 1, TestContext.Current.CancellationToken));
        Assert.DoesNotContain(context.ChangeTracker.Entries(), e => e.State == EntityState.Added);
        Assert.True(await repository.AddPhotoToAlbumAsync(3002, 2, TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(1, -1, 0, "CK_AlbumItems_Order_NonNegative")]
    [InlineData(1, 3, -1, "CK_AlbumItems_PhotoDisplayMode_Range")]
    [InlineData(1, 3, 2, "CK_AlbumItems_PhotoDisplayMode_Range")]
    [InlineData(1, 0, 0, "UX_AlbumItems_AlbumId_Order")]
    [InlineData(9999, 0, 0, "FK_AlbumItems_Albums_AlbumId")]
    public async Task AlbumPhotoDisplayItem_DatabaseRejectsInvalidItem(int albumId, int order, int displayMode, string constraintName)
    {
        await using var context = Fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        context.AlbumPhotoDisplayItems.Add(new AlbumPhotoDisplayItem
        {
            AlbumId = albumId, Order = order, DisplayMode = (AlbumPhotoDisplayItem.PhotoDisplayMode)displayMode
        });

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(TestContext.Current.CancellationToken));
        Assert.Equal(constraintName, Assert.IsType<PostgresException>(exception.InnerException).ConstraintName);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AlbumPhoto_DatabaseRequiresItemInSameAlbum(bool existingItem)
    {
        await using var context = Fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var albumPhotoDisplayItemId = existingItem
            ? await context.AlbumPhotoDisplayItems.Where(ag => ag.AlbumId == 1).Select(ag => ag.Id).FirstAsync(TestContext.Current.CancellationToken)
            : 0;
        context.AlbumPhotos.Add(new AlbumPhoto { AlbumId = 3002, PhotoId = 2, AlbumPhotoDisplayItemId = albumPhotoDisplayItemId, Order = 1 });

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(TestContext.Current.CancellationToken));
        Assert.Equal("FK_AlbumPhoto_AlbumItems_AlbumId_ItemId_ItemType", Assert.IsType<PostgresException>(exception.InnerException).ConstraintName);
    }

    [Theory]
    [InlineData(0, "UX_AlbumPhoto_AlbumPhotoDisplayItemId_Order")]
    [InlineData(-1, "CK_AlbumPhoto_Order_NonNegative")]
    public async Task AlbumPhoto_DatabaseRequiresUniqueNonNegativeOrderWithinItem(int order, string constraintName)
    {
        await using var context = Fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var albumPhotoDisplayItem = new AlbumPhotoDisplayItem { AlbumId = 3002, Order = 0, DisplayMode = AlbumPhotoDisplayItem.PhotoDisplayMode.Carousel };
        context.AlbumPhotos.Add(new AlbumPhoto { AlbumId = 3002, PhotoId = 2, AlbumPhotoDisplayItem = albumPhotoDisplayItem, Order = 0 });
        var albumPhoto = new AlbumPhoto { AlbumId = 3002, PhotoId = 3, AlbumPhotoDisplayItem = albumPhotoDisplayItem, Order = 1 };
        context.AlbumPhotos.Add(albumPhoto);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        albumPhoto.Order = order;
        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(TestContext.Current.CancellationToken));
        Assert.Equal(constraintName, Assert.IsType<PostgresException>(exception.InnerException).ConstraintName);
    }

    [Fact]
    public async Task DeletePhotoByIdAsync_RemovesEmptyItemsAndPreservesOtherPhotos()
    {
        await using var context = Fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var albumPhotoDisplayItem = new AlbumPhotoDisplayItem { AlbumId = 3002, Order = 0 };
        context.AlbumPhotos.Add(new AlbumPhoto { AlbumId = 3002, PhotoId = 2, AlbumPhotoDisplayItem = albumPhotoDisplayItem, Order = 0 });
        context.AlbumPhotos.Add(new AlbumPhoto { AlbumId = 3002, PhotoId = 3, AlbumPhotoDisplayItem = albumPhotoDisplayItem, Order = 1 });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var albumPhotoDisplayItemId = await context.AlbumPhotos.Where(ap => ap.AlbumId == 1 && ap.PhotoId == 2)
            .Select(ap => ap.AlbumPhotoDisplayItemId).SingleAsync(TestContext.Current.CancellationToken);
        context.ChangeTracker.Clear();
        var repository = new PhotoRepository(context);

        Assert.True(await repository.DeletePhotoByIdAsync(2));
        context.ChangeTracker.Clear();

        Assert.Null(await context.Photos.FindAsync([2], TestContext.Current.CancellationToken));
        Assert.NotNull(await context.Photos.FindAsync([3], TestContext.Current.CancellationToken));
        Assert.False(await context.AlbumPhotoDisplayItems.AnyAsync(ag => ag.Id == albumPhotoDisplayItemId, TestContext.Current.CancellationToken));
        Assert.False(await context.AlbumPhotos.AnyAsync(ap => ap.PhotoId == 2, TestContext.Current.CancellationToken));
        var remaining = await context.AlbumPhotoDisplayItems.Include(ag => ag.AlbumPhotos)
            .SingleAsync(ag => ag.Id == albumPhotoDisplayItem.Id, TestContext.Current.CancellationToken);
        Assert.Equal(3, Assert.Single(remaining.AlbumPhotos).PhotoId);
        Assert.Equal(3, await context.AlbumPhotoDisplayItems.CountAsync(ag => ag.AlbumId == 1, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AlbumPresentation_PublishAndQuery_UsesAlbumIdentityAndDefaults()
    {
        await using var context = Fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var albums = await CreatePresentationAlbumsAsync(context, 2);
        var repository = new PortfolioPageRepository(context);

        Assert.Equal(-1, albums[0].NavbarOrder);
        Assert.False(albums[0].Published);
        Assert.Equal(PageLayoutPreset.Default, albums[0].LayoutPreset);
        Assert.Null(context.Model.FindEntityType("WebApplication6.Backend.Models.PortfolioPage"));
        Assert.False(await repository.AssignAlbumInNavAsync(albums[0].Id, 0));

        albums[1].NavTitle = "";
        Assert.Equal(0, await repository.PublishAlbumAsync(albums[0].Id, 4));
        Assert.Equal(-1, await repository.PublishAlbumAsync(albums[1].Id, null));
        Assert.Equal(albums[1].Name, albums[1].NavTitle);
        Assert.True(await repository.SetLayoutPresetAsync(albums[0].Id, PageLayoutPreset.Spooky));
        Assert.True(await repository.UpdateAlbumPresentationAsync(albums[0].Id,
            new IPortfolioPageRepository.UpdateAlbumPresentationDto("Gallery")));

        context.ChangeTracker.Clear();
        var published = (await repository.GetAllPublishedAsync()).ToList();
        Assert.Equal(2, published.Count);
        var inNav = Assert.Single(await repository.GetPublishedInNavbarOrdered());
        Assert.Equal(albums[0].Id, inNav.Id);
        Assert.Equal("Gallery", inNav.NavTitle);
        Assert.Equal(PageLayoutPreset.Spooky, inNav.LayoutPreset);
        Assert.Equal(albums[1].Id, Assert.Single(await repository.GetPublishedNotInNavbar()).Id);
        Assert.Equal(albums[0].Name, (await repository.GetAlbumByIdAsync(albums[0].Id))!.Name);
    }

    [Fact]
    public async Task AlbumPresentation_FullNavbar_AllowsReorderAndSwapButRejectsSixthEntry()
    {
        await using var context = Fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var albums = await CreatePresentationAlbumsAsync(context, 6);
        var repository = new PortfolioPageRepository(context);
        foreach (var album in albums) Assert.Equal(-1, await repository.PublishAlbumAsync(album.Id, null));
        for (var index = 0; index < 5; index++)
            Assert.True(await repository.AssignAlbumInNavAsync(albums[index].Id, index));

        Assert.False(await repository.AssignAlbumInNavAsync(albums[5].Id, 0));
        Assert.True(await repository.AssignAlbumInNavAsync(albums[0].Id, 4));
        Assert.True(await repository.SwapAlbumsInNavOrderAsync(albums[0].Id, albums[1].Id));

        context.ChangeTracker.Clear();
        var navbar = (await repository.GetPublishedInNavbarOrdered()).ToList();
        Assert.Equal(new[] {0, 1, 2, 3, 4}, navbar.Select(a => a.NavbarOrder));
        Assert.Equal(new[] {albums[0].Id, albums[2].Id, albums[3].Id, albums[4].Id, albums[1].Id},
            navbar.Select(a => a.Id));
    }

    [Fact]
    public async Task AlbumPresentation_UnpublishRemoveAndDelete_NormalizesNavbarAndPreservesPhotos()
    {
        await using var context = Fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var albums = await CreatePresentationAlbumsAsync(context, 3);
        context.AlbumPhotos.Add(new AlbumPhoto
        {
            AlbumId = albums[1].Id, PhotoId = 2, Order = 0,
            AlbumPhotoDisplayItem = new AlbumPhotoDisplayItem { AlbumId = albums[1].Id, Order = 0 }
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var repository = new PortfolioPageRepository(context);
        for (var index = 0; index < albums.Length; index++)
            Assert.Equal(index, await repository.PublishAlbumAsync(albums[index].Id, index));

        Assert.True(await repository.UnpublishAlbumAsync(albums[0].Id));
        Assert.False(albums[0].Published);
        Assert.Equal(-1, albums[0].NavbarOrder);
        Assert.True(await repository.AssignAlbumInNavAsync(albums[2].Id, -1));
        Assert.True(albums[2].Published);
        Assert.True(await repository.AssignAlbumInNavAsync(albums[2].Id, 1));
        Assert.True(await new AlbumRepository(context).DeleteAlbumByIdAsync(albums[1].Id));

        context.ChangeTracker.Clear();
        var remaining = Assert.Single(await repository.GetPublishedInNavbarOrdered());
        Assert.Equal(albums[2].Id, remaining.Id);
        Assert.Equal(0, remaining.NavbarOrder);
        Assert.NotNull(await context.Photos.FindAsync([2], TestContext.Current.CancellationToken));
        Assert.False(await context.AlbumPhotos.AnyAsync(ap => ap.AlbumId == albums[1].Id, TestContext.Current.CancellationToken));
        Assert.False(await context.AlbumPhotoDisplayItems.AnyAsync(ag => ag.AlbumId == albums[1].Id, TestContext.Current.CancellationToken));
        Assert.Null(await repository.GetAlbumByIdAsync(albums[1].Id));
    }

    [Fact]
    public async Task AlbumPresentation_ExplicitFirstNavbarPosition_IsSavedOnInsert()
    {
        await using var context = Fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var album = new Album { Id = 7000, Name = "First", Published = true, NavbarOrder = 0 };
        context.Albums.Add(album);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        context.ChangeTracker.Clear();

        Assert.Equal(0, (await context.Albums.FindAsync([album.Id], TestContext.Current.CancellationToken))!.NavbarOrder);
    }

    [Theory]
    [InlineData(false, 0, "valid")]
    [InlineData(true, -2, "valid")]
    [InlineData(true, 5, "valid")]
    [InlineData(false, -1, "123456789012345678901")]
    public async Task AlbumPresentation_DatabaseRejectsInvalidPresentation(bool published, int order, string navTitle)
    {
        await using var context = Fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        context.Albums.Add(new Album
        {
            Id = 7000, Name = "Invalid presentation", Published = published,
            NavbarOrder = order, NavTitle = navTitle
        });
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AlbumPresentation_DatabaseRejectsDuplicateNavbarPosition()
    {
        await using var context = Fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var albums = await CreatePresentationAlbumsAsync(context, 2);
        foreach (var album in albums)
        {
            album.Published = true;
            album.NavbarOrder = 0;
        }
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    private static async Task<Album[]> CreatePresentationAlbumsAsync(
        WebApplication6.Backend.Data.ApplicationDbContext context, int count)
    {
        var albums = Enumerable.Range(0, count).Select(index => new Album
        {
            Id = 7000 + index,
            Name = $"Gallery {index}",
            NavTitle = $"Gallery {index}"
        }).ToArray();
        context.Albums.AddRange(albums);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        return albums;
    }
}