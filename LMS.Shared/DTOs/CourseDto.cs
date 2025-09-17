
using System.ComponentModel.DataAnnotations;
namespace LMS.Shared.DTOs;

public class CourseDto
{
    public int Id
    {
        get; set;
    }
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    [Required]
    public DateTime StartDate
    {
        get; set;
    }
    public string TeacherName { get; set; } = string.Empty;
    public List<ModuleDto> Modules { get; set; } = new();
    public List<string> StudentNames { get; set; } = new();
    public List<DocumentDto> Documents { get; set; } = new();
}
