using System.Text.RegularExpressions;
using Business.Constants;
using Core.Utilities.Results;
using Entities.DTOs;

namespace Business.Rules;

/// <summary>
/// Not ile ilgili iş kuralları (business rules).
///
/// Neden ayrı bir sınıf? Bu kodlar veritabanına, Redis'e veya HTTP'ye
/// ihtiyaç duymaz; sadece girdi alır, sonuç döner ("saf" kod).
/// Bu yüzden birim testi (unit test) yazmak çok kolaydır.
/// </summary>
public static class NoteRules
{
    public const int TitleMaxLength = 10;
    public const int ContentMaxLength = 500;

    /// <summary>
    /// Baştaki ve sondaki boşlukları siler, aradaki çoklu boşlukları teke indirir.
    /// "  alışveriş    listesi  "  ->  "alışveriş listesi"
    /// </summary>
    public static string Clean(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return Regex.Replace(text.Trim(), @"\s+", " ");
    }

    /// <summary>Not eklenebilir / güncellenebilir mi? İlk bulunan hatayı döner.</summary>
    public static IResult Validate(NoteCreateDto dto)
    {
        var title = Clean(dto.Title);
        var content = Clean(dto.Content);

        if (title.Length == 0)
            return new ErrorResult(Messages.TitleRequired);

        if (title.Length > TitleMaxLength)
            return new ErrorResult(Messages.TitleTooLong(TitleMaxLength));

        if (content.Length == 0)
            return new ErrorResult(Messages.ContentRequired);

        if (content.Length > ContentMaxLength)
            return new ErrorResult(Messages.ContentTooLong(ContentMaxLength));

        return new SuccessResult();
    }
}
