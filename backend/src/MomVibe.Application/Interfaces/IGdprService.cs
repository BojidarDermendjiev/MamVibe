namespace MomVibe.Application.Interfaces;

/// <summary>
/// GDPR compliance service:
/// - Article 20 (Data Portability): export all personal data as a structured object.
/// - Article 17 (Right to Erasure): delete or anonymize personal data while retaining
///   financial records required by Bulgarian law (5-year fiscal retention).
/// </summary>
public interface IGdprService
{
    /// <summary>
    /// Exports all personal data held for the specified user.
    /// Returns a structured object suitable for JSON serialization and download.
    /// </summary>
    Task<object> ExportDataAsync(string userId);

    /// <summary>
    /// Erases all personal data for the specified user:
    /// - Deletes items (and their stored photos), likes, follows, saved searches,
    ///   offers, feedbacks, doctor reviews, child-friendly places, ratings, and refresh tokens.
    /// - Anonymizes sent message content to "[deleted]" (retains conversation thread structure).
    /// - Does NOT delete payment records (fiscal retention obligation).
    /// Call UserManager.DeleteAsync separately after this method to remove the identity account.
    /// </summary>
    Task ErasePersonalDataAsync(string userId);
}
