using Microsoft.EntityFrameworkCore;
using OdevAPI.Data;
using OdevAPI.DTOs;
using OdevAPI.Entities;
using OdevAPI.Enums;

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

    public async Task<Loan?> CreateAsync(LoanCreateDto loanCreate)
    {
        var user = await _context.Users.FindAsync(loanCreate.UserId);
        if (user is null) return null;
        
        var book = await _context.Books.FindAsync(loanCreate.BookId);
        if (book is null) return null;
        if (book.AvailableCopies < 1) return null;

        book.AvailableCopies -= 1;
        _context.Update(book);

        Loan loan = new()
        {
            Notes = loanCreate.Notes,
            Status = LoanStatus.Active,
            LoanDate = DateTimeOffset.UtcNow,
            DueDate = DateTimeOffset.UtcNow.AddDays(14),
            UserId = loanCreate.UserId,
            BookId = loanCreate.BookId,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Loans.AddAsync(loan);
        await _context.SaveChangesAsync();

        return loan;
    }
}
