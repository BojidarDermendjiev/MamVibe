namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Application.Interfaces;

/// <summary>
/// Public API controller for browsing item categories:
/// - List all categories
/// - Retrieve a single category by its identifier
/// Returns projected fields: Id, Name, Description, and Slug.
/// </summary>

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoriesController"/>.
    /// </summary>
    /// <param name="context">Application database context used to query categories.</param>
    public CategoriesController(IApplicationDbContext context)
    {
        this._context = context;
    }

    /// <summary>
    /// Retrieves all categories.
    /// </summary>
    /// <returns>
    /// 200 OK with a list of category projections (Id, Name, Description, Slug).
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await this._context.Categories
            .Select(c => new { c.Id, c.Name, c.Description, c.Slug })
            .ToListAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Retrieves a single category by its GUID.
    /// </summary>
    /// <param name="id">The GUID of the category to fetch.</param>
    /// <returns>
    /// 404 Not Found if the category does not exist.<br/>
    /// 200 OK with the category projection (Id, Name, Description, Slug) on success.
    /// </returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await this._context.Categories.FindAsync(id);
        if (category == null) return NotFound();
        return Ok(new { category.Id, category.Name, category.Description, category.Slug });
    }
}
