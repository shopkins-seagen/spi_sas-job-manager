using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SasJobManager.Domain.Migrations
{
    public partial class AddedSchedulerTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SchedulerRun",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LaunchedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulerRun", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchedulerRunJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    IsSecure = table.Column<bool>(type: "bit", nullable: false),
                    DriverLoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    User = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsOk = table.Column<bool>(type: "bit", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityGroup = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SchedulerRunId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulerRunJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchedulerRunJobs_SchedulerRun_SchedulerRunId",
                        column: x => x.SchedulerRunId,
                        principalTable: "SchedulerRun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerRunJobs_SchedulerRunId",
                table: "SchedulerRunJobs",
                column: "SchedulerRunId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchedulerRunJobs");

            migrationBuilder.DropTable(
                name: "SchedulerRun");
        }
    }
}
