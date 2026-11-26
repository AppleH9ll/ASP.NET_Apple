using AirShop.Web.Data;
using AirShop.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AirShop.Web.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly AppDbContext _db;
    public CartController(AppDbContext db) => _db = db;

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task<Cart> GetOrCreateCartAsync()
    {
        int userId = GetUserId();
        var cart = await _db.Carts
            .Include(c => c.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync();
        }
        return cart;
    }

    public async Task<IActionResult> Index()
    {
        var cart = await GetOrCreateCartAsync();
        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1, string? returnUrl = null)
    {
        if (quantity < 1) quantity = 1;
        if (await _db.Products.FindAsync(productId) is null) return NotFound();

        var cart = await GetOrCreateCartAsync();
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

        if (item != null) item.Quantity += quantity;
        else _db.CartItems.Add(new CartItem { CartId = cart.Id, ProductId = productId, Quantity = quantity });

        await _db.SaveChangesAsync();

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int itemId, int quantity)
    {
        var item = await _db.CartItems
            .Include(i => i.Cart)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.Cart.UserId == GetUserId());

        if (item != null)
        {
            if (quantity < 1) _db.CartItems.Remove(item);
            else item.Quantity = quantity;
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int itemId)
    {
        var item = await _db.CartItems
            .Include(i => i.Cart)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.Cart.UserId == GetUserId());

        if (item != null)
        {
            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout()
    {
        try
        {
            var cart = await GetOrCreateCartAsync();
            if (!cart.Items.Any()) return RedirectToAction(nameof(Index));

            var order = new Order
            {
                UserId = GetUserId(),
                Status = "created",
                TotalAmount = cart.Items.Sum(i => i.Quantity * i.Product.Price),
                CreatedAt = DateTime.UtcNow
            };
            _db.Orders.Add(order);
            await _db.SaveChangesAsync(); // Сохраняем заказ, чтобы получить его Id

            foreach (var item in cart.Items)
            {
                _db.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                });
            }

            _db.CartItems.RemoveRange(cart.Items);
            await _db.SaveChangesAsync(); // Сохраняем позиции и очищаем корзину

            return RedirectToAction("Success", new { id = order.Id });
        }
        catch (Exception ex)
        {
            // В реальном проекте тут пишут в лог (ILogger)
            Console.WriteLine($"ОШИБКА ОФОРМЛЕНИЯ ЗАКАЗА: {ex.Message}");

            // Передаем ошибку пользователю через TempData
            TempData["Error"] = "Произошла ошибка при оформлении заказа. Попробуйте позже или обратитесь в поддержку.";
            return RedirectToAction(nameof(Index));
        }
    }

    public IActionResult Success(int id)
    {
        ViewBag.OrderId = id;
        return View();
    }
}