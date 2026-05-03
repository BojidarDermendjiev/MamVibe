using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomVibe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorReviewIsApproved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "DoctorReviews",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Approve all existing reviews so they remain visible after adding moderation.
            migrationBuilder.Sql(@"UPDATE ""DoctorReviews"" SET ""IsApproved"" = TRUE;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "DoctorReviews");
        }
    }
}
