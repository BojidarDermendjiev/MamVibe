using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomVibe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessVertical_Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessPolicyVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BodyMarkdown = table.Column<string>(type: "text", maxLength: 32000, nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessPolicyVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BusinessProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ProfileKind = table.Column<int>(type: "integer", nullable: false),
                    LegalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Bio = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    ContactPhone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Website = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StripeCustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DeviceFingerprintHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IpAtRegistration = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgentAtRegistration = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PolicyVersionAcceptedId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeviceCheckBypassedByAdminId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    DeviceCheckBypassReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoachReferrals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContactEmail = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ActivityType = table.Column<int>(type: "integer", nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReferrerUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ReferralCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IpHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    AdminNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ActionedByAdminId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ActionedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachReferrals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachReferrals_AspNetUsers_ReferrerUserId",
                        column: x => x.ReferrerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeviceFingerprints",
                columns: table => new
                {
                    Hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FirstSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LinkedUserCount = table.Column<int>(type: "integer", nullable: false),
                    ReviewedByAdmin = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceFingerprints", x => x.Hash);
                });

            migrationBuilder.CreateTable(
                name: "PromoterProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ReferralCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TotalReferrals = table.Column<int>(type: "integer", nullable: false),
                    TotalActivations = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoterProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromoterProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MonthlyPriceEur = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    RankBoost = table.Column<int>(type: "integer", nullable: false),
                    TrialDays = table.Column<int>(type: "integer", nullable: false),
                    StripePriceId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    FeaturesJson = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BusinessListings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ActivityType = table.Column<int>(type: "integer", nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AddressLine = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Latitude = table.Column<decimal>(type: "numeric(10,7)", nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(10,7)", nullable: true),
                    AgeFromMonths = table.Column<short>(type: "smallint", nullable: true),
                    AgeToMonths = table.Column<short>(type: "smallint", nullable: true),
                    PriceFromEur = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    Schedule = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    RankBoost = table.Column<int>(type: "integer", nullable: false),
                    ViewCount = table.Column<long>(type: "bigint", nullable: false),
                    LikeCount = table.Column<long>(type: "bigint", nullable: false),
                    CommentCount = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessListings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessListings_BusinessProfiles_BusinessProfileId",
                        column: x => x.BusinessProfileId,
                        principalTable: "BusinessProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessPolicyAcceptances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessPolicyAcceptances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessPolicyAcceptances_BusinessPolicyVersions_PolicyVers~",
                        column: x => x.PolicyVersionId,
                        principalTable: "BusinessPolicyVersions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BusinessPolicyAcceptances_BusinessProfiles_BusinessProfileId",
                        column: x => x.BusinessProfileId,
                        principalTable: "BusinessProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    StripeSubscriptionId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CurrentPeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentPeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TrialEndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CanceledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GracePeriodEndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessSubscriptions_BusinessProfiles_BusinessProfileId",
                        column: x => x.BusinessProfileId,
                        principalTable: "BusinessProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceFingerprintUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FingerprintHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    FirstSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceFingerprintUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceFingerprintUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeviceFingerprintUsers_DeviceFingerprints_FingerprintHash",
                        column: x => x.FingerprintHash,
                        principalTable: "DeviceFingerprints",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessListingComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Body = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ParentCommentId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false),
                    HiddenReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessListingComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessListingComments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BusinessListingComments_BusinessListingComments_ParentComme~",
                        column: x => x.ParentCommentId,
                        principalTable: "BusinessListingComments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BusinessListingComments_BusinessListings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "BusinessListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessListingDailyStats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Views = table.Column<long>(type: "bigint", nullable: false),
                    UniqueViewers = table.Column<long>(type: "bigint", nullable: false),
                    Likes = table.Column<long>(type: "bigint", nullable: false),
                    Comments = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessListingDailyStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessListingDailyStats_BusinessListings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "BusinessListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessListingLikes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessListingLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessListingLikes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BusinessListingLikes_BusinessListings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "BusinessListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessListingPhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsCover = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessListingPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessListingPhotos_BusinessListings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "BusinessListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessListingViewEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewerHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessListingViewEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessListingViewEvents_BusinessListings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "BusinessListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessSubscriptionEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StripeEventId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    RawType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PayloadJson = table.Column<string>(type: "text", maxLength: 32000, nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessSubscriptionEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessSubscriptionEvents_BusinessSubscriptions_Subscripti~",
                        column: x => x.SubscriptionId,
                        principalTable: "BusinessSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessListingComments_Listing_CreatedAt",
                table: "BusinessListingComments",
                columns: new[] { "ListingId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessListingComments_ListingId",
                table: "BusinessListingComments",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessListingComments_ParentCommentId",
                table: "BusinessListingComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessListingComments_UserId",
                table: "BusinessListingComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UX_BusinessListingDailyStats_Listing_Date",
                table: "BusinessListingDailyStats",
                columns: new[] { "ListingId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessListingLikes_ListingId",
                table: "BusinessListingLikes",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessListingLikes_UserId",
                table: "BusinessListingLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UX_BusinessListingLikes_User_Listing",
                table: "BusinessListingLikes",
                columns: new[] { "UserId", "ListingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessListingPhotos_ListingId",
                table: "BusinessListingPhotos",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "UX_BusinessListingPhotos_Listing_Order",
                table: "BusinessListingPhotos",
                columns: new[] { "ListingId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessListings_ActivityType",
                table: "BusinessListings",
                column: "ActivityType");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessListings_BrowseSort",
                table: "BusinessListings",
                columns: new[] { "IsActive", "IsApproved", "RankBoost", "CreatedAt" },
                descending: new[] { false, false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessListings_City",
                table: "BusinessListings",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessListings_IsApproved",
                table: "BusinessListings",
                column: "IsApproved");

            migrationBuilder.CreateIndex(
                name: "UX_BusinessListings_BusinessProfileId",
                table: "BusinessListings",
                column: "BusinessProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessListingViewEvents_Listing_OccurredAt",
                table: "BusinessListingViewEvents",
                columns: new[] { "ListingId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessPolicyAcceptances_PolicyVersionId",
                table: "BusinessPolicyAcceptances",
                column: "PolicyVersionId");

            migrationBuilder.CreateIndex(
                name: "UX_BusinessPolicyAcceptances_Profile_Version",
                table: "BusinessPolicyAcceptances",
                columns: new[] { "BusinessProfileId", "PolicyVersionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_BusinessPolicyVersions_Language_Current",
                table: "BusinessPolicyVersions",
                columns: new[] { "Language", "IsCurrent" },
                unique: true,
                filter: "\"IsCurrent\" = true");

            migrationBuilder.CreateIndex(
                name: "UX_BusinessPolicyVersions_Language_Version",
                table: "BusinessPolicyVersions",
                columns: new[] { "Language", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessProfiles_City",
                table: "BusinessProfiles",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessProfiles_DeviceFingerprintHash",
                table: "BusinessProfiles",
                column: "DeviceFingerprintHash");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessProfiles_Status",
                table: "BusinessProfiles",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessProfiles_StripeCustomerId",
                table: "BusinessProfiles",
                column: "StripeCustomerId");

            migrationBuilder.CreateIndex(
                name: "UX_BusinessProfiles_UserId",
                table: "BusinessProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessSubscriptionEvents_Subscription_OccurredAt",
                table: "BusinessSubscriptionEvents",
                columns: new[] { "SubscriptionId", "OccurredAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "UX_BusinessSubscriptionEvents_StripeEventId",
                table: "BusinessSubscriptionEvents",
                column: "StripeEventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessSubscriptions_Status_GraceEndsAt",
                table: "BusinessSubscriptions",
                columns: new[] { "Status", "GracePeriodEndsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessSubscriptions_Status_TrialEndsAt",
                table: "BusinessSubscriptions",
                columns: new[] { "Status", "TrialEndsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessSubscriptions_StripeSubscriptionId",
                table: "BusinessSubscriptions",
                column: "StripeSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "UX_BusinessSubscriptions_BusinessProfileId",
                table: "BusinessSubscriptions",
                column: "BusinessProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoachReferrals_ContactEmail",
                table: "CoachReferrals",
                column: "ContactEmail");

            migrationBuilder.CreateIndex(
                name: "IX_CoachReferrals_ContactPhone",
                table: "CoachReferrals",
                column: "ContactPhone");

            migrationBuilder.CreateIndex(
                name: "IX_CoachReferrals_ReferralCode",
                table: "CoachReferrals",
                column: "ReferralCode");

            migrationBuilder.CreateIndex(
                name: "IX_CoachReferrals_ReferrerUserId",
                table: "CoachReferrals",
                column: "ReferrerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachReferrals_Status_CreatedAt",
                table: "CoachReferrals",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceFingerprintUsers_UserId",
                table: "DeviceFingerprintUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UX_DeviceFingerprintUsers_Hash_User",
                table: "DeviceFingerprintUsers",
                columns: new[] { "FingerprintHash", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_PromoterProfiles_ReferralCode",
                table: "PromoterProfiles",
                column: "ReferralCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_PromoterProfiles_UserId",
                table: "PromoterProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_SortOrder",
                table: "SubscriptionPlans",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "UX_SubscriptionPlans_Code",
                table: "SubscriptionPlans",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessListingComments");

            migrationBuilder.DropTable(
                name: "BusinessListingDailyStats");

            migrationBuilder.DropTable(
                name: "BusinessListingLikes");

            migrationBuilder.DropTable(
                name: "BusinessListingPhotos");

            migrationBuilder.DropTable(
                name: "BusinessListingViewEvents");

            migrationBuilder.DropTable(
                name: "BusinessPolicyAcceptances");

            migrationBuilder.DropTable(
                name: "BusinessSubscriptionEvents");

            migrationBuilder.DropTable(
                name: "CoachReferrals");

            migrationBuilder.DropTable(
                name: "DeviceFingerprintUsers");

            migrationBuilder.DropTable(
                name: "PromoterProfiles");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.DropTable(
                name: "BusinessListings");

            migrationBuilder.DropTable(
                name: "BusinessPolicyVersions");

            migrationBuilder.DropTable(
                name: "BusinessSubscriptions");

            migrationBuilder.DropTable(
                name: "DeviceFingerprints");

            migrationBuilder.DropTable(
                name: "BusinessProfiles");
        }
    }
}
