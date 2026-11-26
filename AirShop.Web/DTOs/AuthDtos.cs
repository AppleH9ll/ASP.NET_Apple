namespace AirShop.Web.DTOs;

public class LoginRequestDto
{
    public string Login { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class RegisterRequestDto
{
    public string Login { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FullName { get; set; } = null!;
}

public class AuthResponseDto
{
    public string Token { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string FullName { get; set; } = null!;
}