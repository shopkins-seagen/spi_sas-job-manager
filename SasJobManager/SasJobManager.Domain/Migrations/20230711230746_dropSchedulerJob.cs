using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SasJobManager.Domain.Migrations
{
    public partial class dropSchedulerJob : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchedulerRunJobs");

            migrationBuilder.RenameColumn(
                name: "NumberOfJobs",
                table: "SchedulerRun",
                newName: "JobId");

            migrationBuilder.RenameColumn(
                name: "AppPoolIdentity",
                table: "SchedulerRun",
                newName: "Owner");

            migrationBuilder.AddColumn<string>(
                name: "DriverLoc",
                table: "SchedulerRun",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Msg",
                table: "SchedulerRun",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "SchedulerRun",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecurityGroup",
                table: "SchedulerRun",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppPoolIdentity",
                table: "JobRuns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriverLoc",
                table: "SchedulerRun");

            migrationBuilder.DropColumn(
                name: "Msg",
                table: "SchedulerRun");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "SchedulerRun");

            migrationBuilder.DropColumn(
                name: "SecurityGroup",
                table: "SchedulerRun");

            migrationBuilder.DropColumn(
                name: "AppPoolIdentity",
                table: "JobRuns");

            migrationBuilder.RenameColumn(
                name: "Owner",
                table: "SchedulerRun",
                newName: "AppPoolIdentity");

            migrationBuilder.RenameColumn(
                name: "JobId",
                table: "SchedulerRun",
                newName: "NumberOfJobs");

            migrationBuilder.CreateTable(
                name: "SchedulerRunJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchedulerRunId = table.Column<int>(type: "int", nullable: false),
                    DriverLoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDefaultSecurity = table.Column<bool>(type: "bit", nullable: false),
                    IsFatal = table.Column<bool>(type: "bit", nullable: false),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    Msg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Owner = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SecurityGroup = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
    }
}
