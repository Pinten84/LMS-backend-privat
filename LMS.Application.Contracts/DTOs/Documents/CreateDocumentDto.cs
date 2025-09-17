using System.ComponentModel.DataAnnotations;

namespace LMS.Application.Contracts.DTOs.Documents
{
	public class CreateDocumentDto
	{
		[Required, StringLength(200)]
		public string Title { get; set; } = string.Empty;

		[StringLength(1000)]
		public string? Description { get; set; }

		// Ange vilken typ av entitet dokumentet kopplas till (Course | Module | Activity)
		[Required, StringLength(20)]
		public string LinkedEntityType { get; set; } = string.Empty;

		[Required]
		public int LinkedEntityId { get; set; }
	}
}
