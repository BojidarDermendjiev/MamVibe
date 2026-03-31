using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomVibe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiModerationToItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiModerationNotes",
                table: "Items",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "AiModerationScore",
                table: "Items",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AiModerationStatus",
                table: "Items",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiModerationNotes",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "AiModerationScore",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "AiModerationStatus",
                table: "Items");
        }
    }
}
