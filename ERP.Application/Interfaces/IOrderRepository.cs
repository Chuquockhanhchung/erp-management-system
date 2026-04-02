using ERP.Application.DTOs;
using ERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Application.Interfaces
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order?> GetByIdAsync(int id);
        Task<int> CreateAdvancedAsync(CreateOrderDto dto); // gọi SP CreateOrderAdvanced
        Task<bool> UpdateStatusAsync(int orderId, string status);
    }
}
