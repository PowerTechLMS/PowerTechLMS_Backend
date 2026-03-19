using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionExplanation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Explanation",
                table: "QuestionBanks",
                type: "nvarchar(max)",
                nullable: true);



            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(5813), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(5813) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(5818), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(5819) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(5821), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(5821) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6033), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6033) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6035), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6035) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6060), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6060) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6062), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6062) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6063), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6079) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6080), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6080) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6082), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6083) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6085), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6085) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6086), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6086) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6087), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6088) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6089), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6089) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6091), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6091) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6092), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6092) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6093), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6094) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 1, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6897), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6896), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6897) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 2, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6899), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6899), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6899) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 3, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6900), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6900), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6901) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 4, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6901), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6901), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6902) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 5, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6902), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6902), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6903) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 6, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6903), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6903), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6904) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 7, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6904), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6904), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6905) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 8, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6905), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6905), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6906) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 9, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6906), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6906), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6907) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 10, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6907), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6907), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6908) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 11, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6908), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6908), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6909) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 12, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6909), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6909), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6910) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 13, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6911), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6910), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6911) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 14, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6912), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6911), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6912) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 15, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6913), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6912), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6913) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 16, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6914), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6913), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6914) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 17, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6915), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6914), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6915) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 1, 1 },
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6530), new DateTime(2026, 3, 18, 8, 21, 13, 970, DateTimeKind.Utc).AddTicks(6531) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$wapKDVUHQ4ppCcHvK2/0q.7lM2t6Z5XE1ELaYhIMU84j559sMHuju");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Explanation",
                table: "QuestionBanks");



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
    }
}
