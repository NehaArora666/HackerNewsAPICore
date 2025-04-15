using HackerNews.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HackerNewsAPICore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HackerNewsController : ControllerBase
    {
        private readonly ICachedHackerNews _hackerNewsService;

        public HackerNewsController(ICachedHackerNews hackerNewsService)
        {
            _hackerNewsService = hackerNewsService;
        }

        /// <summary>
        /// Retrieves a paginated list of top stories from Hacker News.
        /// </summary>
        /// <param name="page">The page number to retrieve. Defaults to 0 (first page).</param>
        /// <param name="pageSize">The number of stories per page. Defaults to 10.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing a paginated list of top stories and the total count.
        /// Returns a 500 status code if an error occurs.
        /// </returns>
        /// <remarks>
        /// This endpoint fetches the top stories using the service layer, applies pagination, and returns the result
        /// as a JSON object with <c>items</c> and <c>totalCount</c> properties.
        /// </remarks>

        [HttpGet("top-stories")]
        public async Task<IActionResult> GetTopStories([FromQuery] int page = 0, [FromQuery] int pageSize = 10)
        {
            try
            {
                var topStories = await _hackerNewsService.GetTopStoriesAsync();
                var paginatedStories = topStories
                    .Skip(page * pageSize)
                    .Take(pageSize)
                    .ToList();
                var totalCount = topStories.Count();
                return Ok(new { items = paginatedStories, totalCount = totalCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
