using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication6.Backend.Models;

public class Photo
{
    [Key]
    public int Id { get; set; }
 

    public int ImageId { get; set; }
    
    [ForeignKey("ImageId")]
    [Required]
    public Image Image { get; set; }
    
    [MaxLength(100)]
    public string? Name { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public DateTime? CreatedAt { get; set; }
    
    [Range(1900, 2100)]
    public int? YearContentCreated { get; set; }

    public ICollection<PhotoDisplay> PhotoDisplays { get; set; } = [];

}