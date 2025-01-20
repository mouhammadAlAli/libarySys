using DAL.Authontication;
using DAL.Repositories;
using Domain.Authontication;
using Domain.Repositries;
using Infrastructure.Authontication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;




namespace DAL.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection InjectInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        var jwtConfigurationSection = configuration.GetSection(JwtSettings.SectionName);
        var jwtSettings = jwtConfigurationSection.Get<JwtSettings>();

        services.Configure<JwtSettings>(options =>
        {
            options.Issuer = jwtSettings.Issuer;
            options.Secret = jwtSettings.Secret;
            options.Audience = jwtSettings.Audience;
            options.ExpiryMinutes = jwtSettings.ExpiryMinutes;
        });
        services.AddScoped<ITokenGenerator, JWTTokenGenerator>();
        services.AddScoped<IAuthonticateUserSerivce, AuthonticateUserSerivce>();
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidIssuer = jwtSettings.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),

                };
            });
        services.AddAuthorization();

        return services;
    }
}
