using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomVibe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShoesAndFeedingCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use ON CONFLICT DO NOTHING so this is safe to run even if the
            // categories were already inserted by the legacy demo data seeder.
            migrationBuilder.Sql(@"
                INSERT INTO ""Categories"" (""Id"", ""CreatedAt"", ""Description"", ""Name"", ""Slug"", ""UpdatedAt"")
                VALUES
                    ('a7b8c9d0-e1f2-3456-0123-567890123456', '2024-01-01 00:00:00+00', 'Kids shoes and footwear',                      'Shoes',   'shoes',   NULL),
                    ('b8c9d0e1-f2a3-4567-1234-678901234567', '2024-01-01 00:00:00+00', 'Bottles, breast pumps, and feeding accessories', 'Feeding', 'feeding', NULL)
                ON CONFLICT DO NOTHING;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM ""Categories""
                WHERE ""Slug"" IN ('shoes', 'feeding');
            ");
        }
    }
}
