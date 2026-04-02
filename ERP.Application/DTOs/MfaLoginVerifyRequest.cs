namespace ERP.Application.DTOs;

public record MfaLoginVerifyRequest(string TempToken, string Code);