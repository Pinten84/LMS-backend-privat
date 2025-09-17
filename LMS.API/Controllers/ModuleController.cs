using LMS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using LMS.Application;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using LMS.Application.Contracts.DTOs;
using LMS.Application.Contracts.DTOs.Modules;
using Mapster;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS.API.Controllers
{
    [ApiController]
    [Route("api/modules")]
    [Authorize]
    public class ModuleController : ControllerBase
    {
        private readonly ModuleService _moduleService;
        public ModuleController(ModuleService moduleService)
        {
            _moduleService = moduleService;
        }

        [HttpGet]
        /// <summary>
        /// Hämtar alla moduler med paginering och sökning.
        /// </summary>
        /// <param name="page">Sidnummer (default 1)</param>
        /// <param name="pageSize">Antal per sida (default 10)</param>
        /// <param name="search">Söksträng för namn/beskrivning</param>
        /// <returns>Lista av ModuleDto</returns>
        [SwaggerOperation(Summary = "Hämta alla moduler", Description = "Returnerar en paginerad lista av moduler. Kräver Teacher eller Student.")]
        [Authorize(Roles = "Teacher,Student")]
        public async Task<ActionResult<object>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var (items, total) = await _moduleService.GetPagedAsync(page, pageSize, search);
            var result = items.Adapt<List<ModuleDto>>();
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
        /// Hämtar en modul med alla dess relationer.
        /// </summary>
        /// <param name="id">Modulens ID</param>
        /// <returns>ModuleDto med aktiviteter och dokument</returns>
        [SwaggerOperation(Summary = "Hämta modul med ID", Description = "Returnerar en modul med dess aktiviteter och dokument. Kräver Teacher eller Student.")]
        [Authorize(Roles = "Teacher,Student")]
        public async Task<ActionResult<Module>> GetById(int id)
        {
            var module = await _moduleService.GetModuleByIdAsync(id);
            if (module == null)
                return NotFound();
            var moduleDto = module.Adapt<ModuleDto>();
            return Ok(moduleDto);
        }

        [HttpPost]
        /// <summary>
        /// Skapar en ny modul.
        /// </summary>
        /// <param name="dto">DTO för att skapa modul</param>
        /// <returns>Skapad modul som ModuleDto</returns>
        [SwaggerOperation(Summary = "Skapa modul", Description = "Skapar en ny modul. Endast Teacher.")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult> Create(CreateModuleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    error = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                });
            var module = new Module
            {
                Name = dto.Name,
                Description = dto.Description ?? string.Empty,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CourseId = dto.CourseId
            };
            try
            {
                await _moduleService.AddModuleAsync(module);
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
                id = module.Id
            }, module);
        }

        [HttpPut("{id}")]
        /// <summary>
        /// Uppdaterar en modul.
        /// </summary>
        /// <param name="id">Modulens ID</param>
        /// <param name="dto">DTO för uppdatering</param>
        /// <returns>NoContent vid lyckad uppdatering</returns>
        [SwaggerOperation(Summary = "Uppdatera modul", Description = "Uppdaterar en modul. Endast Teacher.")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult> Update(int id, UpdateModuleDto dto)
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
                var existing = await _moduleService.GetModuleByIdAsync(id);
                if (existing == null)
                    return NotFound();
                existing.Name = dto.Name;
                existing.Description = dto.Description ?? string.Empty;
                existing.StartDate = dto.StartDate;
                existing.EndDate = dto.EndDate;
                await _moduleService.UpdateModuleAsync(existing);
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
        /// Tar bort en modul.
        /// </summary>
        /// <param name="id">Modulens ID</param>
        /// <returns>NoContent vid lyckad borttagning</returns>
        [SwaggerOperation(Summary = "Ta bort modul", Description = "Tar bort en modul. Endast Teacher.")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _moduleService.DeleteModuleAsync(id);
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
