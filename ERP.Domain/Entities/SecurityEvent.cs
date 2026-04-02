using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Domain.Entities
{
    public class SecurityEvent
    {
        public long Id { get; set; }
        public int? UserId { get; set; }
        public string EventType { get; set; } = default!;
        public string? Email { get; set; }
        public string IpAddress { get; set; } = default!;
        public string? UserAgent { get; set; }
        public string? Metadata { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
