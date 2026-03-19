using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizRetakeColumnsToCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxRetakesPerDay",
                table: "Quizzes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetakeWaitTimeMinutes",
                table: "Quizzes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuizMaxRetakesPerDay",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuizRetakeWaitTimeMinutes",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6070), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6071) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6077), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6077) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6079), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6079) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6080), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6080) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6082), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6082) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6100), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6101) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6102), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6102) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6103), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6132) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6134), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6134) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6145), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6147) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6149), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6149) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6150), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6150) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6152), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6152) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6153), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6153) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6154), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6155) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6161), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6161) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6162), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6162) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 1, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6566), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6565), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6566) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 2, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6568), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6568), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6568) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 3, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6569), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6569), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6569) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 4, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6570), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6570), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6570) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 5, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6571), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6571), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6571) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 6, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6572), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6571), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6572) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 7, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6573), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6572), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6573) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 8, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6574), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6573), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6574) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 9, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6574), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6574), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6575) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 10, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6575), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6575), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6575) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 11, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6576), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6576), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6576) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 12, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6711), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6710), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6711) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 13, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6712), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6712), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6713) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 14, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6713), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6713), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6714) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 15, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6714), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6714), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6715) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 16, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6715), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6715), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6715) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 17, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6716), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6716), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6716) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 1, 1 },
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6369), new DateTime(2026, 3, 18, 4, 44, 40, 619, DateTimeKind.Utc).AddTicks(6369) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$sK8euPbRGZPJbYeKPbfj3e74vsUx.GL3BC6Xc61mDHAw5MUDi.uZa");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxRetakesPerDay",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "RetakeWaitTimeMinutes",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "QuizMaxRetakesPerDay",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "QuizRetakeWaitTimeMinutes",
                table: "Courses");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4132), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4133) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4180), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4192) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4196), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4197) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4200), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4202) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4318), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4319) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4339), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4340) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4343), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4344) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4347), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4347) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4350), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4351) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4357), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4357) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4360), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4361) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4363), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4364) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4367), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4368) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4371), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4372) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4375), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4375) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4378), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4379) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4381), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4382) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 1, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4995), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4993), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4995) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 2, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4999), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4998), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4999) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 3, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5001), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5000), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5001) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 4, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5003), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5002), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5003) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 5, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5005), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5004), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5006) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 6, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5007), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5006), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5008) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 7, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5009), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5008), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5010) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 8, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5011), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5011), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5012) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 9, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5013), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5013), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5014) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 10, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5015), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5015), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5016) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 11, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5017), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5017), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5018) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 12, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5019), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5019), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5020) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 13, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5021), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5020), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5022) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 14, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5023), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5022), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5024) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 15, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5025), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5024), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5026) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 16, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5027), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5026), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5028) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 17, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5029), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5028), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(5030) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 1, 1 },
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4609), new DateTime(2026, 3, 16, 2, 51, 16, 775, DateTimeKind.Utc).AddTicks(4609) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$gZ9Zo0JwV0sVH7oWzQkBJ.6522nMWjjs/unVzaY6eEmp1snMMPqku");
        }
    }
}
