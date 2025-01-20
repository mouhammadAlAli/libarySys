using Domain.Services;
using Presentation.Services;

namespace Presentation;

public static class DependencyInjection
{
    public static IServiceCollection InjectPresentation(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection));
        services.AddScoped(typeof(IDomainService<,,>), typeof(DomainService<,,>));
        services.AddScoped(typeof(IBookService), typeof(BookService));
        services.AddScoped(typeof(IUserService), typeof(UserService));
        return services;
    }
}
