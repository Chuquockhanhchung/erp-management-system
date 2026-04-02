using ERP.Application.DTOs;
using ERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Application.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<int> CreateAdvancedAsync(CreateOrderDto dto);
        Task<bool> UpdateStatusAsync(int orderId, UpdateOrderStatusDto dto);
    }
}
