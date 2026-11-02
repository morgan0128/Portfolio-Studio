using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebApplication6.Backend.Data;
using WebApplication6.Backend.Models;

namespace WebApplication6.Tests;

public class TransactionalTestDatabaseFixture : IDisposable
{
    private readonly string _connectionString = CreateConnectionString();

    public TransactionalTestDatabaseFixture()
    {
        using var context = CreateContext();
        try
        {
            context.Database.EnsureCreated();
        }
        catch
        {
            context.Database.EnsureDeleted();
            throw;
        }
    }

    public void Dispose()
    {
        using var context = CreateContext();
        context.Database.EnsureDeleted();
    }

    private static string CreateConnectionString()
    {
        var connectionString = Environment.GetEnvironmentVariable("WEBAPPLICATION6_TEST_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=webapplication6transactionaltest;Username=postgres;Password=morg";
        var builder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            // Each class fixture owns its database, even across simultaneous test processes.
            Database = $"webapplication6test_tx_{Guid.NewGuid():N}"
        };
        return builder.ConnectionString;
    }

    public void Cleanup()
    {
        using var context = CreateContext();

        context.UntrackedFiles.RemoveRange(context.UntrackedFiles);
        context.AlbumItems.RemoveRange(context.AlbumItems);
        context.Albums.RemoveRange(context.Albums);
        context.Photos.RemoveRange(context.Photos);
        context.Images.RemoveRange(context.Images);

        context.SaveChanges();
    }

    public ApplicationDbContext CreateContext()
        => new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(_connectionString)
                .Options);
}
