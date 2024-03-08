using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SasJobManager.Domain.Migrations
{
    public partial class fixSundayInJobs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsSunay",
                table: "Jobs",
                newName: "IsSunday");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsSunday",
                table: "Jobs",
                newName: "IsSunay");
        }
    }
}
