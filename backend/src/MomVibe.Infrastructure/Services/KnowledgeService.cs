namespace MomVibe.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Application.Interfaces;

public class KnowledgeService : IKnowledgeService
{
    private readonly IApplicationDbContext _context;

    public KnowledgeService(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<KnowledgeArticleDto>> SearchAsync(string query, string language, int topK = 4)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query)) return [];

            var q = query.Trim();
            var lang = string.IsNullOrWhiteSpace(language) ? "en" : language.Trim().ToLowerInvariant();

            // EF Core 8 SqlQuery maps result columns to KnowledgeArticleDto by name (case-insensitive).
            // websearch_to_tsquery safely handles arbitrary user input without injection risk.
            var rows = await _context.Database
                .SqlQuery<KnowledgeArticleDto>($"""
                    SELECT "Title", "Content"
                    FROM "KnowledgeArticles"
                    WHERE ("Language" = {lang} OR "Language" = 'en')
                      AND "SearchVector" @@ websearch_to_tsquery('simple', {q})
                    ORDER BY ts_rank_cd("SearchVector", websearch_to_tsquery('simple', {q})) DESC
                    LIMIT {topK}
                    """)
                .ToListAsync();

            return rows;
        }
        catch
        {
            return [];
        }
    }
}
