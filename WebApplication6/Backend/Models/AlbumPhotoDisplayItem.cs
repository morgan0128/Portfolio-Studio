namespace WebApplication6.Backend.Models;

public class AlbumPhotoDisplayItem : AlbumItem
{
    public PhotoDisplayMode DisplayMode { get; set; } = PhotoDisplayMode.Static;

    public ICollection<AlbumPhoto> AlbumPhotos { get; set; } = [];

    public enum PhotoDisplayMode
    {
        Static = 0,
        Carousel = 1
    }
}
