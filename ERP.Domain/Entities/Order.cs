using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public string Status { get; set; } = default!;
        public decimal? TotalAmount { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
