using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomVibe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCarSeatsAndStrollersCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO ""Categories"" (""Id"", ""Name"", ""Slug"", ""Description"", ""CreatedAt"")
                SELECT gen_random_uuid(), 'Car Seats', 'car-seats', 'Child car seats and boosters', NOW()
                WHERE NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Slug"" = 'car-seats');

                INSERT INTO ""Categories"" (""Id"", ""Name"", ""Slug"", ""Description"", ""CreatedAt"")
                SELECT gen_random_uuid(), 'Strollers', 'strollers', 'Baby strollers and prams', NOW()
                WHERE NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Slug"" = 'strollers');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM ""Categories"" WHERE ""Slug"" IN ('car-seats', 'strollers');
            ");
        }
    }
}
