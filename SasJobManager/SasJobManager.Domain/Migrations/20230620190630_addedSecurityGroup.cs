using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SasJobManager.Domain.Migrations
{
    public partial class addedSecurityGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomSecurityGroup",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCustomSecurity",
                table: "Jobs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomSecurityGroup",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "IsCustomSecurity",
                table: "Jobs");
        }
    }
}
