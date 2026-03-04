using Microsoft.EntityFrameworkCore;
using RealEstate.DataAccess;
using RealEstate.Repositories;
using System.Linq.Expressions;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _db;
    internal DbSet<T> dbSet;
    public Repository(ApplicationDbContext db) { _db = db; this.dbSet = _db.Set<T>(); }

    public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter = null, string includeProperties = null)
    {
        IQueryable<T> query = dbSet;
        if (filter != null) query = query.Where(filter);
        if (includeProperties != null)
        {
            foreach (var prop in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(prop);
        }
        return await query.ToListAsync();
    }

    public IQueryable<T> Query(Expression<Func<T, bool>> filter = null, string includeProperties = null)
    {
        IQueryable<T> query = dbSet;
        if (filter != null) query = query.Where(filter);
        if (includeProperties != null)
        {
            foreach (var prop in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(prop);
        }
        return query;
    }

    public async Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter, string includeProperties = null)
    {
        IQueryable<T> query = dbSet.Where(filter);
        if (includeProperties != null)
        {
            foreach (var prop in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(prop);
        }
        return await query.FirstOrDefaultAsync();
    }
    public void Add(T entity) => dbSet.Add(entity);
    public void Remove(T entity) => dbSet.Remove(entity);
}