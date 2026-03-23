namespace MomVibe.Infrastructure.Persistence.Seed;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using MomVibe.Domain.Entities;

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
}
