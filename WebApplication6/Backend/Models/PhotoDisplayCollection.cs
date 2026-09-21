namespace WebApplication6.Backend.Models;

public class PhotoDisplayCollection : AlbumItem
{
    public PhotoDisplayMode DisplayMode { get; set; } = PhotoDisplayMode.Static;
    public ICollection<PhotoDisplay> PhotoDisplays { get; set; } = [];

    public enum PhotoDisplayMode
    {
        Static = 0,
        Carousel = 1
    }
}
