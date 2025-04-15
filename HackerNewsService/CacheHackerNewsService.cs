using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using HackerNews.Interfaces;
using HackerNews.Models;

public class CachedHackerNewsService : ICachedHackerNews
{
    private readonly IHackerNewsService _innerService;
    private readonly ICacheService _cache;
    private readonly ILogger<CachedHackerNewsService> _logger;
    private static readonly string _topStoriesCacheKey = "TopStoriesIds";

    public CachedHackerNewsService(IHackerNewsService innerService, ICacheService cache, ILogger<CachedHackerNewsService> logger)
    {
        _innerService = innerService;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the top stories from the cache or fetches them using the inner service if not cached.
    /// </summary>
    /// <remarks>
    /// This method returns the top 200 stories with caching support.
    /// </remarks>
    public async Task<List<Story>> GetTopStoriesAsync()
    {
        try
        {
            var topStoryIds = await GetCachedTopStoryIdsAsync();
            var selectedIds = topStoryIds.Take(200);

            var tasks = selectedIds.Select(id => GetCachedStoryDetailsAsync(id)).ToList();
            var fetchedStories = await Task.WhenAll(tasks);

            return fetchedStories.Where(s => s != null).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get top stories with caching.");
            return new List<Story>();
        }
    }
    /// <summary>
    /// Retrieves the top story IDs from the cache or fetches them from the inner service if not cached.
    /// </summary>
    /// <returns>
    /// A list of top story IDs as <see cref="int"/> values. If not found in cache, the list is fetched and cached for 1 minute.
    /// </returns>
    /// <remarks>
    /// This method uses a cache-first strategy to minimize external service calls. If the data is not cached,
    /// it fetches the IDs from the inner service and stores them in the cache for subsequent requests.
    /// </remarks>

    private async Task<List<int>> GetCachedTopStoryIdsAsync()
    {
        if (_cache.TryGetValue(_topStoriesCacheKey, out List<int> cachedIds))
        {
            return cachedIds;
        }

        var storyIds = await _innerService.FetchTopStoryIdsAsync();

        _cache.Set(_topStoriesCacheKey, storyIds, TimeSpan.FromMinutes(1));
        return storyIds;
    }

    /// <summary>
    /// Retrieves a story from the cache or fetches it from the inner service if not cached.
    /// </summary>
    /// <param name="storyId">The ID of the story to retrieve.</param>
    /// <returns>
    /// A <see cref="Story"/> object from the cache or fetched from the inner service; 
    /// returns <c>null</c> if the story cannot be retrieved.
    /// </returns>
    /// <remarks>
    /// If the story is not present in the cache, it will be fetched and then cached for 5 minutes.
    /// </remarks>

    private async Task<Story> GetCachedStoryDetailsAsync(int storyId)
    {
        if (_cache.TryGetValue(storyId, out Story cachedStory))
        {
            return cachedStory;
        }

        var story = await _innerService.FetchStoryDetailsAsync(storyId);

        if (story != null)
        {
            _cache.Set(storyId, story, TimeSpan.FromMinutes(5));
        }

        return story;
    }
}
