using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs.Books;

public class CreateBookDto
{
    [Required]
    public string Title { get; set; }
    [Required]
    public string Author { get; set; }
    [Required]
    public string ISBN { get; set; }
    public bool IsAvailable { get; set; } = true;
}
