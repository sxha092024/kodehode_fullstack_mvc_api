using System.ComponentModel.DataAnnotations;

namespace BloggoApi.Models;

public class BlogPost
{
    [MaxLength(36)]
    public Guid BlogPostId { get; set; }

    [MaxLength(36)]
    public Guid OwnerId { get; set; }
    public ICollection<User> Authors { get; set; } = [];
    public User Owner { get; set; } = null!;
    public required string Title { get; set; } = string.Empty;
    public required string Content { get; set; } = string.Empty;
    public required bool Published { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required DateTimeOffset? ModifiedAt { get; set; }
}
