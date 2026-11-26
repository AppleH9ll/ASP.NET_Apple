using AirShop.Web.Data;
using AirShop.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace AirShop.Web.Controllers;

[Authorize(Roles = "admin")]
[Route("Admin/Roles")]
public class AdminRolesController : Controller
{
    private readonly AppDbContext _db;
    public AdminRolesController(AppDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index() => View(await _db.Roles.ToListAsync());

    [HttpGet("Create")]
    public IActionResult Create() => View(new Role());

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Role role)
    {
        if (ModelState.IsValid) { _db.Roles.Add(role); await _db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
        return View(role);
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var role = await _db.Roles.FindAsync(id);
        return role == null ? NotFound() : View(role);
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Role role)
    {
        if (id != role.Id) return NotFound();
        if (ModelState.IsValid) { _db.Update(role); await _db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
        return View(role);
    }

    [HttpGet("Delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var role = await _db.Roles.FindAsync(id);
        return role == null ? NotFound() : View(role);
    }

    [HttpPost("Delete/{id}"), ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var role = await _db.Roles.FindAsync(id);
        if (role != null) { _db.Roles.Remove(role); await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }
}