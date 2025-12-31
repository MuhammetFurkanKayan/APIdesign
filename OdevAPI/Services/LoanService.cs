using Microsoft.EntityFrameworkCore;
using OdevAPI.Data;
using OdevAPI.Entities;

namespace OdevAPI.Services;

public class LoanService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<List<Loan>> GetAllAsync()
    {
        var loans = await _context.Loans.ToListAsync();
        return loans;
    }

    public async Task<Loan?> GetByIdAsync(int id)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan is null) return null;
        return loan;
    }
}
