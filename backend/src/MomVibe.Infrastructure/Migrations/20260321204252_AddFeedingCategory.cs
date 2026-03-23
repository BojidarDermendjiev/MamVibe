using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomVibe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedingCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO ""Categories"" (""Id"", ""Name"", ""Slug"", ""Description"", ""CreatedAt"")
                SELECT gen_random_uuid(), 'Feeding', 'feeding', 'Bottles, breast pumps, and feeding accessories', NOW()
                WHERE NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Slug"" = 'feeding');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM ""Categories"" WHERE ""Slug"" = 'feeding';");
        }
    }
}
