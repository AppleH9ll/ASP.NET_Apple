using AirShop.Web.DTOs;
using AirShop.Web.Models;
using AirShop.Web.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace AirShop.Web.Services;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto dto);
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto dto);
}

public class AuthService : IAuthService
{
    private readonly IGenericRepository<User> _userRepo;
    private readonly IGenericRepository<Role> _roleRepo;
    private readonly IConfiguration _config;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthService(IGenericRepository<User> userRepo, IGenericRepository<Role> roleRepo, IConfiguration config)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _config = config;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto dto)
    {
        if (await _userRepo.Query().AnyAsync(u => u.Login == dto.Login))
            return Result<AuthResponseDto>.Failure("Пользователь с таким логином уже существует");

        var defaultRole = await _roleRepo.Query().FirstOrDefaultAsync(r => r.Name == "customer");
        if (defaultRole == null) return Result<AuthResponseDto>.Failure("Роль по умолчанию не найдена");

        var user = new User
        {
            Login = dto.Login,
            FullName = dto.FullName,
            RoleId = defaultRole.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Хеширование пароля через Microsoft Identity
        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();

        return Result<AuthResponseDto>.Success(GenerateJwtToken(user, defaultRole.Name));
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto dto)
    {
        var user = await _userRepo.Query().Include(u => u.Role).FirstOrDefaultAsync(u => u.Login == dto.Login);

        if (user == null || !user.IsActive)
            return Result<AuthResponseDto>.Failure("Неверный логин или пароль (или аккаунт заблокирован)");

        // Проверка пароля
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
            return Result<AuthResponseDto>.Failure("Неверный логин или пароль");

        return Result<AuthResponseDto>.Success(GenerateJwtToken(user, user.Role?.Name ?? "customer"));
    }

    private AuthResponseDto GenerateJwtToken(User user, string roleName)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Claims (Id и Role обязательны по заданию)
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.Role, roleName),
            new Claim("FullName", user.FullName ?? user.Login)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryInMinutes"]!)),
            signingCredentials: creds
        );

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Role = roleName,
            FullName = user.FullName ?? user.Login
        };
    }
}