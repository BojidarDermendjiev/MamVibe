namespace MomVibe.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Application.Interfaces;
using Infrastructure.Configuration;

public class AiService : IAiService
{
    private readonly AnthropicSettings _settings;
    private readonly GroqSettings _groqSettings;
    private readonly IApplicationDbContext _context;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public AiService(
        IOptions<AnthropicSettings> settings,
        IOptions<GroqSettings> groqSettings,
        IApplicationDbContext context,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _settings        = settings.Value;
        _groqSettings    = groqSettings.Value;
        _context         = context;
        _serviceProvider = serviceProvider;
        _configuration   = configuration;
    }

    private async Task<string> GetModelAsync()
    {
        var setting = await _context.AppSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == "AI:Model");
        return setting?.Value ?? _settings.Model;
    }

    private async Task<string?> GetSettingAsync(string key) =>
        (await _context.AppSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == key))?.Value;

    public async Task<string> ChatAsync(
        string systemPrompt,
        IReadOnlyList<(string role, string content)> history)
    {
        var providerKey = await GetSettingAsync("AI:ChatProvider")
            ?? _configuration["AI:ChatProvider"]
            ?? "anthropic";

        var model = providerKey == "groq"
            ? (await GetSettingAsync("AI:GroqModel") ?? _groqSettings.Model)
            : await GetModelAsync();

        var client = _serviceProvider.GetRequiredKeyedService<IChatClient>(providerKey);

        var messages = new List<ChatMessage>(history.Count + 1)
        {
            new ChatMessage(ChatRole.System, systemPrompt)
        };
        foreach (var (role, content) in history)
        {
            var chatRole = role == "assistant" ? ChatRole.Assistant : ChatRole.User;
            messages.Add(new ChatMessage(chatRole, content));
        }

        var result = await client.GetResponseAsync(messages, new ChatOptions { ModelId = model });
        return result.Text ?? string.Empty;
    }
}
