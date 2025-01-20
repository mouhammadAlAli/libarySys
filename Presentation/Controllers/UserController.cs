using Domain.DTOs.Users;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    [HttpPost("Register")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUserDto createUserDto)
    {
        await _userService.CreateAsync(createUserDto);
        return Ok("Book created successfully.");
    }
}
