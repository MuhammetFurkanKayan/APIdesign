using OdevAPI.DTOs;
using OdevAPI.Entities;

namespace OdevAPI.Interfaces;

public interface ILoanService
{
    Task<List<Loan>> GetAllAsync();
    
    Task<Loan> GetByIdAsync(int id);
    
    Task<Loan> CreateAsync(LoanCreateDto loanCreate);
    
    Task<Loan> UpdateAsync(int id, LoanUpdateDto loanUpdate);
    
    Task<Loan> PatchAsync(int id, LoanPatchDto patchDto);
    
    Task<bool> DeleteAsync(int id);
}
