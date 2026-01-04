using Microsoft.AspNetCore.Mvc;
using OdevAPI.Common;
using OdevAPI.DTOs;
using OdevAPI.Interfaces;

namespace OdevAPI.Controllers;

[ApiController]
[Route("/api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        _logger.LogInformation("Login attempt: {Username}", request.Username);

        var result = await _authService.LoginAsync(request);

        if (result == null)
        {
            return Unauthorized(new ApiResponse<LoginResponseDto>
            {
                Success = false,
                Message = "Invalid username or password",
                Data = null
            });
        }

        return Ok(new ApiResponse<LoginResponseDto>
        {
            Success = true,
            Message = "Login successful",
            Data = result
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        _logger.LogInformation("Registration attempt: {Username}", request.Username);

        var user = await _authService.RegisterAsync(request);

        if (user == null)
        {
            return Conflict(new ApiResponse<UserResponseDto>
            {
                Success = false,
                Message = "User already exists with this username or email",
                Data = null
            });
        }

        return Created($"/users/{user.Id}", new ApiResponse<UserResponseDto>
        {
            Success = true,
            Message = "Registration successful",
            Data = user.ToDto()
        });
    }
}
