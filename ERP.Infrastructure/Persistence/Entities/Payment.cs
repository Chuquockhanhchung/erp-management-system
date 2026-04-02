using System;
using System.Collections.Generic;

namespace ERP.Infrastructure.Persistence.Entities;

public partial class Payment
{
    public int Id { get; set; }

    public int? InvoiceId { get; set; }

    public decimal? Amount { get; set; }

    public string? PaymentMethod { get; set; }

    public DateTime? PaidAt { get; set; }

    public virtual Invoice? Invoice { get; set; }
}
