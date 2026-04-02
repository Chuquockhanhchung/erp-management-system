using ERP.Application.DTOs.Admin;
using System.Security.Claims;

namespace ERP.Application.Interfaces;

public interface IAdminUserService
{
    Task<bool> UnlockUserAsync(int userId, ClaimsPrincipal actor);
    Task<bool> ResetPasswordAsync(int userId, string newPassword, ClaimsPrincipal actor);
    Task<bool> ForceLogoutAsync(int userId, string ip, ClaimsPrincipal actor);
    Task<bool> AssignRoleAsync(int userId, string roleName, ClaimsPrincipal actor);
    Task<PagedResult<SecurityEventVm>> GetSecurityEventsAsync(int? userId, string? eventType, int page, int size);
}