using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Application.DTOs;

public record LoginRequest(string Email, string Password);
public record TokenResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAtUtc);
public record RefreshRequest(string RefreshToken);
public record AccessTokenResult(
    string Token,
    string JwtId,
    DateTime ExpiresAtUtc
);
public record MfaEnableRequest(string Code);
public record MfaSetupResponse(
    string ManualKey,
    string QrCodeUri,
    string[] RecoveryCodes
);
