using Core.CrossCuttingConcerns.Caching;

namespace Business.Tests.Fakes;

/// <summary>Testler için SAHTE cache: Redis yerine basit bir sözlük (Dictionary).</summary>
public class FakeCacheService : ICacheService
{
    public Dictionary<string, object?> Items { get; } = new();

    public Task<T?> GetAsync<T>(string key)
        => Task.FromResult(Items.TryGetValue(key, out var value) ? (T?)value : default);

    public Task SetAsync<T>(string key, T value, TimeSpan duration)
    {
        Items[key] = value;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        Items.Remove(key);
        return Task.CompletedTask;
    }
}
