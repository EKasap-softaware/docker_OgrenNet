using Entities.Concrete;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Concrete.EntityFramework;

/// <summary>
/// DbContext: C# sınıfları ile veritabanı tabloları arasındaki köprü.
/// Bağlantı bilgisi (connection string) dışarıdan, DbContextOptions ile gelir.
/// </summary>
public class NotDefteriContext : DbContext
{
    public NotDefteriContext(DbContextOptions<NotDefteriContext> options) : base(options)
    {
    }

    // Her DbSet bir tablodur.
    public DbSet<Note> Notes => Set<Note>();

    // Tablo kuralları (Fluent API): sütun uzunlukları, zorunlu alanlar...
    // Bu kurallar migration ile veritabanına da yansır.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Note>(note =>
        {
            note.HasKey(n => n.Id);
            note.Property(n => n.Title).IsRequired().HasMaxLength(100);
            note.Property(n => n.Content).IsRequired().HasMaxLength(500);
            note.Property(n => n.CreatedAt).IsRequired();
        });
    }
}
