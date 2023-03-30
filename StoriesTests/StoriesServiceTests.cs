using MemoryCache.Testing.Moq;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Protected;
using Stories.Models;
using Stories.Service;
using System.Net;
using System.Text.Json;

[TestClass]
public class StoriesServiceTests
{
    private Mock<IHttpClientFactory> _httpClientFactoryMock;
    private StoriesService storiesService;
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;

    [TestInitialize]
    public void Initialize()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
    }

    [TestMethod]
    public async Task GetStories_HasCachedStories_ShouldReturnStoriesFromCache()
    {
        // Arrange
        object expectedStories = new List<Story> {
            new Story { Id = 1 },
            new Story { Id = 2 }
        };

        var mockedCache = Create.MockedMemoryCache();

        mockedCache.Set("storiesList", expectedStories, new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(300))
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                        .SetPriority(CacheItemPriority.Normal)
                        .SetSize(1024));

        storiesService = new StoriesService(_httpClientFactoryMock.Object, mockedCache);

        // Act
        var actualStories = await storiesService.GetStories();

        // Assert
        CollectionAssert.AreEqual((List<Story>)expectedStories, (List<Story>)actualStories);
    }

    [TestMethod]
    public async Task GetStories_DoesNotHaveCachedStories_ShouldReturnStoriesFromApi()
    {
        // Arrange
        var expectedStories = new List<Story> {
            new Story { Id = 1 },
            new Story { Id = 2 }
        };

        var storyIds = new List<int> { 1, 2 };

        HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(storyIds))
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
        ItExpr.IsAny<CancellationToken>()
                        )
            .ReturnsAsync(result)
            .Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://www.hackernews.com/")
        };

        _httpClientFactoryMock.Setup(_ => _.CreateClient("hackerNews")).Returns(httpClient);

        var mockedCache = Create.MockedMemoryCache();

        var storiesServiceMock = new Mock<StoriesService>(_httpClientFactoryMock.Object, mockedCache) { CallBase = true };

        storiesServiceMock.Setup(x => x.GetStoryById(1)).Returns(Task.FromResult(expectedStories.First(s => s.Id == 1)));
        storiesServiceMock.Setup(x => x.GetStoryById(2)).Returns(Task.FromResult(expectedStories.First(s => s.Id == 2)));

        // Act
        var actualStories = await storiesServiceMock.Object.GetStories();

        // Assert
        CollectionAssert.AreEqual(expectedStories, actualStories.ToList());
        storiesServiceMock.Verify(x => x.GetStoryById(It.IsAny<int>()), Times.Exactly(2));
        
    }

    [TestMethod]
    public async Task GetStoryById_ApiReturnsSuccessResult_ShouldReturnStory()
    {
        // Arrange
        var expectedStory = new Story { Id = 1 };

        HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(expectedStory))
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
        ItExpr.IsAny<CancellationToken>()
                        )
            .ReturnsAsync(result)
            .Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://www.hackernews.com/")
        };

        _httpClientFactoryMock.Setup(_ => _.CreateClient("hackerNews")).Returns(httpClient);

        var mockedCache = Create.MockedMemoryCache();

        var storiesService = new StoriesService(_httpClientFactoryMock.Object, mockedCache);

        // Act
        var actualStory = await storiesService.GetStoryById(expectedStory.Id);

        // Assert
        Assert.AreEqual(expectedStory.Id, actualStory.Id);
    }

    [TestMethod]
    public async Task GetStoryById_ApiDoesNotReturnStory_ShouldReturnEmptyStory()
    {
        // Arrange
        var expectedStory = new Story();

        HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(expectedStory))
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
        ItExpr.IsAny<CancellationToken>()
                        )
            .ReturnsAsync(result)
            .Verifiable();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://www.hackernews.com/")
        };

        _httpClientFactoryMock.Setup(_ => _.CreateClient("hackerNews")).Returns(httpClient);

        var mockedCache = Create.MockedMemoryCache();

        var storiesService = new StoriesService(_httpClientFactoryMock.Object, mockedCache);

        // Act
        var actualStory = await storiesService.GetStoryById(expectedStory.Id);

        // Assert
        Assert.AreEqual(expectedStory.Id, actualStory.Id);
    }
}