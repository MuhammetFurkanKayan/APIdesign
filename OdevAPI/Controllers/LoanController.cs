using Microsoft.AspNetCore.Mvc;
using OdevAPI.Common;
using OdevAPI.DTOs;
using OdevAPI.Entities;
using OdevAPI.Interfaces;

namespace OdevAPI.Controllers;

[ApiController]
[Route("/api/v1/loans")]
public class LoanController : Controller
{
    private readonly ILoanService _loanService;
    private readonly ILogger<LoanController> _logger;

    public LoanController(ILoanService loanService, ILogger<LoanController> logger)
    {
        _loanService = loanService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ApiResponse<List<Loan>>> GetAll()
    {
        _logger.LogInformation("Fetching all loans");
        var loans = await _loanService.GetAllAsync();
        _logger.LogInformation("Found {Count} loans", loans.Count);
        return new ApiResponse<List<Loan>>()
        {
            Success = true,
            Message = "Loans listed",
            Data = loans
        };
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<Loan>> Get([FromRoute] int id)
    {
        try
        {
            _logger.LogInformation("Searching for loan with ID: {LoanId}", id);
            var loan = await _loanService.GetByIdAsync(id);
            _logger.LogInformation("Loan found with ID: {LoanId}", id);
            return new ApiResponse<Loan>()
            {
                Success = true,
                Message = "Loan found",
                Data = loan
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Loan not found with ID: {LoanId}", id);
            return new ApiResponse<Loan>()
            {
                Success = false,
                Message = e.Message,
                Data = null
            };
        }
    }

    [HttpPost]
    public async Task<ApiResponse<Loan>> Create([FromBody] LoanCreateDto loanCreate)
    {
        try
        {
            _logger.LogInformation("Creating new loan: User={UserId}, Book={BookId}", 
                loanCreate.UserId, loanCreate.BookId);
            var loan = await _loanService.CreateAsync(loanCreate);
            _logger.LogInformation("Loan created successfully: LoanId={LoanId}", loan.Id);
            return new ApiResponse<Loan>()
            {
                Success = true,
                Message = "Loan created",
                Data = loan
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating loan: User={UserId}, Book={BookId}", 
                loanCreate.UserId, loanCreate.BookId);
            return new ApiResponse<Loan>()
            {
                Success = false,
                Message = e.Message,
                Data = null
            };
        }
    }

    [HttpPut("{id}")]
    public async Task<ApiResponse<Loan>> Update([FromRoute] int id, [FromBody] LoanUpdateDto loanUpdate)
    {
        try
        {
            _logger.LogInformation("Updating loan: LoanId={LoanId}", id);
            var loan = await _loanService.UpdateAsync(id, loanUpdate);
            _logger.LogInformation("Loan updated successfully: LoanId={LoanId}", id);
            return new ApiResponse<Loan>()
            {
                Success = true,
                Message = "Loan updated",
                Data = loan
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating loan: LoanId={LoanId}", id);
            return new ApiResponse<Loan>()
            {
                Success = false,
                Message = e.Message,
                Data = null
            };
        }
    }

    [HttpPatch("{id}")]
    public async Task<ApiResponse<Loan>> Patch([FromRoute] int id, [FromBody] LoanPatchDto patchDto)
    {
        try
        {
            _logger.LogInformation("Patching loan: LoanId={LoanId}", id);
            var loan = await _loanService.PatchAsync(id, patchDto);
            _logger.LogInformation("Loan patched successfully: LoanId={LoanId}", id);
            return new ApiResponse<Loan>()
            {
                Success = true,
                Message = "Loan updated (patch)",
                Data = loan
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error patching loan: LoanId={LoanId}", id);
            return new ApiResponse<Loan>()
            {
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    [HttpDelete("{id}")]
    public async Task<ApiResponse<bool>> Delete([FromRoute] int id)
    {
        try
        {
            _logger.LogInformation("Deleting loan: LoanId={LoanId}", id);
            var result = await _loanService.DeleteAsync(id);
            _logger.LogInformation("Loan deleted successfully: LoanId={LoanId}", id);
            return new ApiResponse<bool>()
            {
                Success = true,
                Message = "Loan deleted",
                Data = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting loan: LoanId={LoanId}", id);
            return new ApiResponse<bool>()
            {
                Success = false,
                Message = ex.Message,
                Data = false
            };
        }
    }
}
