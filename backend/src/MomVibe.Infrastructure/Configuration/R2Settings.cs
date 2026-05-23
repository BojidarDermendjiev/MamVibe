namespace MomVibe.Infrastructure.Configuration;

public class R2Settings
{
    public string AccountId { get; set; } = string.Empty;
    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretAccessKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;

    /// <summary>Public base URL for the R2 bucket, e.g. https://pub-xxx.r2.dev or a custom domain.</summary>
    public string PublicUrl { get; set; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(AccountId) &&
        !string.IsNullOrWhiteSpace(AccessKeyId) &&
        !string.IsNullOrWhiteSpace(SecretAccessKey) &&
        !string.IsNullOrWhiteSpace(BucketName) &&
        !string.IsNullOrWhiteSpace(PublicUrl);
}
