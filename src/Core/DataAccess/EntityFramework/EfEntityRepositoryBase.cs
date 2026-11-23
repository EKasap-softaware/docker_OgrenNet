using System.Linq.Expressions;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.DataAccess.EntityFramework;

/// <summary>
/// IEntityRepository'nin Entity Framework Core ile yazılmış hali.
/// Bir tablo için Dal sınıfı yazarken bu sınıftan miras alırsın, CRUD kodunu
/// tekrar yazmazsın:   public class EfNoteDal : EfEntityRepositoryBase&lt;Note, NotDefteriContext&gt;
/// </summary>
public class EfEntityRepositoryBase<TEntity, TContext> : IEntityRepository<TEntity>
    where TEntity : class, IEntity, new()
    where TContext : DbContext
{
    // DbContext dışarıdan (Dependency Injection ile) gelir. "new" ile oluşturmayız.
    protected readonly TContext Context;

    public EfEntityRepositoryBase(TContext context)
    {
        Context = context;
    }

    public async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? filter = null)
    {
        // AsNoTracking: sadece okuyacağız, EF değişiklik takibi yapmasın (daha hızlı).
        var query = Context.Set<TEntity>().AsNoTracking();
        return filter == null
            ? await query.ToListAsync()
            : await query.Where(filter).ToListAsync();
    }

    public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> filter)
    {
        return await Context.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(filter);
    }

    public async Task AddAsync(TEntity entity)
    {
        await Context.Set<TEntity>().AddAsync(entity);
        await Context.SaveChangesAsync();      // SQL: INSERT INTO ...
    }

    public async Task UpdateAsync(TEntity entity)
    {
        Context.Set<TEntity>().Update(entity);
        await Context.SaveChangesAsync();      // SQL: UPDATE ...
    }

    public async Task DeleteAsync(TEntity entity)
    {
        Context.Set<TEntity>().Remove(entity);
        await Context.SaveChangesAsync();      // SQL: DELETE FROM ...
    }
}
