using System.ComponentModel.DataAnnotations;

namespace LMS.Application.Contracts.DTOs.Users;

public class UpdateUserDto
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string UserName { get; set; } = string.Empty;

    public string? Role
    {
        get; set;
    } // Optional rollbyte
}
