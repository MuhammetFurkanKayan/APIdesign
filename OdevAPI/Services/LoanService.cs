using Microsoft.EntityFrameworkCore;
using OdevAPI.Data;
using OdevAPI.DTOs;
using OdevAPI.Entities;
using OdevAPI.Enums;
using OdevAPI.Interfaces;

namespace OdevAPI.Services;

public class LoanService : ILoanService
{
    private readonly AppDbContext _context;
    private readonly ILogger<LoanService> _logger;
    private readonly IEmailService _emailService;

    public LoanService(AppDbContext context, ILogger<LoanService> logger, IEmailService emailService)
    {
        _context = context;
        _logger = logger;
        _emailService = emailService;
    }

    public async Task<List<Loan>> GetAllAsync()
    {
        _logger.LogDebug("GetAllAsync method called");
        var loans = await _context.Loans.ToListAsync();
        _logger.LogDebug("{Count} loans retrieved from database", loans.Count);
        return loans;
    }

    public async Task<Loan> GetByIdAsync(int id)
    {
        _logger.LogDebug("GetByIdAsync method called: LoanId={LoanId}", id);
        var loan = await _context.Loans.FindAsync(id);
        if (loan is null)
        {
            _logger.LogWarning("Loan not found: LoanId={LoanId}", id);
            throw new KeyNotFoundException("Loan not found");
        }
        _logger.LogDebug("Loan found: LoanId={LoanId}", id);
        return loan;
    }

    public async Task<Loan> CreateAsync(LoanCreateDto loanCreate)
    {
        _logger.LogDebug("CreateAsync method called: User={UserId}, Book={BookId}",
            loanCreate.UserId, loanCreate.BookId);

        var user = await _context.Users.FindAsync(loanCreate.UserId);
        if (user is null)
        {
            _logger.LogWarning("User not found: UserId={UserId}", loanCreate.UserId);
            throw new Exception("User not found");
        }

        var book = await _context.Books.FindAsync(loanCreate.BookId);
        if (book is null)
        {
            _logger.LogWarning("Book not found: BookId={BookId}", loanCreate.BookId);
            throw new Exception("Book not found");
        }

        if (book.AvailableCopies < 1)
        {
            _logger.LogWarning("No copies available: BookId={BookId}, Available={Available}",
                loanCreate.BookId, book.AvailableCopies);
            throw new Exception($"No copies available. Available: {book.AvailableCopies}");
        }

        book.AvailableCopies -= 1;
        _context.Books.Update(book);
        _logger.LogDebug("Book copies updated: BookId={BookId}, NewAvailable={NewAvailable}",
            book.Id, book.AvailableCopies);

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

        _logger.LogInformation("Loan created: LoanId={LoanId}, User={UserId}, Book={BookId}",
            loan.Id, loan.UserId, loan.BookId);

        try
        {
            await _emailService.SendLoanConfirmationEmailAsync(loan, user, book);
            _logger.LogInformation("Loan confirmation email sent: LoanId={LoanId}, Email={Email}",
                loan.Id, user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send confirmation email: LoanId={LoanId}", loan.Id);
        }

        return loan;
    }

    public async Task<Loan> UpdateAsync(int id, LoanUpdateDto loanUpdate)
    {
        _logger.LogDebug("UpdateAsync method called: LoanId={LoanId}", id);

        var existingLoan = await _context.Loans.FindAsync(id);
        if (existingLoan is null)
        {
            _logger.LogWarning("Loan not found: LoanId={LoanId}", id);
            throw new Exception("Loan not found");
        }

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
                _logger.LogDebug("Book copy restored: BookId={BookId}, NewAvailable={NewAvailable}",
                    book.Id, book.AvailableCopies);
            }
            existingLoan.ReturnDate = DateTimeOffset.UtcNow;
        }

        _context.Loans.Update(existingLoan);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Loan updated: LoanId={LoanId}", id);
        return existingLoan;
    }

    public async Task<Loan> PatchAsync(int id, LoanPatchDto patchDto)
    {
        _logger.LogDebug("PatchAsync method called: LoanId={LoanId}", id);

        var loan = await _context.Loans.FindAsync(id);
        if (loan is null)
        {
            _logger.LogWarning("Loan not found: LoanId={LoanId}", id);
            throw new Exception("Loan not found");
        }

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
                    _logger.LogDebug("Book copy restored: BookId={BookId}, NewAvailable={NewAvailable}",
                        book.Id, book.AvailableCopies);
                }
                loan.ReturnDate = DateTimeOffset.UtcNow;
            }
            loan.Status = patchDto.Status.Value;
        }

        loan.UpdatedAt = DateTime.UtcNow;
        _context.Loans.Update(loan);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Loan patched: LoanId={LoanId}", id);
        return loan;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogDebug("DeleteAsync method called: LoanId={LoanId}", id);

        var loan = await _context.Loans.FindAsync(id);
        if (loan is null)
        {
            _logger.LogWarning("Loan not found: LoanId={LoanId}", id);
            throw new Exception("Loan not found");
        }

        var user = await _context.Users.FindAsync(loan.UserId);
        if (user is null)
        {
            _logger.LogWarning("User not found: UserId={UserId}", loan.UserId);
            throw new Exception("User not found");
        }

        var book = await _context.Books.FindAsync(loan.BookId);
        if (book is null)
        {
            _logger.LogWarning("Book not found: BookId={BookId}", loan.BookId);
            throw new Exception("Book not found");
        }

        // If not returned, restore book copy
        if (loan.Status != LoanStatus.Returned)
        {
            book.AvailableCopies += 1;
            _context.Books.Update(book);
            _logger.LogDebug("Book copy restored: BookId={BookId}, NewAvailable={NewAvailable}",
                book.Id, book.AvailableCopies);
        }

        _context.Loans.Remove(loan);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Loan deleted: LoanId={LoanId}, BookId={BookId}", id, loan.BookId);

        try
        {
            await _emailService.SendLoanReturnEmailAsync(loan, user, book);
            _logger.LogInformation("Loan return email sent: LoanId={LoanId}, Email={Email}",
                loan.Id, user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send return email: LoanId={LoanId}", loan.Id);
        }

        return true;
    }
}
