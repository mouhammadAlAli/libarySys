using AutoMapper;
using Domain.Authontication;
using Domain.DTOs.Users;
using Domain.Entities;

namespace Presentation.Mappers;

internal class UserMapProfile : Profile
{
    public UserMapProfile()
    {
        CreateMap<CreateUserDto, User>().ForMember(x => x.Password, src => src.MapFrom(x => PasswordHasher.HashPassword(x.Password)));
        CreateMap<User, UserDto>();
    }
}
