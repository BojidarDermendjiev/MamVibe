namespace MomVibe.Application.Interfaces;

public interface IKnowledgeService
{
    Task<IReadOnlyList<KnowledgeArticleDto>> SearchAsync(string query, string language, int topK = 4);
}

public sealed class KnowledgeArticleDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
