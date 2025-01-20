using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs.Users;

public class CreateUserDto
{
    [Required]
    public string Name { get; set; }
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
}
