using System.Security.Cryptography;
using System.Text;
using ERP.Application.DTOs.Auth;
using ERP.Application.Interfaces;
using OtpNet;
using QRCoder;

namespace ERP.Application.Services;

public class MfaService : IMfaService
{
    private readonly IAuthRepository _repo; // thêm methods lưu secret/recovery
    private readonly IDataProtectorService _protector; // bạn tự wrap DataProtection
    public MfaService(IAuthRepository repo, IDataProtectorService protector)
    {
        _repo = repo;
        _protector = protector;
    }

    public async Task<MfaSetupResponse> BeginSetupAsync(int userId, string email)
    {
        var secret = KeyGeneration.GenerateRandomKey(20);
        var base32 = Base32Encoding.ToString(secret);

        var issuer = "ERP_SENIOR";
        var uri = new OtpUri(OtpType.Totp, base32, email, issuer).ToString();

        using var qr = new QRCodeGenerator();
        using var data = qr.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(data).GetGraphic(20);
        var dataUrl = $"data:image/png;base64,{Convert.ToBase64String(png)}";

        var enc = _protector.Protect(base32);
        var recovery = GenerateRecoveryCodes(10);
        var recoveryHashJson = System.Text.Json.JsonSerializer.Serialize(
            recovery.Select(HashText).ToArray());

        await _repo.SaveMfaSetupAsync(userId, enc, recoveryHashJson);

        return new MfaSetupResponse(base32, uri, dataUrl);
    }

    public async Task<bool> VerifyAndEnableAsync(int userId, string code)
    {
        var user = await _repo.GetUserForLoginAsyncById(userId);
        if (user is null || string.IsNullOrWhiteSpace(user.MfaSecretEnc)) return false;

        var secret = _protector.Unprotect(user.MfaSecretEnc);
        var ok = VerifyTotp(secret, code);
        if (!ok) return false;

        await _repo.EnableMfaAsync(userId);
        return true;
    }

    public async Task<bool> VerifyCodeAsync(int userId, string code)
    {
        var user = await _repo.GetUserForLoginAsyncById(userId);
        if (user is null || !user.MfaEnabled || string.IsNullOrWhiteSpace(user.MfaSecretEnc)) return false;

        var secret = _protector.Unprotect(user.MfaSecretEnc);
        return VerifyTotp(secret, code);
    }

    public async Task<(bool ok, bool consumed)> VerifyRecoveryCodeAsync(int userId, string recoveryCode)
    {
        var user = await _repo.GetUserForLoginAsyncById(userId);
        if (user is null || string.IsNullOrWhiteSpace(user.MfaRecoveryCodesHash)) return (false, false);

        var arr = System.Text.Json.JsonSerializer.Deserialize<List<string>>(user.MfaRecoveryCodesHash) ?? new();
        var h = HashText(recoveryCode.Trim().ToUpperInvariant());

        if (!arr.Contains(h)) return (false, false);

        arr.Remove(h);
        await _repo.UpdateRecoveryCodesAsync(userId, System.Text.Json.JsonSerializer.Serialize(arr));
        return (true, true);
    }

    public Task DisableAsync(int userId) => _repo.DisableMfaAsync(userId);

    private static bool VerifyTotp(string base32Secret, string code)
    {
        var secret = Base32Encoding.ToBytes(base32Secret);
        var totp = new Totp(secret, mode: OtpHashMode.Sha1, step: 30, totpSize: 6);
        return totp.VerifyTotp(code, out _, new VerificationWindow(previous: 1, future: 1));
    }

    private static string[] GenerateRecoveryCodes(int n)
    {
        var res = new string[n];
        for (int i = 0; i < n; i++)
            res[i] = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)); // 8 hex chars
        return res;
    }

    private static string HashText(string s)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(s)));
    }
}