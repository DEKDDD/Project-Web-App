using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplicationASP.Migrations
{
    /// <inheritdoc />
    public partial class AddUserActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedActivityID",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "JoinedActivityID",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedActivityID",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "JoinedActivityID",
                table: "Users");
        }
    }
}
