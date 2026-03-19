using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Link = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5706), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5707) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5717), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5718) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5720), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5721) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5723), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5724) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5727), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5728) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5756), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5757) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5759), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5760) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5762), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5789) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5793), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5794) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5801), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5802) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5804), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5805) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5808), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5809) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5811), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5812) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5814), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5815) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5817), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5818) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5821), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5822) });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5824), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(5825) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 1, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6829), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6828), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6830) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 2, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6835), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6834), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6836) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 3, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6839), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6838), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6840) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 4, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6842), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6841), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6843) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 5, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6846), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6845), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6847) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 6, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6849), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6848), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6850) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 7, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6853), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6852), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6854) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 8, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6856), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6855), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6857) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 9, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6860), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6859), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6861) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 10, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6863), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6862), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6864) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 11, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6867), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6866), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6868) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 12, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6870), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6869), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6871) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 13, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6874), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6873), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6875) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 14, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(7072), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(7071), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(7073) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 15, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(7079), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(7078), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(7080) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 16, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(7083), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(7082), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(7084) });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 17, 1 },
                columns: new[] { "CreatedAt", "GrantedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(7086), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(7085), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(7087) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 1, 1 },
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6233), new DateTime(2026, 3, 18, 9, 15, 48, 381, DateTimeKind.Utc).AddTicks(6234) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$tD.4bSSWpN0hKzjmhxNGvu9/8lE/4e538qcshGwu9Ew8zgGtboDsO");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

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
    }
}
