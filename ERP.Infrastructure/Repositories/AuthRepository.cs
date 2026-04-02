using ERP.Application.DTOs;
using ERP.Application.DTOs.Auth;
using ERP.Application.Interfaces;
using ERP.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _db;
    public AuthRepository(AppDbContext db) => _db = db;

    public async Task<AuthUserRecord?> GetUserForLoginAsync(string email)
    {
        var norm = email.Trim().ToUpperInvariant();

        return await (from u in _db.Users
                      join r in _db.Roles on u.RoleId equals r.Id
                      where u.NormalizedEmail == norm
                      select new AuthUserRecord
                      {
                          UserId = u.Id,
                          Email = u.Email,
                          PasswordHash = u.PasswordHash,
                          RoleName = r.Name,
                          IsDeleted = u.IsDeleted ?? false,
                          FailedLoginCount = u.FailedLoginCount,
                          LockoutEndUtc = u.LockoutEndUtc,
                          MfaEnabled = u.MfaEnabled,
                          MfaSecretEnc = u.MfaSecretEnc,
                          MfaRecoveryCodesHash = u.MfaRecoveryCodesHash
                      }).FirstOrDefaultAsync();
    }

    public async Task<AuthUserRecord?> GetUserForLoginAsyncById(int userId)
    {
        return await (from u in _db.Users
                      join r in _db.Roles on u.RoleId equals r.Id
                      where u.Id == userId
                      select new AuthUserRecord
                      {
                          UserId = u.Id,
                          Email = u.Email,
                          PasswordHash = u.PasswordHash,
                          RoleName = r.Name,
                          IsDeleted = u.IsDeleted??false,
                          FailedLoginCount = u.FailedLoginCount,
                          LockoutEndUtc = u.LockoutEndUtc,
                          MfaEnabled = u.MfaEnabled,
                          MfaSecretEnc = u.MfaSecretEnc,
                          MfaRecoveryCodesHash = u.MfaRecoveryCodesHash
                      }).FirstOrDefaultAsync();
    }

    public async Task ResetFailedCountAsync(int userId)
    {
        var u = await _db.Users.FirstAsync(x => x.Id == userId);
        u.FailedLoginCount = 0;
        u.LockoutEndUtc = null;
        await _db.SaveChangesAsync();
    }

    public async Task IncreaseFailedCountAsync(int userId)
    {
        var u = await _db.Users.FirstAsync(x => x.Id == userId);
        u.FailedLoginCount += 1;
        await _db.SaveChangesAsync();
    }

    public async Task SetLockoutAsync(int userId, DateTime lockoutEndUtc)
    {
        var u = await _db.Users.FirstAsync(x => x.Id == userId);
        u.LockoutEndUtc = lockoutEndUtc;
        await _db.SaveChangesAsync();
    }

    public async Task UpdateLastLoginAsync(int userId, DateTime lastLoginAtUtc)
    {
        var u = await _db.Users.FirstAsync(x => x.Id == userId);
        u.LastLoginAtUtc = lastLoginAtUtc;
        await _db.SaveChangesAsync();
    }

    public async Task UpdateLastMfaAsync(int userId, DateTime lastMfaAtUtc)
    {
        var u = await _db.Users.FirstAsync(x => x.Id == userId);
        u.LastMfaAtUtc = lastMfaAtUtc;
        await _db.SaveChangesAsync();
    }

    public async Task SaveMfaSetupAsync(int userId, string mfaSecretEnc, string recoveryCodesHashJson)
    {
        var u = await _db.Users.FirstAsync(x => x.Id == userId);
        u.MfaSecretEnc = mfaSecretEnc;
        u.MfaRecoveryCodesHash = recoveryCodesHashJson;
        u.MfaEnabled = false;
        await _db.SaveChangesAsync();
    }

    public async Task EnableMfaAsync(int userId)
    {
        var u = await _db.Users.FirstAsync(x => x.Id == userId);
        u.MfaEnabled = true;
        u.LastMfaAtUtc = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    public async Task DisableMfaAsync(int userId)
    {
        var u = await _db.Users.FirstAsync(x => x.Id == userId);
        u.MfaEnabled = false;
        u.MfaSecretEnc = null;
        u.MfaRecoveryCodesHash = null;
        await _db.SaveChangesAsync();
    }

    public async Task UpdateRecoveryCodesAsync(int userId, string recoveryCodesHashJson)
    {
        var u = await _db.Users.FirstAsync(x => x.Id == userId);
        u.MfaRecoveryCodesHash = recoveryCodesHashJson;
        await _db.SaveChangesAsync();
    }

    public async Task<long> AddRefreshTokenAsync(int userId, string tokenHash, string jwtId, DateTime exp, string ip)
    {
        var e = new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            JwtId = jwtId,
            ExpiresAtUtc = exp,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByIp = ip
        };
        _db.RefreshTokens.Add(e);
        await _db.SaveChangesAsync();
        return e.Id;
    }

    public async Task<(long Id, int UserId, string TokenHash, string JwtId, DateTime ExpiresAtUtc, DateTime? RevokedAtUtc)?> GetRefreshTokenAsync(string tokenHash)
    {
        var e = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash);
        if (e is null) return null;
        return (e.Id, e.UserId, e.TokenHash, e.JwtId, e.ExpiresAtUtc, e.RevokedAtUtc);
    }

    public async Task RevokeRefreshTokenAsync(long tokenId, string ip, long? replacedBy)
    {
        var e = await _db.RefreshTokens.FirstAsync(x => x.Id == tokenId);
        e.RevokedAtUtc = DateTime.UtcNow;
        e.RevokedByIp = ip;
        e.ReplacedByTokenId = replacedBy;
        await _db.SaveChangesAsync();
    }

    public async Task RevokeAllActiveTokensAsync(int userId, string ip)
    {
        var active = await _db.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAtUtc == null && x.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync();

        foreach (var t in active)
        {
            t.RevokedAtUtc = DateTime.UtcNow;
            t.RevokedByIp = ip;
        }

        await _db.SaveChangesAsync();
    }
}