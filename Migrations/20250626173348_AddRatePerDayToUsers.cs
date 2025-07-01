using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaterJarAttendanceSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddRatePerDayToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RatePerDay",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RatePerDay",
                table: "AspNetUsers");
        }
    }
}
