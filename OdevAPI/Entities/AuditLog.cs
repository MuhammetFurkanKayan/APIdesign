using System.ComponentModel.DataAnnotations.Schema;

namespace OdevAPI.Entities;

public class AuditLog
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public string TableName { get; set; }
    
    public string Action { get; set; } // INSERT, UPDATE, DELETE
    
    public string EntityId { get; set; }
    
    public string? OldValues { get; set; }
    
    public string? NewValues { get; set; }
    
    public string? ChangedBy { get; set; }
    
    public DateTimeOffset ChangedAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; }
}
