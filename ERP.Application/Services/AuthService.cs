using ERP.Application.DTOs;
using ERP.Application.DTOs.Auth;
using ERP.Application.Interfaces;

namespace ERP.Application.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _token;
    private readonly IMfaService _mfa;
    private readonly ITempTokenService _tempToken;
    private readonly IAuditService _audit;

    public AuthService(
        IAuthRepository repo,
        IPasswordHasher hasher,
        ITokenService token,
        IMfaService mfa,
        ITempTokenService tempToken,
        IAuditService audit)
    {
        _repo = repo;
        _hasher = hasher;
        _token = token;
        _mfa = mfa;
        _tempToken = tempToken;
        _audit = audit;
    }

    public async Task<LoginResult?> LoginAsync(LoginRequest req, string ip, string userAgent)
    {
        var email = req.Email.Trim();
        var user = await _repo.GetUserForLoginAsync(email);
        if (user is null || user.IsDeleted)
        {
            await _audit.WriteSecurityEventAsync(null, "LOGIN_FAILED", email, ip, userAgent, "reason=user_not_found_or_deleted");
            return null;
        }

        if (user.LockoutEndUtc.HasValue && user.LockoutEndUtc.Value > DateTime.UtcNow)
        {
            await _audit.WriteSecurityEventAsync(user.UserId, "LOGIN_BLOCKED_LOCKOUT", user.Email, ip, userAgent, null);
            return null;
        }

        if (!_hasher.Verify(req.Password, user.PasswordHash))
        {
            await _repo.IncreaseFailedCountAsync(user.UserId);

            var failed = user.FailedLoginCount + 1;
            if (failed >= 5)
            {
                await _repo.SetLockoutAsync(user.UserId, DateTime.UtcNow.AddMinutes(15));
                await _audit.WriteSecurityEventAsync(user.UserId, "ACCOUNT_LOCKED", user.Email, ip, userAgent, "failed>=5");
            }

            await _audit.WriteSecurityEventAsync(user.UserId, "LOGIN_FAILED", user.Email, ip, userAgent, "reason=bad_password");
            return null;
        }

        await _repo.ResetFailedCountAsync(user.UserId);
        await _repo.UpdateLastLoginAsync(user.UserId, DateTime.UtcNow);

        var isAdmin = string.Equals(user.RoleName, "Admin", StringComparison.OrdinalIgnoreCase);

        if (isAdmin && user.MfaEnabled)
        {
            var temp = _tempToken.GenerateMfaTempToken(user.UserId, user.Email, ttlMinutes: 5);
            await _audit.WriteSecurityEventAsync(user.UserId, "MFA_CHALLENGE", user.Email, ip, userAgent, null);

            return new LoginResult(true, temp, null, null, null);
        }

        var access = _token.GenerateAccessToken(user, isMfaVerified: false);
        var refresh = await _token.GenerateAndStoreRefreshTokenAsync(user.UserId, access.JwtId, ip, userAgent);

        await _audit.WriteSecurityEventAsync(user.UserId, "LOGIN_SUCCESS", user.Email, ip, userAgent, "mfa=false");

        return new LoginResult(false, null, access.Token, refresh, access.ExpiresAtUtc);
    }

    public async Task<LoginResult?> VerifyMfaLoginAsync(DTOs.MfaLoginVerifyRequest req, string ip, string userAgent)
    {
        var principal = _tempToken.ValidateMfaTempToken(req.TempToken);
        if (principal is null) return null;

        var sub = principal.FindFirst("sub")?.Value;
        if (!int.TryParse(sub, out var userId)) return null;

        var user = await _repo.GetUserForLoginAsyncById(userId);
        if (user is null || user.IsDeleted || !user.MfaEnabled) return null;

        var ok = await _mfa.VerifyCodeAsync(userId, req.Code);
        if (!ok)
        {
            await _audit.WriteSecurityEventAsync(user.UserId, "MFA_FAILED", user.Email, ip, userAgent, null);
            return null;
        }

        var access = _token.GenerateAccessToken(user, isMfaVerified: true);
        var refresh = await _token.GenerateAndStoreRefreshTokenAsync(user.UserId, access.JwtId, ip, userAgent);

        await _repo.UpdateLastMfaAsync(user.UserId, DateTime.UtcNow);
        await _audit.WriteSecurityEventAsync(user.UserId, "MFA_SUCCESS", user.Email, ip, userAgent, null);
        await _audit.WriteSecurityEventAsync(user.UserId, "LOGIN_SUCCESS", user.Email, ip, userAgent, "mfa=true");

        return new LoginResult(false, null, access.Token, refresh, access.ExpiresAtUtc);
    }

    public Task<TokenResponse?> RefreshAsync(string refreshToken, string ip, string userAgent)
        => _token.RefreshAsync(refreshToken, ip, userAgent);

    public Task LogoutAsync(string refreshToken, string ip)
        => _token.LogoutAsync(refreshToken, ip);
}