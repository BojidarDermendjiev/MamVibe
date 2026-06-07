using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomVibe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Items.BumpedAt — every browse query sorts by this expression; without an index
            // PostgreSQL must evaluate (BumpedAt IS NOT NULL AND BumpedAt > cutoff) for every row.
            migrationBuilder.CreateIndex(
                name: "IX_Items_BumpedAt",
                table: "Items",
                column: "BumpedAt");

            // Items.(IsActive, UserId) — "my listings" page filters active items by owner.
            // The existing IX_Items_UserId is single-column; this composite avoids a second
            // filter pass on IsActive after the index scan.
            migrationBuilder.CreateIndex(
                name: "IX_Items_IsActive_UserId",
                table: "Items",
                columns: new[] { "IsActive", "UserId" });

            // Offers composite indexes — the existing single-column indexes on ItemId/BuyerId/SellerId
            // don't cover the Status filter that every offer query adds.
            migrationBuilder.CreateIndex(
                name: "IX_Offers_ItemId_Status",
                table: "Offers",
                columns: new[] { "ItemId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Offers_BuyerId_Status",
                table: "Offers",
                columns: new[] { "BuyerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Offers_SellerId_Status",
                table: "Offers",
                columns: new[] { "SellerId", "Status" });

            // PurchaseRequests — as-buyer and as-seller list queries filter by (userId, status).
            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_BuyerId_Status",
                table: "PurchaseRequests",
                columns: new[] { "BuyerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_SellerId_Status",
                table: "PurchaseRequests",
                columns: new[] { "SellerId", "Status" });

            // Bundles browse — filters on IsActive and IsSold together.
            migrationBuilder.CreateIndex(
                name: "IX_Bundles_IsActive_IsSold",
                table: "Bundles",
                columns: new[] { "IsActive", "IsSold" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Items_BumpedAt", table: "Items");
            migrationBuilder.DropIndex(name: "IX_Items_IsActive_UserId", table: "Items");
            migrationBuilder.DropIndex(name: "IX_Offers_ItemId_Status", table: "Offers");
            migrationBuilder.DropIndex(name: "IX_Offers_BuyerId_Status", table: "Offers");
            migrationBuilder.DropIndex(name: "IX_Offers_SellerId_Status", table: "Offers");
            migrationBuilder.DropIndex(name: "IX_PurchaseRequests_BuyerId_Status", table: "PurchaseRequests");
            migrationBuilder.DropIndex(name: "IX_PurchaseRequests_SellerId_Status", table: "PurchaseRequests");
            migrationBuilder.DropIndex(name: "IX_Bundles_IsActive_IsSold", table: "Bundles");
        }
    }
}
