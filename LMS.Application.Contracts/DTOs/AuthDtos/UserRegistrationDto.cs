using System.ComponentModel.DataAnnotations;
namespace LMS.Application.Contracts.DTOs.AuthDtos;
public record UserRegistrationDto
{
    [Required]
    public string Password { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string UserName { get; init; } = string.Empty;

    public string? Role { get; init; } = string.Empty;
}
