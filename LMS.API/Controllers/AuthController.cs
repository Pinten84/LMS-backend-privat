namespace LMS.API.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using LMS.Application.Contracts.DTOs.AuthDtos;
    using LMS.Application.Contracts;
    using Swashbuckle.AspNetCore.Annotations;
    using Microsoft.AspNetCore.Authorization;
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("login")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Login", Description = "Validates user credentials and returns access and refresh tokens.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Login successful", typeof(TokenDto))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Invalid credentials")]
        public async Task<ActionResult<TokenDto>> Login([FromBody] UserAuthDto login)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var token = await _authService.LoginAsync(login);
            if (token is null)
                return Unauthorized(new
                {
                    error = "Fel användarnamn eller lösenord"
                });
            return Ok(token);
        }
        [HttpPost("refresh")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Refresh JWT token", Description = "Returns new access and refresh tokens for a valid refresh token.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Token refreshed", typeof(TokenDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid or revoked refresh token")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User not authorized")]
        public async Task<ActionResult<TokenDto>> RefreshToken([FromBody] RefreshRequestDto request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            // Send the original TokenDto to AuthService (to preserve signature) – IP/UA can be injected later.
            var dto = new TokenDto(request.AccessToken, request.RefreshToken);
            var newToken = await _authService.RefreshTokenAsync(dto);
            return Ok(newToken);
        }
    }
}
