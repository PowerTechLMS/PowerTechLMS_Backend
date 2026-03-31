using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LMS.Infrastructure.Migrations.PostgreSql
{
    /// <inheritdoc/>
    public partial class NewMigrationForRBACUpdate : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "Position", table: "Users", type: "text", nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Enrollments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(name: "UserGroupId", table: "Courses", type: "integer", nullable: true);

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
                values: new object[,]
                {
                {
                    1,
                    2,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    false,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                },
                {
                    2,
                    2,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    false,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                },
                {
                    3,
                    2,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    false,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                },
                {
                    6,
                    2,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    false,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                },
                {
                    9,
                    2,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    false,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                },
                {
                    10,
                    2,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    false,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                },
                {
                    12,
                    2,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    false,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                },
                {
                    13,
                    2,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    false,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                },
                {
                    14,
                    2,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    false,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                },
                {
                    18,
                    2,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    false,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                },
                {
                    1,
                    3,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    false,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                },
                {
                    9,
                    3,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    false,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                },
                {
                    18,
                    3,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    false,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                }
                });

            migrationBuilder.UpdateData(table: "Users", keyColumn: "Id", keyValue: 1, column: "Position", value: null);

            migrationBuilder.CreateIndex(name: "IX_Courses_UserGroupId", table: "Courses", column: "UserGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_UserGroups_UserGroupId",
                table: "Courses",
                column: "UserGroupId",
                principalTable: "UserGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Courses_UserGroups_UserGroupId", table: "Courses");

            migrationBuilder.DropIndex(name: "IX_Courses_UserGroupId", table: "Courses");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 2, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 3, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 6, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 9, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 10, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 12, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 13, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 14, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 18, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 1, 3 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 9, 3 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 18, 3 });

            migrationBuilder.DropColumn(name: "Position", table: "Users");

            migrationBuilder.DropColumn(name: "RejectionReason", table: "Enrollments");

            migrationBuilder.DropColumn(name: "UserGroupId", table: "Courses");
        }
    }
}
