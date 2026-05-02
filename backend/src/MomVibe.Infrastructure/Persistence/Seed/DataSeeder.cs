namespace MomVibe.Infrastructure.Persistence.Seed;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using MomVibe.Domain.Entities;
using MomVibe.Domain.Constants;

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
                    IsAnonymous = true,
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
                    IsAnonymous = true,
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
