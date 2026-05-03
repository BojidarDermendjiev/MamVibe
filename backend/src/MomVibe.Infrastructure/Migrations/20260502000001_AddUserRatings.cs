using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomVibe.Infrastructure.Migrations
{
    public partial class AddUserRatings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""UserRatings"" (
                    ""Id""                uuid                        NOT NULL,
                    ""RaterId""           text                        NOT NULL,
                    ""RatedUserId""       text                        NOT NULL,
                    ""PurchaseRequestId"" uuid                        NOT NULL,
                    ""Rating""            integer                     NOT NULL,
                    ""Comment""           character varying(500)      NULL,
                    ""CreatedAt""         timestamp with time zone    NOT NULL,
                    ""UpdatedAt""         timestamp with time zone    NULL,
                    CONSTRAINT ""PK_UserRatings"" PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_UserRatings_AspNetUsers_RaterId""
                        FOREIGN KEY (""RaterId"") REFERENCES ""AspNetUsers""(""Id"") ON DELETE CASCADE,
                    CONSTRAINT ""FK_UserRatings_AspNetUsers_RatedUserId""
                        FOREIGN KEY (""RatedUserId"") REFERENCES ""AspNetUsers""(""Id"") ON DELETE CASCADE,
                    CONSTRAINT ""FK_UserRatings_PurchaseRequests_PurchaseRequestId""
                        FOREIGN KEY (""PurchaseRequestId"") REFERENCES ""PurchaseRequests""(""Id"") ON DELETE CASCADE,
                    CONSTRAINT ""UQ_UserRatings_PurchaseRequest"" UNIQUE (""PurchaseRequestId"")
                );
            ");

            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_UserRatings_RatedUserId"" ON ""UserRatings""(""RatedUserId"");");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_UserRatings_RaterId""     ON ""UserRatings""(""RaterId"");");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""UserRatings"" CASCADE;");
        }
    }
}
