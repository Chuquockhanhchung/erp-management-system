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
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;

        public ProductService(IProductRepository repo) => _repo = repo;

        public async Task<IEnumerable<ProductResponseDto>> GetAllAsync()
            => (await _repo.GetAllAsync())
                .Select(p => new ProductResponseDto(p.Id, p.Name, p.Price, p.CreatedAt));

        public async Task<ProductResponseDto?> GetByIdAsync(int id)
        {
            var p = await _repo.GetByIdAsync(id);
            return p is null ? null : new ProductResponseDto(p.Id, p.Name, p.Price, p.CreatedAt);
        }

        public async Task<int> CreateAsync(CreateProductDto dto)
        {
            var product = new Product { Name = dto.Name, Price = dto.Price };
            return await _repo.CreateAsync(product);
        }

        public async Task<bool> UpdateAsync(int id, UpdateProductDto dto)
        {
            var product = new Product { Id = id, Name = dto.Name, Price = dto.Price };
            return await _repo.UpdateAsync(product);
        }

        public Task<bool> DeleteAsync(int id) => _repo.SoftDeleteAsync(id);
    }
}
