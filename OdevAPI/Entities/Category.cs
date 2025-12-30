using System.ComponentModel.DataAnnotations.Schema;

namespace OdevAPI.Entities;

public class Category
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public required string Name { get; set; }
    
    public string Description { get; set; }
}
