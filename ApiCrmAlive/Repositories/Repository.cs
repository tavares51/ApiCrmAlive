using ApiCrmAlive.Context;
using ApiCrmAlive.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ApiCrmAlive.Repositories;

public class Repository<T>(AppDbContext ctx) : IRepository<T> where T : class
{
    protected readonly AppDbContext _ctx = ctx;
    protected readonly DbSet<T> _db = ctx.Set<T>();

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.FindAsync([id], ct);

    public virtual async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? filter = null, CancellationToken ct = default)
        => filter == null ? await _db.AsNoTracking().ToListAsync(ct)
                          : await _db.AsNoTracking().Where(filter).ToListAsync(ct);

    public virtual async Task AddAsync(T entity, CancellationToken ct = default)
        => await _db.AddAsync(entity, ct);

    public virtual void Update(T entity) => _db.Update(entity);
    public virtual void Remove(T entity) => _db.Remove(entity);
    public IQueryable<T> Query() => _db.AsQueryable();
}
