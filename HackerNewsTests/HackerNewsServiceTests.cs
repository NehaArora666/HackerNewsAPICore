using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using Xunit;
using HackerNews.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace HackerNews.Tests
{
    public class HackerNewsServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHandler;
        private readonly HttpClient _httpClient;
        private readonly Mock<ILogger<HackerNewsService>> _mockLogger;
        private readonly HackerNewsService _service;

        public HackerNewsServiceTests()
        {
            _mockHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHandler.Object);
            _mockLogger = new Mock<ILogger<HackerNewsService>>();
            _service = new HackerNewsService(_httpClient, _mockLogger.Object);
        }

        [Fact]
        public async Task FetchTopStoryIdsAsync_ReturnsTopStoryIds_WhenApiCallIsSuccessful()
        {
            // Arrange
            var topStoryIds = new List<int> { 1, 2, 3, 4, 5 };
            var jsonResponse = JsonConvert.SerializeObject(topStoryIds);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse)
            };

            _mockHandler.SetupRequest(HttpMethod.Get,
                "https://hacker-news.firebaseio.com/v0/topstories.json?print=pretty",
                response);

            // Act
            var result = await _service.FetchTopStoryIdsAsync();

            // Assert
            Assert.Equal(topStoryIds.Count, result.Count);
            Assert.Equal(topStoryIds[0], result[0]);
        }


        [Fact]
        public async Task FetchStoryDetailsAsync_ReturnsStory_WhenApiCallIsSuccessful()
        {
            // Arrange
            var story = new Story { Id = 1, Title = "Story 1", Url = "http://example.com" };
            var jsonResponse = JsonConvert.SerializeObject(story);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse)
            };

            _mockHandler.SetupRequest(
                HttpMethod.Get,
                "https://hacker-news.firebaseio.com/v0/item/1.json?print=pretty",
                response);

            // Act
            var result = await _service.FetchStoryDetailsAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(story.Title, result.Title);
            Assert.Equal(story.Url, result.Url);
        }

        [Fact]
        public async Task FetchStoryDetailsAsync_ReturnsNull_WhenApiCallFails()
        {
            // Arrange
            var storyId = 1;
            var url = $"https://hacker-news.firebaseio.com/v0/item/{storyId}.json?print=pretty";

            _mockHandler.SetupRequestThrows(HttpMethod.Get, url);

            // Act
            var result = await _service.FetchStoryDetailsAsync(storyId);

            // Assert
            Assert.Null(result);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error fetching story ID")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }

    }
}
