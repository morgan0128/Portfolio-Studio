using System.ComponentModel.DataAnnotations;

namespace WebApplication6.Backend.Models;

public abstract class AlbumItem
{
    [Key]
    public int Id { get; set; }

    public int AlbumId { get; set; }
    public Album Album { get; set; } = null!;

    public int Order { get; set; }

}
