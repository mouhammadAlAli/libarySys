using AutoMapper;
using Domain.DTOs.Books;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositries;
using Domain.Services;

namespace Presentation.Services;

internal class BookService : DomainService<Book, BookDto, CreateBookDto>, IBookService
{
    private readonly IRepository<Borrowing> _borrowingRepository;
    public BookService(IRepository<Book> repository, IMapper mapper, IRepository<Borrowing> borrowingRepository) : base(repository, mapper)
    {
        _borrowingRepository = borrowingRepository;
    }

    public async Task BorrowBook(int bookId, int userId)
    {
        var book = await _repository.FirstOrDefaultAsync(x => x.Id == bookId);
        if (book == null)
            throw new NotFoundException(nameof(Book), bookId.ToString());
        if (!book.IsAvailable)
            throw new DomainException("not available book");
        book.IsAvailable = false;
        await _borrowingRepository.CreateAsync(new Borrowing { BookId = book.Id, UserId = userId, BorrowedDate = DateTime.UtcNow, ReturnedDate = null });
        await _repository.UpdateAsync(book);
    }

    public async Task ReturnBook(int bookId, int userId)
    {
        var borrow = await _borrowingRepository.FirstOrDefaultAsync(x => x.BookId == bookId && x.UserId == userId);
        if (borrow == null)
            throw new NotFoundException(nameof(Borrowing), bookId.ToString());
        var book = await _repository.FirstOrDefaultAsync(x => x.Id == bookId);
        book.IsAvailable = true;
        borrow.ReturnedDate = DateTime.UtcNow;
        await _borrowingRepository.UpdateAsync(borrow);
        await _repository.UpdateAsync(book);
    }
}
