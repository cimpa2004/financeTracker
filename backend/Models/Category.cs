using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Category
{
    public Guid CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Icon { get; set; }

    public string? Color { get; set; }

    public Guid? UserId { get; set; }

    public string Type { get; set; } = null!;

    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual User? User { get; set; }
}
