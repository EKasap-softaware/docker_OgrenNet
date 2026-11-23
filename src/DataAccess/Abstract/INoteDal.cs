using Core.DataAccess;
using Entities.Concrete;

namespace DataAccess.Abstract;

/// <summary>
/// Note tablosu için veri erişim sözleşmesi (Dal = Data Access Layer).
/// CRUD işlemleri IEntityRepository'den hazır gelir.
/// Note'a özel bir sorgu gerekirse (örn. "son 10 not") buraya eklenir.
/// </summary>
public interface INoteDal : IEntityRepository<Note>
{
}
