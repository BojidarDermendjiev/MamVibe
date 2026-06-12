namespace MomVibe.Infrastructure.EmailTemplates;

/// <summary>
/// Inline HTML email templates for user-moderation notifications. Two locales: <c>bg</c> (primary
/// market) and <c>en</c>. Locale falls back to <c>bg</c> when unset to match the audience default.
/// </summary>
/// <remarks>
/// Templates are intentionally simple HTML strings — the codebase has no Razor email templating
/// engine, and this matches the existing forgot-password email pattern in <c>AuthService</c>.
/// </remarks>
public static class ModerationEmails
{
    public record RenderedEmail(string Subject, string HtmlBody);

    public static RenderedEmail Render(
        string templateKey,
        string locale,
        string displayName,
        string publicReason,
        DateTime? expiresAtUtc)
    {
        var isBg = string.Equals(locale, "en", StringComparison.OrdinalIgnoreCase) == false;
        var expiry = expiresAtUtc.HasValue
            ? expiresAtUtc.Value.ToString("yyyy-MM-dd HH:mm 'UTC'")
            : (isBg ? "безсрочно" : "indefinite");
        var safeName = System.Net.WebUtility.HtmlEncode(displayName ?? string.Empty);
        var safeReason = System.Net.WebUtility.HtmlEncode(publicReason ?? string.Empty);

        return templateKey switch
        {
            "moderation.warned"     => isBg ? Bg_Warned(safeName, safeReason)               : En_Warned(safeName, safeReason),
            "moderation.restricted" => isBg ? Bg_Restricted(safeName, safeReason)           : En_Restricted(safeName, safeReason),
            "moderation.suspended"  => isBg ? Bg_Suspended(safeName, safeReason, expiry)    : En_Suspended(safeName, safeReason, expiry),
            "moderation.banned"     => isBg ? Bg_Banned(safeName, safeReason)               : En_Banned(safeName, safeReason),
            "moderation.cleared"    => isBg ? Bg_Cleared(safeName)                          : En_Cleared(safeName),
            _                       => isBg ? Bg_Generic(safeName, safeReason)              : En_Generic(safeName, safeReason),
        };
    }

    private static RenderedEmail Bg_Warned(string name, string reason) => new(
        "Известие от екипа на MamVibe",
        $"<p>Здравей, {name},</p><p>Получаваш това съобщение, защото забелязахме поведение в акаунта ти, което може да наруши общностните правила: <em>{reason}</em>.</p><p>Това е информационно предупреждение — няма ограничения върху акаунта ти. Ако смяташ, че сме сгрешили, отговори на този email.</p><p>— Екипът на MamVibe</p>");

    private static RenderedEmail En_Warned(string name, string reason) => new(
        "A note from the MamVibe team",
        $"<p>Hi {name},</p><p>You're receiving this because we noticed account behaviour that may violate our community rules: <em>{reason}</em>.</p><p>This is an informational warning — no restrictions have been applied. If you believe we got it wrong, reply to this email.</p><p>— The MamVibe team</p>");

    private static RenderedEmail Bg_Restricted(string name, string reason) => new(
        "Акаунтът ти в MamVibe е в режим само за четене",
        $"<p>Здравей, {name},</p><p>Акаунтът ти временно е в режим само за четене поради следната причина: <em>{reason}</em>.</p><p>Може да разглеждаш платформата, но няма да можеш да създаваш обяви, да изпращаш съобщения, да правиш оферти или да купуваш, докато ограничението е активно.</p><p>Можеш да обжалваш това решение в твоя профил.</p><p>— Екипът на MamVibe</p>");

    private static RenderedEmail En_Restricted(string name, string reason) => new(
        "Your MamVibe account is now read-only",
        $"<p>Hi {name},</p><p>Your account has been placed in read-only mode for the following reason: <em>{reason}</em>.</p><p>You can keep browsing, but cannot list, message, make offers, or buy while the restriction is in place.</p><p>You can submit an appeal from your profile.</p><p>— The MamVibe team</p>");

    private static RenderedEmail Bg_Suspended(string name, string reason, string expiry) => new(
        $"Акаунтът ти в MamVibe е спрян до {expiry}",
        $"<p>Здравей, {name},</p><p>Акаунтът ти е временно спрян. Причина: <em>{reason}</em>. Спирането изтича на: <strong>{expiry}</strong>.</p><p>През този период няма да можеш да влизаш в MamVibe. След изтичането достъпът ти ще бъде възстановен автоматично.</p><p>Ако смяташ, че сме сгрешили, можеш да подадеш жалба от профила си.</p><p>— Екипът на MamVibe</p>");

    private static RenderedEmail En_Suspended(string name, string reason, string expiry) => new(
        $"Your MamVibe account is suspended until {expiry}",
        $"<p>Hi {name},</p><p>Your account has been suspended. Reason: <em>{reason}</em>. The suspension expires on: <strong>{expiry}</strong>.</p><p>You won't be able to sign in to MamVibe during this period. Access will be restored automatically after the expiry.</p><p>If you believe this is a mistake, you can submit an appeal from your profile.</p><p>— The MamVibe team</p>");

    private static RenderedEmail Bg_Banned(string name, string reason) => new(
        "Акаунтът ти в MamVibe е затворен",
        $"<p>Здравей, {name},</p><p>Акаунтът ти е затворен за постоянно. Причина: <em>{reason}</em>.</p><p>Имаш право да подадеш една жалба от страницата за вход. Тя ще бъде разгледана от член на нашия екип.</p><p>— Екипът на MamVibe</p>");

    private static RenderedEmail En_Banned(string name, string reason) => new(
        "Your MamVibe account has been permanently closed",
        $"<p>Hi {name},</p><p>Your account has been permanently closed. Reason: <em>{reason}</em>.</p><p>You may submit one appeal from the sign-in page. It will be reviewed by a member of our team.</p><p>— The MamVibe team</p>");

    private static RenderedEmail Bg_Cleared(string name) => new(
        "Достъпът ти в MamVibe е възстановен",
        $"<p>Здравей, {name},</p><p>Ограничението върху акаунта ти е премахнато. Можеш да продължиш да използваш MamVibe както обикновено.</p><p>— Екипът на MamVibe</p>");

    private static RenderedEmail En_Cleared(string name) => new(
        "Your MamVibe access has been restored",
        $"<p>Hi {name},</p><p>The moderation action on your account has been cleared. You can resume using MamVibe as usual.</p><p>— The MamVibe team</p>");

    private static RenderedEmail Bg_Generic(string name, string reason) => new(
        "Известие за акаунта в MamVibe",
        $"<p>Здравей, {name},</p><p>Има промяна в състоянието на акаунта ти. Причина: <em>{reason}</em>.</p><p>— Екипът на MamVibe</p>");

    private static RenderedEmail En_Generic(string name, string reason) => new(
        "MamVibe account notice",
        $"<p>Hi {name},</p><p>Your account state has changed. Reason: <em>{reason}</em>.</p><p>— The MamVibe team</p>");
}
