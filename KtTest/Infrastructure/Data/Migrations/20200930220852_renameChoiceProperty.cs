using Microsoft.EntityFrameworkCore.Migrations;

namespace KtTest.Infrastructure.Data.Migrations
{
    public partial class renameChoiceProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Text",
                table: "Choice");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Choice",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Choice");

            migrationBuilder.AddColumn<string>(
                name: "Text",
                table: "Choice",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
