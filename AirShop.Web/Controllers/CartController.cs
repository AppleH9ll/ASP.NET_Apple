using AirShop.Web.Data;
using AirShop.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirShop.Web.Controllers;

public class CartController : Controller
{
    // Демонстрационный «текущий пользователь» — ivanov (id = 2)
    private const int CurrentUserId = 2;

    private readonly AppDbContext _db;
    public CartController(AppDbContext db) => _db = db;

    private async Task<Cart> GetOrCreateCartAsync()
    {
        var cart = await _db.Carts
            .Include(c => c.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == CurrentUserId);

        if (cart == null)
        {
            cart = new Cart { UserId = CurrentUserId, CreatedAt = DateTime.UtcNow };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync();
        }
        return cart;
    }

    public async Task<IActionResult> Index() => View(await GetOrCreateCartAsync());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1, string? returnUrl = null)
    {
        if (quantity < 1) quantity = 1;
        if (await _db.Products.FindAsync(productId) is null) return NotFound();

        var cart = await GetOrCreateCartAsync();
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null) item.Quantity += quantity;
        else _db.CartItems.Add(new CartItem
        {
            CartId = cart.Id,
            ProductId = productId,
            Quantity = quantity
        });

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
            .FirstOrDefaultAsync(i => i.Id == itemId && i.Cart.UserId == CurrentUserId);

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
            .FirstOrDefaultAsync(i => i.Id == itemId && i.Cart.UserId == CurrentUserId);

        if (item != null)
        {
            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}