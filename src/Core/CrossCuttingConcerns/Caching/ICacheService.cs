namespace Core.CrossCuttingConcerns.Caching;

/// <summary>
/// Önbellek (cache): Sık okunan veriyi veritabanına gitmeden, hafızadan hızlıca vermek.
/// "Cross-cutting concern" = Her katmanı kesen ortak ihtiyaç (cache, log, doğrulama...).
///
/// Business katmanı Redis'i TANIMAZ, sadece bu arayüzü tanır.
/// Yarın Redis yerine başka bir şey kullanırsak Business'ta tek satır değişmez.
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);

    Task SetAsync<T>(string key, T value, TimeSpan duration);

    Task RemoveAsync(string key);
}
