using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // user table (lowercase to match later rename steps)
            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    username = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    salt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    modified_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__user__B9BE370FEFF28101", x => x.user_id);
                });

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

            // category table
            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    category_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    icon = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__category__category_id", x => x.category_id);
                    table.ForeignKey(
                        name: "FK_category_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_category_user_id",
                table: "category",
                column: "user_id");

            // household table
            migrationBuilder.CreateTable(
                name: "household",
                columns: table => new
                {
                    household_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__household__household_id", x => x.household_id);
                });

            // household_member table
            migrationBuilder.CreateTable(
                name: "household_member",
                columns: table => new
                {
                    household_member_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    household_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    role = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__household_member__household_member_id", x => x.household_member_id);
                    table.ForeignKey(
                        name: "FK_household_member_household",
                        column: x => x.household_id,
                        principalTable: "household",
                        principalColumn: "household_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_household_member_user",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_household_member_household_id",
                table: "household_member",
                column: "household_id");

            migrationBuilder.CreateIndex(
                name: "IX_household_member_user_id",
                table: "household_member",
                column: "user_id");

            // external account table
            migrationBuilder.CreateTable(
                name: "external_account",
                columns: table => new
                {
                    external_account_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    provider = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    balance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__external_account__external_account_id", x => x.external_account_id);
                    table.ForeignKey(
                        name: "FK_external_account_user",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_external_account_user_id",
                table: "external_account",
                column: "user_id");

            // budget table
            migrationBuilder.CreateTable(
                name: "budget",
                columns: table => new
                {
                    budget_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    household_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    start_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    end_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__budget__budget_id", x => x.budget_id);
                    table.ForeignKey(
                        name: "FK_budget_user",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_budget_household",
                        column: x => x.household_id,
                        principalTable: "household",
                        principalColumn: "household_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_budget_user_id",
                table: "budget",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_budget_household_id",
                table: "budget",
                column: "household_id");

            // transaction table (base schema; later migrations add 'name' and 'subscription_id')
            migrationBuilder.CreateTable(
                name: "transaction",
                columns: table => new
                {
                    transaction_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    category_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    date = table.Column<DateTime>(type: "datetime", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__transact__85C600AFBB62D269", x => x.transaction_id);
                    table.ForeignKey(
                        name: "FK__transacti__categ__32E0915F",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__transacti__user___31EC6D26",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_transaction_category_id",
                table: "transaction",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_user_id",
                table: "transaction",
                column: "user_id");

            // subscription table (create here so later migrations that alter it don't fail)
            migrationBuilder.CreateTable(
                name: "subscription",
                columns: table => new
                {
                    subscription_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    category_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    amount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    interval = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    payment_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__subscription__subscription_id", x => x.subscription_id);
                    table.ForeignKey(
                        name: "FK__subscription__category__idx",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "category_id");
                    table.ForeignKey(
                        name: "FK__subscription__user__idx",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_subscription_category_id",
                table: "subscription",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_user_id",
                table: "subscription",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // drop in reverse order of creation to avoid FK issues
            migrationBuilder.DropTable(
                name: "subscription");

            migrationBuilder.DropTable(
                name: "transaction");

            migrationBuilder.DropTable(
                name: "budget");

            migrationBuilder.DropTable(
                name: "external_account");

            migrationBuilder.DropTable(
                name: "household_member");

            migrationBuilder.DropTable(
                name: "household");

            migrationBuilder.DropTable(
                name: "category");

            // remove user unique indexes then drop user
            migrationBuilder.DropIndex(
                name: "UQ__user__AB6E6164796C5540",
                table: "user");

            migrationBuilder.DropIndex(
                name: "UQ__user__F3DBC57200E5048C",
                table: "user");

            migrationBuilder.DropTable(
                name: "user");
        }
    }
}
