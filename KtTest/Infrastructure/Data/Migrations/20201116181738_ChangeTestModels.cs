using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KtTest.Infrastructure.Data.Migrations
{
    public partial class ChangeTestModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestItems_Tests_TestId",
                table: "TestItems");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTests_Tests_TestId",
                table: "UserTests");

            migrationBuilder.DropTable(
                name: "Tests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTests",
                table: "UserTests");

            migrationBuilder.DropIndex(
                name: "IX_UserTests_TestId",
                table: "UserTests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAnswers",
                table: "UserAnswers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TestItems",
                table: "TestItems");

            migrationBuilder.DropColumn(
                name: "TestId",
                table: "UserTests");

            migrationBuilder.DropColumn(
                name: "TestId",
                table: "UserAnswers");

            migrationBuilder.DropColumn(
                name: "TestId",
                table: "TestItems");

            migrationBuilder.AddColumn<int>(
                name: "ScheduledTestId",
                table: "UserTests",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ScheduledTestId",
                table: "UserAnswers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TestTemplateId",
                table: "TestItems",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTests",
                table: "UserTests",
                columns: new[] { "UserId", "ScheduledTestId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAnswers",
                table: "UserAnswers",
                columns: new[] { "ScheduledTestId", "QuestionId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestItems",
                table: "TestItems",
                columns: new[] { "TestTemplateId", "QuestionId" });

            migrationBuilder.CreateTable(
                name: "TestTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    AuthorId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledTests",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TestTemplateId = table.Column<int>(nullable: false),
                    Duration = table.Column<int>(nullable: false),
                    PublishedAt = table.Column<DateTime>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledTests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledTests_TestTemplates_TestTemplateId",
                        column: x => x.TestTemplateId,
                        principalTable: "TestTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserTests_ScheduledTestId",
                table: "UserTests",
                column: "ScheduledTestId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTests_TestTemplateId",
                table: "ScheduledTests",
                column: "TestTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestItems_TestTemplates_TestTemplateId",
                table: "TestItems",
                column: "TestTemplateId",
                principalTable: "TestTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTests_ScheduledTests_ScheduledTestId",
                table: "UserTests",
                column: "ScheduledTestId",
                principalTable: "ScheduledTests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestItems_TestTemplates_TestTemplateId",
                table: "TestItems");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTests_ScheduledTests_ScheduledTestId",
                table: "UserTests");

            migrationBuilder.DropTable(
                name: "ScheduledTests");

            migrationBuilder.DropTable(
                name: "TestTemplates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTests",
                table: "UserTests");

            migrationBuilder.DropIndex(
                name: "IX_UserTests_ScheduledTestId",
                table: "UserTests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAnswers",
                table: "UserAnswers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TestItems",
                table: "TestItems");

            migrationBuilder.DropColumn(
                name: "ScheduledTestId",
                table: "UserTests");

            migrationBuilder.DropColumn(
                name: "ScheduledTestId",
                table: "UserAnswers");

            migrationBuilder.DropColumn(
                name: "TestTemplateId",
                table: "TestItems");

            migrationBuilder.AddColumn<int>(
                name: "TestId",
                table: "UserTests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TestId",
                table: "UserAnswers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TestId",
                table: "TestItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTests",
                table: "UserTests",
                columns: new[] { "UserId", "TestId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAnswers",
                table: "UserAnswers",
                columns: new[] { "TestId", "QuestionId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestItems",
                table: "TestItems",
                columns: new[] { "TestId", "QuestionId" });

            migrationBuilder.CreateTable(
                name: "Tests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserTests_TestId",
                table: "UserTests",
                column: "TestId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestItems_Tests_TestId",
                table: "TestItems",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTests_Tests_TestId",
                table: "UserTests",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
