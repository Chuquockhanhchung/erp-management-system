using System;
using System.Collections.Generic;

namespace ERP.Infrastructure.Persistence.Entities;

public partial class UserPermission
{
    public int UserId { get; set; }

    public int PermissionId { get; set; }

    public bool IsGranted { get; set; }

    public virtual Permission Permission { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
