using ERP.Application.DTOs.Admin;

namespace ERP.Application.Interfaces;

public interface IAdminUserRepository
{
    Task<bool> UserExistsAsync(int userId);
    Task UnlockAsync(int userId);
    Task UpdatePasswordHashAsync(int userId, string passwordHash);
    Task<bool> AssignRoleAsync(int userId, string roleName);
    Task<int> SaveChangesAsync();
    Task<PagedResult<SecurityEventVm>> GetSecurityEventsAsync(int? userId, string? eventType, int page, int size);
}