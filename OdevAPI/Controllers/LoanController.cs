using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OdevAPI.Common;
using OdevAPI.DTOs;
using OdevAPI.Interfaces;
using Serilog.Context;

namespace OdevAPI.Controllers;

[ApiController]
[Route("/api/v1/loans")]
[Authorize]
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
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Fetching all loans");
        var loans = await _loanService.GetAllAsync();
        _logger.LogInformation("Found {Count} loans", loans.Count);

        return Ok(new ApiResponse<List<LoanResponseDto>>()
        {
            Success = true,
            Message = "Loans listed",
            Data = loans.ToDto()
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute] int id)
    {
        try
        {
            _logger.LogInformation("Searching for loan with ID: {LoanId}", id);
            var loan = await _loanService.GetByIdAsync(id);
            _logger.LogInformation("Loan found with ID: {LoanId}", id);
            return Ok(new ApiResponse<LoanResponseDto>()
            {
                Success = true,
                Message = "Loan found",
                Data = loan.ToDto()
            });
        }
        catch (Exception e)
        {
            using (LogContext.PushProperty("StatusCode", StatusCodes.Status404NotFound))
            {
                _logger.LogError(e, "Loan not found with ID: {LoanId}", id);
            }
            return NotFound(new ApiResponse<LoanResponseDto>()
            {
                Success = false,
                Message = e.Message,
                Data = null
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LoanCreateDto loanCreate)
    {
        try
        {
            _logger.LogInformation("Creating new loan: User={UserId}, Book={BookId}",
                loanCreate.UserId, loanCreate.BookId);
            var loan = await _loanService.CreateAsync(loanCreate);
            _logger.LogInformation("Loan created successfully: LoanId={LoanId}", loan.Id);

            return CreatedAtAction(nameof(Get), new { id = loan.Id }, new ApiResponse<LoanResponseDto>()
            {
                Success = true,
                Message = "Loan created",
                Data = loan.ToDto()
            });
        }
        catch (Exception e)
        {
            using (LogContext.PushProperty("StatusCode", StatusCodes.Status400BadRequest))
            {
                _logger.LogError(e, "Error creating loan: User={UserId}, Book={BookId}",
                    loanCreate.UserId, loanCreate.BookId);
            }
            return BadRequest(new ApiResponse<LoanResponseDto>()
            {
                Success = false,
                Message = e.Message,
                Data = null
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] LoanUpdateDto loanUpdate)
    {
        try
        {
            _logger.LogInformation("Updating loan: LoanId={LoanId}", id);
            var loan = await _loanService.UpdateAsync(id, loanUpdate);
            _logger.LogInformation("Loan updated successfully: LoanId={LoanId}", id);

            return Ok(new ApiResponse<LoanResponseDto>()
            {
                Success = true,
                Message = "Loan updated",
                Data = loan.ToDto()
            });
        }
        catch (Exception e)
        {
            using (LogContext.PushProperty("StatusCode", StatusCodes.Status400BadRequest))
            {
                _logger.LogError(e, "Error updating loan: LoanId={LoanId}", id);
            }
            return BadRequest(new ApiResponse<LoanResponseDto>()
            {
                Success = false,
                Message = e.Message,
                Data = null
            });
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch([FromRoute] int id, [FromBody] LoanPatchDto patchDto)
    {
        try
        {
            _logger.LogInformation("Patching loan: LoanId={LoanId}", id);
            var loan = await _loanService.PatchAsync(id, patchDto);
            _logger.LogInformation("Loan patched successfully: LoanId={LoanId}", id);
            return Ok(new ApiResponse<LoanResponseDto>()
            {
                Success = true,
                Message = "Loan updated (patch)",
                Data = loan.ToDto()
            });
        }
        catch (Exception ex)
        {
            using (LogContext.PushProperty("StatusCode", StatusCodes.Status400BadRequest))
            {
                _logger.LogError(ex, "Error patching loan: LoanId={LoanId}", id);
            }
            return BadRequest(new ApiResponse<LoanResponseDto>()
            {
                Success = false,
                Message = ex.Message,
                Data = null
            });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        try
        {
            _logger.LogInformation("Deleting loan: LoanId={LoanId}", id);
            var result = await _loanService.DeleteAsync(id);
            _logger.LogInformation("Loan deleted successfully: LoanId={LoanId}", id);

            return Ok(new ApiResponse<bool>()
            {
                Success = true,
                Message = "Loan deleted",
                Data = result
            });
        }
        catch (Exception ex)
        {
            using (LogContext.PushProperty("StatusCode", StatusCodes.Status404NotFound))
            {
                _logger.LogError(ex, "Error deleting loan: LoanId={LoanId}", id);
            }
            return NotFound(new ApiResponse<bool>()
            {
                Success = false,
                Message = ex.Message,
                Data = false
            });
        }
    }
}
