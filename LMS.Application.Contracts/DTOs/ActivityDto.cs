using System.ComponentModel.DataAnnotations;
namespace LMS.Application.Contracts.DTOs;

public class ActivityDto
{
    public int Id
    {
        get; set;
    }
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty;
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    [Required]
    public DateTime StartTime
    {
        get; set;
    }
    [Required]
    public DateTime EndTime
    {
        get; set;
    }
    public List<DocumentDto> Documents { get; set; } = new();
}
