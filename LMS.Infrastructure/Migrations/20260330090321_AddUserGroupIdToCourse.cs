using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserGroupIdToCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserGroupId",
                table: "Courses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_UserGroupId",
                table: "Courses",
                column: "UserGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_UserGroups_UserGroupId",
                table: "Courses",
                column: "UserGroupId",
                principalTable: "UserGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_UserGroups_UserGroupId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_UserGroupId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "UserGroupId",
                table: "Courses");
        }
    }
}
