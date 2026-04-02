using System;
using System.Collections.Generic;

namespace ERP.Infrastructure.Persistence.Entities;

public partial class User
{
    public int Id { get; set; }

    public string? FullName { get; set; }

    public string Email { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public int? RoleId { get; set; }

    public bool? IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public int FailedLoginCount { get; set; }

    public DateTime? LockoutEndUtc { get; set; }

    public DateTime? LastLoginAtUtc { get; set; }

    public DateTime? PasswordChangedAtUtc { get; set; }

    public string? NormalizedEmail { get; set; }

    public bool MfaEnabled { get; set; }

    public string? MfaSecretEnc { get; set; }

    public string? MfaRecoveryCodesHash { get; set; }

    public DateTime? LastMfaAtUtc { get; set; }

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<SecurityEvent> SecurityEvents { get; set; } = new List<SecurityEvent>();

    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}
