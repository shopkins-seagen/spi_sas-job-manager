using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SasJobManager.Domain.Migrations
{
    public partial class AddHourToJob : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hour",
                table: "Schedules");

            migrationBuilder.AddColumn<int>(
                name: "Hour",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hour",
                table: "Jobs");

            migrationBuilder.AddColumn<int>(
                name: "Hour",
                table: "Schedules",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
