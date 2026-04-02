using ERP.Application.DTOs.Admin;
using ERP.Application.Interfaces;
using System.Security.Claims;

namespace ERP.Application.Services;

public class AdminUserService : IAdminUserService
{
    private readonly IAdminUserRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly IAuthRepository _authRepo;
    private readonly IAuditService _audit;

    public AdminUserService(IAdminUserRepository repo, IPasswordHasher hasher, IAuthRepository authRepo, IAuditService audit)
    {
        _repo = repo;
        _hasher = hasher;
        _authRepo = authRepo;
        _audit = audit;
    }

    public async Task<bool> UnlockUserAsync(int userId, ClaimsPrincipal actor)
    {
        if (!await _repo.UserExistsAsync(userId)) return false;
        await _repo.UnlockAsync(userId);
        await _repo.SaveChangesAsync();
        await _audit.WriteSecurityEventAsync(userId, "ADMIN_UNLOCK_USER", null, "system", null, $"actor={actor.Identity?.Name}");
        return true;
    }

    public async Task<bool> ResetPasswordAsync(int userId, string newPassword, ClaimsPrincipal actor)
    {
        if (!await _repo.UserExistsAsync(userId)) return false;
        var hash = _hasher.Hash(newPassword);
        await _repo.UpdatePasswordHashAsync(userId, hash);
        await _authRepo.RevokeAllActiveTokensAsync(userId, "admin_reset_password");
        await _repo.SaveChangesAsync();
        await _audit.WriteSecurityEventAsync(userId, "ADMIN_RESET_PASSWORD", null, "system", null, $"actor={actor.Identity?.Name}");
        return true;
    }

    public async Task<bool> ForceLogoutAsync(int userId, string ip, ClaimsPrincipal actor)
    {
        if (!await _repo.UserExistsAsync(userId)) return false;
        await _authRepo.RevokeAllActiveTokensAsync(userId, ip);
        await _audit.WriteSecurityEventAsync(userId, "ADMIN_FORCE_LOGOUT", null, ip, null, $"actor={actor.Identity?.Name}");
        return true;
    }

    public async Task<bool> AssignRoleAsync(int userId, string roleName, ClaimsPrincipal actor)
    {
        var ok = await _repo.AssignRoleAsync(userId, roleName);
        if (!ok) return false;
        await _repo.SaveChangesAsync();
        await _authRepo.RevokeAllActiveTokensAsync(userId, "admin_assign_role");
        await _audit.WriteSecurityEventAsync(userId, "ADMIN_ASSIGN_ROLE", null, "system", null, $"role={roleName};actor={actor.Identity?.Name}");
        return true;
    }

    public Task<PagedResult<SecurityEventVm>> GetSecurityEventsAsync(int? userId, string? eventType, int page, int size)
        => _repo.GetSecurityEventsAsync(userId, eventType, page, size);
}