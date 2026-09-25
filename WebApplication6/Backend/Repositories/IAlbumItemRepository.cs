using System.Text.Json.Serialization;
using WebApplication6.Backend.Models;

namespace WebApplication6.Backend.Repositories;

public interface IAlbumItemRepository
{
    // Task<List<PhotoDisplay>> FetchPhotoDisplaysByIds(int albumId, List<int> photoDisplayIds);
    
    Task<PhotoDisplayCollectionAlbumItemDto?> CreateCarouselPhotoDisplayCollection(
        int albumId, IReadOnlyList<int> photoDisplayIds, CancellationToken cancellationToken = default);
    
    // Task<IEnumerable<PhotoDisplayDto>> GetAlbumPhotoDisplays(int albumId);

    Task<IEnumerable<AlbumItemDto>> GetAlbumItems(int albumId);
    
    Task<bool> AddPhotoToAlbumAsync(int albumId, int photoId, CancellationToken cancellationToken = default);
    
    // Task<bool> ReorderPhotoInAlbum(int albumId, int photoId, int newOrder);
    
    Task<bool> ReorderAlbumItem(int albumId, int itemId, int newOrder);

    Task<bool> ReorderPhotoDisplayInCollection(int albumId, int photoDisplayCollectionId, int photoDisplayId, int newOrder);
    
    Task<bool> ModifyFieldsDisplayed(int albumId, int photoId,
        IAlbumItemRepository.PhotoDisplayFieldsDisplayedRequest request);
    
    // Task<bool> ToggleDisplaysName(int albumId, int photoId);
    
    // Task<bool> ToggleDisplaysDescription(int albumId, int photoId);
    
    // Task<bool> ToggleDisplaysYearContentCreated(int albumId, int photoId);
    // public sealed record ImageDto(
    //     int Id,
    //     string FileName,
    //     string ContentType,
    //     long? FileSize,
    //     string StorageFileName,
    //     string Url,
    //     string AltText,
    //     int Width,
    //     int Height);
    // public sealed record PhotoDto(
    //     int Id,
    //     ImageDto Image,
    //     string Name,
    //     string Description,
    //     DateTime? CreatedAt,
    //     int? YearContentCreated
    //     );
    // public sealed record PhotoDisplayDto(
    //     int Id,
    //     int AlbumId,
    //     int Order,
    //     PhotoDto Photo,
    //     bool DisplaysName = true,
    //     bool DisplaysDescription = true,
    //     bool DisplaysYearContentCreated = true
    // );

    // public sealed record PhotoDisplayCollectionDto(
    //     
    // );

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(PhotoDisplayAlbumItemDto), "photoDisplay")]
    [JsonDerivedType(typeof(PhotoDisplayCollectionAlbumItemDto), "photoDisplayCollection")]
    public abstract record AlbumItemDto(int Id, int Order);

    public sealed record PhotoDisplayAlbumItemDto(
        int Id,
        int Order,
        PhotoDisplayDto Content
    ) : AlbumItemDto(Id, Order);

    public sealed record PhotoDisplayCollectionAlbumItemDto(
        int Id,
        int Order,
        PhotoDisplayCollectionDto Content
    ) : AlbumItemDto(Id, Order);

    public sealed record PhotoDisplayCollectionDto(
        PhotoDisplayCollection.PhotoDisplayMode DisplayMode,
        IReadOnlyList<PhotoDisplayAlbumItemDto> PhotoDisplays
    );

    public sealed record PhotoDto(
        int Id,
        ImageDto Image,
        string Name,
        string Description,
        int? YearContentCreated
    );

    public sealed record ImageDto(
        int Id,
        string FileName,
        string ContentType,
        long? FileSize,
        string StorageFileName,
        string Url,
        string AltText,
        int Width,
        int Height
    );
    
    
    public sealed record PhotoDisplayDto(
        PhotoDto Photo,
        int? PhotoDisplayCollectionId,
        bool DisplaysName,
        bool DisplaysDescription,
        bool DisplaysYearContentCreated
    );
    
    public sealed record PhotoDisplayFieldsDisplayedRequest(
        bool? DisplaysName,
        bool? DisplaysDescription,
        bool? DisplaysYearContentCreated);

}
