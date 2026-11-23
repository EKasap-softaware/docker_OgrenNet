using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete;

namespace DataAccess.Concrete.EntityFramework;

/// <summary>
/// INoteDal'ın Entity Framework ile çalışan hali.
/// Tüm CRUD kodu EfEntityRepositoryBase'ten miras gelir; burada tek satır kod yok.
/// </summary>
public class EfNoteDal : EfEntityRepositoryBase<Note, NotDefteriContext>, INoteDal
{
    public EfNoteDal(NotDefteriContext context) : base(context)
    {
    }
}
