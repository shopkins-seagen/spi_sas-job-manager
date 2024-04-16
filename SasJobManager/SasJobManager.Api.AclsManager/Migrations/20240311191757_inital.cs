using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SasJobManager.Api.AclsManager.Migrations
{
    public partial class inital : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SasLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LogFile = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SasLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SasLogsHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SasLogId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SasLogsHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SasLogsHistory_SasLogs_SasLogId",
                        column: x => x.SasLogId,
                        principalTable: "SasLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SasLogsHistory_SasLogId",
                table: "SasLogsHistory",
                column: "SasLogId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SasLogsHistory");

            migrationBuilder.DropTable(
                name: "SasLogs");
        }
    }
}
