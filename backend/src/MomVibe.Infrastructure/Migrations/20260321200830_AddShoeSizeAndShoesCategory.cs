using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomVibe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShoeSizeAndShoesCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShoeSize",
                table: "Items",
                type: "integer",
                nullable: true);

            // Insert the Shoes category if it doesn't already exist
            migrationBuilder.Sql(@"
                INSERT INTO ""Categories"" (""Id"", ""Name"", ""Slug"", ""Description"", ""CreatedAt"")
                SELECT gen_random_uuid(), 'Shoes', 'shoes', 'Kids shoes and footwear', NOW()
                WHERE NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Slug"" = 'shoes');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShoeSize",
                table: "Items");

            migrationBuilder.Sql(@"DELETE FROM ""Categories"" WHERE ""Slug"" = 'shoes';");
        }
    }
}
