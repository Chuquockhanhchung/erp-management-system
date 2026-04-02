using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ERP.Application.Interfaces;
using ERP.Application.Sercurity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ERP.Application.Services;

public class TempTokenService : ITempTokenService
{
    private readonly JwtOptions _opt;
    public TempTokenService(IOptions<JwtOptions> opt) => _opt = opt.Value;

    public string GenerateMfaTempToken(int userId, string email, int ttlMinutes = 5)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, email),
        new Claim("typ", "mfa_temp")
    };

        var now = DateTime.Now;

        var jwt = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(ttlMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    public ClaimsPrincipal?  ValidateMfaTempTokenRaw(string token)
    {
        var handler = new JwtSecurityTokenHandler
        {
            MapInboundClaims = false // giữ nguyên "sub"
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.SecretKey));

        try
        {
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _opt.Issuer,
                ValidateAudience = true,
                ValidAudience = _opt.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),
                NameClaimType = JwtRegisteredClaimNames.Sub
            }, out _);

            var typ = principal.FindFirst("typ")?.Value;
            if (typ != "mfa_temp") return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }
}