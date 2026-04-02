using ERP.Application.DTOs;
using ERP.Application.DTOs.Auth;
using ERP.Application.Interfaces;
using ERP.Application.Sercurity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ERP.Application.Services;

public class TokenService : ITokenService
{
    private readonly JwtOptions _opt;
    private readonly IAuthRepository _repo;

    public TokenService(IOptions<JwtOptions> opt, IAuthRepository repo)
    {
        _opt = opt.Value;
        _repo = repo;
    }

    public AccessTokenResult GenerateAccessToken(AuthUserRecord user, bool isMfaVerified = false)
    {
        var jti = Guid.NewGuid().ToString("N");
        var exp = DateTime.UtcNow.AddMinutes(_opt.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.RoleName),
            new(JwtRegisteredClaimNames.Jti, jti),
            new("amr", isMfaVerified ? "mfa" : "pwd")
        };

        if (user.UserPermissions is not null)
            claims.AddRange(user.UserPermissions.Select(p => new Claim("perm", p)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            expires: exp,
            signingCredentials: creds);

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return new AccessTokenResult(token, jti, exp);
    }

    public async Task<string> GenerateAndStoreRefreshTokenAsync(int userId, string jwtId, string ip, string userAgent)
    {
        var raw = CreateRefreshTokenRaw();
        var hash = HashToken(raw);
        var exp = DateTime.UtcNow.AddDays(_opt.RefreshTokenDays);

        await _repo.AddRefreshTokenAsync(userId, hash, jwtId, exp, ip);
        return raw;
    }

    public async Task<TokenResponse?> RefreshAsync(string refreshToken, string ip, string userAgent)
    {
        var hash = HashToken(refreshToken);
        var existing = await _repo.GetRefreshTokenAsync(hash);
        if (existing is null) return null;
        if (existing.Value.RevokedAtUtc is not null) return null;
        if (existing.Value.ExpiresAtUtc <= DateTime.UtcNow) return null;

        var user = await _repo.GetUserForLoginAsyncById(existing.Value.UserId);
        if (user is null || user.IsDeleted) return null;

        var access = GenerateAccessToken(user, isMfaVerified: true); // refresh coi như đã qua MFA session
        var newRaw = await GenerateAndStoreRefreshTokenAsync(user.UserId, access.JwtId, ip, userAgent);

        // revoke old refresh token, replacedBy chưa map id mới thì để null
        await _repo.RevokeRefreshTokenAsync(existing.Value.Id, ip, null);

        return new TokenResponse(access.Token, newRaw, access.ExpiresAtUtc);
    }

    public async Task LogoutAsync(string refreshToken, string ip)
    {
        var hash = HashToken(refreshToken);
        var existing = await _repo.GetRefreshTokenAsync(hash);
        if (existing is null) return;
        if (existing.Value.RevokedAtUtc is not null) return;

        await _repo.RevokeRefreshTokenAsync(existing.Value.Id, ip, null);
    }

    private static string CreateRefreshTokenRaw()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private static string HashToken(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes);
    }
}