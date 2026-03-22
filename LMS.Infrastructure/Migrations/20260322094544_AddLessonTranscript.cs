using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc/>
    public partial class AddLessonTranscript : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Transcript",
                table: "Lessons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9878),
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9878)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9888),
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9888)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9890),
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9891)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9892),
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9892)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9894),
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9894)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9905),
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9906)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9907),
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9908)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9909),
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9928)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9930),
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9930)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9933),
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9933)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9935),
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9935)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9936),
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9937)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9938),
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9938)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9940),
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9940)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9941),
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9942)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9947),
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9947)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9949),
                    new DateTime(2026, 3, 22, 9, 45, 44, 216, DateTimeKind.Utc).AddTicks(9949)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(56),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(56)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(58),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(58)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 1, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(655),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(654),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(655)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 2, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(658),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(658),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(658)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 3, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(659),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(659),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(659)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 4, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(660),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(660),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(661)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 5, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(662),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(661),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(662)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 6, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(663),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(662),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(663)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 7, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(664),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(664),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(664)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 8, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(665),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(665),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(666)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 9, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(666),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(666),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(667)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 10, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(668),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(667),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(668)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 11, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(669),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(668),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(669)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 12, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(670),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(670),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(670)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 13, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(671),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(671),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(671)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 14, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(672),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(672),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(673)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 15, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(673),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(673),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(674)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 16, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(675),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(674),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(675)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 17, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(676),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(675),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(676)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 18, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(677),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(677),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(677)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 19, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(678),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(678),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(678)
                });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 1, 1 },
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(366),
                    new DateTime(2026, 3, 22, 9, 45, 44, 217, DateTimeKind.Utc).AddTicks(367)
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$ol651NsR1fPmK5qvft0Gn.aXIEznspABRHHdlJoYu3ADSDsIw6JjG");
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Transcript", table: "Lessons");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(6240),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(6241)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(6247),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(6248)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(6249),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(6249)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(6251),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(6251)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(6252),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(6253)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(6267),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(6267)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(6269),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(6269)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(6270),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(6294)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(6295),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(6295)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(7610),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(7616)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(7619),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(7619)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(7620),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(7621)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(7622),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(7623)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(7624),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(7624)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(7626),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(7626)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(7635),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(7635)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(7637),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(7637)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(7640),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(7640)
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(7641),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(7642)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 1, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8500),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8500),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8501)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 2, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8503),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8503),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8504)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 3, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8504),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8504),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8504)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 4, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8505),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8505),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8505)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 5, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8506),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8506),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8506)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 6, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8507),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8507),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8507)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 7, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8508),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8508),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8508)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 8, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8509),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8508),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8509)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 9, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8510),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8509),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8510)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 10, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8510),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8510),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8511)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 11, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8511),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8511),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8512)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 12, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8512),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8512),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8512)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 13, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8513),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8513),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8513)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 14, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8514),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8514),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8514)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 15, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8515),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8514),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8515)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 16, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8516),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8515),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8516)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 17, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8516),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8516),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8517)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 18, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8517),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8517),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8517)
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 19, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8627),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8627),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8627)
                });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 1, 1 },
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8204),
                    new DateTime(2026, 3, 22, 8, 46, 38, 754, DateTimeKind.Utc).AddTicks(8204)
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$eFFLu5A83.LUh/YDvb.GR.JRWfxNDMgatCaYthF297Rs//27dFIjy");
        }
    }
}
