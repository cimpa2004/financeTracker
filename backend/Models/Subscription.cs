using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Subscription
{
    public Guid SubscriptionId { get; set; }

    public Guid UserId { get; set; }

    public Guid? CategoryId { get; set; }

    public decimal? Amount { get; set; }

    public string? Name { get; set; }

    // interval string, e.g. "monthly", "weekly", "yearly"
    public string? Interval { get; set; }

    // next or scheduled payment date
    public DateTime? PaymentDate { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual Category? Category { get; set; }

    // transactions generated from this subscription
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}