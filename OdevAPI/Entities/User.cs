using System.ComponentModel.DataAnnotations.Schema;

namespace OdevAPI.Entities;

public class User
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public string LastName { get; set; }
    
    public string FullName => $"{Name} {LastName}";
    
    public string Email { get; set; }
    
    public string Phone { get; set; }
    
    public string Address { get; set; }
    
    // Audit Fields
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; }
}
