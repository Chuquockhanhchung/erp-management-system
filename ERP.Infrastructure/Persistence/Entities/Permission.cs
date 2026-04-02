using System;
using System.Collections.Generic;

namespace ERP.Infrastructure.Persistence.Entities;

public partial class Permission
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
