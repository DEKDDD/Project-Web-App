using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplicationASP.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "JoinedActivityID",
                table: "Users",
                newName: "JoinedActivityIds");

            migrationBuilder.RenameColumn(
                name: "CreatedActivityID",
                table: "Users",
                newName: "CreatedActivityIds");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "JoinedActivityIds",
                table: "Users",
                newName: "JoinedActivityID");

            migrationBuilder.RenameColumn(
                name: "CreatedActivityIds",
                table: "Users",
                newName: "CreatedActivityID");
        }
    }
}
