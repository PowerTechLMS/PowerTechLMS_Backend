using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

#nullable disable

namespace LMS.Infrastructure.Migrations.PostgreSql
{
    /// <inheritdoc/>
    public partial class AddRolePlayEntities : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RolePlayConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LessonId = table.Column<int>(type: "integer", nullable: false),
                    ContextLessonIds = table.Column<string>(type: "text", nullable: true),
                    ScoringCriteria = table.Column<string>(type: "text", nullable: false),
                    AdditionalRequirements = table.Column<string>(type: "text", nullable: true),
                    InitialAiMessage = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePlayConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePlayConfigs_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePlaySessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    LessonId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: true),
                    Feedback = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePlaySessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePlaySessions_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePlaySessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePlayMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SessionId = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePlayMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePlayMessages_RolePlaySessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "RolePlaySessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    20,
                    "RolePlay",
                    "roleplay.manage",
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    null,
                    false,
                    "Quản lý Role Play",
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
                    20,
                    1,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    false,
                    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                });

            migrationBuilder.CreateIndex(
                name: "IX_RolePlayConfigs_LessonId",
                table: "RolePlayConfigs",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePlayMessages_SessionId",
                table: "RolePlayMessages",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePlaySessions_LessonId",
                table: "RolePlaySessions",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePlaySessions_UserId",
                table: "RolePlaySessions",
                column: "UserId");
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "RolePlayConfigs");

            migrationBuilder.DropTable(name: "RolePlayMessages");

            migrationBuilder.DropTable(name: "RolePlaySessions");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 20, 1 });

            migrationBuilder.DeleteData(table: "Permissions", keyColumn: "Id", keyValue: 20);
        }
    }
}
