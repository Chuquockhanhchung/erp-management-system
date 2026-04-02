using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using ERP.Domain.Entities;
using ERP.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Order = ERP.Domain.Entities.Order;

namespace ERP.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .AsNoTracking()
                .OrderByDescending(x => x.Id)
                .Select(x => new Order
                {
                    Id = x.Id,
                    CustomerId = x.CustomerId,
                    Status = x.Status,
                    TotalAmount = x.TotalAmount,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new Order
                {
                    Id = x.Id,
                    CustomerId = x.CustomerId,
                    Status = x.Status,
                    TotalAmount = x.TotalAmount,
                    CreatedAt = x.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int> CreateAdvancedAsync(CreateOrderDto dto)
        {
            // tương đương logic CreateOrderAdvanced (đơn giản bản EF)
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(x => x.ProductId == dto.ProductId && x.WarehouseId == dto.WarehouseId);

            if (inventory is null || inventory.Quantity < dto.Quantity)
                throw new Exception("Out of stock");

            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == dto.ProductId && (!x.IsDeleted ?? false));
            if (product is null)
                throw new Exception("Product not found");

            using var tran = await _context.Database.BeginTransactionAsync();

            inventory.Quantity -= dto.Quantity;

            _context.InventoryTransactions.Add(new InventoryTransaction
            {
                ProductId = dto.ProductId,
                WarehouseId = dto.WarehouseId,
                QuantityChange = -dto.Quantity,
                TransactionType = "SALE",
                ReferenceId = null,
                CreatedAt = DateTime.Now
            });

            var total = dto.Quantity * product.Price;

            var orderEntity = new Persistence.Entities.Order
            {
                CustomerId = dto.CustomerId,
                Status = "Pending",
                TotalAmount = total,
                CreatedAt = DateTime.Now
            };
            _context.Orders.Add(orderEntity);
            await _context.SaveChangesAsync();

            _context.OrderDetails.Add(new OrderDetail
            {
                OrderId = orderEntity.Id,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                Price = product.Price
            });

            _context.Invoices.Add(new Invoice
            {
                OrderId = orderEntity.Id,
                TotalAmount = total,
                Status = "UNPAID",
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            await tran.CommitAsync();

            return orderEntity.Id;
        }

        public async Task<bool> UpdateStatusAsync(int orderId, string status)
        {
            var entity = await _context.Orders.FirstOrDefaultAsync(x => x.Id == orderId);
            if (entity is null) return false;

            entity.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}