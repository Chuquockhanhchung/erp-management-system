namespace ERP.Application.DTOs.Auth;

public record MfaSetupResponse(string ManualEntryKey, string OtpAuthUri, string QrCodeDataUrl);
public record MfaVerifySetupRequest(string Code);
public record MfaLoginVerifyRequest(string Email, string Code, string? TempToken);
public record MfaDisableRequest(string Code);
public record MfaRecoveryLoginRequest(string Email, string RecoveryCode, string? TempToken);