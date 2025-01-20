using Domain.Authontication;
using Domain.DTOs.Users;
using Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthonticateUserSerivce _authonticateUserSerivce;

    public AuthController(IUserService userService, IAuthonticateUserSerivce authonticateUserSerivce)
    {
        _userService = userService;
        _authonticateUserSerivce = authonticateUserSerivce;
    }

    [HttpPost(Name = "Login")]
    public async Task<ActionResult<string>> LoginAsync(string username, string password)
    {
        return Ok(await _userService.Login(username, password));
    }
    [Authorize]
    [HttpGet("GetProfile")]
    public async Task<ActionResult<UserDto>> GetUserProfile()
    {
        var userId = _authonticateUserSerivce.GetUserId();
        var result = await _userService.FirstOrDefaultAsync(x => x.Id == userId);
        return Ok(result);

    }

}
