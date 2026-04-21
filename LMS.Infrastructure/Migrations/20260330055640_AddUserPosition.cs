using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc/>
    public partial class AddUserPosition : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "Position", table: "Users", type: "nvarchar(max)", nullable: true);

            migrationBuilder.UpdateData(table: "Users", keyColumn: "Id", keyValue: 1, column: "Position", value: null);
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        { migrationBuilder.DropColumn(name: "Position", table: "Users"); }
    }
}
