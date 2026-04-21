using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc/>
    public partial class RenameContextToSupportLessonIds : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ContextLessonIds",
                table: "RolePlayConfigs",
                newName: "SupportLessonIds");
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SupportLessonIds",
                table: "RolePlayConfigs",
                newName: "ContextLessonIds");
        }
    }
}
