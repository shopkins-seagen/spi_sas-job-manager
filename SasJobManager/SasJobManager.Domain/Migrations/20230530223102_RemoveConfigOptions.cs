using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SasJobManager.Domain.Migrations
{
    public partial class RemoveConfigOptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Recipient");

            migrationBuilder.DropColumn(
                name: "Context",
                table: "Configurations");

            migrationBuilder.DropColumn(
                name: "DoesCheckLog",
                table: "Configurations");

            migrationBuilder.DropColumn(
                name: "DoesMacroLogging",
                table: "Configurations");

            migrationBuilder.DropColumn(
                name: "IsAsync",
                table: "Configurations");

            migrationBuilder.DropColumn(
                name: "Port",
                table: "Configurations");

            migrationBuilder.DropColumn(
                name: "Server",
                table: "Configurations");

            migrationBuilder.DropColumn(
                name: "SummaryFn",
                table: "Configurations");

            migrationBuilder.RenameColumn(
                name: "UseBestServer",
                table: "Configurations",
                newName: "IsProgram");

            migrationBuilder.RenameColumn(
                name: "Csv",
                table: "Configurations",
                newName: "BatFile");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsProgram",
                table: "Configurations",
                newName: "UseBestServer");

            migrationBuilder.RenameColumn(
                name: "BatFile",
                table: "Configurations",
                newName: "Csv");

            migrationBuilder.AddColumn<string>(
                name: "Context",
                table: "Configurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "DoesCheckLog",
                table: "Configurations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesMacroLogging",
                table: "Configurations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAsync",
                table: "Configurations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Port",
                table: "Configurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Server",
                table: "Configurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SummaryFn",
                table: "Configurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Recipient",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfigurationId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipient", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recipient_Configurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "Configurations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recipient_ConfigurationId",
                table: "Recipient",
                column: "ConfigurationId");
        }
    }
}
