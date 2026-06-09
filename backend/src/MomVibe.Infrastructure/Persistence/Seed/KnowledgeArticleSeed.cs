namespace MomVibe.Infrastructure.Persistence.Seed;

using Microsoft.EntityFrameworkCore;
using Domain.Entities;

public static class KnowledgeArticleSeed
{
    private static readonly DateTime SeedDate = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static void Seed(ModelBuilder builder) =>
        builder.Entity<KnowledgeArticle>().HasData(Articles);

    private static readonly KnowledgeArticle[] Articles =
    [
        new() {
            Id = 1, Language = "en",
            Tags = ["shipping", "delivery", "econt", "speedy", "boxnow", "pigeon"],
            Title = "Shipping & Delivery on MamVibe",
            Content = "MamVibe integrates four couriers: Econt, Speedy, BoxNow, and PigeonExpress. After a purchase is paid, the seller ships within 3 business days. Buyers choose between courier office pickup, home address delivery, or parcel locker. Cash-on-delivery (COD) is supported for Econt and Speedy. Track shipments in Dashboard → Shipments.",
            CreatedAt = SeedDate,
        },
        new() {
            Id = 2, Language = "bg",
            Tags = ["доставка", "куриер", "еконт", "спиди"],
            Title = "Доставка в MamVibe",
            Content = "MamVibe работи с четири куриера: Еконт, Спиди, BoxNow и PigeonExpress. След потвърдено плащане продавачът изпраща в рамките на 3 работни дни. Купувачите избират между офис на куриера, доставка до адрес или автоматична станция. Наложен платеж се поддържа при Еконт и Спиди. Следете пратките в Табло → Пратки.",
            CreatedAt = SeedDate,
        },
        new() {
            Id = 3, Language = "en",
            Tags = ["payment", "stripe", "card", "cod", "revolut"],
            Title = "Payments on MamVibe",
            Content = "Pay by card via Stripe checkout. Cash-on-delivery (COD) is available for Econt and Speedy shipments. Sellers list their Revolut Tag on their profile for peer-to-peer transfers. Sellers receive funds only after the buyer confirms receipt.",
            CreatedAt = SeedDate,
        },
        new() {
            Id = 4, Language = "bg",
            Tags = ["плащане", "карта", "наложен платеж", "revolut"],
            Title = "Плащания в MamVibe",
            Content = "Платете с карта чрез Stripe. Наложен платеж е наличен при Еконт и Спиди. Продавачите могат да посочат Revolut Tag в профила си за директни преводи. Продавачите получават пари след потвърждение от купувача.",
            CreatedAt = SeedDate,
        },
        new() {
            Id = 5, Language = "en",
            Tags = ["sell", "listing", "create", "post", "upload", "photo"],
            Title = "How to Sell on MamVibe",
            Content = "To post a listing: log in, click Create in the top navigation, upload at least one clear photo, fill in title, description, category, age group, and size, then set a price (leave blank for a free donation). AI moderates the listing automatically — most are approved instantly. Once approved, your listing appears on the Browse page. Manage active listings in Dashboard → My Listings.",
            CreatedAt = SeedDate,
        },
        new() {
            Id = 6, Language = "bg",
            Tags = ["продавам", "обява", "създаване", "снимка", "качване"],
            Title = "Как да продавам в MamVibe",
            Content = "За да публикувате обява: влезте в профила, натиснете Създай в навигацията, качете поне една ясна снимка, попълнете заглавие, описание, категория, възраст и размер, след което задайте цена (оставете празно за дарение). AI автоматично модерира — повечето се одобряват моментално. Активните обяви се управляват от Табло → Моите обяви.",
            CreatedAt = SeedDate,
        },
        new() {
            Id = 7, Language = "en",
            Tags = ["buy", "purchase", "request", "order", "browse"],
            Title = "How to Buy on MamVibe",
            Content = "To buy an item: go to /browse and filter by category, age group, price, or listing type. Click an item and press Send Purchase Request. The seller has 48 hours to accept — no response means the request is auto-cancelled. Once accepted, complete payment by card via Stripe. The seller ships within 3 business days. Confirm receipt in Dashboard → Purchases; it auto-confirms after 5 days.",
            CreatedAt = SeedDate,
        },
        new() {
            Id = 8, Language = "bg",
            Tags = ["купуване", "заявка", "поръчка", "разглеждане"],
            Title = "Как да купувам в MamVibe",
            Content = "За да купите продукт: отидете на /browse и филтрирайте по категория, възраст, цена. Кликнете на продукта и натиснете Изпрати заявка. Продавачът има 48 часа да приеме — без отговор заявката се отменя автоматично. След приемане завършете плащането. Продавачът изпраща в 3 работни дни. Потвърдете получаването в Табло → Покупки; автоматично потвърждение след 5 дни.",
            CreatedAt = SeedDate,
        },
        new() {
            Id = 9, Language = "en",
            Tags = ["return", "dispute", "refund", "problem", "complaint", "damage"],
            Title = "Returns & Disputes",
            Content = "All sales are final by default (second-hand marketplace). If an item arrives significantly different from its description, open a dispute within 48 hours of delivery: Dashboard → Purchases → Report Problem. Disputes are reviewed by MamVibe admins and resolved within 3–5 business days. For urgent help: support@mamvibe.com",
            CreatedAt = SeedDate,
        },
        new() {
            Id = 10, Language = "bg",
            Tags = ["връщане", "спор", "рекламация", "проблем", "повреда"],
            Title = "Връщания и Спорове",
            Content = "Всички продажби са окончателни (пазар за употребявани стоки). Ако продукт пристигне значително различен от описанието, отворете спор в 48 часа след доставката: Табло → Покупки → Докладвай проблем. Споровете се решават от администраторите в 3–5 работни дни. За спешна помощ: support@mamvibe.com",
            CreatedAt = SeedDate,
        },
        new() {
            Id = 11, Language = "en",
            Tags = ["doctor", "review", "pediatrician", "gynecologist", "specialist"],
            Title = "Doctor Reviews on MamVibe",
            Content = "MamVibe hosts verified parent reviews of Bulgarian doctors — pediatricians, gynecologists, and other specialists. Access at /doctor-reviews (nav: Doctors). Filter by city and specialization. To write a review, log in — it goes live after admin approval. Only verified reviews are published.",
            CreatedAt = SeedDate,
        },
        new() {
            Id = 12, Language = "bg",
            Tags = ["лекар", "отзив", "педиатър", "гинеколог", "специалист"],
            Title = "Отзиви за лекари в MamVibe",
            Content = "MamVibe събира верифицирани отзиви от родители за български лекари — педиатри, гинеколози и специалисти. Достъпно на /doctor-reviews (навигация: Лекари). Филтрирайте по град и специализация. За да напишете отзив, влезте в профила — публикува се след одобрение от администратор.",
            CreatedAt = SeedDate,
        },
        new() {
            Id = 13, Language = "en",
            Tags = ["account", "settings", "password", "profile", "register", "login", "google", "delete"],
            Title = "Account & Settings",
            Content = "Register with email and password, or sign in with Google. Profile types: Mom, Dad, Other. Switch language (BG/EN) with the top-right switcher. Toggle dark/light theme with the top-right icon. Change email or password at Settings → Account. Forgot password: use the Forgot password link on the login page. Delete account at Settings → Account → Delete Account (all listings are removed).",
            CreatedAt = SeedDate,
        },
        new() {
            Id = 14, Language = "bg",
            Tags = ["акаунт", "настройки", "парола", "профил", "регистрация", "изтриване"],
            Title = "Акаунт и Настройки",
            Content = "Регистрирайте се с имейл и парола или влезте с Google. Типове профил: Мама, Татко, Друго. Сменете езика (БГ/EN) горе вдясно. Сменете тъмна/светла тема с иконата горе вдясно. Сменете имейл или парола в Настройки → Акаунт. Забравена парола: линкът Забравена парола на страницата за вход. Изтрийте акаунт в Настройки → Акаунт → Изтриване.",
            CreatedAt = SeedDate,
        },
        new() {
            Id = 15, Language = "en",
            Tags = ["places", "playground", "park", "family", "restaurant", "cafe"],
            Title = "Child-Friendly Places",
            Content = "Discover parks, playgrounds, cafes, and family-friendly restaurants across Bulgaria at /child-friendly-places (nav: Places). Filter by city and place type. Logged-in users can submit a new place — it goes live after admin approval.",
            CreatedAt = SeedDate,
        },
        new() {
            Id = 16, Language = "bg",
            Tags = ["места", "детска площадка", "парк", "семейно", "ресторант"],
            Title = "Детски Места",
            Content = "Открийте паркове, детски площадки, кафенета и семейни ресторанти в България на /child-friendly-places (навигация: Места). Филтрирайте по град и вид место. Влезлите потребители могат да предложат ново место — публикува се след одобрение.",
            CreatedAt = SeedDate,
        },
        new() {
            Id = 17, Language = "en",
            Tags = ["safety", "meetup", "trust", "scam", "tips", "secure"],
            Title = "Safety Tips for Buying & Selling",
            Content = "MamVibe recommends: always use the in-platform chat for communication; never share phone number, address, or bank details in messages; prefer courier shipping over cash meetups with unknown buyers; if meeting in person, choose a busy public place (shopping mall, courier office); report suspicious users with the Report button.",
            CreatedAt = SeedDate,
        },
        new() {
            Id = 18, Language = "bg",
            Tags = ["безопасност", "среща", "доверие", "измама", "съвети"],
            Title = "Съвети за безопасност",
            Content = "MamVibe препоръчва: използвайте само вградения чат; никога не споделяйте телефон, адрес или банкови данни; предпочитайте куриер пред лични срещи с непознати; при лична среща изберете оживено обществено място (мол, офис на куриера); докладвайте подозрителни потребители с бутона Докладвай.",
            CreatedAt = SeedDate,
        },
        new() {
            Id = 19, Language = "en",
            Tags = ["rating", "review", "seller", "stars", "feedback", "trust"],
            Title = "Seller Ratings",
            Content = "After each completed sale, the buyer can rate the seller from 1 to 5 stars and leave a comment. Ratings are visible on every seller's public profile. You cannot rate a seller until you have completed a transaction with them.",
            CreatedAt = SeedDate,
        },
        new() {
            Id = 20, Language = "en",
            Tags = ["mamvibe", "about", "contact", "support", "help", "navigation"],
            Title = "About MamVibe & Contact",
            Content = "MamVibe is a Bulgarian community marketplace for buying, selling, and donating second-hand baby and children's items. It also features verified doctor reviews and child-friendly places. For help: support@mamvibe.com. Pages: Home (/), Browse (/browse), Create (/create), Chat (/chat), Dashboard (/dashboard), Doctor Reviews (/doctor-reviews), Child-Friendly Places (/child-friendly-places), Profile (/profile), Settings (/settings).",
            CreatedAt = SeedDate,
        },
    ];
}
