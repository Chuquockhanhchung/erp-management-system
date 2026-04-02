namespace ERP.Application.DTOs.Admin;

public record ResetPasswordDto(string NewPassword);
public record AssignRoleDto(string RoleName);

public record SecurityEventVm(
    long Id, int? UserId, string EventType, string? Email, string IpAddress, string? UserAgent, string? Metadata, DateTime CreatedAtUtc);

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int Size, int Total);