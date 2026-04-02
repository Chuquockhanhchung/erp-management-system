using ERP.Application.DTOs.Admin;
using ERP.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
[Authorize(Policy = "RequireMfa")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _service;
    public AdminUsersController(IAdminUserService service) => _service = service;

    [HttpPost("{id:int}/unlock")]
    public async Task<IActionResult> Unlock(int id)
    {
        var ok = await _service.UnlockUserAsync(id, User);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto dto)
    {
        var ok = await _service.ResetPasswordAsync(id, dto.NewPassword, User);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/force-logout")]
    public async Task<IActionResult> ForceLogout(int id)
    {
        var ok = await _service.ForceLogoutAsync(id, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown", User);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/role")]
    public async Task<IActionResult> AssignRole(int id, [FromBody] AssignRoleDto dto)
    {
        var ok = await _service.AssignRoleAsync(id, dto.RoleName, User);
        return ok ? NoContent() : NotFound();
    }

    [HttpGet("security-events")]
    public async Task<IActionResult> SecurityEvents([FromQuery] int? userId, [FromQuery] string? eventType, [FromQuery] int page = 1, [FromQuery] int size = 20)
        => Ok(await _service.GetSecurityEventsAsync(userId, eventType, page, size));
}