using LMS.Domain.Entities;
using LMS.Application.Contracts.DTOs;
using LMS.Application.Contracts.DTOs.Courses;
using Microsoft.AspNetCore.Authorization;
using LMS.Application;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapster;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS.API.Controllers
{
    [ApiController]
    [Route("api/courses")]
    [Authorize]
    public class CourseController : ControllerBase
    {
        private readonly CourseService _courseService;
        public CourseController(CourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet]
        /// <summary>
        /// Retrieves all courses with pagination and search.
        /// </summary>
        /// <param name="page">Page number (default 1)</param>
        /// <param name="pageSize">Items per page (default 10)</param>
        /// <param name="search">Search string for name/description</param>
        /// <returns>List of CourseDto</returns>
        [SwaggerOperation(Summary = "Get all courses", Description = "Returns a paginated list of courses. Requires Teacher or Student role.")]
        [Authorize(Roles = "Teacher,Student")]
        public async Task<ActionResult<object>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var (items, total) = await _courseService.GetPagedAsync(page, pageSize, search);
            var result = items.Adapt<List<CourseDto>>();
            return Ok(new
            {
                total,
                page,
                pageSize,
                items = result
            });
        }

        [HttpGet("{id}")]
        /// <summary>
        /// Retrieves a course with all its relationships.
        /// </summary>
        /// <param name="id">Course ID</param>
        /// <returns>CourseDto with modules, activities, and documents</returns>
        [SwaggerOperation(Summary = "Get course by ID", Description = "Returns a course with its modules, activities, and documents. Requires Teacher or Student role.")]
        [Authorize(Roles = "Teacher,Student")]
        public async Task<ActionResult<Course>> GetById(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
                return NotFound();
            var courseDto = course.Adapt<CourseDto>();
            return Ok(courseDto);
        }

        [HttpPost]
        /// <summary>
        /// Creates a new course.
        /// </summary>
        /// <param name="dto">DTO for creating course</param>
        /// <returns>Created course as CourseDto</returns>
        [SwaggerOperation(Summary = "Create course", Description = "Creates a new course. Teacher only.")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult> Create(CreateCourseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    error = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                });
            var course = new Course
            {
                Name = dto.Name,
                Description = dto.Description ?? string.Empty,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                TeacherId = dto.TeacherId
            };
            try
            {
                await _courseService.AddCourseAsync(course);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message
                });
            }
            var courseDto = new CourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                StartDate = course.StartDate,
                TeacherName = course.Teacher != null ? course.Teacher.UserName ?? "" : "",
                StudentNames = course.Students?.Select(s => s.UserName ?? "").ToList() ?? new List<string>(),
                Modules = course.Modules?.Select(m => new ModuleDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    StartDate = m.StartDate,
                    EndDate = m.EndDate,
                    Activities = m.Activities?.Select(a => new ActivityDto
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Type = a.Type,
                        Description = a.Description,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime,
                        Documents = a.Documents?.Select(d => new DocumentDto
                        {
                            Id = d.Id,
                            Name = d.Name,
                            Description = d.Description,
                            Timestamp = d.Timestamp,
                            UploadedByUserName = d.UploadedByUser != null ? d.UploadedByUser.UserName ?? "" : "",
                            LinkedEntityType = d.LinkedEntityType,
                            LinkedEntityId = d.LinkedEntityId
                        }).ToList() ?? new List<DocumentDto>()
                    }).ToList() ?? new List<ActivityDto>(),
                    Documents = m.Documents?.Select(d => new DocumentDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Description = d.Description,
                        Timestamp = d.Timestamp,
                        UploadedByUserName = d.UploadedByUser != null ? d.UploadedByUser.UserName ?? "" : "",
                        LinkedEntityType = d.LinkedEntityType,
                        LinkedEntityId = d.LinkedEntityId
                    }).ToList() ?? new List<DocumentDto>()
                }).ToList() ?? new List<ModuleDto>(),
                Documents = course.Documents?.Select(d => new DocumentDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    Timestamp = d.Timestamp,
                    UploadedByUserName = d.UploadedByUser != null ? d.UploadedByUser.UserName ?? "" : "",
                    LinkedEntityType = d.LinkedEntityType,
                    LinkedEntityId = d.LinkedEntityId
                }).ToList() ?? new List<DocumentDto>()
            };
            return CreatedAtAction(nameof(GetById), new
            {
                id = course.Id
            }, courseDto);
        }

        [HttpPut("{id}")]
        /// <summary>
        /// Uppdaterar en kurs.
        /// </summary>
        /// <param name="id">Kursens ID</param>
        /// <param name="dto">DTO för uppdatering</param>
        /// <returns>NoContent vid lyckad uppdatering</returns>
        [SwaggerOperation(Summary = "Uppdatera kurs", Description = "Uppdaterar en kurs. Endast Teacher.")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult> Update(int id, UpdateCourseDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new
                {
                    error = "ID i URL och body matchar inte."
                });
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    error = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                });
            try
            {
                var existing = await _courseService.GetCourseByIdAsync(id);
                if (existing == null)
                    return NotFound();
                existing.Name = dto.Name;
                existing.Description = dto.Description ?? string.Empty;
                existing.StartDate = dto.StartDate;
                existing.EndDate = dto.EndDate;
                existing.TeacherId = dto.TeacherId;
                await _courseService.UpdateCourseAsync(existing);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message
                });
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        /// <summary>
        /// Tar bort en kurs.
        /// </summary>
        /// <param name="id">Kursens ID</param>
        /// <returns>NoContent vid lyckad borttagning</returns>
        [SwaggerOperation(Summary = "Ta bort kurs", Description = "Tar bort en kurs. Endast Teacher.")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _courseService.DeleteCourseAsync(id);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message
                });
            }
            return NoContent();
        }
    }
}
