namespace ERP.Application.DTOs;

public record LoginResult(
    bool MfaRequired,
    string? TempToken,
    string? AccessToken,
    string? RefreshToken,
    DateTime? ExpiresAtUtc
);