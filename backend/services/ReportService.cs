using backend.Models;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using SkiaSharp;

namespace backend.services;

public class ReportService
{
  private readonly FinancetrackerContext _db;

  public ReportService(FinancetrackerContext db)
  {
    _db = db;
  }

  public async Task<byte[]> GenerateBudgetReportPdfAsync(Guid userId, DateTime from, DateTime to, CancellationToken ct = default)
  {
    var budgets = await _db.Budgets
        .Where(b => b.UserId == userId)
        .Include(b => b.Category)
        .ToListAsync(ct);

    // Use Date if present, otherwise CreatedAt as the transaction timestamp so we don't drop rows with null Date
    var transactions = await _db.Transactions
      .Where(t => t.UserId == userId && ((t.Date ?? t.CreatedAt) >= from && (t.Date ?? t.CreatedAt) <= to))
      .Include(t => t.Category)
      .ToListAsync(ct);

    // Prepare chart data
    // Budgets: compute spent per budget
    var budgetItems = budgets.Select(b =>
    {
      // use transaction effective date (Date or CreatedAt)
      var spent = transactions.Where(t => t.CategoryId == b.CategoryId && ((t.Date ?? t.CreatedAt) >= (b.StartDate ?? from) && (t.Date ?? t.CreatedAt) <= (b.EndDate ?? to))).Sum(t => t.Amount);
      return new { Name = b.Name ?? "(no name)", Amount = (double)b.Amount, Spent = (double)Math.Abs(spent) };
    }).ToList();

    // gather category info from budgets and transactions; try to load missing Category
    // records from the database by CategoryId so we can reliably use Category.Type
    // to determine expense vs income even when the navigation property is null.
    var categoryIds = budgets.Select(b => b.CategoryId)
      .Concat(transactions.Select(t => (Guid?)t.CategoryId))
      .Where(id => id.HasValue)
      .Select(id => id.GetValueOrDefault())
      .Distinct()
      .ToList();

    var dbCategories = await _db.Categories.Where(c => categoryIds.Contains(c.CategoryId)).ToListAsync(ct);

    var categoryInfos = budgets.Select(b => b.Category)
      .Concat(transactions.Select(t => t.Category))
      .Where(c => c != null)
      .Select(c => c!)
      .Concat(dbCategories)
      .DistinctBy(c => c.CategoryId)
      .ToList();

    var categoryById = categoryInfos.ToDictionary(c => c.CategoryId);

    bool IsExpenseCategory(Transaction t)
    {
      Category? c = t.Category ?? (categoryById.TryGetValue(t.CategoryId, out var cc) ? cc : null);
      return string.Equals(c?.Type, "expense", StringComparison.OrdinalIgnoreCase);
    }

    var spentByCategory = transactions
      .Where(t => IsExpenseCategory(t))
      .GroupBy(t => t.CategoryId)
      .Select(g => new { CategoryId = g.Key, Spent = (double)g.Sum(t => Math.Abs(t.Amount)) })
      .ToList();

    var pieDataExpenses = spentByCategory.Select(s =>
    {
      var cat = categoryInfos.FirstOrDefault(c => c.CategoryId == s.CategoryId);
      var color = ParseColorOrNull(cat?.Color);
      return (Label: cat?.Name ?? "Uncategorized", Value: s.Spent, Color: color);
    }).ToList();

    // Income by category (non-expense categories)
    var incomeByCategory = transactions
      .Where(t => !IsExpenseCategory(t))
      .GroupBy(t => t.CategoryId)
      .Select(g => new { CategoryId = g.Key, Income = (double)g.Sum(t => Math.Abs(t.Amount)) })
      .ToList();

    var pieDataIncome = incomeByCategory.Select(s =>
    {
      var cat = categoryInfos.FirstOrDefault(c => c.CategoryId == s.CategoryId);
      var color = ParseColorOrNull(cat?.Color);
      return (Label: cat?.Name ?? "Uncategorized", Value: s.Income, Color: color);
    }).ToList();

    // Line chart: daily spent in range (group by day)
    var dayGroups = transactions
      .GroupBy(t => (t.Date ?? t.CreatedAt).GetValueOrDefault().Date)
      .Select(g => new
      {
        Day = g.Key,
        Spent = (double)g.Where(t => IsExpenseCategory(t)).Sum(t => Math.Abs(t.Amount)),
        Income = (double)g.Where(t => !IsExpenseCategory(t)).Sum(t => Math.Abs(t.Amount))
      })
      .OrderBy(g => g.Day)
      .ToList();

    // Render charts to PNGs
    // convert anonymous lists to typed lists for rendering
    var pieTypedExpenses = pieDataExpenses.Select(p => (Label: p.Label, Value: p.Value, Color: p.Color.HasValue ? p.Color.Value : (SKColor?)null)).ToList();
    var pieTypedIncome = pieDataIncome.Select(p => (Label: p.Label, Value: p.Value, Color: p.Color.HasValue ? p.Color.Value : (SKColor?)null)).ToList();
    var daysTyped = dayGroups.Select(d => (Day: d.Day, Value: d.Spent)).ToList();
    var budgetsTyped = budgetItems.Select(b => (Name: b.Name, Amount: b.Amount, Spent: b.Spent)).ToList();
    var piePngExpenses = RenderPieChartPng(pieTypedExpenses, 700, 520);
    var piePngIncome = RenderPieChartPng(pieTypedIncome, 700, 520);
    var linePng = RenderLineChartPng(daysTyped, 700, 220);
    var budgetPng = RenderBudgetBarChartPng(budgetsTyped, 700, Math.Max(120, budgetItems.Count * 36));

    var doc = Document.Create(container =>
    {
      container.Page(page =>
      {
        page.Size(PageSizes.A4);
        page.Margin(20);
        page.PageColor(Colors.White);
        page.DefaultTextStyle(x => x.FontSize(11));

        page.Header().Text($"Trackaroo report — {from:yyyy-MM-dd} — {to:yyyy-MM-dd}").SemiBold().FontSize(16);

        page.Content().Column(col =>
        {
          col.Item().Text("Budgets summary").Bold().FontSize(13);
          if (!budgetItems.Any())
          {
            col.Item().Text("No budgets found");
          }
          else
          {
            col.Item().Height((float)Math.Min(400, budgetItems.Count * 36 + 40)).Element(e => e.Image(budgetPng));
            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
          }

          col.Item().PaddingTop(10).Text("Statistics").Bold().FontSize(13);
          var hasExpense = pieTypedExpenses.Any();
          var hasIncome = pieTypedIncome.Any();
          if (!hasExpense && !hasIncome)
          {
            col.Item().Text("No transactions data for this period");
          }
          else
          {
            var totalExpenses = pieTypedExpenses.Sum(p => p.Value);
            var totalIncome = pieTypedIncome.Sum(p => p.Value);

            // Totals
            col.Item().Row(rTotals =>
            {
              rTotals.RelativeItem().Text($"Total income: {totalIncome:0.##}").FontSize(12);
              rTotals.RelativeItem().AlignRight().Text($"Total expenses: {totalExpenses:0.##}").FontSize(12);
            });

            // pies side-by-side (show placeholder if missing)
            col.Item().Row(r =>
            {
              r.RelativeItem().Column(left =>
              {
                if (hasIncome)
                  left.Item().Height(220).Element(e => e.Image(piePngIncome));
                else
                {
                  left.Item().Height(220).AlignCenter().Text("No income data");
                }
              });

              r.ConstantItem(20);

              r.RelativeItem().Column(right =>
              {
                if (hasExpense)
                  right.Item().Height(220).Element(e => e.Image(piePngExpenses));
                else
                {
                  right.Item().Height(220).AlignCenter().Text("No expense data");
                }
              });
            });

            // line chart for expenses by day
            col.Item().Height(220).Element(e => e.Image(linePng));
          }
        });

        page.Footer().AlignCenter().Text("Generated by Trackaroo");
      });

      // Page: All transactions listing (Name, Category, Value with +/-)
      container.Page(page =>
      {
        page.Size(PageSizes.A4);
        page.Margin(20);
        page.PageColor(Colors.White);
        page.DefaultTextStyle(x => x.FontSize(11));

        page.Header().Text($"Trackaroo report — {from:yyyy-MM-dd} — {to:yyyy-MM-dd}").SemiBold().FontSize(16);

        page.Content().Column(col =>
        {
          col.Item().Text("Transactions").Bold().FontSize(13);

          var txRows = transactions
            .OrderBy(t => (t.Date ?? t.CreatedAt) ?? DateTime.MinValue)
            .Select(t =>
            {
              var cat = t.Category ?? (categoryById.TryGetValue(t.CategoryId, out var c) ? c : null);
              var isExpense = string.Equals(cat?.Type, "expense", StringComparison.OrdinalIgnoreCase);
              var signedText = (isExpense ? "-" : "+") + Math.Abs(t.Amount).ToString("0.##");
              return new
              {
                Name = string.IsNullOrWhiteSpace(t.Name) ? (t.Description ?? "(no name)") : t.Name!,
                Category = cat?.Name ?? "Uncategorized",
                Value = signedText
              };
            })
            .ToList();

          col.Item().Table(table =>
          {
            table.ColumnsDefinition(columns =>
            {
              columns.RelativeColumn(3); // Name
              columns.RelativeColumn(2); // Category
              columns.RelativeColumn(1); // Value
            });

            table.Header(header =>
            {
              header.Cell().Text("Name").SemiBold();
              header.Cell().Text("Category").SemiBold();
              header.Cell().AlignRight().Text("Value").SemiBold();
            });

            for (int i = 0; i < txRows.Count; i++)
            {
              var r = txRows[i];
              // alternate row background for readability
              var bg = (i % 2 == 1) ? Colors.Grey.Lighten3 : Colors.White;

              table.Cell().Element(cell => cell.Background(bg).Padding(6).Text(r.Name));
              table.Cell().Element(cell => cell.Background(bg).Padding(6).Text(r.Category));
              table.Cell().Element(cell => cell.Background(bg).Padding(6).AlignRight().Text(r.Value));
            }
          });
        });

        page.Footer().AlignCenter().Text("Generated by Trackaroo");
      });
    });

    using var ms = new MemoryStream();
    doc.GeneratePdf(ms);
    return ms.ToArray();
  }

