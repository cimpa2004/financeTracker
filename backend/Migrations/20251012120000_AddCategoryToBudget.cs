using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
  public partial class AddCategoryToBudget : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<Guid>(
          name: "category_id",
          table: "budget",
          type: "uniqueidentifier",
          nullable: true);

      migrationBuilder.CreateIndex(
          name: "IX_budget_category_id",
          table: "budget",
          column: "category_id");

      migrationBuilder.AddForeignKey(
          name: "FK__budget__category__37A5467C",
          table: "budget",
          column: "category_id",
          principalTable: "category",
          principalColumn: "category_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropForeignKey(
          name: "FK__budget__category__37A5467C",
          table: "budget");

      migrationBuilder.DropIndex(
          name: "IX_budget_category_id",
          table: "budget");

      migrationBuilder.DropColumn(
          name: "category_id",
          table: "budget");
    }
  }
}
