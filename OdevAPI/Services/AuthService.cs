using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OdevAPI.Data;
using OdevAPI.DTOs;
using OdevAPI.Entities;
using OdevAPI.Interfaces;

namespace OdevAPI.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AppDbContext context, IOptions<JwtSettings> jwtSettings, ILogger<AuthService> logger)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username && !u.IsDeleted);

        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
        {
            _logger.LogWarning("Login failed: User not found - {Username}", request.Username);
            return null;
        }

        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: Invalid password - {Username}", request.Username);
            return null;
        }

        var token = GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        _logger.LogInformation("User logged in successfully: {Username}", request.Username);

        return new LoginResponseDto
        {
            Token = token,
            Username = user.Username!,
            Role = user.Role,
            ExpiresAt = expiresAt
        };
    }

    public async Task<User?> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);

        if (existingUser != null)
        {
            _logger.LogWarning("Registration failed: User already exists - {Username}", request.Username);
            return null;
        }

        var user = new User
        {
            Name = request.Name,
            LastName = request.LastName,
            Email = request.Email,
            Username = request.Username,
            PasswordHash = HashPassword(request.Password),
            Role = "User",
            Phone = request.Phone,
            Address = request.Address,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User registered successfully: {Username}", request.Username);

        return user;
    }

    public string GenerateToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username ?? user.Email),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("FullName", user.FullName)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var salt = Guid.NewGuid().ToString();
        var saltedPassword = password + salt;
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
        return Convert.ToBase64String(hash) + ":" + salt;
    }

    public bool VerifyPassword(string password, string hash)
    {
        var parts = hash.Split(':');
        if (parts.Length != 2) return false;

        var storedHash = parts[0];
        var salt = parts[1];

        using var sha256 = SHA256.Create();
        var saltedPassword = password + salt;
        var computedHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
        var computedHashString = Convert.ToBase64String(computedHash);

        return storedHash == computedHashString;
    }
}
