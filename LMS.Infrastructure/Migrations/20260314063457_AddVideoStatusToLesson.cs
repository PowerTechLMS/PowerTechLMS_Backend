using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoStatusToLesson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VideoStatus",
                table: "Lessons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3676), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3677) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3695), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3696) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3698), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3698) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3699), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3700) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3701), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3702) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3713), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3713) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3715), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3715) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3716), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3729) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3730), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3730) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3739), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3739) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3740), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3740) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3742), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3742) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3743), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3743) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3745), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3745) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3746), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3747) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3751), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3751) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3753), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(3753) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 1, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4359), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4358), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4359) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 2, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4362), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4361), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4362) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 3, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4363), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4362), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4363) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 4, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4364), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4364), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4364) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 5, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4365), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4365), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4365) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 6, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4366), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4366), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4366) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 7, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4367), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4367), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4368) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 8, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4368), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4368), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4369) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 9, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4369), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4369), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4370) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 10, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4370), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4370), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4371) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 11, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4372), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4371), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4372) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 12, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4373), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4372), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4373) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 13, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4374), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4373), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4374) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 14, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4375), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4374), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4375) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 15, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4376), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4376), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4376) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 16, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4377), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4377), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4377) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 17, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4378), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4378), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4378) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 1, 1 },
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4033), new DateTime(2026, 3, 14, 6, 34, 56, 369, DateTimeKind.Utc).AddTicks(4033) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$q57j3CfMEGTcShPXqYEd5uDl45COUBnDvAiQvrbk4xuhfn/pqY/dG");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoStatus",
                table: "Lessons");

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
    }
}
