using BloggoApi.Contexts;
using BloggoApi.Models;

public class DeletionService(ILogger<DeletionService> logger, SqliteDbContext context)
    : BackgroundService
{
    private readonly ILogger<DeletionService> _logger = logger;
    private readonly SqliteDbContext db = context;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DeletionService is starting");
        stoppingToken.Register(() =>
        {
            _logger.LogInformation("DeletionService background task is stopping.");
        });

        while (!stoppingToken.IsCancellationRequested)
        {
            var overdue = db.ScheduledDeletions.Where(d => d._when <= DateTimeOffset.UtcNow);

            foreach (var item in overdue)
            {
                if (item.User is User user)
                {
                    _logger.LogCritical("deleting records associted with user {}", user.UserId);
                    db.Remove(user);
                    foreach (var authoredPost in user.AuthoredPosts)
                    {
                        _logger.LogCritical(
                            "deleting blogpost with id {}",
                            authoredPost.BlogPostId
                        );
                        db.Remove(authoredPost);
                    }
                }
                if (item.Post is BlogPost post)
                {
                    foreach (var author in post.Authors)
                    {
                        _logger.LogCritical(
                            "removing post {} from author {}",
                            post.BlogPostId,
                            author.UserId
                        );
                        author.AuthoredPosts.Remove(post);
                    }
                    ;
                    _logger.LogCritical("deleting blog post {}", post.BlogPostId);
                    db.Remove(post);
                }
                // ensure we don't re-fire events by keeping old schedules around
                db.Remove(item);
            }
            // expeditiously propogate deletions by attempting to commit the transaction now
            await db.SaveChangesAsync();
            // TODO: turn into injected variable to allow tuning
            Thread.Sleep(1000 * 60 * 10);
        }
        _logger.LogInformation("cancelling {} background thread", this);
    }
}
