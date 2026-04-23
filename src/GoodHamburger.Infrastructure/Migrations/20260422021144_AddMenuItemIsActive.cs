using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoodHamburger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuItemIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "MenuItems",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "MenuItems");
        }
    }
}
