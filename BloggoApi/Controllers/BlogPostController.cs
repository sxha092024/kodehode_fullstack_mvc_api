using BloggoApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloggoApi.Controllers;

public class BlogPostController(ILogger<BlogPostController> logger, BlogPostService blogPostService) : ControllerBase
{
    private readonly ILogger _logger = logger;
    private readonly BlogPostService _blogPostService = blogPostService;
}