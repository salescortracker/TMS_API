namespace TMS.DataAccessLayer.Repositories.Interfaces;

public interface IRepository<TEntity> where TEntity : class
{
    Task<IEnumerable<TEntity>> GetAllAsync();

    Task<TEntity?> GetByIdAsync(params object?[] keyValues);

    Task<TEntity> AddAsync(TEntity entity);

    Task UpdateAsync(TEntity entity);

    Task<bool> DeleteAsync(params object?[] keyValues);
}
