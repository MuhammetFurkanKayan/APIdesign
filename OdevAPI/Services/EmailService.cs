using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using OdevAPI.DTOs;
using OdevAPI.Entities;
using OdevAPI.Enums;
using OdevAPI.Interfaces;

namespace OdevAPI.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly string _templatePath;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger, IWebHostEnvironment environment)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
        _environment = environment;
        _templatePath = Path.Combine(_environment.ContentRootPath, "Templates", "LoanConfirmationTemplate.html");
    }

    private string GetTemplatePath(string templateName)
    {
        return Path.Combine(_environment.ContentRootPath, "Templates", templateName);
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            using var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
            {
                EnableSsl = _emailSettings.EnableSsl,
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            mailMessage.To.Add(to);

            await smtpClient.SendMailAsync(mailMessage);
            
            _logger.LogInformation("Email successfully sent to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }

    public async Task SendLoanConfirmationEmailAsync(Loan loan, User user, Book book)
    {
        var subject = $"✅ Kitap Ödünç Onayı - #{loan.Id}";
        
        if (!File.Exists(_templatePath))
        {
            _logger.LogError("Email template not found at: {TemplatePath}", _templatePath);
            throw new FileNotFoundException("Email template not found", _templatePath);
        }

        var templateContent = await File.ReadAllTextAsync(_templatePath);
        
        var statusText = loan.Status switch
        {
            LoanStatus.Active => "Aktif",
            LoanStatus.Returned => "İade Edildi",
            LoanStatus.Overdue => "Gecikmiş",
            _ => "Bilinmiyor"
        };
        
        var notesRow = string.IsNullOrEmpty(loan.Notes) 
            ? "" 
            : $@"
                                            <tr>
                                                <td style='color: #6c757d; font-size: 14px;'><strong>Notlar:</strong></td>
                                                <td style='color: #212529; font-size: 14px;'>{loan.Notes}</td>
                                            </tr>";
        
        var body = templateContent
            .Replace("{{CustomerName}}", $"{user.Name} {user.LastName}")
            .Replace("{{CustomerEmail}}", user.Email)
            .Replace("{{CustomerAddress}}", user.Address)
            .Replace("{{LoanId}}", loan.Id.ToString())
            .Replace("{{LoanDate}}", loan.LoanDate.ToString("dd MMMM yyyy, HH:mm"))
            .Replace("{{DueDate}}", loan.DueDate.ToString("dd MMMM yyyy, HH:mm"))
            .Replace("{{LoanNotes}}", notesRow)
            .Replace("{{BookTitle}}", book.Title)
            .Replace("{{BookAuthor}}", book.Author)
            .Replace("{{BookDescription}}", book.Description ?? "")
            .Replace("{{Status}}", statusText);

        await SendEmailAsync(user.Email, subject, body, true);
    }

    public async Task SendLoanReturnEmailAsync(Loan loan, User user, Book book)
    {
        var subject = $"📚 Kitap İade Onayı - #{loan.Id}";
        
        var templatePath = GetTemplatePath("LoanReturnTemplate.html");
        
        if (!File.Exists(templatePath))
        {
            _logger.LogError("Email template not found at: {TemplatePath}", templatePath);
            throw new FileNotFoundException("Email template not found", templatePath);
        }

        var templateContent = await File.ReadAllTextAsync(templatePath);
        
        var notesRow = string.IsNullOrEmpty(loan.Notes) 
            ? "" 
            : $@"
                                            <tr>
                                                <td style='color: #6c757d; font-size: 14px;'><strong>Notlar:</strong></td>
                                                <td style='color: #212529; font-size: 14px;'>{loan.Notes}</td>
                                            </tr>";
        
        var body = templateContent
            .Replace("{{CustomerName}}", $"{user.Name} {user.LastName}")
            .Replace("{{CustomerEmail}}", user.Email)
            .Replace("{{CustomerAddress}}", user.Address)
            .Replace("{{LoanId}}", loan.Id.ToString())
            .Replace("{{LoanDate}}", loan.LoanDate.ToString("dd MMMM yyyy, HH:mm"))
            .Replace("{{DueDate}}", loan.DueDate.ToString("dd MMMM yyyy, HH:mm"))
            .Replace("{{ReturnDate}}", (loan.ReturnDate ?? DateTimeOffset.UtcNow).ToString("dd MMMM yyyy, HH:mm"))
            .Replace("{{LoanNotes}}", notesRow)
            .Replace("{{BookTitle}}", book.Title)
            .Replace("{{BookAuthor}}", book.Author)
            .Replace("{{BookDescription}}", book.Description ?? "");

        await SendEmailAsync(user.Email, subject, body, true);
    }
}
