using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SasJobManager.Domain.Migrations
{
    public partial class changedSchedulerRunTAbles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDefaultSecurity",
                table: "SchedulerRun");

            migrationBuilder.RenameColumn(
                name: "User",
                table: "SchedulerRunJobs",
                newName: "Owner");

            migrationBuilder.AddColumn<string>(
                name: "Msg",
                table: "SchedulerRunJobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "SchedulerRunJobs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AppPoolIdentity",
                table: "SchedulerRun",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfJobs",
                table: "SchedulerRun",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Msg",
                table: "SchedulerRunJobs");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "SchedulerRunJobs");

            migrationBuilder.DropColumn(
                name: "AppPoolIdentity",
                table: "SchedulerRun");

            migrationBuilder.DropColumn(
                name: "NumberOfJobs",
                table: "SchedulerRun");

            migrationBuilder.RenameColumn(
                name: "Owner",
                table: "SchedulerRunJobs",
                newName: "User");

            migrationBuilder.AddColumn<bool>(
                name: "IsDefaultSecurity",
                table: "SchedulerRun",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
