using ERP.Application.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace ERP.API.Security;

public class DataProtectorService : IDataProtectorService
{
    private readonly IDataProtector _protector;

    public DataProtectorService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("ERP.MFA.Secret.v1");
    }

    public string Protect(string plainText) => _protector.Protect(plainText);

    public string Unprotect(string cipherText) => _protector.Unprotect(cipherText);
}