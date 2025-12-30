using System.ComponentModel.DataAnnotations.Schema;

namespace OdevAPI.Entities;

public enum LoanStatus
{
    Active,
    Returned,
    Overdue
}

public class Loan
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public string Notes { get; set; }
    
    public LoanStatus Status { get; set; }
    
    public DateTimeOffset LoanDate { get; set; }
    
    public DateTimeOffset DueDate { get; set; }
    
    public DateTimeOffset? ReturnDate { get; set; }
    
    public int UserId { get; set; }
    
    public int BookId { get; set; }
    
    // Audit Fields
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; }
}
