namespace Core.Entities;

/// <summary>
/// DTO (Data Transfer Object): Dışarıdan gelen veya dışarıya giden veri paketi.
/// Tablonun kendisi değildir. Örneğin not eklerken kullanıcıdan Id veya tarih
/// almayız; sadece başlık ve içerik alırız. Bunun için ayrı bir DTO kullanırız.
/// </summary>
public interface IDto
{
}
