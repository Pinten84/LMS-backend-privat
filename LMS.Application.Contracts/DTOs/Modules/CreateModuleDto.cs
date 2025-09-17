using System.ComponentModel.DataAnnotations;

namespace LMS.Application.Contracts.DTOs.Modules;

public class CreateModuleDto
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
    [StringLength(500)]
    public string? Description
    {
        get; set;
    }
    [Required]
    public DateTime StartDate
    {
        get; set;
    }
    [Required]
    public DateTime EndDate
    {
        get; set;
    }
    [Required]
    public int CourseId
    {
        get; set;
    }
}
