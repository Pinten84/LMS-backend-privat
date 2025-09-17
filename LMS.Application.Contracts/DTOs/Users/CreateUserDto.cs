using System.ComponentModel.DataAnnotations;

namespace LMS.Application.Contracts.DTOs.Users;

public class CreateUserDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty; // Teacher or Student (Admin restriktivt)
}
