using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoodHamburger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DiscountRules_Parameterization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AppliedDiscountRuleId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppliedDiscountRuleName",
                table: "Orders",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "discount_rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Percent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    MatchMode = table.Column<int>(type: "integer", nullable: false),
                    RequiresSandwich = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresFries = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresDrink = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    MinimumSubtotal = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discount_rules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "discount_rule_required_items",
                columns: table => new
                {
                    DiscountRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    MenuItemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discount_rule_required_items", x => new { x.DiscountRuleId, x.MenuItemId });
                    table.ForeignKey(
                        name: "FK_discount_rule_required_items_discount_rules_DiscountRuleId",
                        column: x => x.DiscountRuleId,
                        principalTable: "discount_rules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "discount_rule_required_items");

            migrationBuilder.DropTable(
                name: "discount_rules");

            migrationBuilder.DropColumn(
                name: "AppliedDiscountRuleId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AppliedDiscountRuleName",
                table: "Orders");
        }
    }
}
