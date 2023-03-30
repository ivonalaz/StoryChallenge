using Microsoft.Extensions.Caching.Memory;
using Stories.Interfaces;
using Stories.Models;
using System.Text.Json;

namespace Stories.Service
{
    public class StoriesService : IStoriesService
    {
        private const string storiesCacheKey = "storiesList";
        private IMemoryCache _memoryCache;
        private HttpClient httpClient;

        public StoriesService(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            httpClient = httpClientFactory.CreateClient("hackerNews");
        }

        public async Task<IEnumerable<Story>> GetStories()
        {
            var storyIds = new List<int>();

            if (!_memoryCache.TryGetValue(storiesCacheKey, out List<Story> stories))
            {

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://hacker-news.firebaseio.com/v0/newstories.json");

                var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    using var contentStream =
                        await httpResponseMessage.Content.ReadAsStreamAsync();

                    storyIds = await JsonSerializer.DeserializeAsync<List<int>>(contentStream);
                }

                var tasks = storyIds.Select(id => GetStoryById(id));
                stories = (await Task.WhenAll(tasks)).Where(s => s != null).ToList();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(300))
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                        .SetPriority(CacheItemPriority.Normal)
                        .SetSize(1024);

                _memoryCache.Set(storiesCacheKey, stories, cacheEntryOptions);
            }

            return stories;
        }

        public virtual async Task<Story> GetStoryById(int id)
        {
            var story = new Story();

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://hacker-news.firebaseio.com/v0/item/{id}.json?print=pretty");

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
