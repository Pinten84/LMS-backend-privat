using LMS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using LMS.Application;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using LMS.Application.Contracts.DTOs;
using LMS.Application.Contracts.DTOs.Activities;
using Mapster;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS.API.Controllers
{
    [ApiController]
    [Route("api/activities")]
    [Authorize]
    public class ActivityController : ControllerBase
    {
        private readonly ActivityService _activityService;
        public ActivityController(ActivityService activityService)
        {
            _activityService = activityService;
        }

        [HttpGet]
        /// <summary>
        /// Retrieves all activities with pagination and search.
        /// </summary>
        /// <param name="page">Page number (default 1)</param>
        /// <param name="pageSize">Items per page (default 10)</param>
        /// <param name="search">Search string for name/description</param>
        /// <returns>List of ActivityDto</returns>
        [SwaggerOperation(Summary = "Get all activities", Description = "Returns a paginated list of activities. Requires Teacher or Student role.")]
        [Authorize(Roles = "Teacher,Student")]
        public async Task<ActionResult<object>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var (items, total) = await _activityService.GetPagedAsync(page, pageSize, search);
            var result = items.Adapt<List<ActivityDto>>();
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
        /// Retrieves an activity with all its documents.
        /// </summary>
        /// <param name="id">Activity ID</param>
        /// <returns>ActivityDto with documents</returns>
        [SwaggerOperation(Summary = "Get activity by ID", Description = "Returns an activity with its documents. Requires Teacher or Student role.")]
        [Authorize(Roles = "Teacher,Student")]
        public async Task<ActionResult<Activity>> GetById(int id)
        {
            var activity = await _activityService.GetActivityByIdAsync(id);
            if (activity == null)
                return NotFound();
            var activityDto = activity.Adapt<ActivityDto>();
            return Ok(activityDto);
        }

        [HttpPost]
        /// <summary>
        /// Creates a new activity.
        /// </summary>
        /// <param name="dto">DTO for creating activity</param>
        /// <returns>Created activity as ActivityDto</returns>
        [SwaggerOperation(Summary = "Create activity", Description = "Creates a new activity. Teacher only.")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult> Create(CreateActivityDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    error = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                });
            var activity = new Activity
            {
                Name = dto.Name,
                Type = dto.Type,
                Description = dto.Description ?? string.Empty,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                ModuleId = dto.ModuleId
            };
            try
            {
                await _activityService.AddActivityAsync(activity);
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
            return CreatedAtAction(nameof(GetById), new
            {
                id = activity.Id
            }, activity);
        }

        [HttpPut("{id}")]
        /// <summary>
        /// Updates an activity.
        /// </summary>
        /// <param name="id">Activity ID</param>
        /// <param name="dto">DTO for update</param>
        /// <returns>NoContent on successful update</returns>
        [SwaggerOperation(Summary = "Update activity", Description = "Updates an activity. Teacher only.")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult> Update(int id, UpdateActivityDto dto)
        {
            // No Id in update DTO beyond route; rely on route id
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    error = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                });
            try
            {
                var existing = await _activityService.GetActivityByIdAsync(id);
                if (existing == null)
                    return NotFound();
                existing.Name = dto.Name;
                existing.Type = dto.Type;
                existing.Description = dto.Description ?? string.Empty;
                existing.StartTime = dto.StartTime;
                existing.EndTime = dto.EndTime;
                await _activityService.UpdateActivityAsync(existing);
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
        /// Deletes an activity.
        /// </summary>
        /// <param name="id">Activity ID</param>
        /// <returns>NoContent on successful deletion</returns>
        [SwaggerOperation(Summary = "Delete activity", Description = "Deletes an activity. Teacher only.")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _activityService.DeleteActivityAsync(id);
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
