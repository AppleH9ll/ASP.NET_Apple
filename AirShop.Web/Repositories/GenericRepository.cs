using AirShop.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace AirShop.Web.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
    public IQueryable<T> Query() => _dbSet.AsQueryable();
    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
    public void Update(T entity) => _dbSet.Update(entity);
    public void Delete(T entity) => _dbSet.Remove(entity);

    // Этот метод сохраняет изменения для ВСЕХ сущностей в текущем DbContext (в рамках одного HTTP-запроса)
    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
}