  // --- Chart helpers (SkiaSharp) ---
  private static SKColor? ParseColorOrNull(string? hex)
  {
    if (string.IsNullOrWhiteSpace(hex))
      return null;

    try
    {
      // accept formats like #RRGGBB or RRGGBB
      return SKColor.Parse(hex.Trim());
    }
    catch
    {
      return null;
    }
  }

  private static byte[] RenderPieChartPng(List<(string Label, double Value, SKColor? Color)> data, int width, int height)
  {
    using var bitmap = new SKBitmap(width, height);
    using var canvas = new SKCanvas(bitmap);
    canvas.Clear(SKColors.White);

    if (data == null || data.Count == 0 || data.Sum(d => d.Value) <= 0)
    {
      using var paint = new SKPaint { Color = SKColors.Black, TextSize = 14 };
      canvas.DrawText("No data", width / 2f - 20f, height / 2f, paint);
      using var img = SKImage.FromBitmap(bitmap);
      using var enc1 = img.Encode(SKEncodedImageFormat.Png, 90);
      return enc1.ToArray();
    }

    var total = data.Sum(d => d.Value);
    // Reserve space at the bottom for legend
    var margin = 10f;
    var legendAreaHeight = 80f;
    var pieDiameter = Math.Min(width - 2 * margin, height - legendAreaHeight - 2 * margin);
    var pieRect = new SKRect(
      (width - pieDiameter) / 2f,
      margin,
      (width + pieDiameter) / 2f,
      margin + pieDiameter
    );
    float start = -90;

    var defaultColors = new[] { SKColor.Parse("#4CAF50"), SKColor.Parse("#2196F3"), SKColor.Parse("#FF9800"), SKColor.Parse("#E91E63"), SKColor.Parse("#9C27B0") };

    for (int i = 0; i < data.Count; i++)
    {
      var slice = data[i];
      var sweep = (float)(slice.Value / total * 360.0);
      using var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = slice.Color ?? defaultColors[i % defaultColors.Length] };
      canvas.DrawArc(pieRect, start, sweep, true, paint);
      start += sweep;
    }

