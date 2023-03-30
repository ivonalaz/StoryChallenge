using Microsoft.AspNetCore.Mvc;
using Stories.Interfaces;
using Stories.Models;

namespace Stories.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StoriesController : ControllerBase
    {
        private readonly IStoriesService _storiesService;

        public StoriesController(IStoriesService storiesService)
        {
            _storiesService = storiesService;
        }

        // GET: api/<StoriesController>
        [HttpGet]
        public Task<IEnumerable<Story>> Get()
        {
            return _storiesService.GetStories();
        }
    }
}
