using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

#nullable disable

#pragma warning disable CA1814

namespace MomVibe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKnowledgeArticlesWithFts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KnowledgeArticles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Language = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false, defaultValue: "en"),
                    Tags = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SearchVector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true,
                        computedColumnSql: "to_tsvector('simple', coalesce(\"Title\",'') || ' ' || coalesce(\"Content\",''))",
                        stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeArticles", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "KnowledgeArticles",
                columns: new[] { "Id", "Content", "CreatedAt", "Language", "Tags", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "MamVibe integrates four couriers: Econt, Speedy, BoxNow, and PigeonExpress. After a purchase is paid, the seller ships within 3 business days. Buyers choose between courier office pickup, home address delivery, or parcel locker. Cash-on-delivery (COD) is supported for Econt and Speedy. Track shipments in Dashboard → Shipments.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "en", new[] { "shipping", "delivery", "econt", "speedy", "boxnow", "pigeon" }, "Shipping & Delivery on MamVibe", null },
                    { 2, "MamVibe работи с четири куриера: Еконт, Спиди, BoxNow и PigeonExpress. След потвърдено плащане продавачът изпраща в рамките на 3 работни дни. Купувачите избират между офис на куриера, доставка до адрес или автоматична станция. Наложен платеж се поддържа при Еконт и Спиди. Следете пратките в Табло → Пратки.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bg", new[] { "доставка", "куриер", "еконт", "спиди" }, "Доставка в MamVibe", null },
                    { 3, "Pay with a MamVibe Wallet balance or directly by card via Stripe checkout. Top up the wallet from Settings → Wallet (minimum 5 BGN). Wallet balance never expires. Sellers receive funds only after the buyer confirms receipt. Withdraw earnings from Settings → Wallet → Withdraw (IBAN required, processed in 2 business days). Cash-on-delivery is available for Econt and Speedy.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "en", new[] { "payment", "wallet", "stripe", "card", "cod", "withdraw" }, "Payments & MamVibe Wallet", null },
                    { 4, "Платете с баланс в MamVibe Портфейл или директно с карта чрез Stripe. Портфейлът се зарежда от Настройки → Портфейл (минимум 5 лв.). Балансът не изтича. Продавачите получават пари след потвърждение от купувача. Тегленето е от Настройки → Портфейл → Теглене (необходим IBAN, обработен в 2 работни дни). Наложен платеж е наличен при Еконт и Спиди.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bg", new[] { "плащане", "портфейл", "карта", "наложен платеж", "теглене" }, "Плащания и Портфейл в MamVibe", null },
                    { 5, "To post a listing: log in, click Create in the top navigation, upload at least one clear photo, fill in title, description, category, age group, and size, then set a price (leave blank for a free donation). AI moderates the listing automatically — most are approved instantly. Once approved, your listing appears on the Browse page. Manage active listings in Dashboard → My Listings.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "en", new[] { "sell", "listing", "create", "post", "upload", "photo" }, "How to Sell on MamVibe", null },
                    { 6, "За да публикувате обява: влезте в профила, натиснете Създай в навигацията, качете поне една ясна снимка, попълнете заглавие, описание, категория, възраст и размер, след което задайте цена (оставете празно за дарение). AI автоматично модерира — повечето се одобряват моментално. Активните обяви се управляват от Табло → Моите обяви.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bg", new[] { "продавам", "обява", "създаване", "снимка", "качване" }, "Как да продавам в MamVibe", null },
                    { 7, "To buy an item: go to /browse and filter by category, age group, price, or listing type. Click an item and press Send Purchase Request. The seller has 48 hours to accept — no response means the request is auto-cancelled. Once accepted, complete payment via Wallet or card. The seller ships within 3 business days. Confirm receipt in Dashboard → Purchases; it auto-confirms after 5 days.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "en", new[] { "buy", "purchase", "request", "order", "browse" }, "How to Buy on MamVibe", null },
                    { 8, "За да купите продукт: отидете на /browse и филтрирайте по категория, възраст, цена. Кликнете на продукта и натиснете Изпрати заявка. Продавачът има 48 часа да приеме — без отговор заявката се отменя автоматично. След приемане завършете плащането. Продавачът изпраща в 3 работни дни. Потвърдете получаването в Табло → Покупки; автоматично потвърждение след 5 дни.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bg", new[] { "купуване", "заявка", "поръчка", "разглеждане" }, "Как да купувам в MamVibe", null },
                    { 9, "All sales are final by default (second-hand marketplace). If an item arrives significantly different from its description, open a dispute within 48 hours of delivery: Dashboard → Purchases → Report Problem. Disputes are reviewed by MamVibe admins and resolved within 3–5 business days. For urgent help: support@mamvibe.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "en", new[] { "return", "dispute", "refund", "problem", "complaint", "damage" }, "Returns & Disputes", null },
                    { 10, "Всички продажби са окончателни (пазар за употребявани стоки). Ако продукт пристигне значително различен от описанието, отворете спор в 48 часа след доставката: Табло → Покупки → Докладвай проблем. Споровете се решават от администраторите в 3–5 работни дни. За спешна помощ: support@mamvibe.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bg", new[] { "връщане", "спор", "рекламация", "проблем", "повреда" }, "Връщания и Спорове", null },
                    { 11, "MamVibe hosts verified parent reviews of Bulgarian doctors — pediatricians, gynecologists, and other specialists. Access at /doctor-reviews (nav: Doctors). Filter by city and specialization. To write a review, log in — it goes live after admin approval. Only verified reviews are published.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "en", new[] { "doctor", "review", "pediatrician", "gynecologist", "specialist" }, "Doctor Reviews on MamVibe", null },
                    { 12, "MamVibe събира верифицирани отзиви от родители за български лекари — педиатри, гинеколози и специалисти. Достъпно на /doctor-reviews (навигация: Лекари). Филтрирайте по град и специализация. За да напишете отзив, влезте в профила — публикува се след одобрение от администратор.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bg", new[] { "лекар", "отзив", "педиатър", "гинеколог", "специалист" }, "Отзиви за лекари в MamVibe", null },
                    { 13, "Register with email and password, or sign in with Google. Profile types: Mom, Dad, Other. Switch language (BG/EN) with the top-right switcher. Toggle dark/light theme with the top-right icon. Change email or password at Settings → Account. Forgot password: use the Forgot password link on the login page. Delete account at Settings → Account → Delete Account (all listings are removed).", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "en", new[] { "account", "settings", "password", "profile", "register", "login", "google", "delete" }, "Account & Settings", null },
                    { 14, "Регистрирайте се с имейл и парола или влезте с Google. Типове профил: Мама, Татко, Друго. Сменете езика (БГ/EN) горе вдясно. Сменете тъмна/светла тема с иконата горе вдясно. Сменете имейл или парола в Настройки → Акаунт. Забравена парола: линкът Забравена парола на страницата за вход. Изтрийте акаунт в Настройки → Акаунт → Изтриване.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bg", new[] { "акаунт", "настройки", "парола", "профил", "регистрация", "изтриване" }, "Акаунт и Настройки", null },
                    { 15, "Discover parks, playgrounds, cafes, and family-friendly restaurants across Bulgaria at /child-friendly-places (nav: Places). Filter by city and place type. Logged-in users can submit a new place — it goes live after admin approval.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "en", new[] { "places", "playground", "park", "family", "restaurant", "cafe" }, "Child-Friendly Places", null },
                    { 16, "Открийте паркове, детски площадки, кафенета и семейни ресторанти в България на /child-friendly-places (навигация: Места). Филтрирайте по град и вид место. Влезлите потребители могат да предложат ново место — публикува се след одобрение.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bg", new[] { "места", "детска площадка", "парк", "семейно", "ресторант" }, "Детски Места", null },
                    { 17, "MamVibe recommends: always use the in-platform chat for communication; never share phone number, address, or bank details in messages; prefer courier shipping over cash meetups with unknown buyers; if meeting in person, choose a busy public place (shopping mall, courier office); report suspicious users with the Report button.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "en", new[] { "safety", "meetup", "trust", "scam", "tips", "secure" }, "Safety Tips for Buying & Selling", null },
                    { 18, "MamVibe препоръчва: използвайте само вградения чат; никога не споделяйте телефон, адрес или банкови данни; предпочитайте куриер пред лични срещи с непознати; при лична среща изберете оживено обществено място (мол, офис на куриера); докладвайте подозрителни потребители с бутона Докладвай.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bg", new[] { "безопасност", "среща", "доверие", "измама", "съвети" }, "Съвети за безопасност", null },
                    { 19, "After each completed sale, the buyer can rate the seller from 1 to 5 stars and leave a comment. Ratings are visible on every seller's public profile. You cannot rate a seller until you have completed a transaction with them.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "en", new[] { "rating", "review", "seller", "stars", "feedback", "trust" }, "Seller Ratings", null },
                    { 20, "MamVibe is a Bulgarian community marketplace for buying, selling, and donating second-hand baby and children's items. It also features verified doctor reviews and child-friendly places. For help: support@mamvibe.com. Pages: Home (/), Browse (/browse), Create (/create), Chat (/chat), Dashboard (/dashboard), Doctor Reviews (/doctor-reviews), Child-Friendly Places (/child-friendly-places), Profile (/profile), Settings (/settings).", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "en", new[] { "mamvibe", "about", "contact", "support", "help", "navigation" }, "About MamVibe & Contact", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeArticles_Language",
                table: "KnowledgeArticles",
                column: "Language");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeArticles_SearchVector",
                table: "KnowledgeArticles",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "KnowledgeArticles");
        }
    }
}
