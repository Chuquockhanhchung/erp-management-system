using System;
using System.Collections.Generic;

namespace ERP.Infrastructure.Persistence.Entities;

public partial class SecurityEvent
{
    public long Id { get; set; }

    public int? UserId { get; set; }

    public string EventType { get; set; } = null!;

    public string? Email { get; set; }

    public string IpAddress { get; set; } = null!;

    public string? UserAgent { get; set; }

    public string? Metadata { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public virtual User? User { get; set; }
}
