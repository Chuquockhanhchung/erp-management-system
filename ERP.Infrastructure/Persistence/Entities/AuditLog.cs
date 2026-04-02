using System;
using System.Collections.Generic;

namespace ERP.Infrastructure.Persistence.Entities;

public partial class AuditLog
{
    public int Id { get; set; }

    public string? TableName { get; set; }

    public string? Action { get; set; }

    public string? OldData { get; set; }

    public string? NewData { get; set; }

    public int? UserId { get; set; }

    public DateTime? CreatedAt { get; set; }
}
