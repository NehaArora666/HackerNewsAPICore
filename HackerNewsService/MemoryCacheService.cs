using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using HackerNews.Interfaces;
using HackerNews.Models;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    /// <summary>
    /// Attempts to retrieve a cached value by the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key of the cached item.</param>
    /// <param name="value">
    /// When this method returns, contains the cached value associated with the specified key,
    /// if the key is found; otherwise, the default value for the type of the value parameter.
    /// </param>
    /// <returns>
    /// <c>true</c> if the key was found in the cache; otherwise, <c>false</c>.
    /// </returns>
    public bool TryGetValue<T>(object key, out T value)
    {
        return _cache.TryGetValue(key, out value);
    }

    /// <summary>
    /// Sets a value in the cache with the specified expiration time.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="key">The key for the cached item.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="expiration">The duration for which the item should remain in the cache.</param>

    public void Set<T>(object key, T value, TimeSpan expiration)
    {
        _cache.Set(key, value, expiration);
    }
}

