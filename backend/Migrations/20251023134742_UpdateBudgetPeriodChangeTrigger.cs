using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
  /// <inheritdoc />
  public partial class UpdateBudgetPeriodChangeTrigger : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.Sql(@"CREATE OR ALTER TRIGGER dbo.TR_Budget_PeriodChanged_DeleteNotifications
  ON dbo.budget
  AFTER UPDATE
  AS
  BEGIN
    SET NOCOUNT ON;

    -- Only act when start_date or end_date actually changed
    IF EXISTS(
      SELECT 1 FROM inserted i
      JOIN deleted d ON i.budget_id = d.budget_id
      WHERE ISNULL(CONVERT(varchar(30), i.start_date, 126),'1900-01-01') <> ISNULL(CONVERT(varchar(30), d.start_date, 126),'1900-01-01')
         OR ISNULL(CONVERT(varchar(30), i.end_date, 126),'1900-01-01') <> ISNULL(CONVERT(varchar(30), d.end_date, 126),'1900-01-01')
    )
    BEGIN
      DELETE n
      FROM dbo.notification n
      INNER JOIN inserted i ON n.budget_id = i.budget_id
      INNER JOIN deleted d ON i.budget_id = d.budget_id
      WHERE ISNULL(CONVERT(varchar(30), n.period_start, 126),'1900-01-01') <> ISNULL(CONVERT(varchar(30), i.start_date, 126),'1900-01-01')
         OR ISNULL(CONVERT(varchar(30), n.period_end, 126),'1900-01-01') <> ISNULL(CONVERT(varchar(30), i.end_date, 126),'1900-01-01');
    END
  END");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.Sql(@"IF OBJECT_ID('dbo.TR_Budget_PeriodChanged_DeleteNotifications', 'TR') IS NOT NULL
        DROP TRIGGER dbo.TR_Budget_PeriodChanged_DeleteNotifications;");
    }
  }
}
