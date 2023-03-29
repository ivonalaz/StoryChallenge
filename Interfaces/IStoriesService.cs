using Stories.Models;

namespace Stories.Interfaces
{
    public interface IStoriesService
    {
        public Task<IEnumerable<Story>> GetStories();
        public Task<Story> GetStoryById(int id);
    }
}
