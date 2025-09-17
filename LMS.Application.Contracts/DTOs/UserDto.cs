
using System.ComponentModel.DataAnnotations;
namespace LMS.Application.Contracts.DTOs
{
	public class UserDto
	{
		[Required]
		public string Id { get; set; } = string.Empty;

		[Required]
		[StringLength(100)]
		public string Name { get; set; } = string.Empty;

		[Required]
		[EmailAddress]
		public string Email { get; set; } = string.Empty;

		[Required]
		[StringLength(50)]
		public string Role { get; set; } = string.Empty;
	}
}
