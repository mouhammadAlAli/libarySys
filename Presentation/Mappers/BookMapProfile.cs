using AutoMapper;
using Domain.DTOs.Books;
using Domain.Entities;

namespace Presentation.Mappers;

internal class BookMapProfile : Profile
{
    public BookMapProfile()
    {
        CreateMap<CreateBookDto, Book>().ForMember(dest => dest.Id, opt => opt.Ignore());
        CreateMap<Book, BookDto>();
    }
}
