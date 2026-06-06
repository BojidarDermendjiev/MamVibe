namespace MomVibe.Infrastructure.Persistence.Seed;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

using MomVibe.Domain.Entities;
using MomVibe.Domain.Constants;

public static class DataSeeder
{
    private static readonly string[] Roles = ["Admin", "User"];

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
        IConfiguration configuration)
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
}
