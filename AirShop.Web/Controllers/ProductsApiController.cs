using AirShop.Web.DTOs;
using AirShop.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace AirShop.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsApiController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsApiController(IProductService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10)
    {
        var (items, totalCount) = await _service.GetPagedAsync(page, pageSize);

        Response.Headers.Append("X-Total-Count", totalCount.ToString());
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _service.GetByIdAsync(id);
        return product == null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateProductDto dto)
    {
        var success = await _service.UpdateAsync(id, dto);
        return success ? NoContent() : NotFound();
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(int id, PatchProductDto dto)
    {
        var success = await _service.PatchAsync(id, dto);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }
}