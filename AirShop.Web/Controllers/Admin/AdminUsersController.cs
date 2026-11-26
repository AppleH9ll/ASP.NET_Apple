using AirShop.Web.Data;
using AirShop.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace AirShop.Web.Controllers;

[Authorize(Roles = "admin")]
[Route("Admin/Users")]
public class AdminUsersController : Controller
{
    private readonly AppDbContext _db;
    public AdminUsersController(AppDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index() => View(await _db.Users.Include(u => u.Role).ToListAsync());

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Roles = new SelectList(await _db.Roles.ToListAsync(), "Id", "Name");
        return View(new User());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(User user)
    {
        if (ModelState.IsValid)
        {
            // ВНИМАНИЕ: Для реального проекта здесь нужно хешировать пароль!
            // Сейчас мы сохраняем введенный текст как хеш для демонстрации.
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Roles = new SelectList(await _db.Roles.ToListAsync(), "Id", "Name", user.RoleId);
        return View(user);
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        ViewBag.Roles = new SelectList(await _db.Roles.ToListAsync(), "Id", "Name", user.RoleId);
        return View(user);
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, User user)
    {
        if (id != user.Id) return NotFound();
        if (ModelState.IsValid) { _db.Update(user); await _db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
        ViewBag.Roles = new SelectList(await _db.Roles.ToListAsync(), "Id", "Name", user.RoleId);
        return View(user);
    }

    [HttpGet("Delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
        return user == null ? NotFound() : View(user);
    }

    [HttpPost("Delete/{id}"), ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user != null) { _db.Users.Remove(user); await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }
}