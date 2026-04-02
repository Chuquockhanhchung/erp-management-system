using ERP.Application.DTOs;
using ERP.Application.DTOs.Auth;
using ERP.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ERP.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var ua = Request.Headers.UserAgent.ToString();

        var result = await _auth.LoginAsync(req, ip, ua);

        // Sai user/pass
        if (result is null) return Unauthorized("Invalid credentials");

        // Đúng pass nhưng cần MFA step 2
        if (result.MfaRequired)
            return Ok(result); // trả mfa_required + temp_token

        // Login thường
        return Ok(result); // trả access/refresh
    }

    [HttpPost("mfa/verify-login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> VerifyMfaLogin([FromBody] Application.DTOs.MfaLoginVerifyRequest req)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var ua = Request.Headers.UserAgent.ToString();

        var token = await _auth.VerifyMfaLoginAsync(req, ip, ua);
        if (token is null) return Unauthorized("Invalid MFA code or temp token");

        return Ok(token); // access/refresh có claim amr=mfa
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var ua = Request.Headers.UserAgent.ToString();

        var token = await _auth.RefreshAsync(req.RefreshToken, ip, ua);
        if (token is null) return Unauthorized("Invalid refresh token");
        return Ok(token);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest req)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        await _auth.LogoutAsync(req.RefreshToken, ip);
        return NoContent();
    }
}