    // legend at bottom, wrapping horizontally
    using var textPaint = new SKPaint { Color = SKColors.Black, TextSize = 11, IsAntialias = true };
    var legX = margin;
    var legY = height - legendAreaHeight + margin;
    var maxX = width - margin;
    float lineHeight = 18f;
    for (int i = 0; i < data.Count; i++)
    {
      var d = data[i];
      var color = d.Color ?? defaultColors[i % defaultColors.Length];
      using var rectPaint = new SKPaint { Color = color, IsAntialias = true };
      var label = $"{d.Label} ({d.Value:0.##})";
      var labelWidth = textPaint.MeasureText(label);
      var itemWidth = 14 + 6 + labelWidth + 16; // square + gap + text + padding

      if (legX + itemWidth > maxX)
      {
        // wrap to next line
        legX = margin;
        legY += lineHeight;
      }

      // color square
      canvas.DrawRect(new SKRect(legX, legY - 12, legX + 14, legY + 2), rectPaint);
      // text
      canvas.DrawText(label, legX + 20, legY, textPaint);
      legX += itemWidth;
    }

    using var image = SKImage.FromBitmap(bitmap);
    using var enc2 = image.Encode(SKEncodedImageFormat.Png, 90);
    return enc2.ToArray();
  }

  private static byte[] RenderLineChartPng(List<(DateTime Day, double Value)> points, int width, int height)
  {
    using var bitmap = new SKBitmap(width, height);
    using var canvas = new SKCanvas(bitmap);
    canvas.Clear(SKColors.White);

    if (points == null || points.Count == 0)
    {
      using var paint = new SKPaint { Color = SKColors.Black, TextSize = 14 };
      canvas.DrawText("No data", width / 2f - 20f, height / 2f, paint);
      using var img = SKImage.FromBitmap(bitmap);
      using var enc3 = img.Encode(SKEncodedImageFormat.Png, 90);
      return enc3.ToArray();
    }

    var marginLeft = 40f;
    var marginRight = 10f;
    var marginTop = 10f;
    var marginBottom = 30f;

    var plotWidth = width - marginLeft - marginRight;
    var plotHeight = height - marginTop - marginBottom;

    var max = (float)Math.Max(1, points.Max(p => p.Value));
    var min = (float)points.Min(p => p.Value);
    var range = Math.Max(0.0001f, max - min);

    using var axisPaint = new SKPaint { Color = SKColors.LightGray, StrokeWidth = 1 };
    using var linePaint = new SKPaint { Color = SKColors.Blue, StrokeWidth = 2, IsStroke = true, IsAntialias = true };
    using var fillPaint = new SKPaint { Color = SKColor.Parse("#BBDEFB"), IsAntialias = true };
    using var textPaint = new SKPaint { Color = SKColors.Black, TextSize = 10, IsAntialias = true };

    // draw axes
    canvas.DrawLine(marginLeft, marginTop, marginLeft, marginTop + plotHeight, axisPaint);
    canvas.DrawLine(marginLeft, marginTop + plotHeight, marginLeft + plotWidth, marginTop + plotHeight, axisPaint);

    // plot points
    var stepX = plotWidth / Math.Max(1, points.Count - 1);
    var pts = new SKPoint[points.Count];
    for (int i = 0; i < points.Count; i++)
    {
      var x = marginLeft + i * stepX;
      var y = marginTop + plotHeight - (float)((points[i].Value - min) / range * plotHeight);
      pts[i] = new SKPoint(x, y);
    }

    // fill area
    var path = new SKPath();
    path.MoveTo(pts[0]);
    for (int i = 1; i < pts.Length; i++) path.LineTo(pts[i]);
    path.LineTo(marginLeft + plotWidth, marginTop + plotHeight);
    path.LineTo(marginLeft, marginTop + plotHeight);
    path.Close();
    canvas.DrawPath(path, fillPaint);

    // draw line
    for (int i = 0; i < pts.Length - 1; i++)
    {
      canvas.DrawLine(pts[i], pts[i + 1], linePaint);
    }

    // draw points
    foreach (var p in pts) canvas.DrawCircle(p, 3, linePaint);

    // labels (min/max)
    canvas.DrawText(max.ToString("0.##"), 4, marginTop + 10, textPaint);
    canvas.DrawText(min.ToString("0.##"), 4, marginTop + plotHeight, textPaint);

    using var image = SKImage.FromBitmap(bitmap);
    using var enc4 = image.Encode(SKEncodedImageFormat.Png, 90);
    return enc4.ToArray();
  }

  private static byte[] RenderBudgetBarChartPng(List<(string Name, double Amount, double Spent)> items, int width, int height)
  {
    using var bitmap = new SKBitmap(width, height);
    using var canvas = new SKCanvas(bitmap);
    canvas.Clear(SKColors.White);

    if (items == null || items.Count == 0)
    {
      using var paint = new SKPaint { Color = SKColors.Black, TextSize = 14 };
      canvas.DrawText("No budgets", width / 2f - 30f, height / 2f, paint);
      using var img = SKImage.FromBitmap(bitmap);
      using var enc5 = img.Encode(SKEncodedImageFormat.Png, 90);
      return enc5.ToArray();
    }

    var marginLeft = 120f;
    var marginRight = 16f;
    var marginTop = 10f;
    var rowHeight = Math.Max(28f, (height - marginTop) / items.Count);

    var maxBudget = items.Max(i => i.Amount);
    var bgPaint = new SKPaint { Color = SKColor.Parse("#EEEEEE"), IsAntialias = true };
    var fgPaint = new SKPaint { Color = SKColor.Parse("#4CAF50"), IsAntialias = true };
    var textPaint = new SKPaint { Color = SKColors.Black, TextSize = 12, IsAntialias = true };

    for (int i = 0; i < items.Count; i++)
    {
      var it = items[i];
      var y = marginTop + i * rowHeight;
      // label
      canvas.DrawText(it.Name, 8, y + rowHeight / 2 + 5, textPaint);

      var barX = marginLeft;
      var barWidth = width - marginLeft - marginRight;
      var ratio = maxBudget <= 0 ? 0 : (float)(it.Amount / maxBudget);
      var spendRatio = it.Amount <= 0 ? 0 : (float)(Math.Min(it.Spent, it.Amount) / it.Amount);

      // background full budget
      canvas.DrawRect(new SKRect(barX, y + 6, barX + barWidth * ratio, y + rowHeight - 6), bgPaint);
      // spent
      canvas.DrawRect(new SKRect(barX, y + 6, barX + barWidth * ratio * spendRatio, y + rowHeight - 6), fgPaint);

      // amount text
      canvas.DrawText($"{it.Spent:0.##} / {it.Amount:0.##}", barX + 4, y + rowHeight / 2 + 5, textPaint);
    }

    using var image = SKImage.FromBitmap(bitmap);
    using var enc6 = image.Encode(SKEncodedImageFormat.Png, 90);
    return enc6.ToArray();
  }
}
