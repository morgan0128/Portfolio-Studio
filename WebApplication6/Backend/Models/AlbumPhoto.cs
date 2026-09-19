using Microsoft.EntityFrameworkCore;

namespace WebApplication6.Backend.Models;

[PrimaryKey(nameof(AlbumId), nameof(PhotoId))]
public class AlbumPhoto
{
    public int AlbumId { get; set; }
    public int PhotoId { get; set; }
    public int AlbumPhotoDisplayItemId { get; set; }

    public Album Album { get; set; } = null!;
    public Photo Photo { get; set; } = null!;
    public AlbumPhotoDisplayItem AlbumPhotoDisplayItem { get; set; } = null!;

    public int Order { get; set; }

    public bool DisplaysName { get; set; } = true;
    public bool DisplaysDescription { get; set; } = true;
    public bool DisplaysYearContentCreated { get; set; } = true;
}