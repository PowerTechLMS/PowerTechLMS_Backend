using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace LMS.Infrastructure.Migrations
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
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastOutdatedNotifiedAt",
                table: "Documents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OutdatedAt",
                table: "Documents",
                type: "datetime2",
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
