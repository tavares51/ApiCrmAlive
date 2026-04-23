namespace ApiCrmAlive.Utils;

public sealed class SupabaseStorageOptions
{
    public string? Url { get; set; }
    public string? ServiceRoleKey { get; set; }
    public string? Bucket { get; set; }
    public bool UseSignedUrls { get; set; } = true;
    public int SignedUrlExpirySeconds { get; set; } = 3600;
    public bool AllowInvalidTls { get; set; } = false;
}
