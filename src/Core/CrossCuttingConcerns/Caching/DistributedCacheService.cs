using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Core.CrossCuttingConcerns.Caching;

/// <summary>
/// ICacheService'in IDistributedCache (bizde Redis) ile çalışan hali.
///
/// Önemli tasarım kararı: Cache çökerse uygulama ÇÖKMEMELİ.
/// Redis'e ulaşılamazsa hatayı loglar, "cache'te yok" gibi davranır ve
/// veri doğrudan veritabanından okunur. Sadece biraz yavaşlar.
/// </summary>
public class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<DistributedCacheService> _logger;

    public DistributedCacheService(IDistributedCache cache, ILogger<DistributedCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var json = await _cache.GetStringAsync(key);
            return json is null ? default : JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception e)
        {
            _logger.LogWarning("Cache okunamadı ({Key}): {Hata}", key, e.Message);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan duration)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = duration
            });
        }
        catch (Exception e)
        {
            _logger.LogWarning("Cache yazılamadı ({Key}): {Hata}", key, e.Message);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
        }
        catch (Exception e)
        {
            _logger.LogWarning("Cache silinemedi ({Key}): {Hata}", key, e.Message);
        }
    }
}
