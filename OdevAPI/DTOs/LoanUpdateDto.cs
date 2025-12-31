using OdevAPI.Enums;

namespace OdevAPI.DTOs;

public class LoanUpdateDto
{
    public string Notes { get; set; }
    
    public LoanStatus Status { get; set; }
    
    public DateTimeOffset DueDate { get; set; }
}
