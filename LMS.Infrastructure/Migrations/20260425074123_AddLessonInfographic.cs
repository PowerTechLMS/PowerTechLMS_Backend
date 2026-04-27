using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc/>
    public partial class AddLessonInfographic : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LessonInfographics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonInfographics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonInfographics_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LessonInfographicMapping",
                columns: table => new
                {
                    InfographicId = table.Column<int>(type: "int", nullable: false),
                    LessonId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonInfographicMapping", x => new { x.InfographicId, x.LessonId });
                    table.ForeignKey(
                        name: "FK_LessonInfographicMapping_LessonInfographics_InfographicId",
                        column: x => x.InfographicId,
                        principalTable: "LessonInfographics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonInfographicMapping_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonInfographicMapping_LessonId",
                table: "LessonInfographicMapping",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonInfographics_CreatedById",
                table: "LessonInfographics",
                column: "CreatedById");
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "LessonInfographicMapping");

            migrationBuilder.DropTable(name: "LessonInfographics");
        }
    }
}
