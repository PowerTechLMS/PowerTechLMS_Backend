using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations.PostgreSql
{
    /// <inheritdoc/>
    public partial class RenameInitialAiMessageToScenario : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        { migrationBuilder.RenameColumn(name: "InitialAiMessage", table: "RolePlayConfigs", newName: "Scenario"); }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        { migrationBuilder.RenameColumn(name: "Scenario", table: "RolePlayConfigs", newName: "InitialAiMessage"); }
    }
}
