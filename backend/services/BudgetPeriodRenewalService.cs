using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using backend.Models;
using backend.Helpers;

namespace backend.services
{
  public class BudgetPeriodRenewalService : BackgroundService
  {
    private readonly IServiceProvider _services;
    private readonly ILogger<BudgetPeriodRenewalService> _logger;

    public BudgetPeriodRenewalService(IServiceProvider services, ILogger<BudgetPeriodRenewalService> logger)
    {
      _services = services;
      _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      // Run shortly after startup, then every 6 hours
      var delay = TimeSpan.FromMinutes(1);
      try { await Task.Delay(delay, stoppingToken); } catch { }

      while (!stoppingToken.IsCancellationRequested)
      {
        try
        {
          await RenewBudgetsAsync(stoppingToken);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "BudgetPeriodRenewalService error while renewing budgets");
        }

        try { await Task.Delay(TimeSpan.FromHours(6), stoppingToken); } catch { }
      }
    }

    private async Task RenewBudgetsAsync(CancellationToken ct)
    {
      using var scope = _services.CreateScope();
      var db = scope.ServiceProvider.GetRequiredService<FinancetrackerContext>();

      var todayUtc = DateTime.UtcNow.Date;

      // Only budgets with both dates set are eligible
      var expired = await db.Budgets
        .Where(b => b.StartDate != null && b.EndDate != null && b.EndDate < todayUtc)
        .ToListAsync(ct);

      if (!expired.Any()) return;

      foreach (var b in expired)
      {
        if (ct.IsCancellationRequested) break;

        var type = BudgetHelpers.DetectPeriodType(b.StartDate, b.EndDate);
        if (type == BudgetHelpers.BudgetPeriodType.None)
          continue; // custom windows are not auto-renewed

        var s = b.StartDate!.Value;
        var e = b.EndDate!.Value;

        // Advance until the window covers or passes today
        // Guard with max iterations to avoid infinite loops
        for (int i = 0; i < 48 && e.Date < todayUtc; i++)
        {
          (s, e) = BudgetHelpers.NextPeriod(s, e, type);
        }

        if (e.Date < todayUtc)
          continue; // safety guard

        // Update only if changed
        if (s != b.StartDate || e != b.EndDate)
        {
          b.StartDate = s.Date;
          b.EndDate = e.Date;
        }
      }

      await db.SaveChangesAsync(ct);
    }
  }
}
