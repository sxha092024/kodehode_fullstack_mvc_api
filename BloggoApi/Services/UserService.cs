using BloggoApi.Contexts;
using BloggoApi.DTO;
using BloggoApi.Models;

namespace BloggoApi.Services;

public class UserService(ILogger<UserService> logger, SqliteDbContext context)
{
    private readonly ILogger<UserService> _logger = logger;
    private readonly SqliteDbContext db = context;
}
