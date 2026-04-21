using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations.PostgreSql
{
    /// <inheritdoc/>
    public partial class RenameMaxScoreToWeight : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        { migrationBuilder.RenameColumn(name: "MaxScore", table: "EssayQuestions", newName: "Weight"); }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        { migrationBuilder.RenameColumn(name: "Weight", table: "EssayQuestions", newName: "MaxScore"); }
    }
}
