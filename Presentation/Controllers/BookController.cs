using Domain.Authontication;
using Domain.DTOs.Books;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositries.Common;
using Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookController : ControllerBase
{
    private readonly IBookService _bookService;
    private readonly IAuthonticateUserSerivce _authonticateUserSerivce;
    public BookController(IBookService bookService, IAuthonticateUserSerivce authonticateUserSerivce)
    {
        _bookService = bookService;
        _authonticateUserSerivce = authonticateUserSerivce;
    }
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateBookDto createBookDto)
    {
        await _bookService.CreateAsync(createBookDto);
        return Ok("Book created successfully.");
    }
    [Authorize]
    [HttpPost("BorrowBook")]
    public async Task<IActionResult> BorrowBookAsync(int bookId)
    {
        var userId = _authonticateUserSerivce.GetUserId();
        await _bookService.BorrowBook(bookId, userId);
        return Ok();
    }
    [Authorize]
    [HttpPost("ReturnBook")]
    public async Task<IActionResult> ReturnBook(int bookId)
    {
        var userId = _authonticateUserSerivce.GetUserId();
        await _bookService.ReturnBook(bookId, userId);
        return Ok();
    }
    [HttpGet("GetPage")]
    public async Task<ActionResult<PageResult<BookDto>>> GetPage([FromQuery] BookPageRequest bookPageRequest)
    {
        var searchCriteria = new Dictionary<string, object>();

        // Add keyword search for the title
        if (!string.IsNullOrEmpty(bookPageRequest.Keyword))
        {
            searchCriteria.Add("Title", bookPageRequest.Keyword);
        }

        // Add search for ISBN if provided
        if (!string.IsNullOrEmpty(bookPageRequest.ISBN))
        {
            searchCriteria.Add("ISBN", bookPageRequest.ISBN);
        }
        Expression<Func<Book, bool>>? filter = null;
        if (bookPageRequest.IsAvailable.HasValue)
        {
            bool isAvailable = bookPageRequest.IsAvailable.Value;
            filter = (Expression<Func<Book, bool>>)(book => book.IsAvailable == isAvailable);
        }
        var result = await _bookService.GetPageAsync(bookPageRequest, filter, searchCriteria);
        return Ok(result);
    }
    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        await _bookService.DeleteAsync(id);
        return Ok("Book deleted successfully.");
    }
    [HttpPut]
    public async Task<IActionResult> Update(int id, CreateBookDto createBookDto)
    {
        await _bookService.UpdateAsync(id, createBookDto);
        return Ok("Book updated successfully");
    }
    [HttpGet]
    public async Task<ActionResult<BookDto>> Get([FromQuery] int id)
    {
        var result = await _bookService.FirstOrDefaultAsync(x => x.Id == id);
        if (result == null)
        {
            throw new NotFoundException(nameof(Book), id.ToString());
        }
        return Ok(result);
    }
}
