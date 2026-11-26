using AirShop.Web.DTOs;
using AirShop.Web.Models;
using AirShop.Web.Repositories;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AirShop.Web.Services;

public interface IProductService
{
    Task<(IEnumerable<ProductDto> Items, int TotalCount)> GetPagedAsync(int page, int pageSize);
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<bool> UpdateAsync(int id, UpdateProductDto dto);
    Task<bool> PatchAsync(int id, PatchProductDto dto);
    Task<bool> DeleteAsync(int id);
}

public class ProductService : IProductService
{
    private readonly IGenericRepository<Product> _repo;
    private readonly IMapper _mapper;

    public ProductService(IGenericRepository<Product> repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<(IEnumerable<ProductDto> Items, int TotalCount)> GetPagedAsync(int page, int pageSize)
    {
        var query = _repo.Query().Include(p => p.Category).OrderBy(p => p.Id);
        var totalCount = await query.CountAsync();

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        var dtos = _mapper.Map<IEnumerable<ProductDto>>(items);

        return (dtos, totalCount);
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _repo.Query().Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        return product == null ? null : _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var product = _mapper.Map<Product>(dto);
        product.CreatedAt = DateTime.UtcNow;
        product.IsActive = true;

        await _repo.AddAsync(product);
        await _repo.SaveChangesAsync();

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await _repo.GetByIdAsync(id);
        if (product == null) return false;

        _mapper.Map(dto, product);
        _repo.Update(product);
        await _repo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PatchAsync(int id, PatchProductDto dto)
    {
        var product = await _repo.GetByIdAsync(id);
        if (product == null) return false;

        _mapper.Map(dto, product);
        _repo.Update(product);
        await _repo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _repo.GetByIdAsync(id);
        if (product == null) return false;

        _repo.Delete(product);
        await _repo.SaveChangesAsync();
        return true;
    }
}