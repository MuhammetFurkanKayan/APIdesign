using OdevAPI.Entities;

namespace OdevAPI.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task SendLoanConfirmationEmailAsync(Loan loan, User user, Book book);
    Task SendLoanReturnEmailAsync(Loan loan, User user, Book book);
}
