using LMS.Application.Contracts.DTOs.AuthDtos;
using LMS.Application.Contracts;
using LMS.Domain.Configurations;
using LMS.Domain.Entities;
using LMS.Domain.Exceptions;
using LMS.Infrastructure.Data;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LMS.Application;
public class AuthService : IAuthService
{
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly RoleManager<IdentityRole> _roleManager;
            private readonly JwtSettings _jwtSettings;
            private readonly ApplicationDbContext _dbContext;
            private ApplicationUser? _user;
            public AuthService(
                UserManager<ApplicationUser> userManager,
                RoleManager<IdentityRole> roleManager,
                IOptions<JwtSettings> jwtSettings,
                ApplicationDbContext dbContext)
            {
                _userManager = userManager;
                _roleManager = roleManager;
                _jwtSettings = jwtSettings.Value;
                _dbContext = dbContext;
            }
            private static string Hash(string value)
            {
                using var sha = SHA256.Create();
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
                return Convert.ToHexString(bytes);
            }
            // Public interface method (legacy signature retained)
            public async Task<TokenDto> CreateTokenAsync(bool addTime)
                => await CreateTokenInternalAsync(addTime, null, null, null);
            private async Task<TokenDto> CreateTokenInternalAsync(bool addTime, string? ip, string? userAgent, RefreshToken? previousToken)
            {
                var signingCredentials = GetSigningCredentials();
                var claims = await GetClaimsAsync();
                var token = GenerateToken(signingCredentials, claims);
                ArgumentNullException.ThrowIfNull(_user);
                var plainRefresh = GenerateRefreshToken();
                var hashed = Hash(plainRefresh);
                // Expiry: extend only on login (addTime true); otherwise inherit previous token's expiry (legacy behavior)
                var expires = addTime
                    ? DateTime.UtcNow.AddDays(3)
                    : previousToken?.Expires ?? DateTime.UtcNow.AddDays(3);
                if (previousToken != null)
                {
                    previousToken.RevokedAt = DateTime.UtcNow;
                }
                var refreshEntity = new RefreshToken
                {
                    UserId = _user.Id,
                    TokenHash = hashed,
                    Expires = expires,
                    IpAddress = ip,
                    UserAgent = userAgent
                };
                _dbContext.RefreshTokens.Add(refreshEntity);
                await _dbContext.SaveChangesAsync();
                if (previousToken != null)
                {
                    previousToken.ReplacedByTokenId = refreshEntity.Id;
                    await _dbContext.SaveChangesAsync();
                }
                var jwt = new JwtSecurityTokenHandler().WriteToken(token);
                return new TokenDto(jwt, plainRefresh);
            }
            private string GenerateRefreshToken()
            {
                var randomNumber = new byte[32];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
            private JwtSecurityToken GenerateToken(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
                => new(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(Convert.ToDouble(_jwtSettings.Expires)),
                    signingCredentials: signingCredentials);
            private async Task<IEnumerable<Claim>> GetClaimsAsync()
            {
                ArgumentNullException.ThrowIfNull(_user);
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, _user.UserName!),
                    new(ClaimTypes.NameIdentifier, _user.Id.ToString())
                };
                var roles = await _userManager.GetRolesAsync(_user);
                foreach (var role in roles)
                    claims.Add(new Claim(ClaimTypes.Role, role));
                return claims;
            }
            private SigningCredentials GetSigningCredentials()
            {
                var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
                var secret = new SymmetricSecurityKey(key);
                return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
            }
            public async Task<IdentityResult> RegisterUserAsync(UserRegistrationDto userRegistrationDto)
            {
                ArgumentNullException.ThrowIfNull(userRegistrationDto);
                var isRoleValid = !string.IsNullOrWhiteSpace(userRegistrationDto.Role);
                if (isRoleValid)
                {
                    var roleExists = await _roleManager.RoleExistsAsync(userRegistrationDto.Role!);
                    if (!roleExists)
                        return IdentityResult.Failed(new IdentityError { Description = "Role does not exist" });
                }
                var user = userRegistrationDto.Adapt<ApplicationUser>();
                var result = await _userManager.CreateAsync(user, userRegistrationDto.Password);
                if (!result.Succeeded)
                    return result;
                if (isRoleValid)
                    result = await _userManager.AddToRoleAsync(user, userRegistrationDto.Role!);
                return result;
            }
            public async Task<bool> ValidateUserAsync(UserAuthDto userDto)
            {
                ArgumentNullException.ThrowIfNull(userDto);
                _user = await _userManager.FindByNameAsync(userDto.UserName);
                return _user != null && await _userManager.CheckPasswordAsync(_user, userDto.Password);
            }
            public async Task<TokenDto?> LoginAsync(UserAuthDto userDto)
            {
                var isValid = await ValidateUserAsync(userDto);
                if (!isValid)
                    return null;
                return await CreateTokenAsync(addTime: true);
            }
            public async Task<TokenDto> RefreshTokenAsync(TokenDto token)
            {
                var principal = GetPrincipalFromExpiredToken(token.AccessToken);
                var user = await _userManager.FindByNameAsync(principal.Identity?.Name!);
                if (user == null)
                    throw new TokenValidationException("User not found", StatusCodes.Status400BadRequest);
                _user = user;
                var hash = Hash(token.RefreshToken);
                var existing = await _dbContext.RefreshTokens
                    .FirstOrDefaultAsync(r => r.UserId == user.Id && r.TokenHash == hash);
                if (existing == null)
                    throw new TokenValidationException("Refresh token invalid", StatusCodes.Status400BadRequest);
                if (existing.RevokedAt.HasValue || existing.ReplacedByTokenId.HasValue)
                {
                    // Reuse detected; mark compromised chain (all active tokens for user)
                    existing.IsCompromised = true;
                    var now = DateTime.UtcNow;
                    var activeTokens = await _dbContext.RefreshTokens
                        .Where(r => r.UserId == user.Id && r.RevokedAt == null && r.Expires > now)
                        .ToListAsync();
                    foreach (var t in activeTokens)
                    {
                        t.RevokedAt = now;
                        t.IsCompromised = true;
                    }
                    await _dbContext.SaveChangesAsync();
                    throw new TokenValidationException("Refresh token reuse detected", StatusCodes.Status400BadRequest);
                }
                if (existing.Expires <= DateTime.UtcNow)
                    throw new TokenValidationException("Refresh token expired", StatusCodes.Status400BadRequest);
                return await CreateTokenInternalAsync(addTime: false, ip: null, userAgent: null, previousToken: existing);
            }
            private ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken)
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidAudience = _jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey))
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out var securityToken);
                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                   !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    throw new SecurityTokenException("Invalid token");
                return principal;
            }
}
