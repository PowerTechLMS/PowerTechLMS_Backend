using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations.PostgreSql
{
    /// <inheritdoc/>
    public partial class AddVideoDraftScriptToLesson : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "VideoDraftScript", table: "Lessons", type: "text", nullable: true);
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        { migrationBuilder.DropColumn(name: "VideoDraftScript", table: "Lessons"); }
    }
}
