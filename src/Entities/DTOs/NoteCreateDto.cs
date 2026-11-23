using Core.Entities;

namespace Entities.DTOs;

/// <summary>
/// Not eklerken ve güncellerken kullanıcıdan SADECE bunları alırız.
/// Id'yi veritabanı, tarihleri sistem belirler; kullanıcı bunlara karışamaz.
/// </summary>
public class NoteCreateDto : IDto
{
    public string? Title { get; set; }

    public string? Content { get; set; }
}
