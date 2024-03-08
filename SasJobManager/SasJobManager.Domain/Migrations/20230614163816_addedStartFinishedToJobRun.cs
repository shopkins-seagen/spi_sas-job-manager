using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SasJobManager.Domain.Migrations
{
    public partial class addedStartFinishedToJobRun : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Summary",
                table: "JobRuns",
                newName: "WorstFinding");

            migrationBuilder.RenameColumn(
                name: "RunDt",
                table: "JobRuns",
                newName: "Started");

            migrationBuilder.AddColumn<DateTime>(
                name: "Completed",
                table: "JobRuns",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Completed",
                table: "JobRuns");

            migrationBuilder.RenameColumn(
                name: "WorstFinding",
                table: "JobRuns",
                newName: "Summary");

            migrationBuilder.RenameColumn(
                name: "Started",
                table: "JobRuns",
                newName: "RunDt");
        }
    }
}
