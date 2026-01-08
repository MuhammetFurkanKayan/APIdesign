using OdevAPI.DTOs;
using OdevAPI.Entities;

namespace OdevAPI.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    Task<User?> RegisterAsync(RegisterRequestDto request);
    string GenerateToken(User user);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
