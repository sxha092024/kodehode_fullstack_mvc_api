using BloggoApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloggoApi.Controllers;

public class LoginController(ILogger<LoginController> logger, UserService userService)
    : BaseV1ApiController
{
    private readonly ILogger _logger = logger;
    private readonly UserService _userService = userService;
}
