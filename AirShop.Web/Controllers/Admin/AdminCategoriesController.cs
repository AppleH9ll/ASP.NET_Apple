using AirShop.Web.Data;
using AirShop.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace AirShop.Web.Controllers;

[Authorize(Roles = "admin")]
[Route("Admin/Categories")]
public class AdminCategoriesController : Controller
{
    private readonly AppDbContext _db;
    public AdminCategoriesController(AppDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index() => View(await _db.Categories.ToListAsync());

    [HttpGet("Create")]
    public IActionResult Create() => View(new Category());

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (ModelState.IsValid) { _db.Categories.Add(category); await _db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
        return View(category);
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        return category == null ? NotFound() : View(category);
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.Id) return NotFound();
        if (ModelState.IsValid) { _db.Update(category); await _db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
        return View(category);
    }

    [HttpGet("Delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        return category == null ? NotFound() : View(category);
    }

    [HttpPost("Delete/{id}"), ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category != null) { _db.Categories.Remove(category); await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }
}