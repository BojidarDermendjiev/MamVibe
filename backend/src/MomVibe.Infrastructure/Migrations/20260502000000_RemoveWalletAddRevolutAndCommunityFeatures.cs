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
            // Drop wallet tables (IF EXISTS so this is safe on any database state)
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"WalletTransfers\" CASCADE");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"WalletTransactions\" CASCADE");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"Wallets\" CASCADE");

            // Remove IBAN column if it exists
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'AspNetUsers' AND column_name = 'Iban'
                    ) THEN
                        ALTER TABLE ""AspNetUsers"" DROP COLUMN ""Iban"";
                    END IF;
                END $$;
            ");

            // Add RevolutTag if it doesn't already exist
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'AspNetUsers' AND column_name = 'RevolutTag'
                    ) THEN
                        ALTER TABLE ""AspNetUsers""
                            ADD COLUMN ""RevolutTag"" character varying(50) NULL;
                    END IF;
                END $$;
            ");

            // Create DoctorReviews table if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""DoctorReviews"" (
                    ""Id""             uuid                        NOT NULL,
                    ""UserId""         text                        NOT NULL,
                    ""DoctorName""     character varying(100)      NOT NULL,
                    ""Specialization"" character varying(100)      NOT NULL,
                    ""ClinicName""     character varying(150)      NULL,
                    ""City""           character varying(100)      NOT NULL,
                    ""Rating""         integer                     NOT NULL,
                    ""Content""        character varying(2000)     NOT NULL,
                    ""SuperdocUrl""    character varying(2048)     NULL,
                    ""IsAnonymous""    boolean                     NOT NULL DEFAULT false,
                    ""CreatedAt""      timestamp with time zone    NOT NULL,
                    ""UpdatedAt""      timestamp with time zone    NULL,
                    CONSTRAINT ""PK_DoctorReviews"" PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_DoctorReviews_AspNetUsers_UserId""
                        FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers""(""Id"") ON DELETE CASCADE
                );
            ");

            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_DoctorReviews_City""           ON ""DoctorReviews""(""City"");");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_DoctorReviews_Specialization"" ON ""DoctorReviews""(""Specialization"");");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_DoctorReviews_UserId""         ON ""DoctorReviews""(""UserId"");");

            // Create ChildFriendlyPlaces table if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""ChildFriendlyPlaces"" (
                    ""Id""             uuid                        NOT NULL,
                    ""UserId""         text                        NOT NULL,
                    ""Name""           character varying(150)      NOT NULL,
                    ""Description""    character varying(2000)     NOT NULL,
                    ""Address""        character varying(300)      NULL,
                    ""City""           character varying(100)      NOT NULL,
                    ""PlaceType""      integer                     NOT NULL,
                    ""AgeFromMonths""  integer                     NULL,
                    ""AgeToMonths""    integer                     NULL,
                    ""PhotoUrl""       character varying(2048)     NULL,
                    ""Website""        character varying(2048)     NULL,
                    ""IsApproved""     boolean                     NOT NULL DEFAULT false,
                    ""CreatedAt""      timestamp with time zone    NOT NULL,
                    ""UpdatedAt""      timestamp with time zone    NULL,
                    CONSTRAINT ""PK_ChildFriendlyPlaces"" PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_ChildFriendlyPlaces_AspNetUsers_UserId""
                        FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers""(""Id"") ON DELETE CASCADE
                );
            ");

            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_ChildFriendlyPlaces_City""      ON ""ChildFriendlyPlaces""(""City"");");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_ChildFriendlyPlaces_PlaceType"" ON ""ChildFriendlyPlaces""(""PlaceType"");");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_ChildFriendlyPlaces_IsApproved"" ON ""ChildFriendlyPlaces""(""IsApproved"");");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_ChildFriendlyPlaces_UserId""    ON ""ChildFriendlyPlaces""(""UserId"");");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"DoctorReviews\"");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"ChildFriendlyPlaces\"");
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'AspNetUsers' AND column_name = 'RevolutTag'
                    ) THEN
                        ALTER TABLE ""AspNetUsers"" DROP COLUMN ""RevolutTag"";
                    END IF;
                END $$;
            ");
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'AspNetUsers' AND column_name = 'Iban'
                    ) THEN
                        ALTER TABLE ""AspNetUsers""
                            ADD COLUMN ""Iban"" character varying(34) NULL;
                    END IF;
                END $$;
            ");
        }
    }
}
