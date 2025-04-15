
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using HackerNews.Interfaces;
using HackerNews.Models;



public class HackerNewsService : IHackerNewsService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HackerNewsService> _logger;
    private readonly string _topStoriesUrl = "https://hacker-news.firebaseio.com/v0/topstories.json?print=pretty";
    private readonly string _itemUrlTemplate = "https://hacker-news.firebaseio.com/v0/item/{0}.json?print=pretty";

    public HackerNewsService(HttpClient httpClient, ILogger<HackerNewsService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the top stories from the stories url.
    /// </summary>
    /// <remarks>
    /// This method returns the top 200 stories.
    /// </remarks>
    public async Task<List<int>> FetchTopStoryIdsAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync(_topStoriesUrl);
            return JsonConvert.DeserializeObject<List<int>>(response)?.Take(200).ToList() ?? new List<int>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching top story IDs.");
            return new List<int>();
        }
    }

    /// <summary>
    /// Retrieves the details of a story based on its ID.
    /// </summary>
    /// <param name="storyId">The ID of the story to fetch.</param>
    /// <returns>
    /// A object containing the story details, or if null an error occurs.
    /// </returns>
    /// <remarks>
    /// This method fetches the story data from the external Hacker News API using the provided story ID.
    /// </remarks>
    public async Task<Story> FetchStoryDetailsAsync(int storyId)
    {
        try
        {
            var response = await _httpClient.GetStringAsync(string.Format(_itemUrlTemplate, storyId));
            return JsonConvert.DeserializeObject<Story>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching story ID {StoryId}", storyId);
            return null;
        }
    }
}
