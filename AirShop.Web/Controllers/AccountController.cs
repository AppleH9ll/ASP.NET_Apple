using AirShop.Web.Data;
using AirShop.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AirShop.Web.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    public AccountController(AppDbContext db) => _db = db;

    // --- ФОРМА РЕГИСТРАЦИИ ---
    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string login, string fullName, string password)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Логин и пароль обязательны";
            return View();
        }

        if (await _db.Users.AnyAsync(u => u.Login == login))
        {
            ViewBag.Error = "Пользователь с таким логином уже существует";
            return View();
        }

        var defaultRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "customer")
                          ?? new Role { Name = "customer", Description = "Покупатель" };

        var user = new User
        {
            Login = login,
            FullName = fullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            RoleId = defaultRole.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return RedirectToAction("Login");
    }

    // --- ФОРМА ВХОДА ---
    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string login, string password, bool rememberMe = true)
    {
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Login == login);

        if (user == null)
        {
            ViewBag.Error = "Неверный логин или пароль";
            return View();
        }

        if (!user.IsActive)
        {
            ViewBag.Error = "Ваш аккаунт заблокирован администратором";
            return View();
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        if (!isPasswordValid)
        {
            ViewBag.Error = "Неверный логин или пароль";
            return View();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim("FullName", user.FullName ?? user.Login),
            new Claim(ClaimTypes.Role, user.Role?.Name ?? "customer")
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        if (user.Role?.Name == "admin" || user.Role?.Name == "manager")
        {
            return RedirectToAction("Index", "Admin");
        }

        return RedirectToAction("Index", "Catalog");
    }

    // --- ВЫХОД ---
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Catalog");
    }

    // --- ОТКАЗ В ДОСТУПЕ ---
    [HttpGet]
    public IActionResult AccessDenied() => View();
}