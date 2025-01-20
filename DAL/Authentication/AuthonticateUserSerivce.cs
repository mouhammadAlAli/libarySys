using Domain.Authontication;
using Microsoft.AspNetCore.Http;

namespace DAL.Authontication;

internal class AuthonticateUserSerivce : IAuthonticateUserSerivce
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthonticateUserSerivce(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int GetUserId()
    {
        var user = _httpContextAccessor.HttpContext.User;
        if (user.Identity.IsAuthenticated)
        {
            var userId = user.Claims.First(c => c.Type == ITokenGenerator.Id).Value;
            return int.Parse(userId);
        }
        return 0;

    }
}
