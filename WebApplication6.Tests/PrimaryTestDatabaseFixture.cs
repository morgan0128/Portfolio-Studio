using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WebApplication6.Backend.Data;
using WebApplication6.Backend.Models;

namespace WebApplication6.Tests;

public class PrimaryTestDatabaseFixture
{
    private static readonly string ConnectionString =
        Environment.GetEnvironmentVariable("WEBAPPLICATION6_TEST_CONNECTION_STRING")
        ?? "Host=localhost;Port=5432;Database=webapplication6test;Username=postgres;Password=morg";

    private static readonly object _lock = new();
    private static bool _databaseInitialized;

    public PrimaryTestDatabaseFixture()
    {
        lock (_lock)
        {
            if (!_databaseInitialized)
            {
                using (var context = CreateContextWithoutTransaction())
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();

                    var image1 = new Image
                    {
                        Id = 1, FileName = "image1", ContentType = "image/png", FileSize = 617238,
                        StorageFileName = "a7344381-fe36-4139-be80-5c61cb7fe8af.png",
                        Url = "uploads/a7344381-fe36-4139-be80-5c61cb7fe8af.png", AltText = "alt text",
                        Height = 771, Width = 646
                    };
                    context.Add(image1);
                    context.SaveChanges();
                    
                    var image2 = new Image
                    {
                        Id = 3, FileName = "image2", ContentType = "image/png", FileSize = 135767,
                        StorageFileName = "63277fab-ef45-4e71-9eea-c5d8f9ac571b.png",
                        Url = "uploads/a7344381-fe36-4139-be80-5c61cb7fe8af.png", AltText = "alt text",
                        Height = 349, Width = 346
                    };
                    context.Add(image2);
                    context.SaveChanges();

                    var photo1 = new Photo
                    {
                        Id = 2, CreatedAt = new DateTime(DateOnly.MinValue, TimeOnly.MinValue), Name = "MyPhoto",
                        Description = "I am a Photo!", Image = image2, ImageId = image2.Id, YearContentCreated = 2000
                    };
                    context.Add(photo1);
                    context.SaveChanges();
                    
                    var photo2 = new Photo
                    {
                        Id = 64, CreatedAt = new DateTime(DateOnly.MaxValue, TimeOnly.MaxValue), Name = "Cat Picture",
                        Description = "I am a cat!", Image = image1, ImageId = image1.Id, YearContentCreated = 2026
                    };
                    context.Add(photo2);
                    context.SaveChanges();
                    
                    var photo3 = new Photo
                    {
                        Id = 3, CreatedAt = new DateTime(DateOnly.MaxValue, TimeOnly.MaxValue), Name = "Cat Picture?",
                        Description = "I am not a cat!", Image = image1, ImageId = image1.Id, YearContentCreated = 2026
                    };
                    context.Add(photo3);
                    context.SaveChanges();
                    
                    var photo4 = new Photo
                    {
                        Id = 4, CreatedAt = new DateTime(DateOnly.MaxValue, TimeOnly.MaxValue), Name = "A Picture",
                        Description = "I am a clone!", Image = image2, ImageId = image2.Id, YearContentCreated = 2026
                    };
                    context.Add(photo4);
                    context.SaveChanges();

                    var album1 = new Album
                    {
                        Id = 1, Name = "MyPhotoAlbum", Description = "Here are all of my pictures", 
                        NavTitle = "MyPhotoAlbum", Published = true, NavbarOrder = 0
                    };
                    context.Add(album1);
                    context.SaveChanges();
                    
                    var album2 = new Album
                    {
                        Id = 32, Name = "Unnamed", Published = false, NavbarOrder = -1
                    };
                    context.Add(album2);
                    context.SaveChanges();
                    
                    var album3 = new Album
                    {
                        Id = 3, Name = "HelloAlbum", NavTitle = "Hello", Published = true, NavbarOrder = 1, LayoutPreset = PageLayoutPreset.Cozy
                    };
                    context.Add(album3);
                    context.SaveChanges();
                    
                    var album4 = new Album
                    {
                        Id = 2, Name = "HelloAlbum", NavTitle = "Hello", Description = "Hello Album Description", 
                        Published = true, NavbarOrder = 2
                    };
                    context.Add(album4);
                    context.SaveChanges();
                    
                    var album5 = new Album
                    {
                        Id = 5, Name = "HelloAlbum5", NavTitle = "Hello5", 
                        Published = true, NavbarOrder = -1
                    };
                    context.Add(album5);
                    context.SaveChanges();
                    

                    album1.AlbumItems.Add(new PhotoDisplay
                    {
                        Photo = photo1
                    });
                    context.SaveChanges();
                    context.PhotoDisplays.Single(ap => ap.AlbumId == 1 && ap.PhotoId == 2).Order = 1;
                    context.SaveChanges();
                    // photo1.Id == 2, Order in Album where Id==1 is 1

                    album1.AlbumItems.Add(new PhotoDisplay
                    {
                        Photo = photo2
                    });
                    context.SaveChanges();
                    context.PhotoDisplays.Single(ap => ap.AlbumId == 1 && ap.PhotoId == photo2.Id).Order = 4;
                    context.SaveChanges();
                    // photo2.Id == 64, Order in Album where Id==1 is 4
                    
                    album1.AlbumItems.Add(new PhotoDisplay
                    {
                        Photo = photo3
                    });
                    context.SaveChanges();
                    context.PhotoDisplays.Single(ap => ap.AlbumId == 1 && ap.PhotoId == photo3.Id).Order = 2;
                    context.SaveChanges();
                    // photo3.Id == 3, Order in Album where Id==1 is 2
                    

                    album1.AlbumItems.Add(new PhotoDisplay
                    {
                        Photo = photo4
                    });
                    context.SaveChanges();
                    // photo4.Id == 4, Order in Album where Id==1 is 0
                    
                    // For AlbumPhotos of Album where AlbumId==1:
                    // Photo.Id // PhotoDisplay.Order
                    // 4        // 0
                    // 2        // 1
                    // 3        // 2
                    // 64       // 4

                    
                    

                    // AFTER ALL SEEDED DATA HAS BEEN SAVED
                    AdvancePostgresIdentitySequence(context);
                }

                _databaseInitialized = true;
            }
        }
    }

    public ApplicationDbContext CreateContext()
    {
        var context = CreateContextWithoutTransaction();
        context.Database.BeginTransaction();
        return context;
    }

    private static ApplicationDbContext CreateContextWithoutTransaction()
        => new(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(ConnectionString)
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
