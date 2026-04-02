using ERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Application.DTOs
{
    public class AuthUserRecord
    {
        public int UserId { get; set; }
        public string Email { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public string RoleName { get; set; } = default!;
        public bool IsDeleted { get; set; }
        public int FailedLoginCount { get; set; }
        public DateTime? LockoutEndUtc { get; set; }
        public bool MfaEnabled { get; set; }
        public string? MfaSecretEnc { get; set; }
        public string? MfaRecoveryCodesHash { get; set; }
        public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    }
}
