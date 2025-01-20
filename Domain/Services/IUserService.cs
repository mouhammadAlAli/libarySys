using Domain.DTOs.Users;
using Domain.Entities;

namespace Domain.Services;

public interface IUserService : IDomainService<User, UserDto, CreateUserDto>
{
    Task<string> Login(string username, string password);
}
