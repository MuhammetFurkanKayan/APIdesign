using Microsoft.AspNetCore.Mvc;
using OdevAPI.Common;
using OdevAPI.DTOs;
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
    public async Task<ApiResponse<Loan>> Get([FromRoute] int id)
    {
        try
        {
            var loan = await _loanService.GetByIdAsync(id);
            return new ApiResponse<Loan>()
            {
                Success = true,
                Message = "Loan found",
                Data = loan
            };
        }
        catch (Exception e)
        {
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
            var loan = await _loanService.CreateAsync(loanCreate);
            return new ApiResponse<Loan>()
            {
                Success = true,
                Message = "Loan created",
                Data = loan
            };
        }
        catch (Exception e)
        {
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
            var loan = await _loanService.UpdateAsync(id, loanUpdate);
            return new ApiResponse<Loan>()
            {
                Success = true,
                Message = "Loan updated",
                Data = loan
            };
        }
        catch (Exception e)
        {
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
            var loan = await _loanService.PatchAsync(id, patchDto);
            return new ApiResponse<Loan>()
            {
                Success = true,
                Message = "Loan updated (patch)",
                Data = loan
            };
        }
        catch (Exception ex)
        {
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
            var result = await _loanService.DeleteAsync(id);
            return new ApiResponse<bool>()
            {
                Success = true,
                Message = "Loan deleted",
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>()
            {
                Success = false,
                Message = ex.Message,
                Data = false
            };
        }
    }
}
