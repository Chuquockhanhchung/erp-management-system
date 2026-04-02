using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Domain.Entities
{

    public class RefreshToken
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public string TokenHash { get; set; } = default!;
        public string JwtId { get; set; } = default!;
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string CreatedByIp { get; set; } = default!;
        public DateTime? RevokedAtUtc { get; set; }
        public string? RevokedByIp { get; set; }
        public long? ReplacedByTokenId { get; set; }
    }
}
