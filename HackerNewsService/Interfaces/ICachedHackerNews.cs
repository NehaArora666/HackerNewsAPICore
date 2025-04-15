using HackerNews.Models;

namespace HackerNews.Interfaces
{
    public interface ICachedHackerNews
    {
        Task<List<Story>> GetTopStoriesAsync();
    }
}
