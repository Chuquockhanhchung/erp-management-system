using ERP.Application.DTOs.Admin;
using ERP.Application.Interfaces;
using ERP.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;

public class AdminUserRepository : IAdminUserRepository
{
    private readonly AppDbContext _db;
    public AdminUserRepository(AppDbContext db) => _db = db;

    public Task<bool> UserExistsAsync(int userId)
        => _db.Users.AnyAsync(x => x.Id == userId && (!x.IsDeleted ?? false));

    public async Task UnlockAsync(int userId)
    {
        var u = await _db.Users.FirstAsync(x => x.Id == userId);
        u.FailedLoginCount = 0;
        u.LockoutEndUtc = null;
    }

    public async Task UpdatePasswordHashAsync(int userId, string passwordHash)
    {
        var u = await _db.Users.FirstAsync(x => x.Id == userId);
        u.PasswordHash = passwordHash;
        u.PasswordChangedAtUtc = DateTime.UtcNow;
    }

    public async Task<bool> AssignRoleAsync(int userId, string roleName)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(x => x.Name == roleName);
        if (role is null) return false;

        var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId && (!x.IsDeleted??false));
        if (u is null) return false;

        u.RoleId = role.Id;
        return true;
    }

    public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();

    public async Task<PagedResult<SecurityEventVm>> GetSecurityEventsAsync(int? userId, string? eventType, int page, int size)
    {
        var q = _db.SecurityEvents.AsNoTracking().AsQueryable();

        if (userId.HasValue) q = q.Where(x => x.UserId == userId.Value);
        if (!string.IsNullOrWhiteSpace(eventType)) q = q.Where(x => x.EventType == eventType);

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(x => new SecurityEventVm(x.Id, x.UserId, x.EventType, x.Email, x.IpAddress, x.UserAgent, x.Metadata, x.CreatedAtUtc))
            .ToListAsync();

        return new PagedResult<SecurityEventVm>(items, page, size, total);
    }
}