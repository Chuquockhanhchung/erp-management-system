using ERP.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Application.Interfaces
{
    public interface IMfaService
    {
        Task<MfaSetupResponse> BeginSetupAsync(int userId, string email);
        Task<bool> VerifyAndEnableAsync(int userId, string code);
        Task<bool> VerifyCodeAsync(int userId, string code);
        Task<(bool ok, bool consumed)> VerifyRecoveryCodeAsync(int userId, string recoveryCode);
        Task DisableAsync(int userId);
    }
}
