namespace ApiCrmAlive.Utils;

public sealed class MinioOptions
{
    // Can be "http://localhost:9000" or "localhost:9000" etc.
    public string? Endpoint { get; set; }
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
    public string? Bucket { get; set; }

    // If Endpoint has scheme, scheme wins. Otherwise UseSsl controls SSL.
    public bool UseSsl { get; set; }

    // If set, returned URLs will be built from this base instead of Endpoint.
    // Useful when MinIO is behind a reverse proxy or internal DNS.
    public string? PublicBaseUrl { get; set; }

    // If true, returns a presigned GET URL for each uploaded object (works with private buckets).
    public bool UsePresignedGetUrls { get; set; } = true;

    // Expiration for presigned GET URLs (in minutes).
    public int PresignedGetExpiryMinutes { get; set; } = 60;

    // If true, ensures bucket exists on startup of uploader.
    public bool CreateBucketIfMissing { get; set; }
}
