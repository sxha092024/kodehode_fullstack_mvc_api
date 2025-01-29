using BloggoApi.Contexts;

namespace BloggoApi.Services;

public class BlogPostService(ILogger<BlogPostService> logger, SqliteDbContext context)
{
    private readonly ILogger<BlogPostService> _logger = logger;
    private readonly SqliteDbContext db = context;
}
