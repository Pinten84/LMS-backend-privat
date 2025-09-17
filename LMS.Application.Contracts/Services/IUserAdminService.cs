using LMS.Application.Contracts.DTOs.Users;

namespace LMS.Application.Contracts.Services;

public interface IUserAdminService
{
    Task<UserListItemDto?> CreateUserAsync(CreateUserDto dto, CancellationToken ct = default);
    Task<UserListItemDto?> UpdateUserAsync(UpdateUserDto dto, CancellationToken ct = default);
    Task<bool> DeleteUserAsync(string id, CancellationToken ct = default);
    Task<(IEnumerable<UserListItemDto> Users, int TotalCount)> GetUsersAsync(int page, int pageSize, string? role, CancellationToken ct = default);
    Task<UserListItemDto?> GetByIdAsync(string id, CancellationToken ct = default);
}
