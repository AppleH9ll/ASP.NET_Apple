using AirShop.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace AirShop.Web.Controllers;

[Authorize(Roles = "admin")]
[Route("Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _db;
    public AdminController(AppDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewBag.ProductsCount = await _db.Products.CountAsync();
        ViewBag.OrdersCount = await _db.Orders.CountAsync();
        ViewBag.UsersCount = await _db.Users.CountAsync();
        ViewBag.CategoriesCount = await _db.Categories.CountAsync();
        return View();
    }
}