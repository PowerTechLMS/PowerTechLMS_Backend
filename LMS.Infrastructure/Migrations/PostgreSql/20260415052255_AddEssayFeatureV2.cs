using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddEssayFeatureV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EssayAnswers_EssayQuestions_QuestionId",
                table: "EssayAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_EssayAttempts_Users_UserId",
                table: "EssayAttempts");

            migrationBuilder.AddColumn<int>(
                name: "EssayQuestionId",
                table: "EssayAnswers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EssayAnswers_EssayQuestionId",
                table: "EssayAnswers",
                column: "EssayQuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_EssayAnswers_EssayQuestions_EssayQuestionId",
                table: "EssayAnswers",
                column: "EssayQuestionId",
                principalTable: "EssayQuestions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EssayAnswers_EssayQuestions_QuestionId",
                table: "EssayAnswers",
                column: "QuestionId",
                principalTable: "EssayQuestions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EssayAttempts_Users_UserId",
                table: "EssayAttempts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EssayAnswers_EssayQuestions_EssayQuestionId",
                table: "EssayAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_EssayAnswers_EssayQuestions_QuestionId",
                table: "EssayAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_EssayAttempts_Users_UserId",
                table: "EssayAttempts");

            migrationBuilder.DropIndex(
                name: "IX_EssayAnswers_EssayQuestionId",
                table: "EssayAnswers");

            migrationBuilder.DropColumn(
                name: "EssayQuestionId",
                table: "EssayAnswers");

            migrationBuilder.AddForeignKey(
                name: "FK_EssayAnswers_EssayQuestions_QuestionId",
                table: "EssayAnswers",
                column: "QuestionId",
                principalTable: "EssayQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EssayAttempts_Users_UserId",
                table: "EssayAttempts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
