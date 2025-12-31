using Microsoft.AspNetCore.Mvc;
using OdevAPI.Common;
using OdevAPI.Entities;
using OdevAPI.Services;

namespace OdevAPI.Controllers;

[ApiController]
[Route("/api/v1/loans")]
public class LoanController : Controller
{
    private readonly LoanService _loanService;

    public LoanController(LoanService loanService)
    {
        _loanService = loanService;
    }

    [HttpGet]
    public async Task<ApiResponse<List<Loan>>> GetAll()
    {
        var loans = await _loanService.GetAllAsync();
        return new ApiResponse<List<Loan>>()
        {
            Success = true,
            Message = "Loans listed",
            Data = loans
        };
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<Loan>> Get(int id)
    {
        var loan = await _loanService.GetByIdAsync(id);
        return new ApiResponse<Loan>()
        {
            Success = true,
            Message = "Loan found",
            Data = loan
        };
    }
}
