

using HackerNews.Models;

namespace HackerNews.Interfaces
{
    public interface IHackerNewsService
    {
        Task<List<int>> FetchTopStoryIdsAsync();
        Task<Story> FetchStoryDetailsAsync(int storyId);
    }
}
