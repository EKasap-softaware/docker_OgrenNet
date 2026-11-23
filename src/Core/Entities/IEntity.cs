namespace Core.Entities;

/// <summary>
/// Veritabanında tablosu olan her sınıf bu arayüzü taşır (Note gibi).
/// İçi boştur; sadece "ben bir veritabanı nesnesiyim" diye işaretler.
/// Böylece generic repository sadece gerçek tablolarla çalışır.
/// </summary>
public interface IEntity
{
}
