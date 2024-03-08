using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SasJobManager.Domain.Migrations
{
    public partial class update1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "User",
                table: "Programs",
                newName: "Release");

            migrationBuilder.RenameColumn(
                name: "Pgm",
                table: "Programs",
                newName: "Protocol");

            migrationBuilder.AddColumn<string>(
                name: "Analysis",
                table: "Programs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FolderLevel",
                table: "Programs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsLocal",
                table: "Programs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Product",
                table: "Programs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Program",
                table: "Programs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Analysis",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "FolderLevel",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "IsLocal",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "Product",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "Program",
                table: "Programs");

            migrationBuilder.RenameColumn(
                name: "Release",
                table: "Programs",
                newName: "User");

            migrationBuilder.RenameColumn(
                name: "Protocol",
                table: "Programs",
                newName: "Pgm");
        }
    }
}
