using ERP.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Application.Interfaces
{
    using ERP.Application.DTOs;

    public interface IAuthService
    {
        Task<LoginResult?> LoginAsync(LoginRequest req, string ip, string userAgent);
        Task<LoginResult?> VerifyMfaLoginAsync(MfaLoginVerifyRequest req, string ip, string userAgent);
        Task<TokenResponse?> RefreshAsync(string refreshToken, string ip, string userAgent);
        Task LogoutAsync(string refreshToken, string ip);
    }

    public interface IAuthRepository
    {
        Task<AuthUserRecord?> GetUserForLoginAsync(string email);

        Task<AuthUserRecord?> GetUserForLoginAsyncById(int userId);

        Task ResetFailedCountAsync(int userId);
        Task IncreaseFailedCountAsync(int userId);
        Task SetLockoutAsync(int userId, DateTime lockoutEndUtc);
        Task UpdateLastLoginAsync(int userId, DateTime lastLoginAtUtc);
        Task UpdateLastMfaAsync(int userId, DateTime lastMfaAtUtc);

        Task SaveMfaSetupAsync(int userId, string mfaSecretEnc, string recoveryCodesHashJson);
        Task EnableMfaAsync(int userId);
        Task DisableMfaAsync(int userId);
        Task UpdateRecoveryCodesAsync(int userId, string recoveryCodesHashJson);

        Task<long> AddRefreshTokenAsync(int userId, string tokenHash, string jwtId, DateTime exp, string ip);
        Task<(long Id, int UserId, string TokenHash, string JwtId, DateTime ExpiresAtUtc, DateTime? RevokedAtUtc)?> GetRefreshTokenAsync(string tokenHash);
        Task RevokeRefreshTokenAsync(long tokenId, string ip, long? replacedBy);
        Task RevokeAllActiveTokensAsync(int userId, string ip);
    }

    public interface ITokenService
    {
        (string accessToken, string jti, DateTime expUtc) CreateAccessToken(AuthUserRecord user, bool isMfaVerified);
        string CreateRefreshTokenRaw();
        AccessTokenResult GenerateAccessToken(AuthUserRecord user, bool isMfaVerified = false);
        string HashToken(string raw);
    }

    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string hash);
    }

    public interface IAuditService
    {
        Task WriteSecurityEventAsync(int? userId, string eventType, string? email, string ip, string? userAgent, string? metadata = null);
        Task WriteAuditLogAsync(string table, string action, string? oldData, string? newData, int? userId);
    }
}
