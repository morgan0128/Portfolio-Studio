using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using WebApplication6.Backend.Data;
using WebApplication6.Backend.Models;
using WebApplication6.Backend.Repositories;
using WebApplication6.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters
            .Add(new JsonStringEnumConverter<PageLayoutPreset>(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
    });

builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<IPhotoRepository, PhotoRepository>();
builder.Services.AddScoped<IAlbumRepository, AlbumRepository>();
builder.Services.AddScoped<IPortfolioPageRepository, PortfolioPageRepository>();
builder.Services.AddScoped<IUntrackedFileRepository, UntrackedFileRepository>();
builder.Services.AddScoped<IFileHostingService, LocalFileHostingService>();
builder.Services.AddScoped<IUploadPhotoService, UploadPhotoService>();
builder.Services.AddScoped<IAlbumItemRepository, AlbumItemRepository>();

var configuredUploadDirectory =
    builder.Configuration["LocalFileStorage:UploadDirectory"]
    ?? throw new InvalidOperationException("LocalFileStorage:UploadDirectory is not configured.");

var uploadRoot = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, configuredUploadDirectory));

Directory.CreateDirectory(uploadRoot);

builder.Services.AddSingleton(uploadRoot);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
     FileProvider = new PhysicalFileProvider(uploadRoot),
     RequestPath = "/uploads"
});

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// app.Environment.ContentRootPath = "ClientApp/public/";

app.MapFallbackToFile("app/{*path:nonfile}", "app/index.html");

app.Run();

namespace WebApplication6.Backend
{
}