using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomVibe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserModerationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_IdempotencyKey",
                table: "Payments");

            // IsBlocked drop is deferred to AFTER the new moderation columns are added and
            // populated from it — see end of Up(). This preserves any pre-existing blocked
            // accounts under the new graded model (mapped to UserModerationLevel.Banned).

            migrationBuilder.AlterColumn<decimal>(
                name: "Weight",
                table: "Shipments",
                type: "numeric(10,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,3)",
                oldPrecision: 10,
                oldScale: 3,
                oldComment: "Package weight in kilograms.");

            migrationBuilder.AlterColumn<string>(
                name: "WaybillId",
                table: "Shipments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldComment: "Courier waybill identifier for API operations.");

            migrationBuilder.AlterColumn<string>(
                name: "TrackingNumber",
                table: "Shipments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldComment: "Courier tracking number for package lookup.");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Shipments",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Current shipment lifecycle status.");

            migrationBuilder.AlterColumn<decimal>(
                name: "ShippingPrice",
                table: "Shipments",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldComment: "Shipping price charged for this shipment.");

            migrationBuilder.AlterColumn<string>(
                name: "RecipientPhone",
                table: "Shipments",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldComment: "Phone number of the shipment recipient.");

            migrationBuilder.AlterColumn<string>(
                name: "RecipientName",
                table: "Shipments",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldComment: "Full name of the shipment recipient.");

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentId",
                table: "Shipments",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "Foreign key referencing the associated payment.");

            migrationBuilder.AlterColumn<string>(
                name: "OfficeName",
                table: "Shipments",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300,
                oldNullable: true,
                oldComment: "Courier office or locker display name.");

            migrationBuilder.AlterColumn<string>(
                name: "OfficeId",
                table: "Shipments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldComment: "Courier office or locker identifier.");

            migrationBuilder.AlterColumn<string>(
                name: "LabelUrl",
                table: "Shipments",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true,
                oldComment: "URL or path to the generated shipping label PDF.");

            migrationBuilder.AlterColumn<bool>(
                name: "IsInsured",
                table: "Shipments",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldComment: "Whether the shipment has additional insurance.");

            migrationBuilder.AlterColumn<bool>(
                name: "IsCod",
                table: "Shipments",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldComment: "Whether cash on delivery is enabled.");

            migrationBuilder.AlterColumn<decimal>(
                name: "InsuredAmount",
                table: "Shipments",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldComment: "Declared value for shipment insurance.");

            migrationBuilder.AlterColumn<int>(
                name: "DeliveryType",
                table: "Shipments",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Delivery type (Office, Address, Locker).");

            migrationBuilder.AlterColumn<string>(
                name: "DeliveryAddress",
                table: "Shipments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldComment: "Street address for address-based delivery.");

            migrationBuilder.AlterColumn<int>(
                name: "CourierProvider",
                table: "Shipments",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Courier provider used for this shipment (Econt, Speedy).");

            migrationBuilder.AlterColumn<decimal>(
                name: "CodAmount",
                table: "Shipments",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldComment: "Cash on delivery amount to collect from recipient.");

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Shipments",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true,
                oldComment: "City name for delivery destination.");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "RefreshTokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Identifier of the user to whom the token belongs (FK to ApplicationUser.Id).");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "RefreshTokens",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldComment: "Raw or hashed refresh token string.");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RevokedAt",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "UTC timestamp when the token was revoked; null if still valid.");

            migrationBuilder.AlterColumn<string>(
                name: "ReplacedByToken",
                table: "RefreshTokens",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldComment: "Token that replaced this one in a rotation flow; null if not replaced.");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiresAt",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "UTC timestamp when the token expires.");

            migrationBuilder.AlterColumn<string>(
                name: "StripeSessionId",
                table: "Payments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldComment: "Stripe checkout session identifier, if applicable.");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Payments",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Current payment status (e.g., Pending, Succeeded, Failed).");

            migrationBuilder.AlterColumn<string>(
                name: "SellerId",
                table: "Payments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Identifier of the selling user (FK to ApplicationUser.Id).");

            migrationBuilder.AlterColumn<string>(
                name: "ReceiptUrl",
                table: "Payments",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2048)",
                oldMaxLength: 2048,
                oldNullable: true,
                oldComment: "URL to the digital receipt from Take a NAP.");

            migrationBuilder.AlterColumn<int>(
                name: "PaymentMethod",
                table: "Payments",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Payment method (domain-specific enumeration).");

            migrationBuilder.AlterColumn<Guid>(
                name: "ItemId",
                table: "Payments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "Foreign key referencing the purchased item.");

            migrationBuilder.AlterColumn<string>(
                name: "IdempotencyKey",
                table: "Payments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true,
                oldComment: "Client-supplied idempotency key used to dedupe duplicate payment-creation requests.");

            migrationBuilder.AlterColumn<string>(
                name: "EBillNumber",
                table: "Payments",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true,
                oldComment: "Human-readable e-bill number assigned when a purchase is completed (e.g. MV-2026-A1B2C3D4).");

            migrationBuilder.AlterColumn<string>(
                name: "BuyerId",
                table: "Payments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Identifier of the buying user (FK to ApplicationUser.Id).");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Payments",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldComment: "Monetary amount for the payment.");

            migrationBuilder.AlterColumn<string>(
                name: "SenderId",
                table: "Messages",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Identifier of the sending user (FK to ApplicationUser.Id).");

            migrationBuilder.AlterColumn<string>(
                name: "ReceiverId",
                table: "Messages",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Identifier of the receiving user (FK to ApplicationUser.Id).");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRead",
                table: "Messages",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false,
                oldComment: "Indicates whether the message has been read by the receiver.");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Messages",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldComment: "Textual content of the message.");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Likes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Identifier of the user who liked the item (FK to ApplicationUser.Id).");

            migrationBuilder.AlterColumn<Guid>(
                name: "ItemId",
                table: "Likes",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "Foreign key referencing the liked item.");

            migrationBuilder.AlterColumn<int>(
                name: "ViewCount",
                table: "Items",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Total number of views for this item.");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Items",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Foreign key referencing the owning user's identifier.");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Items",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldComment: "Human-readable item title.");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Items",
                type: "numeric(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true,
                oldComment: "Item price in currency units; null if not applicable.");

            migrationBuilder.AlterColumn<int>(
                name: "ListingType",
                table: "Items",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Listing type (domain-specific enumeration).");

            migrationBuilder.AlterColumn<int>(
                name: "LikeCount",
                table: "Items",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Total number of likes for this item.");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Items",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true,
                oldComment: "Indicates whether the listing is active/visible.");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Items",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(5000)",
                oldMaxLength: 5000,
                oldComment: "Detailed description of the item.");

            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryId",
                table: "Items",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "Foreign key referencing the item's category.");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "ItemPhotos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldComment: "Absolute URL to the photo resource.");

            migrationBuilder.AlterColumn<Guid>(
                name: "ItemId",
                table: "ItemPhotos",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "Foreign key referencing the owning item.");

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "ItemPhotos",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Zero-based display order among the item's photos.");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Feedbacks",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Identifier of the user who submitted the feedback (FK to ApplicationUser.Id).");

            migrationBuilder.AlterColumn<int>(
                name: "Rating",
                table: "Feedbacks",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Feedback rating from 1 (lowest) to 5 (highest).");

            migrationBuilder.AlterColumn<bool>(
                name: "IsContactable",
                table: "Feedbacks",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldComment: "Whether the user consents to being contacted regarding this feedback.");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Feedbacks",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldComment: "Textual content of the feedback.");

            migrationBuilder.AlterColumn<int>(
                name: "Category",
                table: "Feedbacks",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Category/type of the feedback (e.g., bug, feature request, general).");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Categories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldComment: "URL-friendly unique identifier for the category.");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldComment: "Human-readable category name.");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Categories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldComment: "Optional description of the category.");

            migrationBuilder.AlterColumn<string>(
                name: "RevolutTag",
                table: "AspNetUsers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldComment: "Revolut username or tag for peer-to-peer payments via the Revolut app.");

            migrationBuilder.AlterColumn<int>(
                name: "ProfileType",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Type/category of the user's profile.");

            migrationBuilder.AlterColumn<string>(
                name: "LanguagePreference",
                table: "AspNetUsers",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "en",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValue: "en",
                oldComment: "Preferred language or locale (e.g., en or en-US).");

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "AspNetUsers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldComment: "Public display name shown to other users.");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW() AT TIME ZONE 'UTC'",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW() AT TIME ZONE 'UTC'",
                oldComment: "UTC timestamp when the user account was created.");

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "AspNetUsers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldComment: "User-provided short biography.");

            migrationBuilder.AlterColumn<string>(
                name: "AvatarUrl",
                table: "AspNetUsers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldComment: "Absolute URL to the user's avatar image.");

            migrationBuilder.AddColumn<Guid>(
                name: "ActiveModerationLogId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModerationExpiresAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModerationLevel",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ModerationPublicReason",
                table: "AspNetUsers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModerationReason",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModerationStartedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            // Backfill existing blocked users into the new graded model so prior moderation
            // is preserved. UserModerationLevel.Banned = 4; ModerationReason.Other = 13.
            migrationBuilder.Sql(@"
                UPDATE ""AspNetUsers""
                SET ""ModerationLevel""     = 4,
                    ""ModerationReason""    = 13,
                    ""ModerationStartedAt"" = NOW() AT TIME ZONE 'UTC'
                WHERE ""IsBlocked"" = TRUE;
            ");

            // IsBlocked column is dropped only after the backfill has rehomed its data into the
            // ModerationLevel column. The C# ApplicationUser.IsBlocked property is now a
            // [NotMapped] computed shim, so the column is no longer needed.
            migrationBuilder.DropColumn(
                name: "IsBlocked",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "AbuseReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReporterId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    TargetType = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    TargetUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ResolvedByAdminId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolutionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ResultingModerationLogId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbuseReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbuseSignals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    SubjectUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Details = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EvidenceTargetId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Acknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    AcknowledgedByAdminId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    AcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbuseSignals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModerationAppeals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ModerationLogId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserStatement = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AdminId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    AdminDecisionNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationAppeals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserModerationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    AdminId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    AdminDisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PreviousLevel = table.Column<int>(type: "integer", nullable: false),
                    NewLevel = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    PublicReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    InternalNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RelatedReportId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedAppealId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserModerationLogs", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "KnowledgeArticles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Content", "Tags", "Title" },
                values: new object[] { "Pay by card via Stripe checkout. Cash-on-delivery (COD) is available for Econt and Speedy shipments. Sellers list their Revolut Tag on their profile for peer-to-peer transfers. Sellers receive funds only after the buyer confirms receipt.", new[] { "payment", "stripe", "card", "cod", "revolut" }, "Payments on MamVibe" });

            migrationBuilder.UpdateData(
                table: "KnowledgeArticles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Content", "Tags", "Title" },
                values: new object[] { "Платете с карта чрез Stripe. Наложен платеж е наличен при Еконт и Спиди. Продавачите могат да посочат Revolut Tag в профила си за директни преводи. Продавачите получават пари след потвърждение от купувача.", new[] { "плащане", "карта", "наложен платеж", "revolut" }, "Плащания в MamVibe" });

            migrationBuilder.UpdateData(
                table: "KnowledgeArticles",
                keyColumn: "Id",
                keyValue: 7,
                column: "Content",
                value: "To buy an item: go to /browse and filter by category, age group, price, or listing type. Click an item and press Send Purchase Request. The seller has 48 hours to accept — no response means the request is auto-cancelled. Once accepted, complete payment by card via Stripe. The seller ships within 3 business days. Confirm receipt in Dashboard → Purchases; it auto-confirms after 5 days.");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_IdempotencyKey",
                table: "Payments",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ModerationLevel_ModerationExpiresAt",
                table: "AspNetUsers",
                columns: new[] { "ModerationLevel", "ModerationExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AbuseReports_Reporter_CreatedAt",
                table: "AbuseReports",
                columns: new[] { "ReporterId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_AbuseReports_Status_CreatedAt",
                table: "AbuseReports",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AbuseReports_TargetUser_CreatedAt",
                table: "AbuseReports",
                columns: new[] { "TargetUserId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "UX_AbuseReports_Reporter_Target_Pending",
                table: "AbuseReports",
                columns: new[] { "ReporterId", "TargetType", "TargetId" },
                unique: true,
                filter: "\"Status\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AbuseSignals_Acknowledged_CreatedAt",
                table: "AbuseSignals",
                columns: new[] { "Acknowledged", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AbuseSignals_Subject_CreatedAt",
                table: "AbuseSignals",
                columns: new[] { "SubjectUserId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_AbuseSignals_Type_CreatedAt",
                table: "AbuseSignals",
                columns: new[] { "Type", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ModerationAppeals_Status_CreatedAt",
                table: "ModerationAppeals",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ModerationAppeals_User_CreatedAt",
                table: "ModerationAppeals",
                columns: new[] { "UserId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "UX_ModerationAppeals_OpenPerEvent",
                table: "ModerationAppeals",
                column: "ModerationLogId",
                unique: true,
                filter: "\"Status\" IN (0, 1)");

            migrationBuilder.CreateIndex(
                name: "IX_UserModerationLogs_NewLevel",
                table: "UserModerationLogs",
                column: "NewLevel");

            migrationBuilder.CreateIndex(
                name: "IX_UserModerationLogs_RelatedReportId",
                table: "UserModerationLogs",
                column: "RelatedReportId");

            migrationBuilder.CreateIndex(
                name: "IX_UserModerationLogs_User_CreatedAt",
                table: "UserModerationLogs",
                columns: new[] { "UserId", "CreatedAt" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbuseReports");

            migrationBuilder.DropTable(
                name: "AbuseSignals");

            migrationBuilder.DropTable(
                name: "ModerationAppeals");

            migrationBuilder.DropTable(
                name: "UserModerationLogs");

            migrationBuilder.DropIndex(
                name: "IX_Payments_IdempotencyKey",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ModerationLevel_ModerationExpiresAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ActiveModerationLogId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ModerationExpiresAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ModerationLevel",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ModerationPublicReason",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ModerationReason",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ModerationStartedAt",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<decimal>(
                name: "Weight",
                table: "Shipments",
                type: "numeric(10,3)",
                precision: 10,
                scale: 3,
                nullable: false,
                comment: "Package weight in kilograms.",
                oldClrType: typeof(decimal),
                oldType: "numeric(10,3)");

            migrationBuilder.AlterColumn<string>(
                name: "WaybillId",
                table: "Shipments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                comment: "Courier waybill identifier for API operations.",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrackingNumber",
                table: "Shipments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                comment: "Courier tracking number for package lookup.",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Shipments",
                type: "integer",
                nullable: false,
                comment: "Current shipment lifecycle status.",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "ShippingPrice",
                table: "Shipments",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                comment: "Shipping price charged for this shipment.",
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "RecipientPhone",
                table: "Shipments",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                comment: "Phone number of the shipment recipient.",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "RecipientName",
                table: "Shipments",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                comment: "Full name of the shipment recipient.",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentId",
                table: "Shipments",
                type: "uuid",
                nullable: false,
                comment: "Foreign key referencing the associated payment.",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "OfficeName",
                table: "Shipments",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true,
                comment: "Courier office or locker display name.",
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OfficeId",
                table: "Shipments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Courier office or locker identifier.",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LabelUrl",
                table: "Shipments",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                comment: "URL or path to the generated shipping label PDF.",
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsInsured",
                table: "Shipments",
                type: "boolean",
                nullable: false,
                comment: "Whether the shipment has additional insurance.",
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsCod",
                table: "Shipments",
                type: "boolean",
                nullable: false,
                comment: "Whether cash on delivery is enabled.",
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<decimal>(
                name: "InsuredAmount",
                table: "Shipments",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                comment: "Declared value for shipment insurance.",
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "DeliveryType",
                table: "Shipments",
                type: "integer",
                nullable: false,
                comment: "Delivery type (Office, Address, Locker).",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "DeliveryAddress",
                table: "Shipments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "Street address for address-based delivery.",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CourierProvider",
                table: "Shipments",
                type: "integer",
                nullable: false,
                comment: "Courier provider used for this shipment (Econt, Speedy).",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "CodAmount",
                table: "Shipments",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                comment: "Cash on delivery amount to collect from recipient.",
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Shipments",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                comment: "City name for delivery destination.",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "RefreshTokens",
                type: "text",
                nullable: false,
                comment: "Identifier of the user to whom the token belongs (FK to ApplicationUser.Id).",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "RefreshTokens",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                comment: "Raw or hashed refresh token string.",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<DateTime>(
                name: "RevokedAt",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: true,
                comment: "UTC timestamp when the token was revoked; null if still valid.",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReplacedByToken",
                table: "RefreshTokens",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "Token that replaced this one in a rotation flow; null if not replaced.",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiresAt",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: false,
                comment: "UTC timestamp when the token expires.",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "StripeSessionId",
                table: "Payments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "Stripe checkout session identifier, if applicable.",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Payments",
                type: "integer",
                nullable: false,
                comment: "Current payment status (e.g., Pending, Succeeded, Failed).",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "SellerId",
                table: "Payments",
                type: "text",
                nullable: false,
                comment: "Identifier of the selling user (FK to ApplicationUser.Id).",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ReceiptUrl",
                table: "Payments",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true,
                comment: "URL to the digital receipt from Take a NAP.",
                oldClrType: typeof(string),
                oldType: "character varying(2048)",
                oldMaxLength: 2048,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PaymentMethod",
                table: "Payments",
                type: "integer",
                nullable: false,
                comment: "Payment method (domain-specific enumeration).",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "ItemId",
                table: "Payments",
                type: "uuid",
                nullable: true,
                comment: "Foreign key referencing the purchased item.",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IdempotencyKey",
                table: "Payments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                comment: "Client-supplied idempotency key used to dedupe duplicate payment-creation requests.",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EBillNumber",
                table: "Payments",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true,
                comment: "Human-readable e-bill number assigned when a purchase is completed (e.g. MV-2026-A1B2C3D4).",
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BuyerId",
                table: "Payments",
                type: "text",
                nullable: false,
                comment: "Identifier of the buying user (FK to ApplicationUser.Id).",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Payments",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                comment: "Monetary amount for the payment.",
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "SenderId",
                table: "Messages",
                type: "text",
                nullable: false,
                comment: "Identifier of the sending user (FK to ApplicationUser.Id).",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ReceiverId",
                table: "Messages",
                type: "text",
                nullable: false,
                comment: "Identifier of the receiving user (FK to ApplicationUser.Id).",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRead",
                table: "Messages",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Indicates whether the message has been read by the receiver.",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Messages",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                comment: "Textual content of the message.",
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Likes",
                type: "text",
                nullable: false,
                comment: "Identifier of the user who liked the item (FK to ApplicationUser.Id).",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Guid>(
                name: "ItemId",
                table: "Likes",
                type: "uuid",
                nullable: false,
                comment: "Foreign key referencing the liked item.",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "ViewCount",
                table: "Items",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Total number of views for this item.",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Items",
                type: "text",
                nullable: false,
                comment: "Foreign key referencing the owning user's identifier.",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Items",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                comment: "Human-readable item title.",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Items",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                comment: "Item price in currency units; null if not applicable.",
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ListingType",
                table: "Items",
                type: "integer",
                nullable: false,
                comment: "Listing type (domain-specific enumeration).",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "LikeCount",
                table: "Items",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Total number of likes for this item.",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Items",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                comment: "Indicates whether the listing is active/visible.",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Items",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: false,
                comment: "Detailed description of the item.",
                oldClrType: typeof(string),
                oldType: "character varying(5000)",
                oldMaxLength: 5000);

            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryId",
                table: "Items",
                type: "uuid",
                nullable: false,
                comment: "Foreign key referencing the item's category.",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "ItemPhotos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                comment: "Absolute URL to the photo resource.",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<Guid>(
                name: "ItemId",
                table: "ItemPhotos",
                type: "uuid",
                nullable: false,
                comment: "Foreign key referencing the owning item.",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "ItemPhotos",
                type: "integer",
                nullable: false,
                comment: "Zero-based display order among the item's photos.",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Feedbacks",
                type: "text",
                nullable: false,
                comment: "Identifier of the user who submitted the feedback (FK to ApplicationUser.Id).",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Rating",
                table: "Feedbacks",
                type: "integer",
                nullable: false,
                comment: "Feedback rating from 1 (lowest) to 5 (highest).",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<bool>(
                name: "IsContactable",
                table: "Feedbacks",
                type: "boolean",
                nullable: false,
                comment: "Whether the user consents to being contacted regarding this feedback.",
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Feedbacks",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                comment: "Textual content of the feedback.",
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<int>(
                name: "Category",
                table: "Feedbacks",
                type: "integer",
                nullable: false,
                comment: "Category/type of the feedback (e.g., bug, feature request, general).",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Categories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                comment: "URL-friendly unique identifier for the category.",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                comment: "Human-readable category name.",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Categories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "Optional description of the category.",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RevolutTag",
                table: "AspNetUsers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Revolut username or tag for peer-to-peer payments via the Revolut app.",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProfileType",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                comment: "Type/category of the user's profile.",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "LanguagePreference",
                table: "AspNetUsers",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "en",
                comment: "Preferred language or locale (e.g., en or en-US).",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValue: "en");

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "AspNetUsers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                comment: "Public display name shown to other users.",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW() AT TIME ZONE 'UTC'",
                comment: "UTC timestamp when the user account was created.",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW() AT TIME ZONE 'UTC'");

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "AspNetUsers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "User-provided short biography.",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AvatarUrl",
                table: "AspNetUsers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "Absolute URL to the user's avatar image.",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBlocked",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Indicates whether the account is blocked from interacting.");

            migrationBuilder.UpdateData(
                table: "KnowledgeArticles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Content", "Tags", "Title" },
                values: new object[] { "Pay with a MamVibe Wallet balance or directly by card via Stripe checkout. Top up the wallet from Settings → Wallet (minimum 5 BGN). Wallet balance never expires. Sellers receive funds only after the buyer confirms receipt. Withdraw earnings from Settings → Wallet → Withdraw (IBAN required, processed in 2 business days). Cash-on-delivery is available for Econt and Speedy.", new[] { "payment", "wallet", "stripe", "card", "cod", "withdraw" }, "Payments & MamVibe Wallet" });

            migrationBuilder.UpdateData(
                table: "KnowledgeArticles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Content", "Tags", "Title" },
                values: new object[] { "Платете с баланс в MamVibe Портфейл или директно с карта чрез Stripe. Портфейлът се зарежда от Настройки → Портфейл (минимум 5 лв.). Балансът не изтича. Продавачите получават пари след потвърждение от купувача. Тегленето е от Настройки → Портфейл → Теглене (необходим IBAN, обработен в 2 работни дни). Наложен платеж е наличен при Еконт и Спиди.", new[] { "плащане", "портфейл", "карта", "наложен платеж", "теглене" }, "Плащания и Портфейл в MamVibe" });

            migrationBuilder.UpdateData(
                table: "KnowledgeArticles",
                keyColumn: "Id",
                keyValue: 7,
                column: "Content",
                value: "To buy an item: go to /browse and filter by category, age group, price, or listing type. Click an item and press Send Purchase Request. The seller has 48 hours to accept — no response means the request is auto-cancelled. Once accepted, complete payment via Wallet or card. The seller ships within 3 business days. Confirm receipt in Dashboard → Purchases; it auto-confirms after 5 days.");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_IdempotencyKey",
                table: "Payments",
                column: "IdempotencyKey",
                unique: true);
        }
    }
}
