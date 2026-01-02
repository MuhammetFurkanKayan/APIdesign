using OdevAPI.Enums;

namespace OdevAPI.DTOs;

public class LoanUpdateDto
{
    public required string Notes { get; set; }
    
    public required LoanStatus Status { get; set; }
    
    public required DateTimeOffset DueDate { get; set; }
}
