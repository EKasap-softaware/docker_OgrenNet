using Core.Entities;

namespace Entities.Concrete;

/// <summary>
/// Veritabanındaki "Notes" tablosunun C# karşılığı.
/// Her özellik (property) tablodaki bir sütundur.
/// </summary>
public class Note : IEntity
{
    public int Id { get; set; }                       // birincil anahtar (primary key), otomatik artar

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }          // ? = boş olabilir (hiç güncellenmediyse)
}
