using System.ComponentModel.DataAnnotations;

namespace LMS.Application.Contracts.DTOs.Documents;

public class UpdateDocumentDto
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;
    [StringLength(1000)]
    public string? Description
    {
        get; set;
    }
    [StringLength(20)]
    public string? LinkedEntityType
    {
        get; set;
    }
    public int? LinkedEntityId
    {
        get; set;
    }
}
