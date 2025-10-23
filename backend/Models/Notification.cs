using System;

namespace backend.Models;

public partial class Notification
{
  public Guid NotificationId { get; set; }
  public Guid BudgetId { get; set; }
  public int Level { get; set; } // 90 or 100
  public DateTime SentAt { get; set; }
  public Guid? TransactionId { get; set; }
  public DateTime? PeriodStart { get; set; }
  public DateTime? PeriodEnd { get; set; }
}