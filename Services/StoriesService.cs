using Microsoft.Extensions.Caching.Memory;
using Stories.Interfaces;
using Stories.Models;
using System.Text.Json;

namespace Stories.Service
{
    public class StoriesService : IStoriesService
    {
        private const string storiesCacheKey = "storiesList";
        private readonly IHttpClientFactory _httpClientFactory;
        private IMemoryCache _memoryCache;

        public StoriesService(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
        {
            _httpClientFactory = httpClientFactory;
            _memoryCache = memoryCache;
        }

        public async Task<IEnumerable<Story>> GetStories()
        {
            var storyIds = new List<int>();

            if (!_memoryCache.TryGetValue(storiesCacheKey, out List<Story> stories))
            {

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://hacker-news.firebaseio.com/v0/newstories.json");

                var httpClient = _httpClientFactory.CreateClient();

                var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    using var contentStream =
                        await httpResponseMessage.Content.ReadAsStreamAsync();

                    storyIds = await JsonSerializer.DeserializeAsync<List<int>>(contentStream);
                }

                await Parallel.ForEachAsync(storyIds, async (id, _) =>
                {
                    var story = await GetStoryById(id);

                    if (stories == null)
                    {
                        stories = new List<Story>();
                    }

                    if (story != null)
                    {
                        stories.Add(story);
                    }
                });

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(300))
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                        .SetPriority(CacheItemPriority.Normal)
                        .SetSize(1024);

                _memoryCache.Set(storiesCacheKey, stories, cacheEntryOptions);
            }

            return stories;
        }

        public async Task<Story> GetStoryById(int id)
        {
            var story = new Story();

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://hacker-news.firebaseio.com/v0/item/{id}.json?print=pretty");

            var httpClient = _httpClientFactory.CreateClient();

            var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var contentStream =
                    await httpResponseMessage.Content.ReadAsStringAsync();

                story = JsonSerializer.Deserialize<Story>(contentStream);
            }

            return story;
        }
    }
}
