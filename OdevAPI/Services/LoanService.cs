using Microsoft.EntityFrameworkCore;
using OdevAPI.Data;
using OdevAPI.DTOs;
using OdevAPI.Entities;
using OdevAPI.Enums;
using OdevAPI.Interfaces;

namespace OdevAPI.Services;

public class LoanService(AppDbContext context) : ILoanService
{
    private readonly AppDbContext _context = context;

    public async Task<List<Loan>> GetAllAsync()
    {
        var loans = await _context.Loans.ToListAsync();
        return loans;
    }

    public async Task<Loan> GetByIdAsync(int id)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan is null) throw new KeyNotFoundException("Loan not found");
        return loan;
    }

    public async Task<Loan> CreateAsync(LoanCreateDto loanCreate)
    {
        var user = await _context.Users.FindAsync(loanCreate.UserId);
        if (user is null)
            throw new Exception("User not found");

        var book = await _context.Books.FindAsync(loanCreate.BookId);
        if (book is null)
            throw new Exception("Book not found");
        if (book.AvailableCopies < 1)
            throw new Exception($"No copies available. Available: {book.AvailableCopies}");

        book.AvailableCopies -= 1;
        _context.Books.Update(book);

        var loan = new Loan
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

    public async Task<Loan> UpdateAsync(int id, LoanUpdateDto loanUpdate)
    {
        var existingLoan = await _context.Loans.FindAsync(id);
        if (existingLoan is null)
            throw new Exception("Loan not found");

        existingLoan.Notes = loanUpdate.Notes;
        existingLoan.Status = loanUpdate.Status;
        existingLoan.DueDate = loanUpdate.DueDate;
        existingLoan.UpdatedAt = DateTime.UtcNow;

        // If returned, restore book copy
        if (loanUpdate.Status == LoanStatus.Returned && existingLoan.ReturnDate is null)
        {
            var book = await _context.Books.FindAsync(existingLoan.BookId);
            if (book is not null)
            {
                book.AvailableCopies += 1;
                _context.Books.Update(book);
            }
            existingLoan.ReturnDate = DateTimeOffset.UtcNow;
        }

        _context.Loans.Update(existingLoan);
        await _context.SaveChangesAsync();
        return existingLoan;
    }

    public async Task<Loan> PatchAsync(int id, LoanPatchDto patchDto)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan is null)
            throw new Exception("Loan not found");

        if (patchDto.Notes is not null)
            loan.Notes = patchDto.Notes;
        if (patchDto.DueDate.HasValue)
            loan.DueDate = patchDto.DueDate.Value;
        if (patchDto.Status.HasValue)
        {
            // If returned, restore book copy
            if (patchDto.Status.Value == LoanStatus.Returned && loan.ReturnDate is null)
            {
                var book = await _context.Books.FindAsync(loan.BookId);
                if (book is not null)
                {
                    book.AvailableCopies += 1;
                    _context.Books.Update(book);
                }
                loan.ReturnDate = DateTimeOffset.UtcNow;
            }
            loan.Status = patchDto.Status.Value;
        }

        loan.UpdatedAt = DateTime.UtcNow;
        _context.Loans.Update(loan);
        await _context.SaveChangesAsync();
        return loan;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan is null)
            throw new Exception("Loan not found");

        // If not returned, restore book copy
        if (loan.Status != LoanStatus.Returned)
        {
            var book = await _context.Books.FindAsync(loan.BookId);
            if (book is not null)
            {
                book.AvailableCopies += 1;
                _context.Books.Update(book);
            }
        }

        _context.Loans.Remove(loan);
        await _context.SaveChangesAsync();
        return true;
    }
}
