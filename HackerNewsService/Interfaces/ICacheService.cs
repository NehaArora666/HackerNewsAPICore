

using HackerNews.Models;

namespace HackerNews.Interfaces
{
    public interface ICacheService
    {
        bool TryGetValue<T>(object key, out T value);
        void Set<T>(object key, T value, TimeSpan expiration);
    }
}
