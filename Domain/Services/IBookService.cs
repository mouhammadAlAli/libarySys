using Domain.DTOs.Books;
using Domain.Entities;

namespace Domain.Services;

public interface IBookService : IDomainService<Book, BookDto, CreateBookDto>
{
    public Task BorrowBook(int bookId, int userId);
    public Task ReturnBook(int bookId, int userId);
}
