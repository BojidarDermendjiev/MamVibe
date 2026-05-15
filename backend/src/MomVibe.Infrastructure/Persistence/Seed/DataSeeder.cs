namespace MomVibe.Infrastructure.Persistence.Seed;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using MomVibe.Domain.Entities;
using MomVibe.Domain.Constants;
using MomVibe.Domain.Enums;

/// <summary>
/// Seeds initial roles, admin user, and demo data into the database.
/// </summary>
public static class DataSeeder
{
    private static readonly string[] Roles = ["Admin", "User"];

    /// <summary>
    /// Ensures the required application roles exist.
    /// </summary>
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    /// <summary>
    /// Creates the admin user from configuration if seeding is enabled,
    /// and ensures the Admin role is always assigned.
    ///
    /// Production: set AdminSeed__Enabled=true and supply credentials
    /// via environment variables or a secrets manager — never commit
    /// real credentials to source control.
    /// </summary>
    public static async Task SeedAdminAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        // Opt-in: seeding is disabled by default in appsettings.json
        // and enabled only in appsettings.Development.json (or via env vars).
        var enabled = configuration.GetValue<bool>("AdminSeed:Enabled");
        if (!enabled)
            return;

        var email = configuration["AdminSeed:Email"];
        var password = configuration["AdminSeed:Password"];
        var displayName = configuration["AdminSeed:DisplayName"] ?? "Admin";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        var admin = await userManager.FindByEmailAsync(email);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                DisplayName = displayName,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
            };

            var result = await userManager.CreateAsync(admin, password);
            if (!result.Succeeded)
                return;
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }

    /// <summary>
    /// Ensures the AI assistant bot user exists in all environments.
    /// Uses a well-known ID so it is never duplicated.
    /// </summary>
    public static async Task SeedAiBotAsync(UserManager<ApplicationUser> userManager)
    {
        var existing = await userManager.FindByIdAsync(AiBotConstants.UserId);
        if (existing != null) return;

        var bot = new ApplicationUser
        {
            Id = AiBotConstants.UserId,
            UserName = AiBotConstants.UserName,
            Email = AiBotConstants.UserName,
            DisplayName = AiBotConstants.DisplayName,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
        };

        await userManager.CreateAsync(bot);
    }

    /// <summary>
    /// Seeds demo categories and sample data when running in Development.
    /// </summary>
    public static async Task SeedDemoDataAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        IWebHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
            return;

        if (dbContext.Categories.Any())
            return;

        var categories = new[]
        {
            new Category { Name = "Clothing", Slug = "clothing", Description = "Baby and maternity clothing" },
            new Category { Name = "Shoes", Slug = "shoes", Description = "Kids shoes and footwear" },
            new Category { Name = "Toys", Slug = "toys", Description = "Toys and games" },
            new Category { Name = "Car Seats", Slug = "car-seats", Description = "Child car seats and boosters" },
            new Category { Name = "Strollers", Slug = "strollers", Description = "Baby strollers and prams" },
            new Category { Name = "Furniture", Slug = "furniture", Description = "Cribs and furniture" },
            new Category { Name = "Feeding", Slug = "feeding", Description = "Bottles, breast pumps, and feeding accessories" },
            new Category { Name = "Other", Slug = "other", Description = "Miscellaneous items" },
        };

        dbContext.Categories.AddRange(categories);
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds demo user accounts, listings, and feedback for development.
    /// Runs only in Development and only when the Items table is empty.
    /// </summary>
    public static async Task SeedDemoUsersAndItemsAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        IWebHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
            return;

        if (dbContext.Items.Any())
            return;

        // Fixed IDs so re-runs never create duplicates
        const string user1Id = "demo-user-sofia-001";
        const string user2Id = "demo-user-plovdiv-002";
        const string user3Id = "demo-user-varna-003";

        async Task EnsureUser(string id, string email, string displayName, string? bio, ProfileType profileType)
        {
            if (await userManager.FindByIdAsync(id) != null) return;
            var user = new ApplicationUser
            {
                Id = id,
                UserName = email,
                Email = email,
                DisplayName = displayName,
                Bio = bio,
                ProfileType = profileType,
                EmailConfirmed = true,
                LanguagePreference = "bg",
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(30, 90)),
            };
            await userManager.CreateAsync(user);
        }

        await EnsureUser(user1Id, "maria.sofia@demo.mamvibe.com", "Мария", "Майка на две деца от София. Продавам неизползвани вещи с любов.", ProfileType.Female);
        await EnsureUser(user2Id, "elena.plovdiv@demo.mamvibe.com", "Елена", "Харесвам устойчивото потребление — даря и продавам бебешки вещи.", ProfileType.Female);
        await EnsureUser(user3Id, "ivan.varna@demo.mamvibe.com", "Иван", "Татко от Варна. Имаме двойни близнаци и много вещи за продаване!", ProfileType.Male);

        var categories = dbContext.Categories.ToList();
        Guid Cat(string slug) => categories.First(c => c.Slug == slug).Id;

        var items = new List<Item>
        {
            // Clothing - sell
            new() { Id = Guid.NewGuid(), Title = "Боди Chicco 3-6 месеца", Description = "Бяло боди с копчета, носено само 2 пъти. Размер 68, Chicco. Отлично състояние.", CategoryId = Cat("clothing"), ListingType = ListingType.Sell, AgeGroup = AgeGroup.Infant, ClothingSize = 68, Price = 5.00m, UserId = user1Id, IsActive = true, AiModerationStatus = AiModerationStatus.AutoApproved, ViewCount = 23, LikeCount = 4 },
            new() { Id = Guid.NewGuid(), Title = "Зимен гащеризон за бебе", Description = "Топъл пухен гащеризон за зимата. Размер 86, ярко жълт. Ползван един сезон.", CategoryId = Cat("clothing"), ListingType = ListingType.Sell, AgeGroup = AgeGroup.Infant, ClothingSize = 86, Price = 18.00m, UserId = user2Id, IsActive = true, AiModerationStatus = AiModerationStatus.AutoApproved, ViewCount = 41, LikeCount = 7 },
            new() { Id = Guid.NewGuid(), Title = "Пакет дрехи 12-18 месеца (10 бр.)", Description = "Смесен пакет: 3 боди, 3 потника, 2 панталончета, 2 блузки. Всичко в добро състояние.", CategoryId = Cat("clothing"), ListingType = ListingType.Donate, AgeGroup = AgeGroup.Infant, ClothingSize = 86, Price = null, UserId = user3Id, IsActive = true, AiModerationStatus = AiModerationStatus.AutoApproved, ViewCount = 67, LikeCount = 15 },
            new() { Id = Guid.NewGuid(), Title = "Лятна рокля H&M 2-3 години", Description = "Сладка флорална рокля H&M Kids. Размер 98. Носена само за снимки, практически нова.", CategoryId = Cat("clothing"), ListingType = ListingType.Sell, AgeGroup = AgeGroup.Toddler, ClothingSize = 98, Price = 8.00m, UserId = user1Id, IsActive = true, AiModerationStatus = AiModerationStatus.AutoApproved, ViewCount = 19, LikeCount = 3 },

            // Shoes - sell/donate
            new() { Id = Guid.NewGuid(), Title = "Буйки Lupilu 20 номер", Description = "Меки кожени буйки за първи стъпки. Номер 20. Леко ползвани, без следи.", CategoryId = Cat("shoes"), ListingType = ListingType.Sell, AgeGroup = AgeGroup.Toddler, ShoeSize = 20, Price = 12.00m, UserId = user2Id, IsActive = true, AiModerationStatus = AiModerationStatus.AutoApproved, ViewCount = 34, LikeCount = 6 },
            new() { Id = Guid.NewGuid(), Title = "Зимни ботушки Elefanten 24", Description = "Топли ватирани ботушки за зима. Водоустойчиви. Ползвани един сезон.", CategoryId = Cat("shoes"), ListingType = ListingType.Sell, AgeGroup = AgeGroup.Toddler, ShoeSize = 24, Price = 22.00m, UserId = user3Id, IsActive = true, AiModerationStatus = AiModerationStatus.AutoApproved, ViewCount = 28, LikeCount = 5 },
            new() { Id = Guid.NewGuid(), Title = "Спортни обувки Nike 30", Description = "Детски маратонки Nike, размер 30. Почти нови — купени голям номер и не се носиха.", CategoryId = Cat("shoes"), ListingType = ListingType.Donate, AgeGroup = AgeGroup.Preschool, ShoeSize = 30, Price = null, UserId = user1Id, IsActive = true, AiModerationStatus = AiModerationStatus.AutoApproved, ViewCount = 52, LikeCount = 11 },

            // Toys
            new() { Id = Guid.NewGuid(), Title = "Дървена железница Brio (40+ части)", Description = "Голям сет Brio с локомотив, вагони и релси. Всички части налични. Децата го обичаха!", CategoryId = Cat("toys"), ListingType = ListingType.Sell, AgeGroup = AgeGroup.Toddler, Price = 45.00m, UserId = user2Id, IsActive = true, AiModerationStatus = AiModerationStatus.AutoApproved, ViewCount = 89, LikeCount = 22 },
            new() { Id = Guid.NewGuid(), Title = "Играчки за баня — сет", Description = "8 броя играчки за баня: жаби, рибки, лодка. Отлично за бебета и малки деца.", CategoryId = Cat("toys"), ListingType = ListingType.Donate, AgeGroup = AgeGroup.Infant, Price = null, UserId = user3Id, IsActive = true, AiModerationStatus = AiModerationStatus.AutoApproved, ViewCount = 31, LikeCount = 8 },
            new() { Id = Guid.NewGuid(), Title = "Лего Duplo 60 части", Description = "Класически Duplo блокчета, 60 броя в различни цветове. Почистени и готови за игра.", CategoryId = Cat("toys"), ListingType = ListingType.Sell, AgeGroup = AgeGroup.Toddler, Price = 28.00m, UserId = user1Id, IsActive = true, AiModerationStatus = AiModerationStatus.AutoApproved, ViewCount = 73, LikeCount = 18 },

            // Strollers
            new() { Id = Guid.NewGuid(), Title = "Бебешка количка Hauck 3в1", Description = "Количка Hauck Rapid 3в1 — включва кош, седалка и база за столче. Ползвана 1 година.", CategoryId = Cat("strollers"), ListingType = ListingType.Sell, AgeGroup = AgeGroup.Newborn, Price = 250.00m, UserId = user2Id, IsActive = true, AiModerationStatus = AiModerationStatus.AutoApproved, ViewCount = 156, LikeCount = 31 },

            // Car Seats
            new() { Id = Guid.NewGuid(), Title = "Столче за кола Britax 0-13 кг", Description = "Britax B-Safe столче за кола, група 0+. Никога не е участвало в катастрофа. Пълна документация.", CategoryId = Cat("car-seats"), ListingType = ListingType.Sell, AgeGroup = AgeGroup.Newborn, Price = 80.00m, UserId = user3Id, IsActive = true, AiModerationStatus = AiModerationStatus.AutoApproved, ViewCount = 112, LikeCount = 24 },

            // Feeding
            new() { Id = Guid.NewGuid(), Title = "Електрическа помпа Medela Swing", Description = "Medela Swing Single електрическа помпа. Комплектована с всички аксесоари. Отлично работи.", CategoryId = Cat("feeding"), ListingType = ListingType.Sell, AgeGroup = AgeGroup.Newborn, Price = 65.00m, UserId = user1Id, IsActive = true, AiModerationStatus = AiModerationStatus.AutoApproved, ViewCount = 94, LikeCount = 17 },
            new() { Id = Guid.NewGuid(), Title = "Биберони Avent 3 бр. (0-6 м)", Description = "Три броя Philips Avent биберони. Само веднъж стерилизирани, никога не са ползвани.", CategoryId = Cat("feeding"), ListingType = ListingType.Donate, AgeGroup = AgeGroup.Newborn, Price = null, UserId = user2Id, IsActive = true, AiModerationStatus = AiModerationStatus.AutoApproved, ViewCount = 44, LikeCount = 9 },

            // Furniture
            new() { Id = Guid.NewGuid(), Title = "Бебешко легло с матрак Ikea Sniglar", Description = "IKEA Sniglar легло 60x120, включва матрак. Лесно сгъваемо. Много добро състояние.", CategoryId = Cat("furniture"), ListingType = ListingType.Sell, AgeGroup = AgeGroup.Newborn, Price = 70.00m, UserId = user3Id, IsActive = true, AiModerationStatus = AiModerationStatus.AutoApproved, ViewCount = 138, LikeCount = 26 },
        };

        dbContext.Items.AddRange(items);
        await dbContext.SaveChangesAsync();

        var feedbacks = new List<Feedback>
        {
            new() { Id = Guid.NewGuid(), UserId = user1Id, Rating = 5, Category = FeedbackCategory.Praise, Content = "Страхотно приложение! Намерих страхотни неща за бебето на много добри цени. Много лесно за ползване.", IsContactable = true },
            new() { Id = Guid.NewGuid(), UserId = user2Id, Rating = 4, Category = FeedbackCategory.FeatureRequest, Content = "Много ми харесва идеята! Бих искала да има и опция за размяна на вещи, не само продажба и дарение.", IsContactable = false },
            new() { Id = Guid.NewGuid(), UserId = user3Id, Rating = 5, Category = FeedbackCategory.Praise, Content = "Дарих детски дрехи и намерих купувач за 2 часа. Невероятно! Препоръчвам на всички родители.", IsContactable = true },
            new() { Id = Guid.NewGuid(), UserId = user1Id, Rating = 4, Category = FeedbackCategory.Improvement, Content = "Би помогнало ако има филтри за окръг/квартал, а не само по град. Понякога транспортирането е проблем.", IsContactable = false },
            new() { Id = Guid.NewGuid(), UserId = user2Id, Rating = 5, Category = FeedbackCategory.FeatureRequest, Content = "Искам чат функция — да мога директно да говоря с продавача без да излизам от приложението.", IsContactable = true },
            new() { Id = Guid.NewGuid(), UserId = user3Id, Rating = 3, Category = FeedbackCategory.BugReport, Content = "На телефон с Android понякога снимките не се зареждат веднага. Малко бавно при слаб интернет.", IsContactable = false },
        };

        dbContext.Feedbacks.AddRange(feedbacks);
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds sample doctor reviews and child-friendly places for development.
    /// Runs only in Development and only when the tables are empty.
    /// </summary>
    public static async Task SeedCommunityDataAsync(
        ApplicationDbContext dbContext,
        IWebHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
            return;

        var botId = AiBotConstants.UserId;

        if (!dbContext.DoctorReviews.Any())
        {
            var reviews = new[]
            {
                new MomVibe.Domain.Entities.DoctorReview
                {
                    Id = Guid.NewGuid(),
                    UserId = botId,
                    DoctorName = "Мария Иванова",
                    Specialization = "Педиатър",
                    ClinicName = "МЦ Здраве",
                    City = "София",
                    Rating = 5,
                    Content = "Изключителен лекар! Много внимателна и търпелива с детето ни. Обяснява всичко подробно и никога не бърза. Горещо препоръчвам!",
                    SuperdocUrl = null,
                    IsAnonymous = false,
                    IsApproved = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-14),
                },
                new MomVibe.Domain.Entities.DoctorReview
                {
                    Id = Guid.NewGuid(),
                    UserId = botId,
                    DoctorName = "Георги Петров",
                    Specialization = "Детски хирург",
                    ClinicName = "УМБАЛ Александровска",
                    City = "София",
                    Rating = 4,
                    Content = "Много компетентен специалист. Операцията мина перфектно и проследяването беше отлично. Малко трудно се вземат часове, но си струва чакането.",
                    SuperdocUrl = null,
                    IsAnonymous = false,
                    IsApproved = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                },
                new MomVibe.Domain.Entities.DoctorReview
                {
                    Id = Guid.NewGuid(),
                    UserId = botId,
                    DoctorName = "Елена Стоянова",
                    Specialization = "Педиатър",
                    ClinicName = "Детска поликлиника Пловдив",
                    City = "Пловдив",
                    Rating = 5,
                    Content = "Д-р Стоянова е невероятна! Децата я обичат и никога не плачат при нея. Дава изчерпателни съвети за хранене и развитие. Нашият семеен педиатър от 3 години.",
                    SuperdocUrl = null,
                    IsAnonymous = false,
                    IsApproved = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                },
                new MomVibe.Domain.Entities.DoctorReview
                {
                    Id = Guid.NewGuid(),
                    UserId = botId,
                    DoctorName = "Стефан Димитров",
                    Specialization = "Детски ортопед",
                    ClinicName = "МЦ Варна",
                    City = "Варна",
                    Rating = 5,
                    Content = "Открихме проблем с тазобедрените стави на бебето ни на 3 месеца. Д-р Димитров беше изключително внимателен и ни обясни всичко. Сега детето е напълно здраво!",
                    SuperdocUrl = null,
                    IsAnonymous = false,
                    IsApproved = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                },
                new MomVibe.Domain.Entities.DoctorReview
                {
                    Id = Guid.NewGuid(),
                    UserId = botId,
                    DoctorName = "Анна Николова",
                    Specialization = "Детски невролог",
                    ClinicName = null,
                    City = "София",
                    Rating = 4,
                    Content = "Много добър специалист с голям опит. Установи проблема бързо и предложи ефективно лечение. Препоръчвам за деца с неврологични въпроси.",
                    SuperdocUrl = null,
                    IsAnonymous = false,
                    IsApproved = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                },
                new MomVibe.Domain.Entities.DoctorReview
                {
                    Id = Guid.NewGuid(),
                    UserId = botId,
                    DoctorName = "Красимир Велков",
                    Specialization = "Алерголог",
                    ClinicName = "Алергоцентър Пловдив",
                    City = "Пловдив",
                    Rating = 5,
                    Content = "Синът ми имаше тежка алергия към хранителни продукти. Д-р Велков направи пълна диагностика и разработи план. 6 месеца по-късно животът ни се промени напълно!",
                    SuperdocUrl = null,
                    IsAnonymous = false,
                    IsApproved = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                },
            };

            dbContext.DoctorReviews.AddRange(reviews);
            await dbContext.SaveChangesAsync();
        }

        if (!dbContext.ChildFriendlyPlaces.Any())
        {
            var places = new[]
            {
                new MomVibe.Domain.Entities.ChildFriendlyPlace
                {
                    Id = Guid.NewGuid(),
                    UserId = botId,
                    Name = "Южен парк",
                    Description = "Огромен парк в центъра на София с прекрасни детски площадки, езеро с патици и много зелени площи. Идеален за разходки с количка и пикник.",
                    Address = "бул. България, Южен парк",
                    City = "София",
                    PlaceType = MomVibe.Domain.Enums.PlaceType.Park,
                    AgeFromMonths = 0,
                    AgeToMonths = 144,
                    PhotoUrl = null,
                    Website = null,
                    IsApproved = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                },
                new MomVibe.Domain.Entities.ChildFriendlyPlace
                {
                    Id = Guid.NewGuid(),
                    UserId = botId,
                    Name = "Детски градски цирк",
                    Description = "Тематичен атракцион за деца с клоунове, акробати и животни. Представленията траят около час и са подходящи за деца от 3 години нагоре.",
                    Address = "бул. Цар Борис III 111",
                    City = "София",
                    PlaceType = MomVibe.Domain.Enums.PlaceType.ThemeAttraction,
                    AgeFromMonths = 36,
                    AgeToMonths = 144,
                    PhotoUrl = null,
                    Website = null,
                    IsApproved = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-18),
                },
                new MomVibe.Domain.Entities.ChildFriendlyPlace
                {
                    Id = Guid.NewGuid(),
                    UserId = botId,
                    Name = "Природонаучен музей",
                    Description = "Богата колекция от животни, минерали и праисторически експонати. Има специална детска зала с интерактивни експозиции. Децата обожават динозаврите!",
                    Address = "бул. Цар Освободител 1",
                    City = "София",
                    PlaceType = MomVibe.Domain.Enums.PlaceType.Museum,
                    AgeFromMonths = 24,
                    AgeToMonths = 180,
                    PhotoUrl = null,
                    Website = null,
                    IsApproved = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                },
                new MomVibe.Domain.Entities.ChildFriendlyPlace
                {
                    Id = Guid.NewGuid(),
                    UserId = botId,
                    Name = "Детска площадка Цар Симеон",
                    Description = "Добре поддържана площадка с люлки, пързалки и катерушки. Има пясъчник и засенчени места за сядане. Родителите харесват спокойната атмосфера.",
                    Address = "Градина Цар Симеон, Пловдив",
                    City = "Пловдив",
                    PlaceType = MomVibe.Domain.Enums.PlaceType.Playground,
                    AgeFromMonths = 12,
                    AgeToMonths = 96,
                    PhotoUrl = null,
                    Website = null,
                    IsApproved = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-12),
                },
                new MomVibe.Domain.Entities.ChildFriendlyPlace
                {
                    Id = Guid.NewGuid(),
                    UserId = botId,
                    Name = "Делфинариум Варна",
                    Description = "Единственият делфинариум в България! Шоуто е невероятно — децата остават с отворени уста. Има и зона за снимки с делфините след представлението.",
                    Address = "Приморски парк, Варна",
                    City = "Варна",
                    PlaceType = MomVibe.Domain.Enums.PlaceType.Zoo,
                    AgeFromMonths = 18,
                    AgeToMonths = null,
                    PhotoUrl = null,
                    Website = null,
                    IsApproved = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-9),
                },
                new MomVibe.Domain.Entities.ChildFriendlyPlace
                {
                    Id = Guid.NewGuid(),
                    UserId = botId,
                    Name = "Ресторант Дино",
                    Description = "Семеен ресторант с тематична детска стая и кът за игра. Има специално детско меню, детски стол и пеленален кът. Персоналът е много внимателен към семействата.",
                    Address = "ул. Витоша 45",
                    City = "София",
                    PlaceType = MomVibe.Domain.Enums.PlaceType.Restaurant,
                    AgeFromMonths = 0,
                    AgeToMonths = 96,
                    PhotoUrl = null,
                    Website = null,
                    IsApproved = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-6),
                },
                new MomVibe.Domain.Entities.ChildFriendlyPlace
                {
                    Id = Guid.NewGuid(),
                    UserId = botId,
                    Name = "Плаж Кабакум",
                    Description = "Семеен плаж с плитка вода — перфектен за малки деца. Има чадъри под наем, детски атракциони и добра инфраструктура. Водата е чиста и топла през лятото.",
                    Address = "Кабакум, Варна",
                    City = "Варна",
                    PlaceType = MomVibe.Domain.Enums.PlaceType.Beach,
                    AgeFromMonths = 6,
                    AgeToMonths = null,
                    PhotoUrl = null,
                    Website = null,
                    IsApproved = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-4),
                },
            };

            dbContext.ChildFriendlyPlaces.AddRange(places);
            await dbContext.SaveChangesAsync();
        }
    }
}
