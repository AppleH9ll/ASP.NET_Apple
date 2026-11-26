using AirShop.Web.Data;
using AirShop.Web.Models;
using Microsoft.AspNetCore.Authorization; // <-- ДОБАВЛЕНА ЭТА СТРОКА
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AirShop.Web.Controllers; // Если вы в подпапке Admin, можно оставить так, проект соберется

[Authorize(Roles = "admin")] // <-- Теперь компилятор знает, что такое Authorize
[Route("Admin/Products")]
public class AdminProductsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public AdminProductsController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var products = await _db.Products.Include(p => p.Category).ToListAsync();
        return View(products);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = new SelectList(await _db.Categories.ToListAsync(), "Id", "Name");
        return View(new Product());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (ModelState.IsValid)
        {
            // Обработка загрузки файла
            if (product.ImageFile != null && product.ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
                var uploadPath = Path.Combine(_env.WebRootPath, "images");

                Directory.CreateDirectory(uploadPath); // Создаем папку, если нет

                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await product.ImageFile.CopyToAsync(stream);
                }
                product.ImageUrl = "/images/" + fileName;
            }

            _db.Products.Add(product);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Categories = new SelectList(await _db.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
        return View(product);
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        ViewBag.Categories = new SelectList(await _db.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
        return View(product);
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.Id) return NotFound();

        if (ModelState.IsValid)
        {
            // Обработка загрузки нового файла
            if (product.ImageFile != null && product.ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
                var uploadPath = Path.Combine(_env.WebRootPath, "images");
                Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await product.ImageFile.CopyToAsync(stream);
                }
                product.ImageUrl = "/images/" + fileName;
            }
            // Если файл не загружен, но в скрытом поле есть старый URL - оставляем его
            else if (string.IsNullOrEmpty(product.ImageUrl))
            {
                var existing = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                product.ImageUrl = existing?.ImageUrl;
            }

            _db.Update(product);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Categories = new SelectList(await _db.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
        return View(product);
    }

    [HttpGet("Delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return NotFound();
        return View(product);
    }

    [HttpPost("Delete/{id}"), ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product != null)
        {
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}