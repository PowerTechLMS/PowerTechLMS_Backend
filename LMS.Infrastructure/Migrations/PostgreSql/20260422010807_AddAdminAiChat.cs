using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LMS.Infrastructure.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddAdminAiChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SessionId",
                table: "AiTasks",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AdminAiSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ThreadId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedById = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminAiSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminAiSessions_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdminAiMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SessionId = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    ToolCallsJson = table.Column<string>(type: "text", nullable: true),
                    PlanJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminAiMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminAiMessages_AdminAiSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "AdminAiSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiTasks_SessionId",
                table: "AiTasks",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminAiMessages_SessionId",
                table: "AdminAiMessages",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminAiSessions_CreatedById",
                table: "AdminAiSessions",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_AiTasks_AdminAiSessions_SessionId",
                table: "AiTasks",
                column: "SessionId",
                principalTable: "AdminAiSessions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiTasks_AdminAiSessions_SessionId",
                table: "AiTasks");

            migrationBuilder.DropTable(
                name: "AdminAiMessages");

            migrationBuilder.DropTable(
                name: "AdminAiSessions");

            migrationBuilder.DropIndex(
                name: "IX_AiTasks_SessionId",
                table: "AiTasks");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "AiTasks");
        }
    }
}
