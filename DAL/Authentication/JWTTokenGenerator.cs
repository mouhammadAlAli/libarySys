using Domain.Authontication;
using Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Authontication;

internal class JWTTokenGenerator : ITokenGenerator
{
    private readonly JwtSettings _jwtSettings;


    public JWTTokenGenerator(IOptions<JwtSettings> jwtOptinos)
    {
        _jwtSettings = jwtOptinos.Value;
    }
    public string GenerateToken(User user)
    {
        byte[] secretKeyBytes = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(secretKeyBytes),
            SecurityAlgorithms.HmacSha256
        );

        var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Name, user.Name),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ITokenGenerator.Id, user.Id.ToString())
    };

        var securityToken = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            claims: claims,
            signingCredentials: signingCredentials
        );

        // Return the serialized token
        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }
}
