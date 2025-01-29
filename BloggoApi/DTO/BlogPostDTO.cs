using System.Text.Json.Serialization;

namespace BloggoApi.DTO;

public class BlogPostDTO
{
    [JsonPropertyName("id")]
    public string BlogPostId { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("content")]
    public required string Content { get; set; }

    [JsonPropertyName("authors")]
    public List<string> Authors { get; set; }

    [JsonPropertyName("published")]
    public required bool Published { get; set; }
}
