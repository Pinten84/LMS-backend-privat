namespace LMS.API.Controllers
{
    using LMS.Application.Contracts.Services;
    using LMS.Application.Contracts.DTOs.Users;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;

    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = "Teacher,Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserAdminService _userAdminService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserAdminService userAdminService, ILogger<UsersController> logger)
        {
            _userAdminService = userAdminService;
            _logger = logger;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Lista användare", Description = "Returnerar paginerad lista över användare. Kan filtreras på roll.")]
        public async Task<ActionResult<IEnumerable<UserListItemDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? role = null, CancellationToken ct = default)
        {
            if (page <= 0 || pageSize <= 0)
                return ValidationProblem("Page och pageSize måste vara > 0");
            var (users, total) = await _userAdminService.GetUsersAsync(page, pageSize, role, ct);
            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(users);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Hämta användare", Description = "Returnerar en användare baserat på ID.")]
        public async Task<ActionResult<UserListItemDto>> GetById(string id, CancellationToken ct = default)
        {
            var user = await _userAdminService.GetByIdAsync(id, ct);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Skapa användare", Description = "Skapar en ny användare (Teacher eller Student).")]
        public async Task<ActionResult<UserListItemDto>> Create([FromBody] CreateUserDto dto, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            try
            {
                var created = await _userAdminService.CreateUserAsync(dto, ct);
                return CreatedAtAction(nameof(GetById), new
                {
                    id = created!.Id
                }, created);
            }
            catch (InvalidOperationException ex)
            {
                return Problem(title: "Kunde inte skapa användare", detail: ex.Message, statusCode: StatusCodes.Status409Conflict);
            }
        }

        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Uppdatera användare", Description = "Uppdaterar e-post, användarnamn och valfritt rollbyte.")]
        public async Task<ActionResult<UserListItemDto>> Update(string id, [FromBody] UpdateUserDto dto, CancellationToken ct = default)
        {
            if (id != dto.Id)
            {
                return ValidationProblem("ID i route matchar inte body");
            }
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }
            try
            {
                var updated = await _userAdminService.UpdateUserAsync(dto, ct);
                if (updated == null)
                {
                    return NotFound();
                }
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return Problem(title: "Kunde inte uppdatera användare", detail: ex.Message, statusCode: StatusCodes.Status409Conflict);
            }
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Ta bort användare", Description = "Tar bort användare permanent.")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct = default)
        {
            try
            {
                await _userAdminService.DeleteUserAsync(id, ct);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Problem(title: "Kunde inte ta bort användare", detail: ex.Message, statusCode: StatusCodes.Status409Conflict);
            }
        }
    }
}
