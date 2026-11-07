// using System;
// using System.Collections.Generic;
// using System.Data.Common;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Diagnostics;
// using WebApplication6.Backend.Data;
// using WebApplication6.Backend.Models;
// using WebApplication6.Backend.Repositories;
// using Xunit;
//
// namespace WebApplication6.Tests.Tests.Integration.Repositories;
//
// [Collection("TransactionalTests")]
// public class AlbumRepositoryTransactionalModifyDataTestsDeprecate(TransactionalTestDatabaseFixture dbFixture) : IDisposable
// {
//     public TransactionalTestDatabaseFixture Fixture { get; } = dbFixture;
//     public void Dispose() => Fixture.Cleanup();
//
//
//     
//     // /* UpdateNavOrderAsync */
//     // [Theory]
//     // [InlineData(-1, -1)]
//     // [InlineData(-1, 0)]
//     // [InlineData(-1, 4)]
//     // [InlineData(0, -1)]
//     // [InlineData(0, 0)]
//     // [InlineData(0, 2)]
//     // [InlineData(0, 4)]
//     // [InlineData(4, -1)]
//     // [InlineData(4, 0)]
//     // public async Task UpdateNavOrderAsync_SatisfactoryConditionsToModifyNavbar_ReturnedTaskResultIndicatesSuccess(int formerNavOrder, int newNavOrder)
//     // {
//     //     if (formerNavOrder is < -1 or > 4) Assert.Fail();
//     //     if (newNavOrder is < -1 or > 4) Assert.Fail();
//     //     
//     //     using var context = Fixture.CreateContext();
//     //     var repository = new AlbumRepository(context);
//     //     
//     //     var album1 = new Album { Name = "One", Published = true, Id = 1, NavbarOrder = formerNavOrder };
//     //     var album2 = new Album { Name = "Two", Published = true, Id = 2, NavbarOrder = (formerNavOrder == 0) ? 2 : 0 };
//     //     var album3 = new Album { Name = "Three", Published = true, Id = 3, NavbarOrder = (formerNavOrder == 1) ? 2 : 1 };
//     //     var albums = new List<Album> { album1, album2, album3 };
//     //     context.AddRange(albums);
//     //     await context.SaveChangesAsync(TestContext.Current.CancellationToken); // Our 'test navbar' is not full.
//     //     
//     //     var outcome = (await repository.UpdateNavOrderAsync(1, newNavOrder)).Item1;
//     //     Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success, outcome);
//     // }
//     //
//     // [Theory]
//     // [InlineData(int.MinValue)]
//     // [InlineData(-2)]
//     // [InlineData(5)]
//     // [InlineData(int.MaxValue)]
//     // public async Task UpdateNavOrderAsync_OutOfBoundsNewNavOrder_ReturnedTaskResultAlwaysIndicatesOutOfBounds(int newNavOrder)
//     // {
//     //     if (newNavOrder is >= -1 and <= 4) Assert.Fail();
//     //     
//     //     using var context = Fixture.CreateContext();
//     //     var repository = new AlbumRepository(context);
//     //     
//     //     var album1 = new Album { Name = "One", Published = true, Id = 1, NavbarOrder = 0 };
//     //     var album2 = new Album { Name = "Two", Published = true, Id = 2, NavbarOrder = 1 };
//     //     var album3 = new Album { Name = "Three", Published = true, Id = 3, NavbarOrder = 2 };
//     //     var album4 = new Album { Name = "Four", Published = true, Id = 4, NavbarOrder = 3 };
//     //     var albums = new List<Album> { album1, album2, album3, album4 };
//     //     context.AddRange(albums);
//     //     await context.SaveChangesAsync(TestContext.Current.CancellationToken); // Our 'test navbar' is not yet full.
//     //     
//     //     var outcome = (await repository.UpdateNavOrderAsync(7, newNavOrder)).Item1; // Does not exist
//     //     Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.OutOfBounds, outcome);
//     //
//     //     var album7 = new Album { Name = "Seven", Published = false, Id = 7, NavbarOrder = -1 };
//     //     context.Add(album7);
//     //     await context.SaveChangesAsync(TestContext.Current.CancellationToken);
//     //     
//     //     outcome = (await repository.UpdateNavOrderAsync(7, newNavOrder)).Item1; // Is not published
//     //     Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.OutOfBounds, outcome);
//     //
//     //     album7.Published = true;
//     //     await context.SaveChangesAsync(TestContext.Current.CancellationToken);
//     //     outcome = (await repository.UpdateNavOrderAsync(7, newNavOrder)).Item1; // Is otherwise valid
//     //     Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.OutOfBounds, outcome);
//     //
//     //     context.Add(new Album { Name = "Five", Published = true, Id = 5, NavbarOrder = 4 });
//     //     await context.SaveChangesAsync(TestContext.Current.CancellationToken);
//     //     outcome = (await repository.UpdateNavOrderAsync(7, newNavOrder)).Item1; // Navbar is full
//     //     Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.OutOfBounds, outcome);
//     // }
//     //
//     // [Fact]
//     // public async Task UpdateNavOrderAsync_IncludeInNavWithNotOutOfBoundsAndAlbumDoesNotExist_ReturnedTaskResultIndicatesNotFound()
//     // {
//     //     using var context = Fixture.CreateContext();
//     //     var repository = new AlbumRepository(context);
//     //     
//     //     var outcome = (await repository.UpdateNavOrderAsync(7, 0)).Item1;
//     //     Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.NotFound, outcome);
//     //     
//     //     var album1 = new Album { Name = "One", Published = true, Id = 1, NavbarOrder = 0 };
//     //     var album2 = new Album { Name = "Two", Published = true, Id = 2, NavbarOrder = 1 };
//     //     var album3 = new Album { Name = "Three", Published = true, Id = 3, NavbarOrder = 2 };
//     //     var album4 = new Album { Name = "Four", Published = true, Id = 4, NavbarOrder = 3 };
//     //     var album5 = new Album { Name = "Five", Published = true, Id = 5, NavbarOrder = 4 };
//     //     var albums = new List<Album> { album1, album2, album3, album4, album5 };
//     //     context.AddRange(albums);
//     //     await context.SaveChangesAsync(TestContext.Current.CancellationToken); // Our 'test navbar' is full.
//     //
//     //     outcome = (await repository.UpdateNavOrderAsync(7, 0)).Item1;
//     //     Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.NotFound, outcome);
//     // }
//     //
//     // [Fact]
//     // public async Task UpdateNavOrderAsync_IncludeInNavWithNotOutOfBoundsAndAlbumIsNotPublished_ReturnedTaskResultIndicatesNotPublished()
//     // {
//     //     using var context = Fixture.CreateContext();
//     //     var repository = new AlbumRepository(context);
//     //     
//     //     var album1 = new Album { Name = "One", Published = true, Id = 1, NavbarOrder = 0 };
//     //     var album2 = new Album { Name = "Two", Published = true, Id = 2, NavbarOrder = 1 };
//     //     var album3 = new Album { Name = "Three", Published = true, Id = 3, NavbarOrder = 2 };
//     //     var album4 = new Album { Name = "Four", Published = true, Id = 4, NavbarOrder = 3 };
//     //     var album5 = new Album { Name = "Five", Published = false, Id = 5, NavbarOrder = -1 };
//     //     var albums = new List<Album> { album1, album2, album3, album4, album5 };
//     //     context.AddRange(albums);
//     //     await context.SaveChangesAsync(TestContext.Current.CancellationToken); // Our 'test navbar' is not yet full.
//     //
//     //     var outcome = (await repository.UpdateNavOrderAsync(5, 0)).Item1;
//     //     Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.NotPublished, outcome);
//     //
//     //     context.Add(new Album { Name = "Six", Published = true, Id = 6, NavbarOrder = 4 });
//     //     await context.SaveChangesAsync(TestContext.Current.CancellationToken); // Our 'test navbar' is full.
//     //     
//     //     outcome = (await repository.UpdateNavOrderAsync(5, 0)).Item1;
//     //     Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.NotPublished, outcome);
//     // }
//     //
//     // [Fact]
//     // public async Task UpdateNavOrderAsync_IncludeInNavWithAllConditionsSatisfiedExceptNavbarIsFull_ReturnedTaskResultIndicatesNavbarFull()
//     // {
//     //     using var context = Fixture.CreateContext();
//     //     var repository = new AlbumRepository(context);
//     //     
//     //     var album1 = new Album { Name = "One", Published = true, Id = 1, NavbarOrder = 0 };
//     //     var album2 = new Album { Name = "Two", Published = true, Id = 2, NavbarOrder = 1 };
//     //     var album3 = new Album { Name = "Three", Published = true, Id = 3, NavbarOrder = 2 };
//     //     var album4 = new Album { Name = "Four", Published = true, Id = 4, NavbarOrder = 3 };
//     //     var album5 = new Album { Name = "Five", Published = true, Id = 5, NavbarOrder = 4 };
//     //     var album6 = new Album { Name = "Six", Published = true, Id = 6, NavbarOrder = -1 };
//     //     var albums = new List<Album> { album1, album2, album3, album4, album5, album6 };
//     //     context.AddRange(albums);
//     //     await context.SaveChangesAsync(TestContext.Current.CancellationToken); // Our 'test navbar' is full.
//     //     
//     //     var outcome = (await repository.UpdateNavOrderAsync(6, 0)).Item1;
//     //     Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.NavbarFull, outcome);
//     // }
//     //
//     // [Fact]
//     // public async Task UpdateNavOrderAsync_ExcludePublishedAlbumFromNav_ReturnedTaskResultAlwaysIndicatesSuccess()
//     // {
//     //     using var context = Fixture.CreateContext();
//     //     var repository = new AlbumRepository(context);
//     //     
//     //     var album1 = new Album { Name = "One", Published = true, Id = 1, NavbarOrder = 0 };
//     //     var album2 = new Album { Name = "Two", Published = true, Id = 2, NavbarOrder = 1 };
//     //     var album3 = new Album { Name = "Three", Published = true, Id = 3, NavbarOrder = 2 };
//     //     var album4 = new Album { Name = "Four", Published = true, Id = 4, NavbarOrder = 3 };
//     //     var album5 = new Album { Name = "Five", Published = true, Id = 5, NavbarOrder = 4 };
//     //     var album6 = new Album { Name = "Six", Published = true, Id = 6, NavbarOrder = -1 };
//     //     var albums = new List<Album> { album1, album2, album3, album4, album5, album6 };
//     //     context.AddRange(albums);
//     //     await context.SaveChangesAsync(TestContext.Current.CancellationToken); // Our 'test navbar' is full.
//     //     
//     //     var outcome = (await repository.UpdateNavOrderAsync(6, -1)).Item1;
//     //     Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success, outcome);
//     //     
//     //     outcome = (await repository.UpdateNavOrderAsync(3, -1)).Item1;
//     //     Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success, outcome);
//     // }
//     //
//     // [Fact]
//     // public async Task UpdateNavOrderAsync_SuccessfulExcludeFromNavbarOperations_OrderingPropertiesMaintainedAndReturnsExpected()
//     // {
//     //     using var context = Fixture.CreateContext();
//     //     var repository = new AlbumRepository(context);
//     //
//     //     var album1 = new Album { Name = "One", Published = true, Id = 1, NavbarOrder = -1 };
//     //     var album2 = new Album { Name = "Two", Published = true, Id = 2, NavbarOrder = 0, Description = "descriptive description" };
//     //     var album3 = new Album { Name = "Three", Published = true, Id = 3, NavbarOrder = 2, NavTitle = "3Three3" };
//     //     var album4 = new Album { Name = "Four", Published = true, Id = 4, NavbarOrder = 1, LayoutPreset = PageLayoutPreset.Cozy };
//     //     var album5 = new Album { Name = "Five", Published = true, Id = 5, NavbarOrder = 4, Description = "I am 5", NavTitle = "Navigate to 5", LayoutPreset = PageLayoutPreset.Spooky };
//     //     var album6 = new Album { Name = "Six", Published = true, Id = 6, NavbarOrder = 3 };
//     //     var albums = new List<Album> { album1, album2, album3, album4, album5, album6 };
//     //     context.AddRange(albums);
//     //     await context.SaveChangesAsync(TestContext.Current.CancellationToken);
//     //
//     //     var albumDtosPrior = albums.ToDictionary(album => album.Id, CreateExpectedAlbumToDto);
//     //     context.ChangeTracker.Clear();
//     //
//     //     var update = await repository.UpdateNavOrderAsync(1, -1);
//     //     Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success, update.Item1);
//     //     Assert.Collection(update.Item2,
//     //         albumOfOrder0 => { Assert.Equal(albumDtosPrior[2], albumOfOrder0); },
//     //         albumOfOrder1 => { Assert.Equal(albumDtosPrior[4], albumOfOrder1); },
//     //         albumOfOrder2 => { Assert.Equal(albumDtosPrior[3], albumOfOrder2); },
//     //         albumOfOrder3 => { Assert.Equal(albumDtosPrior[6], albumOfOrder3); },
//     //         albumOfOrder4 => { Assert.Equal(albumDtosPrior[5], albumOfOrder4); }
//     //     );
//     //     
//     //     var albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
//     //     Assert.Equivalent(new List<IAlbumRepository.AlbumDto>
//     //     {
//     //         albumDtosPrior[1],
//     //         albumDtosPrior[2],
//     //         albumDtosPrior[3],
//     //         albumDtosPrior[4],
//     //         albumDtosPrior[5],
//     //         albumDtosPrior[6]
//     //     }, albumDtosPost, true);
//     //
//     //     update = await repository.UpdateNavOrderAsync(2, -1);
//     //     Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success, update.Item1);
//     //     Assert.Collection(update.Item2,
//     //         albumOfOrder0 => { Assert.Equal(albumDtosPrior[4] with { NavbarOrder = 0 }, albumOfOrder0); },
//     //         albumOfOrder1 => { Assert.Equal(albumDtosPrior[3] with { NavbarOrder = 1 }, albumOfOrder1); },
//     //         albumOfOrder2 => { Assert.Equal(albumDtosPrior[6] with { NavbarOrder = 2 }, albumOfOrder2); },
//     //         albumOfOrder3 => { Assert.Equal(albumDtosPrior[5] with { NavbarOrder = 3 }, albumOfOrder3); }
//     //     );
//     //
//     //     albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
//     //     Assert.Equivalent(new List<IAlbumRepository.AlbumDto>
//     //     {
//     //         albumDtosPrior[1],
//     //         albumDtosPrior[2] with { NavbarOrder = -1 },
//     //         albumDtosPrior[3] with { NavbarOrder = 1 },
//     //         albumDtosPrior[4] with { NavbarOrder = 0 },
//     //         albumDtosPrior[5] with { NavbarOrder = 3 },
//     //         albumDtosPrior[6] with { NavbarOrder = 2 }
//     //     }, albumDtosPost, true);
//     //
//     //     update = await repository.UpdateNavOrderAsync(6, -1);
//     //     Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success, update.Item1);
//     //     Assert.Collection(update.Item2,
//     //         albumOfOrder0 => { Assert.Equal(albumDtosPrior[4] with { NavbarOrder = 0 }, albumOfOrder0); },
//     //         albumOfOrder1 => { Assert.Equal(albumDtosPrior[3] with { NavbarOrder = 1 }, albumOfOrder1); },
//     //         albumOfOrder2 => { Assert.Equal(albumDtosPrior[5] with { NavbarOrder = 2 }, albumOfOrder2); }
//     //     );
//     //
//     //     albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
//     //     Assert.Equivalent(new List<IAlbumRepository.AlbumDto>
//     //     {
//     //         albumDtosPrior[1],
//     //         albumDtosPrior[2] with { NavbarOrder = -1 },
//     //         albumDtosPrior[3] with { NavbarOrder = 1 },
//     //         albumDtosPrior[4] with { NavbarOrder = 0 },
//     //         albumDtosPrior[5] with { NavbarOrder = 2 },
//     //         albumDtosPrior[6] with { NavbarOrder = -1 }
//     //     }, albumDtosPost, true);
//     //
//     //     update = await repository.UpdateNavOrderAsync(4, -1);
//     //     Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success, update.Item1);
//     //     Assert.Collection(update.Item2,
//     //         albumOfOrder0 => { Assert.Equal(albumDtosPrior[3] with { NavbarOrder = 0 }, albumOfOrder0); },
//     //         albumOfOrder1 => { Assert.Equal(albumDtosPrior[5] with { NavbarOrder = 1 }, albumOfOrder1); }
//     //     );
//     //
//     //     albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
//     //     Assert.Equivalent(new List<IAlbumRepository.AlbumDto>
//     //     {
//     //         albumDtosPrior[1],
//     //         albumDtosPrior[2] with { NavbarOrder = -1 },
//     //         albumDtosPrior[3] with { NavbarOrder = 0 },
//     //         albumDtosPrior[4] with { NavbarOrder = -1 },
//     //         albumDtosPrior[5] with { NavbarOrder = 1 },
//     //         albumDtosPrior[6] with { NavbarOrder = -1 }
//     //     }, albumDtosPost, true);
//     //
//     //     update = await repository.UpdateNavOrderAsync(5, -1);
//     //     Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success, update.Item1);
//     //     Assert.Collection(update.Item2, albumOfOrder0 => { Assert.Equal(albumDtosPrior[3] with { NavbarOrder = 0 }, albumOfOrder0); });
//     //
//     //     albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
//     //     Assert.Equivalent(new List<IAlbumRepository.AlbumDto>
//     //     {
//     //         albumDtosPrior[1],
//     //         albumDtosPrior[2] with { NavbarOrder = -1 },
//     //         albumDtosPrior[3] with { NavbarOrder = 0 },
//     //         albumDtosPrior[4] with { NavbarOrder = -1 },
//     //         albumDtosPrior[5] with { NavbarOrder = -1 },
//     //         albumDtosPrior[6] with { NavbarOrder = -1 }
//     //     }, albumDtosPost, true);
//     //     
//     //     update = await repository.UpdateNavOrderAsync(3, -1);
//     //     Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success, update.Item1);
//     //     Assert.Empty(update.Item2);
//     //
//     //     albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
//     //     Assert.Equivalent(new List<IAlbumRepository.AlbumDto>
//     //     {
//     //         albumDtosPrior[1],
//     //         albumDtosPrior[2] with { NavbarOrder = -1 },
//     //         albumDtosPrior[3] with { NavbarOrder = -1 },
//     //         albumDtosPrior[4] with { NavbarOrder = -1 },
//     //         albumDtosPrior[5] with { NavbarOrder = -1 },
//     //         albumDtosPrior[6] with { NavbarOrder = -1 }
//     //     }, albumDtosPost, true);
//     // }
//     
//     // [Fact]
//     // public async Task UpdateNavOrderAsync_SuccessWithNegativeNewNavOrder_AlwaysExcludesFromNavbarOnSuccess()
//     // {
//     //     using var context = Fixture.CreateContext();
//     //     var repository = new AlbumRepository(context);
//     //     
//     //     var album1 = new Album { Name = "One", Published = true, Id = 1, NavbarOrder = -1 };
//     //     var album2 = new Album { Name = "Two", Published = true, Id = 2, NavbarOrder = 0 };
//     //     var album3 = new Album { Name = "Three", Published = true, Id = 3, NavbarOrder = 1 };
//     //     var albums = new List<Album> { album1, album2, album3 };
//     //     context.AddRange(albums);
//     //     await context.SaveChangesAsync();
//     //
//     //     var albumDtosPrior = albums.ToDictionary(album => album.Id, CreateExpectedAlbumToDto);
//     //     context.ChangeTracker.Clear();
//     //     
//     //     var update = await repository.UpdateNavOrderAsync(1, -1);
//     //     Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success, update.Item1);
//     //
//     //     var albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
//     //     Assert.Equivalent(new List<IAlbumRepository.AlbumDto>
//     //     {
//     //         albumDtosPrior[1],
//     //         albumDtosPrior[2],
//     //         albumDtosPrior[3]
//     //     }, albumDtosPost, true);
//     //
//     //     update = await repository.UpdateNavOrderAsync(2, -1);
//     //     Assert.Equal(IAlbumRepository.UpdateNavOrderOutcome.Success, update.Item1);
//     //
//     //     albumDtosPost = await GetAlbumStatesFromDatabaseAsync();
//     //     Assert.Equivalent(new List<IAlbumRepository.AlbumDto>
//     //     {
//     //         albumDtosPrior[1],
//     //         albumDtosPrior[2] with { NavbarOrder = -1 },
//     //         albumDtosPrior[3] with { NavbarOrder = 0 }
//     //     }, albumDtosPost, true);
//     // }
//
//     // private async Task<List<IAlbumRepository.AlbumDto>> GetAlbumStatesFromDatabaseAsync()
//     // {
//     //     using var context = Fixture.CreateContext();
//     //     var albums = await context.Albums
//     //         .AsNoTracking()
//     //         .ToListAsync();
//     //     return [.. albums.Select(CreateExpectedAlbumToDto)];
//     // }
//     //
//     // /// <summary>
//     // ///     Test version of AlbumRepository.AlbumToDto(Album album), kept independent/hardcoded:
//     // ///     results in less chance of a dto mapping bug slipping through.
//     // /// </summary>
//     // /// <returns>
//     // ///     An AlbumDto, which represents the expected Album-to-dto mapping utilized in AlbumRepository.
//     // /// </returns>
//     // private static IAlbumRepository.AlbumDto CreateExpectedAlbumToDto(Album album)
//     // {
//     //     return new IAlbumRepository.AlbumDto
//     //     (
//     //         Id: album.Id,
//     //         Name: album.Name,
//     //         Description: album.Description,
//     //         NavTitle: album.NavTitle,
//     //         Published: album.Published,
//     //         LayoutPreset: album.LayoutPreset, NavbarOrder: album.NavbarOrder);
//     // }
//
//
//
//
// }
