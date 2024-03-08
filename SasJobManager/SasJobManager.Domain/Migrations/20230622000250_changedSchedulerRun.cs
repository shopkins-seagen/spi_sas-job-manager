using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SasJobManager.Domain.Migrations
{
    public partial class changedSchedulerRun : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsSecure",
                table: "SchedulerRunJobs",
                newName: "IsDefaultSecurity");

            migrationBuilder.AddColumn<bool>(
                name: "IsDefaultSecurity",
                table: "SchedulerRun",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDefaultSecurity",
                table: "SchedulerRun");

            migrationBuilder.RenameColumn(
                name: "IsDefaultSecurity",
                table: "SchedulerRunJobs",
                newName: "IsSecure");
        }
    }
}
