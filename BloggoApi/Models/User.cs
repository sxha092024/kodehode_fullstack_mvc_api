using Microsoft.EntityFrameworkCore;

namespace BloggoApi.Models;

[Index(nameof(UserName), IsUnique = true)]
public class User
{
    public Guid UserId { get; set; } = Guid.CreateVersion7();
    public required string UserName { get; set; } = string.Empty;
    public required string HashedPassword { get; set; } = string.Empty;
    public ICollection<BlogPost> OwnedPosts { get; set; } = [];
    public ICollection<BlogPost> AuthoredPosts { get; set; } = [];
}
