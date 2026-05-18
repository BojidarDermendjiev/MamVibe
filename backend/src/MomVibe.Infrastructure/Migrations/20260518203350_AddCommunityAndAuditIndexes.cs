using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomVibe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityAndAuditIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AdminId",
                table: "ItemModerationLogs",
                type: "character varying(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AuditLogs",
                type: "character varying(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "TargetId",
                table: "AuditLogs",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "AuditLogs",
                type: "character varying(45)",
                maxLength: 45,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                table: "AuditLogs",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "AuditLogs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Items_IsActive_CategoryId_CreatedAt",
                table: "Items",
                columns: new[] { "IsActive", "CategoryId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Items_IsActive_LikeCount",
                table: "Items",
                columns: new[] { "IsActive", "LikeCount" });

            migrationBuilder.CreateIndex(
                name: "IX_Items_IsActive_ListingType_CreatedAt",
                table: "Items",
                columns: new[] { "IsActive", "ListingType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Items_IsActive_ViewCount",
                table: "Items",
                columns: new[] { "IsActive", "ViewCount" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemModerationLogs_Action",
                table: "ItemModerationLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_ItemModerationLogs_AdminId",
                table: "ItemModerationLogs",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemModerationLogs_ItemId_CreatedAt",
                table: "ItemModerationLogs",
                columns: new[] { "ItemId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_CreatedAt",
                table: "Feedbacks",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorReviews_IsApproved_CreatedAt",
                table: "DoctorReviews",
                columns: new[] { "IsApproved", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ChildFriendlyPlaces_AgeToMonths",
                table: "ChildFriendlyPlaces",
                column: "AgeToMonths");

            migrationBuilder.CreateIndex(
                name: "IX_ChildFriendlyPlaces_IsApproved_CreatedAt",
                table: "ChildFriendlyPlaces",
                columns: new[] { "IsApproved", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action",
                table: "AuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Items_IsActive_CategoryId_CreatedAt",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_IsActive_LikeCount",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_IsActive_ListingType_CreatedAt",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_IsActive_ViewCount",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_ItemModerationLogs_Action",
                table: "ItemModerationLogs");

            migrationBuilder.DropIndex(
                name: "IX_ItemModerationLogs_AdminId",
                table: "ItemModerationLogs");

            migrationBuilder.DropIndex(
                name: "IX_ItemModerationLogs_ItemId_CreatedAt",
                table: "ItemModerationLogs");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_CreatedAt",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_DoctorReviews_IsApproved_CreatedAt",
                table: "DoctorReviews");

            migrationBuilder.DropIndex(
                name: "IX_ChildFriendlyPlaces_AgeToMonths",
                table: "ChildFriendlyPlaces");

            migrationBuilder.DropIndex(
                name: "IX_ChildFriendlyPlaces_IsApproved_CreatedAt",
                table: "ChildFriendlyPlaces");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_Action",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_UserId_CreatedAt",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<string>(
                name: "AdminId",
                table: "ItemModerationLogs",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AuditLogs",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "TargetId",
                table: "AuditLogs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "AuditLogs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(45)",
                oldMaxLength: 45,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                table: "AuditLogs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "AuditLogs",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }
    }
}
