namespace WebApplication6.Backend.Models;

public class PhotoDisplay : AlbumItem
{
    public int PhotoId { get; set; }
    public Photo Photo { get; set; } = null!;

    public int? PhotoDisplayCollectionId { get; set; }
    public PhotoDisplayCollection? PhotoDisplayCollection { get; set; }

    public bool DisplaysName { get; set; } = true;
    public bool DisplaysDescription { get; set; } = true;
    public bool DisplaysYearContentCreated { get; set; } = true;
}
