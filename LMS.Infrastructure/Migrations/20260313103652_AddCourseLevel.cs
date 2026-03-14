using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9613), new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9613) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9617), new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9618) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9619), new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9619) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9621), new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9621) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9622), new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9622) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9635), new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9635) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9636), new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9636) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9637), new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9700) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9701), new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9702) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9711), new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9712) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9713), new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9713) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9714), new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9714) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9715), new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9716) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9717), new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9717) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9718), new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9718) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9723), new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9724) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9725), new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9725) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 1, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(136), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(136), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(137) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 2, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(138), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(138), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(139) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 3, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(139), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(139), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(140) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 4, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(140), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(140), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(141) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 5, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(141), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(141), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(141) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 6, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(142), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(142), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(142) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 7, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(143), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(143), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(143) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 8, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(144), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(143), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(144) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 9, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(145), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(144), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(145) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 10, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(145), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(145), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(146) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 11, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(146), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(146), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(146) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 12, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(147), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(147), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(147) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 13, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(148), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(147), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(148) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 14, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(148), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(148), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(149) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 15, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(149), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(149), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(149) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 16, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(150), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(150), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(150) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 17, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(151), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(151), new DateTime(2026, 3, 13, 10, 36, 52, 131, DateTimeKind.Utc).AddTicks(151) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 1, 1 },
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9917), new DateTime(2026, 3, 13, 10, 36, 52, 130, DateTimeKind.Utc).AddTicks(9917) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$s4IbYc20Yme1dyeXwkzw7uCMQ3pY1GRcebDqUj4iG1P98Sxcy5o/O");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "Courses");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4351), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4352) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4355), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4356) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4357), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4358) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4359), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4359) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4360), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4360) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4371), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4371) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4372), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4373) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4374), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4374) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4375), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4375) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4377), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4377) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4378), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4378) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4379), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4379) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4380), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4381) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4382), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4382) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4383), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4383) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4384), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4384) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4385), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4385) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 1, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4624), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4623), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4624) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 2, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4687), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4687), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4687) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 3, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4688), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4688), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4688) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 4, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4689), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4689), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4689) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 5, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4690), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4690), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4690) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 6, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4691), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4691), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4691) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 7, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4692), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4692), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4692) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 8, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4693), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4692), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4693) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 9, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4693), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4693), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4694) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 10, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4694), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4694), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4694) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 11, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4695), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4695), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4695) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 12, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4696), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4695), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4696) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 13, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4696), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4696), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4696) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 14, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4697), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4697), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4697) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 15, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4698), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4697), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4698) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 16, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4698), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4698), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4699) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 17, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4699), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4699), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4699) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 1, 1 },
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4465), new DateTime(2026, 3, 13, 1, 36, 25, 352, DateTimeKind.Utc).AddTicks(4465) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$f66HTvHb7ZN5Vnzf8nK.DOjcA88Q77sUeQU5/8l02bRfESOKqpskK");
        }
    }
}
