using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

#nullable disable

namespace LMS.Infrastructure.Migrations.PostgreSql
{
    /// <inheritdoc/>
    public partial class AddAiTasks : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JobId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Topic = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Progress = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ResultJson = table.Column<string>(type: "text", nullable: true),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsFailed = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    CreatedById = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiTasks_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_AiTasks_CreatedById", table: "AiTasks", column: "CreatedById");
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder) { migrationBuilder.DropTable(name: "AiTasks"); }
    }
}
