using BloggoApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloggoApi.Controllers;

public class UserController(ILogger<UserController> logger, UserService userService)
    : BaseV1ApiController
{
    private readonly ILogger _logger = logger;
    private readonly UserService _userService = userService;
}
