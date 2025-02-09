using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/[controller]/[action]", Name = "[controller]_[action]")]
public abstract class BaseV1ApiController : ControllerBase { }
