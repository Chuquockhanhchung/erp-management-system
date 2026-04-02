using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Application.DTOs
{
    public record CreateOrderDto(int CustomerId, int ProductId, int WarehouseId, int Quantity);
    public record UpdateOrderStatusDto(string Status);
}
