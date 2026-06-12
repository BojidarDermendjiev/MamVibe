namespace MomVibe.Infrastructure.Persistence.Seed;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Microsoft.EntityFrameworkCore;

using MomVibe.Domain.Entities;
using MomVibe.Domain.Constants;

public static class DataSeeder
{
    private static readonly string[] Roles = ["Admin", "User", "Business", "Promoter"];

    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    public static async Task SeedAdminAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger logger)
    {
        var enabled = configuration.GetValue<bool>("AdminSeed:Enabled");
        if (!enabled)
            return;

        var email = configuration["AdminSeed:Email"];
        var password = configuration["AdminSeed:Password"];
        var displayName = configuration["AdminSeed:DisplayName"] ?? "Admin";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        if (password.StartsWith("CHANGE_ME", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "AdminSeed:Password is a placeholder. Replace the CHANGE_ME value with a real password before enabling AdminSeed.");

        if (password.Length < 12)
            throw new InvalidOperationException(
                "AdminSeed:Password must be at least 12 characters long.");

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
            await userManager.AddToRoleAsync(admin, "Admin");

        // Remind the operator to disable the flag after the first successful seed.
        logger.LogWarning(
            "SECURITY: AdminSeed:Enabled is still true. " +
            "Set ADMIN_SEED_ENABLED=false in .env and restart the container.");
    }

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

    public static async Task SeedCategoriesAsync(ApplicationDbContext dbContext)
    {
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
    /// Seeds the 4 default subscription tiers for the business vertical: Trial (7d free),
    /// Basic (€9.99), Featured (€24.99 — top-of-list boost), Premium (€49.99 — top + analytics).
    /// Stripe Price IDs are intentionally left null and must be populated by the operator in
    /// admin → policies once Stripe is configured for the environment.
    /// </summary>
    public static async Task SeedSubscriptionPlansAsync(ApplicationDbContext dbContext)
    {
        if (await dbContext.SubscriptionPlans.AnyAsync())
            return;

        var plans = new[]
        {
            new SubscriptionPlan
            {
                Code = "Trial",
                DisplayName = "Trial",
                MonthlyPriceEur = 0m,
                RankBoost = 0,
                TrialDays = 7,
                SortOrder = 0,
                FeaturesJson = "{\"badge\":null,\"photoLimit\":5,\"analytics\":false}",
            },
            new SubscriptionPlan
            {
                Code = "Basic",
                DisplayName = "Basic",
                MonthlyPriceEur = 9.99m,
                RankBoost = 0,
                TrialDays = 0,
                SortOrder = 1,
                FeaturesJson = "{\"badge\":null,\"photoLimit\":5,\"analytics\":false}",
            },
            new SubscriptionPlan
            {
                Code = "Featured",
                DisplayName = "Featured",
                MonthlyPriceEur = 24.99m,
                RankBoost = 50,
                TrialDays = 0,
                SortOrder = 2,
                FeaturesJson = "{\"badge\":\"featured\",\"photoLimit\":10,\"analytics\":true}",
            },
            new SubscriptionPlan
            {
                Code = "Premium",
                DisplayName = "Premium",
                MonthlyPriceEur = 49.99m,
                RankBoost = 100,
                TrialDays = 0,
                SortOrder = 3,
                FeaturesJson = "{\"badge\":\"premium\",\"photoLimit\":15,\"analytics\":true,\"prioritySupport\":true}",
            },
        };

        dbContext.SubscriptionPlans.AddRange(plans);
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds the initial English + Bulgarian business policy version (v1). Admins replace
    /// these placeholders via the policies editor before public launch; the markdown bodies
    /// below are intentionally short and not legally binding — operator must review with counsel.
    /// </summary>
    public static async Task SeedBusinessPolicyAsync(ApplicationDbContext dbContext)
    {
        if (await dbContext.BusinessPolicyVersions.AnyAsync())
            return;

        var now = DateTime.UtcNow;
        var policies = new[]
        {
            new BusinessPolicyVersion
            {
                Version = 1,
                Language = "en",
                Title = "MamVibe Business Vertical — Platform Policy",
                BodyMarkdown =
                    "## Welcome, partner.\n\n" +
                    "By creating a business profile on MamVibe you agree to the following:\n\n" +
                    "1. **One profile per business.** Each business is permitted exactly one active listing. To run a second business you must register a separate account.\n" +
                    "2. **Accuracy.** All listing content, photos, location, age range, and pricing must accurately represent the activity you offer.\n" +
                    "3. **Child safety.** Activities must comply with all applicable Bulgarian and EU regulations for working with minors, including required certifications and parental supervision standards.\n" +
                    "4. **Trial billing.** Your 7-day trial requires a card on file. We charge the Basic plan price (€9.99) on day 7 unless you cancel or upgrade via the Customer Portal.\n" +
                    "5. **Moderation.** Listings are admin-reviewed before going public and may be hidden at any time for policy violations, including misleading claims, harassment, or unsafe practices.\n" +
                    "6. **Reports.** Parents may like, comment, or report your listing. Repeated valid reports may lead to suspension.\n" +
                    "7. **Data retention.** Your device fingerprint is hashed (SHA-256) and stored solely to prevent duplicate-account abuse. See our Privacy Policy for full details.\n" +
                    "8. **Cancellation.** You may cancel anytime via the Customer Portal. Listings remain visible until the end of the current billing period.\n\n" +
                    "Failure to comply may result in immediate listing removal and account suspension at MamVibe's discretion.",
                EffectiveFrom = now,
                IsCurrent = true,
            },
            new BusinessPolicyVersion
            {
                Version = 1,
                Language = "bg",
                Title = "Политика за бизнес профили в MamVibe",
                BodyMarkdown =
                    "## Добре дошли, партньоре.\n\n" +
                    "Със създаването на бизнес профил в MamVibe Вие се съгласявате със следното:\n\n" +
                    "1. **Един профил на бизнес.** Всеки бизнес може да поддържа точно една активна обява. За втора дейност е необходимо да създадете отделен акаунт.\n" +
                    "2. **Достоверност.** Текстът, снимките, локацията, възрастовият диапазон и цените трябва точно да представят предлаганата от Вас дейност.\n" +
                    "3. **Безопасност на децата.** Дейностите трябва да отговарят на българското и европейското законодателство за работа с непълнолетни, включително изискваните сертификати и стандарти за родителски контрол.\n" +
                    "4. **Пробен период.** 7-дневният пробен период изисква активна карта. На 7-ия ден таксуваме плана Basic (€9.99), освен ако не откажете или промените плана чрез клиентския портал.\n" +
                    "5. **Модерация.** Обявите се преглеждат от администратор преди публикуване и могат да бъдат скрити по всяко време при нарушение на политиките.\n" +
                    "6. **Сигнали.** Родителите могат да харесват, коментират или сигнализират Вашата обява. Многократни валидни сигнали могат да доведат до спиране.\n" +
                    "7. **Защита на данните.** Идентификаторът на Вашето устройство се съхранява хеширан (SHA-256) единствено за предотвратяване на злоупотреби с дублиращи се акаунти. Вижте Политиката за поверителност.\n" +
                    "8. **Отказ.** Можете да прекратите по всяко време чрез клиентския портал. Обявите остават видими до края на текущия платежен период.\n\n" +
                    "Неспазването на горните условия може да доведе до незабавно сваляне на обявата и спиране на акаунта по преценка на MamVibe.",
                EffectiveFrom = now,
                IsCurrent = true,
            },
        };

        dbContext.BusinessPolicyVersions.AddRange(policies);
        await dbContext.SaveChangesAsync();
    }
}
