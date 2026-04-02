using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Domain.Entities
{
    public  class UserPermission
    {
        public int UserId { get; set; }

        public int PermissionId { get; set; }

        public bool IsGranted { get; set; }

        public virtual Permission Permission { get; set; } = null!;

        public virtual User User { get; set; } = null!;
    }
}
