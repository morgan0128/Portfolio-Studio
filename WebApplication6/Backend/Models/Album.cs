using System.ComponentModel.DataAnnotations;

namespace WebApplication6.Backend.Models;

public class Album
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string? Name { get; set; }
    
    [StringLength(400)]
    public string? Description { get; set; }

    [MaxLength(20)]
    public string NavTitle { get; set; } = "";

    public bool Published { get; set; } = false;

    [Range(-1, 4)]
    public int NavbarOrder { get; set; } = -1;

    public PageLayoutPreset LayoutPreset { get; set; } = PageLayoutPreset.Default;

    public ICollection<AlbumItem> AlbumItems { get; set; } = [];
}
