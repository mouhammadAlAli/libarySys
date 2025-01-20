using Domain.Repositries.Common;

namespace Domain.DTOs.Books;

public class BookPageRequest : PageRequest
{
    public string? Keyword { get; set; }
    public string? ISBN { get; set; }
    public bool? IsAvailable { get; set; }
}
