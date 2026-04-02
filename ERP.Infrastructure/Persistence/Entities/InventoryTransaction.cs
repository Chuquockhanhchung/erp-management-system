using System;
using System.Collections.Generic;

namespace ERP.Infrastructure.Persistence.Entities;

public partial class InventoryTransaction
{
    public int Id { get; set; }

    public int? ProductId { get; set; }

    public int? WarehouseId { get; set; }

    public int? QuantityChange { get; set; }

    public string? TransactionType { get; set; }

    public int? ReferenceId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Product? Product { get; set; }

    public virtual Warehouse? Warehouse { get; set; }
}
