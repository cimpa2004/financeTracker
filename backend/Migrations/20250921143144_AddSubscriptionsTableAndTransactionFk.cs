using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionsTableAndTransactionFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // drop known FKs that reference user (including category->user)
            migrationBuilder.Sql(@"
-- drop known FKs that reference user
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK__transacti__user___31EC6D26')
    ALTER TABLE [transaction] DROP CONSTRAINT [FK__transacti__user___31EC6D26];

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_external_account_user')
    ALTER TABLE [external_account] DROP CONSTRAINT [FK_external_account_user];

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_household_member_user')
    ALTER TABLE [household_member] DROP CONSTRAINT [FK_household_member_user];

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_budget_user')
    ALTER TABLE [budget] DROP CONSTRAINT [FK_budget_user];

-- drop category->user FK if present (this was missing and caused the failure)
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_category_user_user_id')
    ALTER TABLE [category] DROP CONSTRAINT [FK_category_user_user_id];

-- drop any subscription->user FK (name may vary) safely using dynamic sql
DECLARE @fk sysname;
SELECT @fk = name FROM sys.foreign_keys WHERE name LIKE 'FK%subscription%user%';
IF @fk IS NOT NULL EXEC('ALTER TABLE [subscription] DROP CONSTRAINT [' + @fk + ']');
");

            // now safe to drop the primary key on 'user'
            migrationBuilder.DropPrimaryKey(
                name: "PK__user__B9BE370FEFF28101",
                table: "user");

            migrationBuilder.DropIndex(
                name: "UQ__user__AB6E6164796C5540",
                table: "user");

            migrationBuilder.DropIndex(
                name: "UQ__user__F3DBC57200E5048C",
                table: "user");

            migrationBuilder.RenameTable(
                name: "user",
                newName: "Users");

            migrationBuilder.RenameColumn(
                name: "username",
                table: "Users",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "salt",
                table: "Users",
                newName: "Salt");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Users",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "Users",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "Users",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Users",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Users",
                newName: "UserId");

            migrationBuilder.AddColumn<Guid>(
                name: "subscription_id",
                table: "transaction",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Salt",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldUnicode: false,
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldUnicode: false,
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ModifiedAt",
                table: "Users",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "(newsequentialid())");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "UserId");

            // create subscription table only if it does not exist, and create indexes / FK only if missing
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'subscription' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [subscription] (
        [subscription_id] uniqueidentifier NOT NULL DEFAULT (newsequentialid()),
        [user_id] uniqueidentifier NOT NULL,
        [category_id] uniqueidentifier NULL,
        [amount] decimal(10,2) NULL,
        [name] nvarchar(255) NULL,
        [interval] nvarchar(50) NULL,
        [payment_date] datetime NULL,
        [is_active] bit NOT NULL DEFAULT (CAST(1 AS bit)),
        [created_at] datetime NULL DEFAULT (getdate()),
        CONSTRAINT [PK__subscription__subscription_id] PRIMARY KEY ([subscription_id])
    );
    ALTER TABLE [subscription] ADD CONSTRAINT [FK_subscription_category] FOREIGN KEY ([category_id]) REFERENCES [category]([category_id]);
    ALTER TABLE [subscription] ADD CONSTRAINT [FK_subscription_user] FOREIGN KEY ([user_id]) REFERENCES [Users]([UserId]) ON DELETE CASCADE;
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_transaction_subscription_id' AND object_id = OBJECT_ID('transaction'))
    CREATE INDEX IX_transaction_subscription_id ON [transaction]([subscription_id]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_subscription_category_id' AND object_id = OBJECT_ID('subscription'))
    CREATE INDEX IX_subscription_category_id ON [subscription]([category_id]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_subscription_user_id' AND object_id = OBJECT_ID('subscription'))
    CREATE INDEX IX_subscription_user_id ON [subscription]([user_id]);

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_transaction_subscription')
BEGIN
    ALTER TABLE [transaction] ADD CONSTRAINT FK_transaction_subscription FOREIGN KEY ([subscription_id]) REFERENCES [subscription]([subscription_id]) ON DELETE SET NULL;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__transaction__subscr__...",
                table: "transaction");

            migrationBuilder.DropTable(
                name: "subscription");

            migrationBuilder.DropIndex(
                name: "IX_transaction_subscription_id",
                table: "transaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "subscription_id",
                table: "transaction");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "user");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "user",
                newName: "username");

            migrationBuilder.RenameColumn(
                name: "Salt",
                table: "user",
                newName: "salt");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "user",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "user",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "user",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "user",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "user",
                newName: "user_id");

            migrationBuilder.AlterColumn<string>(
                name: "username",
                table: "user",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "salt",
                table: "user",
                type: "varchar(255)",
                unicode: false,
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "user",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "password_hash",
                table: "user",
                type: "varchar(255)",
                unicode: false,
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified_at",
                table: "user",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "user",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "user",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "(newsequentialid())",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddPrimaryKey(
                name: "PK__user__B9BE370FEFF28101",
                table: "user",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ__user__AB6E6164796C5540",
                table: "user",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__user__F3DBC57200E5048C",
                table: "user",
                column: "username",
                unique: true);
        }
    }
}
