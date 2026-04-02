using System;
using System.Collections.Generic;

namespace ERP.Infrastructure.Persistence.Entities;

public partial class RefreshToken
{
    public long Id { get; set; }

    public int UserId { get; set; }

    public string TokenHash { get; set; } = null!;

    public string JwtId { get; set; } = null!;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public string? CreatedByIp { get; set; }

    public string? UserAgent { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public string? RevokedByIp { get; set; }

    public long? ReplacedByTokenId { get; set; }

    public virtual ICollection<RefreshToken> InverseReplacedByToken { get; set; } = new List<RefreshToken>();

    public virtual RefreshToken? ReplacedByToken { get; set; }

    public virtual User User { get; set; } = null!;
}
