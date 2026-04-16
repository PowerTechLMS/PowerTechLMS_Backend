using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEssayFeatureV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EssayAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalScore = table.Column<int>(type: "int", nullable: true),
                    IsPassed = table.Column<bool>(type: "bit", nullable: false),
                    AiFeedback = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EssayAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EssayAttempts_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EssayAttempts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EssayConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    SupportLessonIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeLimitMinutes = table.Column<int>(type: "int", nullable: true),
                    MaxAttemptsPerWindow = table.Column<int>(type: "int", nullable: true),
                    AttemptWindowHours = table.Column<int>(type: "int", nullable: true),
                    PassScore = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EssayConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EssayConfigs_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EssayQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EssayConfigId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    MaxScore = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EssayQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EssayQuestions_EssayConfigs_EssayConfigId",
                        column: x => x.EssayConfigId,
                        principalTable: "EssayConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EssayAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttemptId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AiScore = table.Column<int>(type: "int", nullable: true),
                    AiFeedback = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EssayQuestionId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EssayAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EssayAnswers_EssayAttempts_AttemptId",
                        column: x => x.AttemptId,
                        principalTable: "EssayAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EssayAnswers_EssayQuestions_EssayQuestionId",
                        column: x => x.EssayQuestionId,
                        principalTable: "EssayQuestions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EssayAnswers_EssayQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "EssayQuestions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EssayAnswers_AttemptId",
                table: "EssayAnswers",
                column: "AttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_EssayAnswers_EssayQuestionId",
                table: "EssayAnswers",
                column: "EssayQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_EssayAnswers_QuestionId",
                table: "EssayAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_EssayAttempts_LessonId",
                table: "EssayAttempts",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_EssayAttempts_UserId",
                table: "EssayAttempts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EssayConfigs_LessonId",
                table: "EssayConfigs",
                column: "LessonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EssayQuestions_EssayConfigId",
                table: "EssayQuestions",
                column: "EssayConfigId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EssayAnswers");

            migrationBuilder.DropTable(
                name: "EssayAttempts");

            migrationBuilder.DropTable(
                name: "EssayQuestions");

            migrationBuilder.DropTable(
                name: "EssayConfigs");
        }
    }
}
