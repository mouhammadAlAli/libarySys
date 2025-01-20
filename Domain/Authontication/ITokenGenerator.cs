
using Domain.Entities;

namespace Domain.Authontication;

public interface ITokenGenerator
{
    public const string Id = "id";
    string GenerateToken(User user);
}
