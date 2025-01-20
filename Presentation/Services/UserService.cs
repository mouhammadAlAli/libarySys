using AutoMapper;
using Domain.Authontication;
using Domain.DTOs.Users;
using Domain.Entities;
using Domain.Repositries;
using Domain.Services;

namespace Presentation.Services;

internal class UserService : DomainService<User, UserDto, CreateUserDto>, IUserService
{
    private readonly ITokenGenerator _tokenGenerator;
    public UserService(IRepository<User> repository, IMapper mapper, ITokenGenerator tokenGenerator) : base(repository, mapper)
    {
        _tokenGenerator = tokenGenerator;
    }

    public async Task<string> Login(string username, string password)
    {
        var user = await _repository.FirstOrDefaultAsync(c => c.Email.ToLower() == username.ToLower()) ?? throw new Exception("user not exists");
        if (!PasswordHasher.VerifyPassword(password, user.Password))
        {
            throw new Exception();
        }
        return _tokenGenerator.GenerateToken(user);
    }
}
