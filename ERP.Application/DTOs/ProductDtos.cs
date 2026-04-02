using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Application.DTOs;

public record CreateProductDto(string Name, decimal Price);
public record UpdateProductDto(string Name, decimal Price);
public record ProductResponseDto(int Id, string Name, decimal? Price, DateTime? CreatedAt);
