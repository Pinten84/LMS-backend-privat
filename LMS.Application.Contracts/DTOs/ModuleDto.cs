
using System.ComponentModel.DataAnnotations;
namespace LMS.Application.Contracts.DTOs
{
	public class ModuleDto
	{
		public int Id { get; set; }

		[Required]
		[StringLength(100)]
		public string Name { get; set; } = string.Empty;

		[StringLength(500)]
		public string Description { get; set; } = string.Empty;

		[Required]
		public DateTime StartDate { get; set; }

		[Required]
		public DateTime EndDate { get; set; }

		public List<ActivityDto> Activities { get; set; } = new();
		public List<DocumentDto> Documents { get; set; } = new();
	}
}
