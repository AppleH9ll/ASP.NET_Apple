using AirShop.Web.Data;
using AirShop.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace AirShop.Web.Controllers;

[Authorize(Roles = "admin")]
[Route("Admin/Orders")]
public class AdminOrdersController : Controller
{
    private readonly AppDbContext _db;
    public AdminOrdersController(AppDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index() => View(await _db.Orders.Include(o => o.User).OrderByDescending(o => o.CreatedAt).ToListAsync());

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var order = await _db.Orders.Include(o => o.User).FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound();
        return View(order);
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string status)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();

        order.Status = status;
        _db.Update(order);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _db.Orders.Include(o => o.User).FirstOrDefaultAsync(o => o.Id == id);
        return order == null ? NotFound() : View(order);
    }

    [HttpPost("Delete/{id}"), ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order != null) { _db.Orders.Remove(order); await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }
}