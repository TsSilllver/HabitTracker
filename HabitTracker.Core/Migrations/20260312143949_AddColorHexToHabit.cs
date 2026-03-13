using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HabitTracker.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddColorHexToHabit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ColorHex",
                table: "Habits",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorHex",
                table: "Habits");
        }
    }
}
