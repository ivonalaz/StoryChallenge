using System.Text.Json.Serialization;

namespace Stories.Models
{
    public class Story
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}