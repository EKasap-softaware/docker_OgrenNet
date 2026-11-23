using System.Linq.Expressions;
using Core.Entities;

namespace Core.DataAccess;

/// <summary>
/// Her tablo için tekrar tekrar yazılan temel işlemler (CRUD):
/// Create (Add), Read (Get/GetAll), Update, Delete.
/// Generic (T) olduğu için Note, Kategori, Kullanıcı... hepsinde aynı kod çalışır.
/// </summary>
public interface IEntityRepository<T> where T : class, IEntity, new()
{
    /// <param name="filter">İsteğe bağlı filtre. Örnek: n => n.Title.Contains("alışveriş")</param>
    Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null);

    Task<T?> GetAsync(Expression<Func<T, bool>> filter);

    Task AddAsync(T entity);

    Task UpdateAsync(T entity);

    Task DeleteAsync(T entity);
}
