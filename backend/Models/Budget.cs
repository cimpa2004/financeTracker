using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Budget
{
  public Guid BudgetId { get; set; }

  public Guid UserId { get; set; }

  public Guid? CategoryId { get; set; }
  public string? Name { get; set; } = null!;

  public DateTime? StartDate { get; set; }
  public DateTime? EndDate { get; set; }
  public DateTime? CreatedAt { get; set; }
  public decimal Amount { get; set; }
  public virtual Category? Category { get; set; }

  public virtual User User { get; set; } = null!;
}
