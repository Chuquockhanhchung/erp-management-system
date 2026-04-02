using ERP.Application.Interfaces;
using ERP.Domain.Entities;
using ERP.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Domain.Entities.Product>> GetAllAsync()
        {
            return await _context.Products
                .AsNoTracking()
                .Where(x => !x.IsDeleted??false)
                .OrderByDescending(x => x.Id)
                .Select(x => new Domain.Entities.Product
                {
                    Id = x.Id,
                    Name = x.Name,
                    Price = x.Price,
                    IsDeleted = x.IsDeleted ?? false,
                    CreatedAt = x.CreatedAt??DateTime.Now
                })
                .ToListAsync();
        }

        public async Task<Domain.Entities.Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .AsNoTracking()
                .Where(x => x.Id == id && (x.IsDeleted??false))
                .Select(x => new Domain.Entities.Product
                {
                    Id = x.Id,
                    Name = x.Name,
                    Price = x.Price,
                    IsDeleted = x.IsDeleted,
                    CreatedAt = x.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int> CreateAsync(Domain.Entities.Product product)
        {
            var entity = new Persistence.Entities.Product
            {
                Name = product.Name,
                Price = product.Price,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };

            _context.Products.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(Domain.Entities.Product product)
        {
            var entity = await _context.Products
                .FirstOrDefaultAsync(x => x.Id == product.Id && (!x.IsDeleted??false));

            if (entity is null) return false;

            entity.Name = product.Name;
            entity.Price = product.Price;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var entity = await _context.Products
                .FirstOrDefaultAsync(x => x.Id == id && (!x.IsDeleted??false));

            if (entity is null) return false;

            entity.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}