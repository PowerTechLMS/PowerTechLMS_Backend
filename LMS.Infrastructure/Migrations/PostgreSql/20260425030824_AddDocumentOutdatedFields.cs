using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace LMS.Infrastructure.Migrations.PostgreSql
{
    /// <inheritdoc/>
    public partial class AddDocumentOutdatedFields : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOutdated",
                table: "Documents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastOutdatedNotifiedAt",
                table: "Documents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OutdatedAt",
                table: "Documents",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsOutdated", table: "Documents");

            migrationBuilder.DropColumn(name: "LastOutdatedNotifiedAt", table: "Documents");

            migrationBuilder.DropColumn(name: "OutdatedAt", table: "Documents");
        }
    }
}
