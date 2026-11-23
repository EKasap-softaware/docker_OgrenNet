using System.Linq.Expressions;
using DataAccess.Abstract;
using Entities.Concrete;

namespace Business.Tests.Fakes;

/// <summary>
/// Testler için SAHTE veritabanı: Notları sadece hafızadaki bir listede tutar.
///
/// NoteManager, INoteDal arayüzünü kullandığı için gerçek PostgreSQL yerine
/// bunu verebiliriz. Testler milisaniyeler içinde, veritabanı olmadan çalışır.
/// (Dependency Injection'ın en büyük faydalarından biri budur.)
/// </summary>
public class FakeNoteDal : INoteDal
{
    public List<Note> Notes { get; } = new();
    private int _nextId = 1;

    public Task<List<Note>> GetAllAsync(Expression<Func<Note, bool>>? filter = null)
    {
        var query = Notes.AsQueryable();
        if (filter != null) query = query.Where(filter);
        return Task.FromResult(query.ToList());
    }

    public Task<Note?> GetAsync(Expression<Func<Note, bool>> filter)
        => Task.FromResult(Notes.AsQueryable().FirstOrDefault(filter));

    public Task AddAsync(Note entity)
    {
        entity.Id = _nextId++;
        Notes.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Note entity) => Task.CompletedTask;   // nesne zaten listede, değişiklik yansıdı

    public Task DeleteAsync(Note entity)
    {
        Notes.Remove(entity);
        return Task.CompletedTask;
    }
}
