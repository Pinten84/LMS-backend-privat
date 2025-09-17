
using System.ComponentModel.DataAnnotations;
namespace LMS.Application.Contracts.DTOs.Courses
{
	public class UpdateCourseDto
	{
		[Required]
		public int Id { get; set; }

		[Required, StringLength(100)]
		public string Name { get; set; } = string.Empty;

		[StringLength(500)]
		public string? Description { get; set; }

		[Required]
		public DateTime StartDate { get; set; }

		public DateTime? EndDate { get; set; }

		public string? TeacherId { get; set; }
	}
}
