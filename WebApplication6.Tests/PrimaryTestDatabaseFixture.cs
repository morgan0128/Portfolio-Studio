using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebApplication6.Backend.Data;
using WebApplication6.Backend.Models;

namespace WebApplication6.Tests;

/// <summary>
/// Each class fixture owns a separate database, including across simultaneous test processes.
/// Contexts within the class use rollback transactions; disposing the fixture drops its database.
/// </summary>
public class PrimaryTestDatabaseFixture : IDisposable
{
    private readonly string _connectionString = CreateConnectionString();

    public PrimaryTestDatabaseFixture()
    {
        using var context = CreateContextWithoutTransaction();
        try
        {
            context.Database.EnsureCreated();
            SeedDatabase(context);
        }
        catch
        {
            context.Database.EnsureDeleted();
            throw;
        }
    }

    public void Dispose()
    {
        using var context = CreateContextWithoutTransaction();
        context.Database.EnsureDeleted();
    }

    private static string CreateConnectionString()
    {
        var connectionString = Environment.GetEnvironmentVariable("WEBAPPLICATION6_TEST_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=webapplication6test;Username=postgres;Password=morg";
        var builder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            // Keep the configured server and credentials, but never recreate its named database.
            // This ASCII name remains below PostgreSQL's 63-byte identifier limit.
            Database = $"webapplication6test_primary_{Guid.NewGuid():N}"
        };
        return builder.ConnectionString;
    }

    private static void SeedDatabase(ApplicationDbContext context)
    {
        var image1 = new Image
        {
            Id = 1, FileName = "image1", ContentType = "image/png", FileSize = 617238,
            StorageFileName = "a7344381-fe36-4139-be80-5c61cb7fe8af.png",
            Url = "uploads/a7344381-fe36-4139-be80-5c61cb7fe8af.png", AltText = "alt text",
            Height = 771, Width = 646
        };
        var image2 = new Image
        {
            Id = 3, FileName = "image2", ContentType = "image/png", FileSize = 135767,
            StorageFileName = "63277fab-ef45-4e71-9eea-c5d8f9ac571b.png",
            Url = "uploads/a7344381-fe36-4139-be80-5c61cb7fe8af.png", AltText = "alt text",
            Height = 349, Width = 346
        };
        context.Images.AddRange(image1, image2);
        
        var photo1 = new Photo
        {
            Id = 2, CreatedAt = new DateTime(DateOnly.MinValue, TimeOnly.MinValue), Name = "MyPhoto",
            Description = "I am a Photo!", Image = image2, ImageId = image2.Id, YearContentCreated = 2000
        };
        var photo2 = new Photo
        {
            Id = 64, CreatedAt = new DateTime(DateOnly.MaxValue, TimeOnly.MaxValue), Name = "Cat Picture",
            Description = "I am a cat!", Image = image1, ImageId = image1.Id, YearContentCreated = 2026
        };
        var photo3 = new Photo
        {
            Id = 3, CreatedAt = new DateTime(DateOnly.MaxValue, TimeOnly.MaxValue), Name = "Cat Picture?",
            Description = "I am not a cat!", Image = image1, ImageId = image1.Id, YearContentCreated = 2026
        };
        var photo4 = new Photo
        {
            Id = 4, CreatedAt = new DateTime(DateOnly.MaxValue, TimeOnly.MaxValue), Name = "A Picture",
            Description = "I am a clone!", Image = image2, ImageId = image2.Id, YearContentCreated = 2026
        };
        context.Photos.AddRange(photo1, photo2, photo3, photo4);

        var album1 = new Album
        {
            Id = 1, Name = "MyPhotoAlbum", Description = "Here are all of my pictures", 
            NavTitle = "MyPhotoAlbum", Published = true, NavbarOrder = 0
        };
        var album2 = new Album
        {
            Id = 32, Name = "Unnamed", Published = false, NavbarOrder = -1
        };
        var album3 = new Album
        {
            Id = 3, Name = "HelloAlbum", NavTitle = "Hello", Published = true, NavbarOrder = 1, LayoutPreset = PageLayoutPreset.Cozy
        };
        var album4 = new Album
        {
            Id = 2, Name = "HelloAlbum", NavTitle = "Hello", Description = "Hello Album Description", 
            Published = true, NavbarOrder = 2
        };
        var album5 = new Album
        {
            Id = 5, Name = "HelloAlbum5", NavTitle = "Hello5", 
            Published = true, NavbarOrder = -1
        };
        context.Albums.AddRange(album1, album2, album3, album4, album5);
        
        context.SaveChanges();

        album1.AlbumItems.Add(new PhotoDisplay
        {
            Photo = photo1, Order = 1
        });
        album1.AlbumItems.Add(new PhotoDisplay
        {
            Photo = photo2, Order = 4
        });
        album1.AlbumItems.Add(new PhotoDisplay
        {
            Photo = photo3, Order = 2
        });
        album1.AlbumItems.Add(new PhotoDisplay
        {
            Photo = photo4, Order = 0
        });
        context.SaveChanges();
        // For AlbumPhotos of Album where AlbumId==1:
        // Photo.Id // PhotoDisplay.Order
        // 4        // 0
        // 2        // 1
        // 3        // 2
        // 64       // 4
        
        

        // AFTER ALL SEEDED DATA HAS BEEN SAVED
        var reservedEmpties = context.Albums
            .Where(a => a.Id == -7 || a.Id == -1 || a.Id == 7)
            .ToList();
        if (reservedEmpties.Count != 0)
        {
            throw new ApplicationException("-7, -1, and 7 are to be reserved in the seed for 'where Album does not exist' testing.");
        }
        AdvancePostgresIdentitySequence(context);
    }

    public ApplicationDbContext CreateContext()
    {
        var context = CreateContextWithoutTransaction();
        context.Database.BeginTransaction();
        return context;
    }

    private ApplicationDbContext CreateContextWithoutTransaction()
        => new(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(_connectionString)
                .Options);

    private static void AdvancePostgresIdentitySequence(ApplicationDbContext context)
    {
        context.Database.ExecuteSqlRaw("""
           SELECT setval(
               pg_get_serial_sequence('public."Images"', 'Id'),
               (SELECT COALESCE(MAX("Id"), 0) + 1 FROM public."Images"),
               false);
       """);
        context.Database.ExecuteSqlRaw("""
           SELECT setval(
               pg_get_serial_sequence('public."Photos"', 'Id'),
               (SELECT COALESCE(MAX("Id"), 0) + 1 FROM public."Photos"),
               false);
       """);
        context.Database.ExecuteSqlRaw("""
           SELECT setval(
               pg_get_serial_sequence('public."Albums"', 'Id'),
               (SELECT COALESCE(MAX("Id"), 0) + 1 FROM public."Albums"),
               false);
       """);
        context.Database.ExecuteSqlRaw("""
           SELECT setval(
               pg_get_serial_sequence('public."AlbumItems"', 'Id'),
               (SELECT COALESCE(MAX("Id"), 0) + 1 FROM public."AlbumItems"),
               false);
       """);
    }


}
