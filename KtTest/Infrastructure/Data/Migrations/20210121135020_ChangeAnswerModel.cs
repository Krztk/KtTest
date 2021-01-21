using Microsoft.EntityFrameworkCore.Migrations;

namespace KtTest.Infrastructure.Data.Migrations
{
    public partial class ChangeAnswerModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "MaxScore",
                table: "Answers",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<bool>(
                name: "AllValidChoicesRequired",
                table: "Answers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxScore",
                table: "Answers");

            migrationBuilder.DropColumn(
                name: "AllValidChoicesRequired",
                table: "Answers");
        }
    }
}
