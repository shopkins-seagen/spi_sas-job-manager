using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SasJobManager.Domain.Migrations
{
    public partial class isFatal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFatal",
                table: "SchedulerRunJobs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFatal",
                table: "SchedulerRunJobs");
        }
    }
}
