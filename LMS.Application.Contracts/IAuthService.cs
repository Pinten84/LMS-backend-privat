using LMS.Application.Contracts.DTOs.AuthDtos;
using Microsoft.AspNetCore.Identity;

namespace LMS.Application.Contracts;
public interface IAuthService
{
            Task<TokenDto> CreateTokenAsync(bool addTime);
            Task<TokenDto> RefreshTokenAsync(TokenDto token);
            Task<IdentityResult> RegisterUserAsync(UserRegistrationDto userRegistrationDto);
            Task<bool> ValidateUserAsync(UserAuthDto user);
            Task<TokenDto?> LoginAsync(UserAuthDto userDto);
}
