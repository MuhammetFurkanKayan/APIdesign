using System.ComponentModel.DataAnnotations.Schema;

namespace OdevAPI.Entities;

public class Book
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public required string Title { get; set; }
    
    public required string Author { get; set; }
    
    public required string ISBN { get; set; }
    
    public string Description { get; set; }
    
    public required int TotalCopies { get; set; }
    
    public required int AvailableCopies { get; set; }
    
    public int CategoryId { get; set; }
    
    // Audit Fields
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; }
}
