using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomVibe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWalletAddRevolutAndCommunityFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop wallet tables
            migrationBuilder.DropTable(name: "WalletTransfers");
            migrationBuilder.DropTable(name: "WalletTransactions");
            migrationBuilder.DropTable(name: "Wallets");

            // Remove IBAN column (was only used for wallet withdrawals)
            migrationBuilder.DropColumn(table: "AspNetUsers", name: "Iban");

            // Add RevolutTag
            migrationBuilder.AddColumn<string>(
                name: "RevolutTag",
                table: "AspNetUsers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Revolut username or tag for peer-to-peer payments via the Revolut app.");

            // Create DoctorReviews table
            migrationBuilder.CreateTable(
                name: "DoctorReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    DoctorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Specialization = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ClinicName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SuperdocUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    IsAnonymous = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorReviews_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_DoctorReviews_City", table: "DoctorReviews", column: "City");
            migrationBuilder.CreateIndex(name: "IX_DoctorReviews_Specialization", table: "DoctorReviews", column: "Specialization");
            migrationBuilder.CreateIndex(name: "IX_DoctorReviews_UserId", table: "DoctorReviews", column: "UserId");

            // Create ChildFriendlyPlaces table
            migrationBuilder.CreateTable(
                name: "ChildFriendlyPlaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PlaceType = table.Column<int>(type: "integer", nullable: false),
                    AgeFromMonths = table.Column<int>(type: "integer", nullable: true),
                    AgeToMonths = table.Column<int>(type: "integer", nullable: true),
                    PhotoUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Website = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChildFriendlyPlaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChildFriendlyPlaces_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_ChildFriendlyPlaces_City", table: "ChildFriendlyPlaces", column: "City");
            migrationBuilder.CreateIndex(name: "IX_ChildFriendlyPlaces_PlaceType", table: "ChildFriendlyPlaces", column: "PlaceType");
            migrationBuilder.CreateIndex(name: "IX_ChildFriendlyPlaces_IsApproved", table: "ChildFriendlyPlaces", column: "IsApproved");
            migrationBuilder.CreateIndex(name: "IX_ChildFriendlyPlaces_UserId", table: "ChildFriendlyPlaces", column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DoctorReviews");
            migrationBuilder.DropTable(name: "ChildFriendlyPlaces");
            migrationBuilder.DropColumn(table: "AspNetUsers", name: "RevolutTag");
            migrationBuilder.AddColumn<string>(
                name: "Iban",
                table: "AspNetUsers",
                type: "character varying(34)",
                maxLength: 34,
                nullable: true);
            // Note: wallet tables not restored in Down() — data loss intentional
        }
    }
}
