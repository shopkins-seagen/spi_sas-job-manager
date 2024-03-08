using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SasJobManager.Domain.Migrations
{
    public partial class removedConfiguration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configurations");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Jobs",
                newName: "Driver");

            migrationBuilder.AddColumn<bool>(
                name: "IsSasPgm",
                table: "Jobs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSasPgm",
                table: "Jobs");

            migrationBuilder.RenameColumn(
                name: "Driver",
                table: "Jobs",
                newName: "Name");

            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    BatFile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsProgram = table.Column<bool>(type: "bit", nullable: false),
                    ProgramFn = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Configurations_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Configurations_JobId",
                table: "Configurations",
                column: "JobId",
                unique: true);
        }
    }
}
