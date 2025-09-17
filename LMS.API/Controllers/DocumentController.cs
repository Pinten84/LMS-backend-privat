using System.Collections.Generic;
using System.Threading.Tasks;
using LMS.Application;
using LMS.Application.Contracts.DTOs;
using LMS.Application.Contracts.DTOs.Documents;
using LMS.Domain.Entities;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS.API.Controllers
{
    [ApiController]
    [Route("api/documents")]
    [Authorize]
    public class DocumentController : ControllerBase
    {
        private readonly DocumentService _documentService;
        public DocumentController(DocumentService documentService)
        {
            _documentService = documentService;
        }
        [HttpGet]
        /// Retrieves all documents with pagination and search.
        /// </summary>
        /// <param name="page">Page number (default 1)</param>
        /// <param name="pageSize">Items per page (default 10)</param>
        /// <param name="search">Search string for name/description</param>
        /// <returns>List of DocumentDto</returns>
        [SwaggerOperation(Summary = "Get all documents", Description = "Returns a paginated list of documents. Requires Teacher or Student role.")]
        [Authorize(Roles = "Teacher,Student")]
        public async Task<ActionResult<object>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var (items, total) = await _documentService.GetPagedAsync(page, pageSize, search);
            var result = items.Select(d => new DocumentDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                Timestamp = d.Timestamp,
                UploadedByUserName = d.UploadedByUser?.UserName ?? string.Empty,
                LinkedEntityType = d.LinkedEntityType,
                LinkedEntityId = d.LinkedEntityId
            }).ToList();
            return Ok(new
            {
                total,
                page,
                pageSize,
                items = result
            });
        }
        [HttpGet("{id}")]
        [HttpDelete("{id}")]
        /// <summary>
        /// Retrieves a document.
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <returns>DocumentDto</returns>
        [SwaggerOperation(Summary = "Get document by ID", Description = "Returns a document. Requires Teacher or Student role.")]
        [Authorize(Roles = "Teacher,Student")]
        public async Task<ActionResult<Document>> GetById(int id)
        {
            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
                return NotFound();
            var documentDto = document.Adapt<DocumentDto>();
            return Ok(documentDto);
        }
        [HttpPost]
        /// <summary>
        /// Creates a new document.
        /// </summary>
        /// <param name="dto">DTO for creating document</param>
        /// <returns>Created document as DocumentDto</returns>
        [SwaggerOperation(Summary = "Create document", Description = "Creates a new document. Teacher only.")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult> Create(CreateDocumentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    error = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                });
            var userId = User?.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var document = new Document
            {
                Name = dto.Title,
                Description = dto.Description ?? string.Empty,
                Timestamp = DateTime.UtcNow,
                LinkedEntityType = dto.LinkedEntityType,
                LinkedEntityId = dto.LinkedEntityId,
                UploadedByUserId = userId
            };
            try
            {
                await _documentService.AddDocumentAsync(document);
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
                id = document.Id
            }, document);
        }
        [HttpPut("{id}")]
        /// <summary>
        /// Uppdaterar ett dokument.
        /// </summary>
        /// <param name="id">Dokumentets ID</param>
        /// <param name="dto">DTO för uppdatering</param>
        /// <returns>NoContent vid lyckad uppdatering</returns>
        [SwaggerOperation(Summary = "Uppdatera dokument", Description = "Uppdaterar ett dokument. Endast Teacher.")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult> Update(int id, UpdateDocumentDto dto)
        {
            // Only route id used for identifying document
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    error = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                });
            try
            {
                var existing = await _documentService.GetDocumentByIdAsync(id);
                if (existing == null)
                    return NotFound();
                existing.Name = dto.Title;
                existing.Description = dto.Description ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(dto.LinkedEntityType))
                    existing.LinkedEntityType = dto.LinkedEntityType;
                if (dto.LinkedEntityId.HasValue)
                    existing.LinkedEntityId = dto.LinkedEntityId.Value;
                await _documentService.UpdateDocumentAsync(existing);
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
        /// Tar bort ett dokument.
        /// </summary>
        /// <param name="id">Dokumentets ID</param>
        /// <returns>NoContent vid lyckad borttagning</returns>
        [SwaggerOperation(Summary = "Ta bort dokument", Description = "Tar bort ett dokument. Endast Teacher.")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _documentService.DeleteDocumentAsync(id);
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
