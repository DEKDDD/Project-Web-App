using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplicationASP.Migrations
{
    /// <inheritdoc />
    public partial class UpdateActivityModel2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Member",
                table: "Activities",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Member",
                table: "Activities");
        }
    }
}
