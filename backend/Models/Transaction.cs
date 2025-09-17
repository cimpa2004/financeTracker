using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Transaction
{
    public Guid TransactionId { get; set; }

    public Guid UserId { get; set; }

    public Guid CategoryId { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public DateTime? Date { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Name { get; set; } 

    public virtual Category Category { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
