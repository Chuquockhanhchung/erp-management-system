using ERP.Application.DTOs;
using ERP.Application.DTOs.Auth;
using ERP.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ERP.API.Controllers;

[ApiController]
[Route("api/auth")]
public class MFAController : ControllerBase
{
    private readonly IMfaService _mfa;

    public MFAController(IMfaService mfa)
    {
        _mfa = mfa;
    }

    [Authorize]
    [HttpPost("mfa/setup")]
    public async Task<IActionResult> SetupMfa()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
        if (!int.TryParse(sub, out var userId) || string.IsNullOrWhiteSpace(email))
            return Unauthorized();

        var result = await _mfa.BeginSetupAsync(userId, email);
        if (result is null) return BadRequest("Cannot setup MFA.");

        return Ok(result);
    }

    [Authorize]
    [HttpPost("mfa/enable")]
    public async Task<IActionResult> EnableMfa([FromBody] MfaEnableRequest req)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!int.TryParse(sub, out var userId))
            return Unauthorized();

        var ok = await _mfa.VerifyAndEnableAsync(userId, req.Code);
        if (!ok) return BadRequest("Invalid MFA code.");

        return Ok(new { message = "MFA enabled." });
    }
}