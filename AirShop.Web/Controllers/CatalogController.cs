using AirShop.Web.Data;
using AirShop.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AirShop.Web.Controllers;

public class CatalogController : Controller
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;

    public CatalogController(AppDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<IActionResult> Index(string? search, int? categoryId, string? sortOrder, int page = 1)
    {
        var query = _db.Products.Include(p => p.Category).Where(p => p.IsActive);

        // 1. Поиск
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));

        // 2. Фильтрация
        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId);

        // 3. Сортировка
        query = sortOrder switch
        {
            "name_desc" => query.OrderByDescending(p => p.Name),
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            _ => query.OrderBy(p => p.Name)
        };

        // 4. Пагинация
        int pageSize = 6;
        int totalItems = await query.CountAsync();
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

        var products = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        // 5. Использование MemoryCache для категорий
        if (!_cache.TryGetValue("CategoriesList", out List<Category>? categories))
        {
            categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
            _cache.Set("CategoriesList", categories, TimeSpan.FromMinutes(10));
        }

        ViewBag.Categories = categories;
        ViewBag.SelectedCategory = categoryId;
        ViewBag.Search = search;
        ViewBag.SortOrder = sortOrder;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        return View(products);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        return product == null ? NotFound() : View(product);
    }
}