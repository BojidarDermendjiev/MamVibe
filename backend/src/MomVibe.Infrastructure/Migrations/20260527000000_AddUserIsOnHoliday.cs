using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomVibe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIsOnHoliday : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOnHoliday",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOnHoliday",
                table: "AspNetUsers");
        }
    }
}
