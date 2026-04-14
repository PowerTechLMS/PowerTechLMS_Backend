using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixErrorGenerateMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RolePlayConfigs_LessonId",
                table: "RolePlayConfigs");

            migrationBuilder.CreateIndex(
                name: "IX_RolePlayConfigs_LessonId",
                table: "RolePlayConfigs",
                column: "LessonId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RolePlayConfigs_LessonId",
                table: "RolePlayConfigs");

            migrationBuilder.CreateIndex(
                name: "IX_RolePlayConfigs_LessonId",
                table: "RolePlayConfigs",
                column: "LessonId");
        }
    }
}
