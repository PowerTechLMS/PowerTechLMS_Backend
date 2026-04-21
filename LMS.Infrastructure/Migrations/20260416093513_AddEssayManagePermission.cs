using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc/>
    public partial class AddEssayManagePermission : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[]
                {
                    "Id",
                    "Category",
                    "Code",
                    "CreatedAt",
                    "DeletedAt",
                    "Description",
                    "IsDeleted",
                    "Name",
                    "UpdatedAt"
                },
                values: new object[]
                {
                    21,
                    "Essay",
                    "essay.manage",
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    null,
                    false,
                    "Quản lý Tự luận AI",
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[]
                {
                    "PermissionId",
                    "RoleId",
                    "CreatedAt",
                    "DeletedAt",
                    "GrantedAt",
                    "GrantedById",
                    "IsDeleted",
                    "UpdatedAt"
                },
                values: new object[]
                {
                    21,
                    1,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    false,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                });
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 21, 1 });

            migrationBuilder.DeleteData(table: "Permissions", keyColumn: "Id", keyValue: 21);
        }
    }
}
