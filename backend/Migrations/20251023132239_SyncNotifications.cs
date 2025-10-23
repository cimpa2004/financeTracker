using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
  /// <inheritdoc />
  public partial class SyncNotifications : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      // Safely ensure 'start_date' exists and, if an older 'last_reset' column exists, copy values and drop it.
      migrationBuilder.Sql(@"
IF COL_LENGTH('budget','start_date') IS NULL
BEGIN
    ALTER TABLE budget ADD start_date DATETIME NULL;
END

IF COL_LENGTH('budget','last_reset') IS NOT NULL
BEGIN
    EXEC('UPDATE budget SET start_date = last_reset WHERE start_date IS NULL');
    EXEC('ALTER TABLE budget DROP COLUMN last_reset');
END
");
      // Add new columns only if they do not already exist
      migrationBuilder.Sql(@"
IF COL_LENGTH('budget','created_at') IS NULL
BEGIN
    ALTER TABLE budget ADD created_at DATETIME NULL;
END

IF COL_LENGTH('budget','end_date') IS NULL
BEGIN
    ALTER TABLE budget ADD end_date DATETIME NULL;
END

IF COL_LENGTH('budget','name') IS NULL
BEGIN
    ALTER TABLE budget ADD name NVARCHAR(255) NULL;
END
");

      migrationBuilder.CreateTable(
    name: "notification",
    columns: table => new
    {
      notification_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
      budget_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
      level = table.Column<int>(type: "int", nullable: false),
      sent_at = table.Column<DateTime>(type: "datetime", nullable: false),
      transaction_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
      period_start = table.Column<DateTime>(type: "datetime", nullable: true),
      period_end = table.Column<DateTime>(type: "datetime", nullable: true)
    },
    constraints: table =>
    {
      table.PrimaryKey("PK__notification__notification_id", x => x.notification_id);
      table.ForeignKey(
                name: "FK__notification__budget__...",
                column: x => x.budget_id,
                principalTable: "budget",
                principalColumn: "budget_id",
                onDelete: ReferentialAction.Cascade);
    });

      migrationBuilder.CreateIndex(
          name: "IX_notification_budget_id",
          table: "notification",
          column: "budget_id");

      // create unique index for (budget_id, level, period_start, period_end) if missing
      migrationBuilder.Sql(@"
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'UQ_notification_budget_level_period' AND object_id = OBJECT_ID('notification'))
BEGIN
    CREATE UNIQUE INDEX UQ_notification_budget_level_period ON notification (budget_id, level, period_start, period_end);
END
");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      // Drop notification table if exists
      migrationBuilder.Sql(@"
IF OBJECT_ID('notification') IS NOT NULL
BEGIN
    DROP TABLE notification;
END
");

      // Drop budget columns only if they exist
      migrationBuilder.Sql(@"
IF COL_LENGTH('budget','created_at') IS NOT NULL
    ALTER TABLE budget DROP COLUMN created_at;
IF COL_LENGTH('budget','end_date') IS NOT NULL
    ALTER TABLE budget DROP COLUMN end_date;
IF COL_LENGTH('budget','name') IS NOT NULL
    ALTER TABLE budget DROP COLUMN name;

-- Recreate last_reset from start_date if absent and start_date exists
IF COL_LENGTH('budget','last_reset') IS NULL AND COL_LENGTH('budget','start_date') IS NOT NULL
BEGIN
    ALTER TABLE budget ADD last_reset DATETIME NULL;
    EXEC('UPDATE budget SET last_reset = start_date WHERE last_reset IS NULL');
    EXEC('ALTER TABLE budget DROP COLUMN start_date');
END
ELSE
BEGIN
    -- If start_date exists and last_reset also exists, just drop start_date
    IF COL_LENGTH('budget','start_date') IS NOT NULL
    BEGIN
        EXEC('ALTER TABLE budget DROP COLUMN start_date');
    END
END
");
    }
  }
}
