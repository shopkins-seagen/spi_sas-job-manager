using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SasJobManager.Domain.Migrations
{
    public partial class movedStatusInSchedulerRun : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOk",
                table: "SchedulerRunJobs");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "SchedulerRunJobs");

            migrationBuilder.AddColumn<bool>(
                name: "IsOk",
                table: "SchedulerRun",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "SchedulerRun",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOk",
                table: "SchedulerRun");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "SchedulerRun");

            migrationBuilder.AddColumn<bool>(
                name: "IsOk",
                table: "SchedulerRunJobs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "SchedulerRunJobs",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
