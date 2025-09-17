using LMS.Application.Contracts.DTOs.Users;
using LMS.Application.Contracts.Services;
using LMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace LMS.Application;

public class UserAdminService : IUserAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserAdminService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<UserListItemDto?> CreateUserAsync(CreateUserDto dto, CancellationToken ct = default)
    {
        var roleExists = await _roleManager.RoleExistsAsync(dto.Role);
        if (!roleExists)
            throw new InvalidOperationException($"Role '{dto.Role}' does not exist");

        var user = new ApplicationUser { UserName = dto.UserName, Email = dto.Email };
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        var roleRes = await _userManager.AddToRoleAsync(user, dto.Role);
        if (!roleRes.Succeeded)
            throw new InvalidOperationException(string.Join("; ", roleRes.Errors.Select(e => e.Description)));

        return await ProjectAsync(user);
    }

    public async Task<UserListItemDto?> UpdateUserAsync(UpdateUserDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == dto.Id, ct);
        if (user == null)
            return null;
        user.Email = dto.Email;
        user.UserName = dto.UserName;
        var updateRes = await _userManager.UpdateAsync(user);
        if (!updateRes.Succeeded)
            throw new InvalidOperationException(string.Join("; ", updateRes.Errors.Select(e => e.Description)));

        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            if (!await _roleManager.RoleExistsAsync(dto.Role))
                throw new InvalidOperationException($"Role '{dto.Role}' does not exist");
            var existingRoles = await _userManager.GetRolesAsync(user);
            var removeRes = await _userManager.RemoveFromRolesAsync(user, existingRoles);
            if (!removeRes.Succeeded)
                throw new InvalidOperationException(string.Join("; ", removeRes.Errors.Select(e => e.Description)));
            var addRes = await _userManager.AddToRoleAsync(user, dto.Role);
            if (!addRes.Succeeded)
                throw new InvalidOperationException(string.Join("; ", addRes.Errors.Select(e => e.Description)));
        }
        return await ProjectAsync(user);
    }

    public async Task<bool> DeleteUserAsync(string id, CancellationToken ct = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user == null)
            return false;
        var res = await _userManager.DeleteAsync(user);
        if (!res.Succeeded)
            throw new InvalidOperationException(string.Join("; ", res.Errors.Select(e => e.Description)));
        return true;
    }

    public async Task<(IEnumerable<UserListItemDto> Users, int TotalCount)> GetUsersAsync(int page, int pageSize, string? role, CancellationToken ct = default)
    {
        var query = _userManager.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(role))
        {
            // Filter by role by joining user roles (simple in-memory join fallback)
            var usersInRole = await _userManager.GetUsersInRoleAsync(role);
            var ids = usersInRole.Select(u => u.Id).ToHashSet();
            query = query.Where(u => ids.Contains(u.Id));
        }
        var total = await query.CountAsync(ct);
        var users = await query
            .OrderBy(u => u.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        var list = new List<UserListItemDto>();
        foreach (var u in users)
            list.Add(await ProjectAsync(u));
        return (list, total);
    }

    public async Task<UserListItemDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user == null)
            return null;
        return await ProjectAsync(user);
    }

    private async Task<UserListItemDto> ProjectAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new UserListItemDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Roles = roles.ToList()
        };
    }
}
