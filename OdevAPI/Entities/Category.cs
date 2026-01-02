using System.ComponentModel.DataAnnotations.Schema;

namespace OdevAPI.Entities;

public class Category
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public required string Name { get; set; }
    
    public string Description { get; set; }
    
    // Audit Fields
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; }
}
