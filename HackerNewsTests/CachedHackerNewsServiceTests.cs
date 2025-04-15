using Xunit;
using Moq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using HackerNews.Models;
using HackerNews.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Castle.Core.Logging;

public class CachedHackerNewsServiceTests
{
    private readonly Mock<IHackerNewsService> _mockInnerService;
    private readonly Mock<ICacheService> _mockCache;
    private readonly Mock<ILogger<CachedHackerNewsService>> _mockLogger;

    private readonly CachedHackerNewsService _service;

    public CachedHackerNewsServiceTests()
    {
        _mockInnerService = new Mock<IHackerNewsService>();
        _mockCache = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<CachedHackerNewsService>>();

        _service = new CachedHackerNewsService(_mockInnerService.Object, _mockCache.Object, _mockLogger.Object);
    }


    [Fact]
    public async Task GetTopStoriesAsync_ReturnsCachedStories_WhenAllStoriesAreCached()
    {
        // Arrange
        var storyIds = Enumerable.Range(1, 5).ToList();
        var stories = storyIds.Select(id => new Story { Id = id, Url = "http://example.com", Title = $"Story {id}" }).ToArray();

        // Set up cache hit for top story IDs
        object outValue = storyIds;
        _mockCache.Setup(c => c.TryGetValue("TopStoriesIds", out outValue)).Returns(true);

        // Set up cache hits for each story based on the story ID (not Title)
        foreach (var story in stories)
        {
            object cachedStory = story;
            _mockCache.Setup(c => c.TryGetValue(story.Id, out cachedStory)).Returns(true); // Use story.Id as key
        }

        // Act
        var result = await _service.GetTopStoriesAsync();

        // Assert
        Assert.Equal(stories.Length, result.Count);  // Ensure correct number of stories returned
        Assert.All(result, s => Assert.NotNull(s));  // Ensure no null stories
    }


    [Fact]
    public async Task GetTopStoriesAsync_ReturnsStories_WhenCacheMiss()
    {
        // Arrange
        var storyIds = new List<int> { 1, 2, 3 };

        // Simulate cache miss for "TopStoriesIds"
        _mockCache.Setup(c => c.TryGetValue("TopStoriesIds", out It.Ref<object>.IsAny))
                  .Returns(false);

        _mockInnerService.Setup(s => s.FetchTopStoryIdsAsync()).ReturnsAsync(storyIds);
        _mockCache.Setup(c => c.Set("TopStoriesIds", storyIds, It.IsAny<TimeSpan>()));

        foreach (var id in storyIds)
        {
            // Simulate cache miss for individual story
            _mockCache.Setup(c => c.TryGetValue(id, out It.Ref<object>.IsAny))
                      .Returns(false);

            var story = new Story { Id = id, Title = $"Story {id}" };

            _mockInnerService.Setup(s => s.FetchStoryDetailsAsync(id)).ReturnsAsync(story);
            _mockCache.Setup(c => c.Set(id, story, It.IsAny<TimeSpan>()));
        }

        // Act
        var result = await _service.GetTopStoriesAsync();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, s => Assert.NotNull(s));
    }


    [Fact]
    public async Task GetTopStoriesAsync_HandlesNullStory_Gracefully()
    {
        // Arrange
        var storyIds = new List<int> { 1, 2 };

        var mockCache = new Mock<ICacheService>();
        mockCache.Setup(c => c.TryGetValue("TopStoriesIds", out It.Ref<List<int>>.IsAny)).Returns(false);
        mockCache.Setup(c => c.Set(It.IsAny<string>(), storyIds, It.IsAny<TimeSpan>()));

        mockCache.Setup(c => c.TryGetValue(1, out It.Ref<Story>.IsAny)).Returns(false);
        mockCache.Setup(c => c.TryGetValue(2, out It.Ref<Story>.IsAny)).Returns(false);

        var mockInnerService = new Mock<IHackerNewsService>();
        mockInnerService.Setup(s => s.FetchTopStoryIdsAsync()).ReturnsAsync(storyIds);
        mockInnerService.Setup(s => s.FetchStoryDetailsAsync(1)).ReturnsAsync((Story)null);
        mockInnerService.Setup(s => s.FetchStoryDetailsAsync(2)).ReturnsAsync(new Story { Id = 2, Title = "Story 2", Url = "http://example.com" });

        var mockLogger = new Mock<ILogger<CachedHackerNewsService>>();
        var service = new CachedHackerNewsService(mockInnerService.Object, mockCache.Object, mockLogger.Object);

        // Act
        var result = await service.GetTopStoriesAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result[0].Id);
    }



    [Fact]
    public async Task GetTopStoriesAsync_ReturnsEmptyList_OnException()
    {
        // Arrange
        var mockInnerService = new Mock<IHackerNewsService>();
        var mockCache = new Mock<ICacheService>();
        var mockLogger = new Mock<ILogger<CachedHackerNewsService>>();

        mockInnerService
            .Setup(s => s.FetchTopStoryIdsAsync())
            .ThrowsAsync(new Exception("fail"));

        var service = new CachedHackerNewsService(mockInnerService.Object, mockCache.Object, mockLogger.Object);

        // Act
        var result = await service.GetTopStoriesAsync();

        // Assert
        Assert.Empty(result);

        // Verify the logger was called with an exception
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to get top stories with caching.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

}
