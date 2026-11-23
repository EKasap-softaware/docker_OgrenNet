namespace Business.Constants;

/// <summary>
/// Kullanıcıya gösterilen tüm mesajlar tek yerde.
/// Mesajı değiştirmek istersen kodun içinde aramazsın, buraya bakarsın.
/// </summary>
public static class Messages
{
    public const string NotesListed = "Notlar listelendi";
    public const string NotesListedFromCache = "Notlar listelendi (cache)";
    public const string NoteListed = "Not getirildi";
    public const string NoteAdded = "Not eklendi";
    public const string NoteUpdated = "Not güncellendi";
    public const string NoteDeleted = "Not silindi";
    public const string NoteNotFound = "Not bulunamadı";

    public const string TitleRequired = "Başlık boş olamaz";
    public const string ContentRequired = "İçerik boş olamaz";
    public static string TitleTooLong(int max) => $"Başlık en fazla {max} karakter olabilir";
    public static string ContentTooLong(int max) => $"İçerik en fazla {max} karakter olabilir";
}
