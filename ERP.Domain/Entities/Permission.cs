using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Domain.Entities
{
    public class Permission
    {
        public int Id { get; set; }

        public string Code { get; set; } = null!;

        public string? Description { get; set; }

        public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();

        public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    }
}
