using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using ERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repo;

        public OrderService(IOrderRepository repo) => _repo = repo;

        public Task<IEnumerable<Order>> GetAllAsync() => _repo.GetAllAsync();

        public Task<int> CreateAdvancedAsync(CreateOrderDto dto) => _repo.CreateAdvancedAsync(dto);

        public Task<bool> UpdateStatusAsync(int orderId, UpdateOrderStatusDto dto)
            => _repo.UpdateStatusAsync(orderId, dto.Status);
    }
}
