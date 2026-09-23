using Microsoft.EntityFrameworkCore;
using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.DataAccessLayer.Repositories.Implementations;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly TmsDbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    public Repository(TmsDbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync() =>
        await DbSet.AsNoTracking().ToListAsync();

    public async Task<TEntity?> GetByIdAsync(params object?[] keyValues) =>
        await DbSet.FindAsync(keyValues);

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        await DbSet.AddAsync(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(TEntity entity)
    {
        DbSet.Update(entity);
        await Context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(params object?[] keyValues)
    {
        var entity = await DbSet.FindAsync(keyValues);
        if (entity is null)
        {
            return false;
        }

        DbSet.Remove(entity);
        await Context.SaveChangesAsync();
        return true;
    }
}
