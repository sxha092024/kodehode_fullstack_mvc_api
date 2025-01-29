using System.Text.Json.Serialization;

namespace BloggoApi.DTO;

public class UserDTO
{
    [JsonPropertyName("id")]
    public string UserId { get; set; }

    [JsonPropertyName("username")]
    public required string UserName { get; set; }
}
