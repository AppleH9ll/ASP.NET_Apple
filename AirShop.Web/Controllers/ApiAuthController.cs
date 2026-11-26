using AirShop.Web.DTOs;
using AirShop.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AirShop.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ApiAuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public ApiAuthController(IAuthService authService) => _authService = authService;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        if (!result.IsSuccess) return BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }

    // ПРИМЕНЯЕМ RATE LIMITING (настроим его в Program.cs)
    [HttpPost("login")]
    [EnableRateLimiting("LoginPolicy")]
    public async Task<IActionResult> Login(LoginRequestDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        if (!result.IsSuccess) return Unauthorized(new { error = result.ErrorMessage }); // 401
        return Ok(result.Data);
    }
}